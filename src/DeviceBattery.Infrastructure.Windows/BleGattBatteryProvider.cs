using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using DeviceBattery.Application;
using DeviceBattery.Domain;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.Storage.Streams;

namespace DeviceBattery.Infrastructure.Windows;

public sealed class BleGattBatteryProvider : IBatteryProvider, IRefreshableBatteryProvider
{
    public const string Id = "BleGattBattery";
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan pollingInterval;
    private readonly ConcurrentDictionary<string, Registration> registrations = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> generations = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<int, Task> pendingOperations = [];
    private CancellationTokenSource? lifetimeCancellation;
    private ChannelWriter<ProviderEvent>? events;
    private DeviceWatcher? watcher;
    private int operationId;
    private int runState;

    public BleGattBatteryProvider(TimeProvider? timeProvider = null, TimeSpan? pollingInterval = null)
    {
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(30);
        if (this.pollingInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(pollingInterval));
    }

    public string ProviderId => Id;

    public async ValueTask RefreshAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (Registration registration in registrations.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Session? session = registration.Session;
            if (session is not null) await session.RefreshAsync().ConfigureAwait(false);
        }

        string selector = GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.Battery);
        DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);
        foreach (DeviceInformation information in devices)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!registrations.ContainsKey(information.Id)) await OpenAsync(information).ConfigureAwait(false);
        }
    }

    public async Task RunAsync(ChannelWriter<ProviderEvent> events, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);
        if (Interlocked.CompareExchange(ref runState, 1, 0) != 0)
            throw new InvalidOperationException("The provider can only be run once.");

        this.events = events;
        lifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        try
        {
            string selector = GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.Battery);
            watcher = DeviceInformation.CreateWatcher(selector);
            watcher.Added += OnAdded;
            watcher.Removed += OnRemoved;
            watcher.Start();
            await Task.Delay(Timeout.InfiniteTimeSpan, lifetimeCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested) { }
        finally
        {
            StopWatcher();
            lifetimeCancellation.Cancel();
            await DrainPendingOperationsAsync().ConfigureAwait(false);
            foreach (Registration registration in registrations.Values)
                await registration.DisposeSessionAsync().ConfigureAwait(false);
            registrations.Clear();
            this.events = null;
            Interlocked.Exchange(ref runState, 2);
        }
    }

    public async ValueTask DisposeAsync()
    {
        lifetimeCancellation?.Cancel();
        while (Volatile.Read(ref runState) == 1) await Task.Delay(10).ConfigureAwait(false);
        lifetimeCancellation?.Dispose();
        lifetimeCancellation = null;
    }

    private void OnAdded(DeviceWatcher sender, DeviceInformation information) => Track(OpenAsync(information));

    private void OnRemoved(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        if (!registrations.TryRemove(update.Id, out Registration? registration)) return;
        registration.MarkRemoved();
        TryPublish(new DeviceRemoved(registration.Key, registration.Generation, registration.NextSequence(), timeProvider.GetUtcNow()));
        Track(registration.DisposeSessionAsync().AsTask());
    }

    private async Task OpenAsync(DeviceInformation information)
    {
        if (registrations.ContainsKey(information.Id)) return;
        int generation = generations.AddOrUpdate(information.Id, 1, (_, value) => checked(value + 1));
        var registration = new Registration(CreateKey(information.Id), generation);
        if (!registrations.TryAdd(information.Id, registration)) return;

        try
        {
            GattDeviceService? service = await GattDeviceService.FromIdAsync(information.Id);
            if (service is null) { PublishFault(registration, "GATT_OPEN_NULL", "BLE Battery Service open returned no service."); return; }

            GattCharacteristicsResult result = await service.GetCharacteristicsForUuidAsync(
                GattCharacteristicUuids.BatteryLevel,
                BluetoothCacheMode.Uncached);
            if (result.Status != GattCommunicationStatus.Success || result.Characteristics.Count == 0)
            {
                service.Dispose();
                PublishFault(registration, "GATT_CHARACTERISTIC_UNAVAILABLE", "BLE Battery Level characteristic is unavailable.");
                return;
            }

            BluetoothLEDevice? bluetoothDevice = await BluetoothLEDevice.FromIdAsync(service.DeviceId);
            string displayName = string.IsNullOrWhiteSpace(information.Name) ? "Bluetooth Battery Device" : information.Name.Trim();
            var session = new Session(service, result.Characteristics[0], bluetoothDevice, displayName, registration, timeProvider, pollingInterval, TryPublish);
            if (!registration.TryAttach(session)) { await session.DisposeAsync().ConfigureAwait(false); return; }
            await session.StartAsync(lifetimeCancellation!.Token).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            PublishFault(registration, $"GATT_OPEN_{ex.GetType().Name}", "BLE Battery Service initialization failed.");
        }
    }

    private void PublishFault(Registration registration, string fingerprint, string message) =>
        TryPublish(new ProviderFaulted(registration.Key, registration.Generation, registration.NextSequence(), timeProvider.GetUtcNow(), fingerprint, message));

    private bool TryPublish(ProviderEvent providerEvent) => events?.TryWrite(providerEvent) == true;

    private static DeviceKey CreateKey(string serviceId)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(serviceId.ToUpperInvariant()));
        return new(Id, Convert.ToHexString(hash)[..24]);
    }

    private void StopWatcher()
    {
        if (watcher is null) return;
        watcher.Added -= OnAdded;
        watcher.Removed -= OnRemoved;
        if (watcher.Status is DeviceWatcherStatus.Started or DeviceWatcherStatus.EnumerationCompleted) watcher.Stop();
        watcher = null;
    }

    private void Track(Task operation)
    {
        int id = Interlocked.Increment(ref operationId);
        pendingOperations[id] = operation;
        _ = operation.ContinueWith(_ => pendingOperations.TryRemove(id, out Task? _), CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
    }

    private async Task DrainPendingOperationsAsync()
    {
        Task[] pending = pendingOperations.Values.ToArray();
        if (pending.Length > 0) await Task.WhenAll(pending).ConfigureAwait(false);
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
        public bool TryAttach(Session value) => !IsRemoved && Interlocked.CompareExchange(ref session, value, null) is null;
        public async ValueTask DisposeSessionAsync()
        {
            Session? current = Interlocked.Exchange(ref session, null);
            if (current is not null) await current.DisposeAsync().ConfigureAwait(false);
        }
    }

    private sealed class Session : IAsyncDisposable
    {
        private readonly object sync = new();
        private readonly GattDeviceService service;
        private readonly GattCharacteristic characteristic;
        private readonly BluetoothLEDevice? bluetoothDevice;
        private readonly string displayName;
        private readonly Registration registration;
        private readonly TimeProvider timeProvider;
        private readonly TimeSpan pollingInterval;
        private readonly Func<ProviderEvent, bool> publish;
        private CancellationTokenSource? pollingCancellation;
        private Task? pollingTask;
        private GattClientCharacteristicConfigurationDescriptorValue cccd;
        private int? lastPercent;
        private bool announced;
        private bool disposed;

        public Session(GattDeviceService service, GattCharacteristic characteristic, BluetoothLEDevice? bluetoothDevice, string displayName, Registration registration, TimeProvider timeProvider, TimeSpan pollingInterval, Func<ProviderEvent, bool> publish)
        {
            this.service = service;
            this.characteristic = characteristic;
            this.bluetoothDevice = bluetoothDevice;
            this.displayName = displayName;
            this.registration = registration;
            this.timeProvider = timeProvider;
            this.pollingInterval = pollingInterval;
            this.publish = publish;
        }

        public async Task StartAsync(CancellationToken lifetimeToken)
        {
            if (bluetoothDevice is not null)
                bluetoothDevice.ConnectionStatusChanged += OnConnectionStatusChanged;

            GattCharacteristicProperties properties = characteristic.CharacteristicProperties;
            cccd = properties.HasFlag(GattCharacteristicProperties.Notify)
                ? GattClientCharacteristicConfigurationDescriptorValue.Notify
                : properties.HasFlag(GattCharacteristicProperties.Indicate)
                    ? GattClientCharacteristicConfigurationDescriptorValue.Indicate
                    : GattClientCharacteristicConfigurationDescriptorValue.None;

            if (cccd != GattClientCharacteristicConfigurationDescriptorValue.None)
            {
                characteristic.ValueChanged += OnValueChanged;
                GattCommunicationStatus status = await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(cccd);
                if (status != GattCommunicationStatus.Success)
                {
                    characteristic.ValueChanged -= OnValueChanged;
                    cccd = GattClientCharacteristicConfigurationDescriptorValue.None;
                }
            }

            if (properties.HasFlag(GattCharacteristicProperties.Read))
                await ReadAndPublishAsync(true).ConfigureAwait(false);

            if (cccd == GattClientCharacteristicConfigurationDescriptorValue.None && properties.HasFlag(GattCharacteristicProperties.Read))
            {
                pollingCancellation = CancellationTokenSource.CreateLinkedTokenSource(lifetimeToken);
                pollingTask = PollAsync(pollingCancellation.Token);
            }
        }

        public Task RefreshAsync() => ReadAndPublishAsync(true);

        private async Task PollAsync(CancellationToken cancellationToken)
        {
            using PeriodicTimer timer = new(pollingInterval, timeProvider);
            try
            {
                while (await timer.WaitForNextTickAsync(cancellationToken).ConfigureAwait(false))
                    await ReadAndPublishAsync(false).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        }

        private async Task ReadAndPublishAsync(bool initial)
        {
            try
            {
                GattReadResult result = await characteristic.ReadValueAsync(BluetoothCacheMode.Uncached);
                if (result.Status != GattCommunicationStatus.Success || !TryParse(result.Value, out int percent)) return;
                PublishPercent(percent, initial);
            }
            catch (Exception) { }
        }

        private void OnValueChanged(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            try { if (TryParse(args.CharacteristicValue, out int percent)) PublishPercent(percent, false); }
            catch (Exception) { }
        }

        private void PublishPercent(int percent, bool initial)
        {
            DeviceDiscovered? discovered = null;
            ProviderEvent? batteryEvent = null;
            lock (sync)
            {
                if (disposed || lastPercent == percent) return;
                bool first = lastPercent is null;
                lastPercent = percent;
                DateTimeOffset observedAt = timeProvider.GetUtcNow();
                if (!announced)
                {
                    announced = true;
                    discovered = new DeviceDiscovered(registration.Key, registration.Generation, registration.NextSequence(), observedAt, displayName);
                }
                BatteryState battery = BatteryState.Available(percent, ChargingState.Unknown, BatteryPrecision.ExactPercent, timeProvider.GetUtcNow(), Id);
                batteryEvent = first || initial
                    ? new ReportRecovered(registration.Key, registration.Generation, registration.NextSequence(), battery.ObservedAt, battery)
                    : new BatteryChanged(registration.Key, registration.Generation, registration.NextSequence(), battery.ObservedAt, battery);
            }
            if (discovered is not null) publish(discovered);
            publish(batteryEvent);
        }

        private void OnConnectionStatusChanged(BluetoothLEDevice sender, object args)
        {
            if (sender.ConnectionStatus == BluetoothConnectionStatus.Connected)
            {
                _ = ReadAndPublishAsync(true);
                return;
            }

            DeviceRemoved? removed = null;
            lock (sync)
            {
                if (disposed || !announced) return;
                announced = false;
                lastPercent = null;
                removed = new DeviceRemoved(registration.Key, registration.Generation, registration.NextSequence(), timeProvider.GetUtcNow());
            }
            publish(removed);
        }

        private static bool TryParse(IBuffer buffer, out int percent)
        {
            percent = 0;
            if (buffer.Length < 1) return false;
            using DataReader reader = DataReader.FromBuffer(buffer);
            byte value = reader.ReadByte();
            return BleBatteryLevelParser.TryParse([value], out percent);
        }

        public async ValueTask DisposeAsync()
        {
            lock (sync) { if (disposed) return; disposed = true; }
            pollingCancellation?.Cancel();
            if (pollingTask is not null) await pollingTask.ConfigureAwait(false);
            pollingCancellation?.Dispose();
            characteristic.ValueChanged -= OnValueChanged;
            if (bluetoothDevice is not null)
                bluetoothDevice.ConnectionStatusChanged -= OnConnectionStatusChanged;
            if (cccd != GattClientCharacteristicConfigurationDescriptorValue.None)
            {
                try { await characteristic.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None); }
                catch (Exception) { }
            }
            bluetoothDevice?.Dispose();
            service.Dispose();
        }
    }
}
