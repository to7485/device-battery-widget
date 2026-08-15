using System.Collections.Concurrent;
using System.Threading.Channels;
using DeviceBattery.Application;
using DeviceBattery.Domain;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

namespace DeviceBattery.Infrastructure.Windows;

public sealed class DualSenseHidProvider : IBatteryProvider, IRefreshableBatteryProvider
{
    private const ushort GenericDesktopUsagePage = 0x0001;
    private const ushort GamepadUsageId = 0x0005;
    private const ushort SonyVendorId = 0x054C;
    private const ushort DualSenseProductId = 0x0CE6;

    private readonly IHidBatteryParser parser;
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan unknownAfter;
    private readonly TimeSpan dormantAfter;
    private readonly ConcurrentDictionary<string, Registration> registrations =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> generations =
        new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<int, Task> pendingOperations = [];
    private CancellationTokenSource? lifetimeCancellation;
    private ChannelWriter<ProviderEvent>? events;
    private DeviceWatcher? watcher;
    private Task? freshnessTask;
    private int operationId;
    private int runState;

    public DualSenseHidProvider(
        IHidBatteryParser? parser = null,
        TimeProvider? timeProvider = null,
        TimeSpan? unknownAfter = null,
        TimeSpan? dormantAfter = null)
    {
        this.parser = parser ?? new DualSenseHidBatteryParser();
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.unknownAfter = unknownAfter ?? TimeSpan.FromSeconds(10);
        this.dormantAfter = dormantAfter ?? TimeSpan.FromSeconds(30);

        if (this.unknownAfter <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(unknownAfter));
        if (this.dormantAfter <= this.unknownAfter)
            throw new ArgumentOutOfRangeException(nameof(dormantAfter));
    }

    public string ProviderId => DualSenseHidBatteryParser.ProviderId;

    public async ValueTask RefreshAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(CreateSelector());
        foreach (DeviceInformation information in devices)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!DualSenseDeviceIdentity.IsSupportedEndpoint(information.Id)) continue;
            if (registrations.TryGetValue(information.Id, out Registration? registration))
                await ReopenReadOnlyAsync(information, registration).ConfigureAwait(false);
            else
                await OpenReadOnlyAsync(information).ConfigureAwait(false);
        }
    }

    public async Task RunAsync(
        ChannelWriter<ProviderEvent> events,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);
        if (Interlocked.CompareExchange(ref runState, 1, 0) != 0)
            throw new InvalidOperationException("The provider can only be run once.");

        this.events = events;
        lifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        CancellationToken lifetimeToken = lifetimeCancellation.Token;

        try
        {
            StartWatcher();
            freshnessTask = MonitorFreshnessAsync(lifetimeToken);
            await Task.Delay(Timeout.InfiniteTimeSpan, lifetimeToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (lifetimeToken.IsCancellationRequested)
        {
        }
        finally
        {
            StopWatcher();
            lifetimeCancellation.Cancel();
            if (freshnessTask is not null)
                await freshnessTask.ConfigureAwait(false);
            await DrainPendingOperationsAsync().ConfigureAwait(false);
            DisposeRegistrations();
            this.events = null;
            Interlocked.Exchange(ref runState, 2);
        }
    }

    public async ValueTask DisposeAsync()
    {
        lifetimeCancellation?.Cancel();
        while (Volatile.Read(ref runState) == 1)
            await Task.Delay(10).ConfigureAwait(false);
        lifetimeCancellation?.Dispose();
        lifetimeCancellation = null;
    }

    private void StartWatcher()
    {
        watcher = DeviceInformation.CreateWatcher(CreateSelector());
        watcher.Added += OnDeviceAdded;
        watcher.Removed += OnDeviceRemoved;
        watcher.Start();
    }

    private static string CreateSelector() => HidDevice.GetDeviceSelector(
            GenericDesktopUsagePage,
            GamepadUsageId,
            SonyVendorId,
            DualSenseProductId);

    private void StopWatcher()
    {
        if (watcher is null)
            return;

        watcher.Added -= OnDeviceAdded;
        watcher.Removed -= OnDeviceRemoved;
        if (watcher.Status is DeviceWatcherStatus.Started or DeviceWatcherStatus.EnumerationCompleted)
            watcher.Stop();
        watcher = null;
    }

    private void OnDeviceAdded(DeviceWatcher sender, DeviceInformation information)
    {
        if (!DualSenseDeviceIdentity.IsSupportedEndpoint(information.Id))
            return;
        Track(OpenReadOnlyAsync(information));
    }

    private void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        if (!registrations.TryRemove(update.Id, out Registration? registration))
            return;

        registration.MarkRemoved();
        registration.DisposeSession();
        TryPublish(new DeviceRemoved(
            registration.Key,
            registration.Generation,
            registration.NextSequence(),
            timeProvider.GetUtcNow()));
    }

    private async Task OpenReadOnlyAsync(DeviceInformation information)
    {
        if (registrations.ContainsKey(information.Id))
            return;

        DeviceKey key = DualSenseDeviceIdentity.CreateKey(information.Id);
        int generation = generations.AddOrUpdate(information.Id, 1, (_, value) => checked(value + 1));
        var registration = new Registration(key, generation);
        if (!registrations.TryAdd(information.Id, registration))
            return;

        DateTimeOffset discoveredAt = timeProvider.GetUtcNow();
        TryPublish(new DeviceDiscovered(
            key,
            generation,
            registration.NextSequence(),
            discoveredAt,
            GetDisplayName(information)));

        try
        {
            HidDevice? device = await HidDevice.FromIdAsync(information.Id, FileAccessMode.Read);
            if (device is null)
            {
                PublishFault(registration, "HID_OPEN_NULL", "Read-only HID open returned no device.");
                return;
            }

            if (registration.IsRemoved || !registrations.ContainsKey(information.Id))
            {
                device.Dispose();
                return;
            }

            var session = new Session(
                device,
                registration,
                parser,
                timeProvider,
                unknownAfter,
                dormantAfter,
                DualSenseDeviceIdentity.UsesReportFreshnessTimeout(information.Id),
                TryPublish);
            if (!registration.TryAttach(session))
            {
                session.Dispose();
                return;
            }
            session.Start();
        }
        catch (Exception ex)
        {
            PublishFault(registration, $"HID_OPEN_{ex.GetType().Name}", "Read-only HID open failed.");
        }
    }

    private async Task ReopenReadOnlyAsync(DeviceInformation information, Registration registration)
    {
        registration.DisposeSession();
        try
        {
            HidDevice? device = await HidDevice.FromIdAsync(information.Id, FileAccessMode.Read);
            if (device is null || registration.IsRemoved || !registrations.TryGetValue(information.Id, out Registration? current) || current != registration)
            {
                device?.Dispose();
                return;
            }
            var session = new Session(device, registration, parser, timeProvider, unknownAfter, dormantAfter,
                DualSenseDeviceIdentity.UsesReportFreshnessTimeout(information.Id), TryPublish);
            if (!registration.TryAttach(session)) { session.Dispose(); return; }
            session.Start();
        }
        catch (Exception ex)
        {
            PublishFault(registration, $"HID_REOPEN_{ex.GetType().Name}", "Read-only HID reopen after resume failed.");
        }
    }

    private async Task MonitorFreshnessAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1), timeProvider);
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
            {
                DateTimeOffset occurredAt = timeProvider.GetUtcNow();
                foreach (Registration registration in registrations.Values)
                    registration.Session?.EvaluateFreshness(occurredAt);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private void PublishFault(Registration registration, string fingerprint, string message) =>
        TryPublish(new ProviderFaulted(
            registration.Key,
            registration.Generation,
            registration.NextSequence(),
            timeProvider.GetUtcNow(),
            fingerprint,
            message));

    private static string GetDisplayName(DeviceInformation information)
    {
        if (DualSenseDeviceIdentity.IsUsbEndpoint(information.Id))
            return "DualSense Controller (USB)";
        return string.IsNullOrWhiteSpace(information.Name)
            ? "DualSense Wireless Controller"
            : information.Name;
    }

    private bool TryPublish(ProviderEvent providerEvent) => events?.TryWrite(providerEvent) == true;

    private void Track(Task operation)
    {
        int id = Interlocked.Increment(ref operationId);
        pendingOperations[id] = operation;
        _ = operation.ContinueWith(
            completedOperation => pendingOperations.TryRemove(id, out Task? _),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private async Task DrainPendingOperationsAsync()
    {
        Task[] pending = pendingOperations.Values.ToArray();
        if (pending.Length > 0)
            await Task.WhenAll(pending).ConfigureAwait(false);
    }

    private void DisposeRegistrations()
    {
        foreach (Registration registration in registrations.Values)
        {
            registration.MarkRemoved();
            registration.DisposeSession();
        }
        registrations.Clear();
    }

    private sealed class Registration(DeviceKey key, int generation)
    {
        private long sequence;
        private int removed;
        private Session? session;

        public DeviceKey Key { get; } = key;
        public int Generation { get; } = generation;
        public bool IsRemoved => Volatile.Read(ref removed) != 0;
        public Session? Session => Volatile.Read(ref session);
        public long NextSequence() => Interlocked.Increment(ref sequence);
        public void MarkRemoved() => Interlocked.Exchange(ref removed, 1);
        public bool TryAttach(Session value) =>
            !IsRemoved && Interlocked.CompareExchange(ref session, value, null) is null;
        public void DisposeSession() => Interlocked.Exchange(ref session, null)?.Dispose();
    }

    private sealed class Session : IDisposable
    {
        private readonly object sync = new();
        private readonly HidDevice device;
        private readonly Registration registration;
        private readonly IHidBatteryParser parser;
        private readonly TimeProvider timeProvider;
        private readonly ReportFreshnessTracker freshness;
        private readonly TimeSpan unknownAfter;
        private readonly bool usesFreshnessTimeout;
        private readonly Func<ProviderEvent, bool> publish;
        private byte lastStatus;
        private bool hasStatus;
        private bool disposed;

        public Session(
            HidDevice device,
            Registration registration,
            IHidBatteryParser parser,
            TimeProvider timeProvider,
            TimeSpan unknownAfter,
            TimeSpan dormantAfter,
            bool usesFreshnessTimeout,
            Func<ProviderEvent, bool> publish)
        {
            this.device = device;
            this.registration = registration;
            this.parser = parser;
            this.timeProvider = timeProvider;
            this.unknownAfter = unknownAfter;
            this.usesFreshnessTimeout = usesFreshnessTimeout;
            freshness = new(timeProvider, unknownAfter, dormantAfter);
            this.publish = publish;
        }

        public void Start() => device.InputReportReceived += OnInputReportReceived;

        public void EvaluateFreshness(DateTimeOffset occurredAt)
        {
            if (!usesFreshnessTimeout)
                return;

            ProviderEvent? first = null;
            ProviderEvent? second = null;
            lock (sync)
            {
                if (disposed)
                    return;

                FreshnessEvaluation evaluation = freshness.Evaluate();
                if (evaluation.ExpiredNow)
                {
                    first = new FreshnessExpired(
                        registration.Key,
                        registration.Generation,
                        registration.NextSequence(),
                        occurredAt,
                        $"No valid DualSense report for {unknownAfter.TotalSeconds:0} seconds");
                }
                if (evaluation.DormantNow)
                {
                    second = new DeviceOffline(
                        registration.Key,
                        registration.Generation,
                        registration.NextSequence(),
                        occurredAt);
                }
            }

            if (first is not null)
                publish(first);
            if (second is not null)
                publish(second);
        }

        public void Dispose()
        {
            lock (sync)
            {
                if (disposed)
                    return;
                disposed = true;
            }
            device.InputReportReceived -= OnInputReportReceived;
            device.Dispose();
        }

        private void OnInputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
        {
            try
            {
                byte[] report = ReadBytes(args.Report.Data);
                DateTimeOffset observedAt = timeProvider.GetUtcNow();
                if (!parser.TryParse(args.Report.Id, report, observedAt, out BatteryObservation observation))
                    return;

                ProviderEvent? semanticEvent = null;
                lock (sync)
                {
                    if (disposed)
                        return;

                    bool recovered = freshness.MarkValidReport();
                    bool changed = hasStatus && lastStatus != observation.RawStatusByte;
                    lastStatus = observation.RawStatusByte;
                    hasStatus = true;

                    if (recovered)
                    {
                        semanticEvent = new ReportRecovered(
                            registration.Key,
                            registration.Generation,
                            registration.NextSequence(),
                            observedAt,
                            observation.Battery);
                    }
                    else if (changed)
                    {
                        semanticEvent = new BatteryChanged(
                            registration.Key,
                            registration.Generation,
                            registration.NextSequence(),
                            observedAt,
                            observation.Battery);
                    }
                }

                if (semanticEvent is not null)
                    publish(semanticEvent);
            }
            catch (Exception ex)
            {
                publish(new ProviderFaulted(
                    registration.Key,
                    registration.Generation,
                    registration.NextSequence(),
                    timeProvider.GetUtcNow(),
                    $"INPUT_{ex.GetType().Name}",
                    "DualSense input report processing failed."));
            }
        }

        private static byte[] ReadBytes(IBuffer buffer)
        {
            using DataReader reader = DataReader.FromBuffer(buffer);
            byte[] data = new byte[reader.UnconsumedBufferLength];
            reader.ReadBytes(data);
            return data;
        }
    }
}
