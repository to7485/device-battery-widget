using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using DeviceBattery.Application;
using DeviceBattery.Domain;
using Windows.Devices.Power;
using Windows.Gaming.Input;

namespace DeviceBattery.Infrastructure.Windows;

public sealed class GamingInputBatteryProvider : IBatteryProvider, IRefreshableBatteryProvider
{
    public const string Id = "WindowsGamingInputBattery";
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan pollingInterval;
    private readonly ConcurrentDictionary<Gamepad, Registration> registrations = [];
    private readonly ConcurrentDictionary<string, int> generations = new(StringComparer.Ordinal);
    private ChannelWriter<ProviderEvent>? events;
    private CancellationTokenSource? lifetimeCancellation;
    private int runState;

    public GamingInputBatteryProvider(TimeProvider? timeProvider = null, TimeSpan? pollingInterval = null)
    {
        this.timeProvider = timeProvider ?? TimeProvider.System;
        this.pollingInterval = pollingInterval ?? TimeSpan.FromSeconds(30);
        if (this.pollingInterval <= TimeSpan.Zero) throw new ArgumentOutOfRangeException(nameof(pollingInterval));
    }

    public string ProviderId => Id;

    public ValueTask RefreshAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        foreach (Gamepad gamepad in Gamepad.Gamepads)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (!registrations.TryGetValue(gamepad, out Registration? registration)) Add(gamepad);
            else ReadAndPublish(gamepad, registration);
        }
        return ValueTask.CompletedTask;
    }

    public async Task RunAsync(ChannelWriter<ProviderEvent> events, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(events);
        if (Interlocked.CompareExchange(ref runState, 1, 0) != 0)
            throw new InvalidOperationException("The provider can only be run once.");

        this.events = events;
        lifetimeCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Gamepad.GamepadAdded += OnGamepadAdded;
        Gamepad.GamepadRemoved += OnGamepadRemoved;
        try
        {
            _ = Gamepad.Gamepads.Count;
            await Task.Delay(TimeSpan.FromSeconds(2.5), lifetimeCancellation.Token).ConfigureAwait(false);
            foreach (Gamepad gamepad in Gamepad.Gamepads) Add(gamepad);

            using PeriodicTimer timer = new(pollingInterval, timeProvider);
            while (await timer.WaitForNextTickAsync(lifetimeCancellation.Token).ConfigureAwait(false))
                foreach ((Gamepad gamepad, Registration registration) in registrations)
                    ReadAndPublish(gamepad, registration);
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested) { }
        finally
        {
            Gamepad.GamepadAdded -= OnGamepadAdded;
            Gamepad.GamepadRemoved -= OnGamepadRemoved;
            foreach (Registration registration in registrations.Values)
                PublishRemovedIfVisible(registration);
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

    private void OnGamepadAdded(object? sender, Gamepad gamepad) => Add(gamepad);

    private void OnGamepadRemoved(object? sender, Gamepad gamepad)
    {
        if (registrations.TryRemove(gamepad, out Registration? registration))
            PublishRemovedIfVisible(registration);
    }

    private void Add(Gamepad gamepad)
    {
        RawGameController? raw = RawGameController.FromGameController(gamepad);
        if (raw is null || raw.HardwareVendorId == 0x054C && raw.HardwareProductId == 0x0CE6) return;
        string identity = string.IsNullOrWhiteSpace(raw.NonRoamableId)
            ? $"{raw.HardwareVendorId:X4}:{raw.HardwareProductId:X4}:{raw.DisplayName}"
            : raw.NonRoamableId;
        DeviceKey key = CreateKey(identity);
        int generation = generations.AddOrUpdate(key.StableId, 1, (_, value) => checked(value + 1));
        string displayName = raw.HardwareVendorId == 0x045E
            ? "Xbox Wireless Controller"
            : string.IsNullOrWhiteSpace(raw.DisplayName) ? "Windows Game Controller" : raw.DisplayName.Trim();
        var registration = new Registration(key, generation, displayName);
        if (!registrations.TryAdd(gamepad, registration)) return;
        ReadAndPublish(gamepad, registration);
    }

    private void ReadAndPublish(Gamepad gamepad, Registration registration)
    {
        try
        {
            BatteryReport? report = gamepad.TryGetBatteryReport();
            DateTimeOffset observedAt = timeProvider.GetUtcNow();
            if (report is null || !GamingInputBatteryMapper.TryCreate(
                    report.RemainingCapacityInMilliwattHours,
                    report.FullChargeCapacityInMilliwattHours,
                    report.Status,
                    observedAt,
                    Id,
                    out BatteryState battery))
            {
                PublishUnknownIfVisible(registration, observedAt);
                return;
            }

            DeviceDiscovered? discovered = null;
            ProviderEvent? batteryEvent = null;
            lock (registration.Sync)
            {
                if (!registration.Visible)
                {
                    registration.Visible = true;
                    discovered = new(registration.Key, registration.Generation, registration.NextSequence(), observedAt, registration.DisplayName);
                }
                bool recovered = registration.Faulted;
                registration.Faulted = false;
                if (!recovered && registration.LastPercent == battery.Percent && registration.LastCharging == battery.Charging) return;
                bool first = registration.LastPercent is null;
                registration.LastPercent = battery.Percent;
                registration.LastCharging = battery.Charging;
                batteryEvent = first || recovered
                    ? new ReportRecovered(registration.Key, registration.Generation, registration.NextSequence(), observedAt, battery)
                    : new BatteryChanged(registration.Key, registration.Generation, registration.NextSequence(), observedAt, battery);
            }
            if (discovered is not null) events?.TryWrite(discovered);
            if (batteryEvent is not null) events?.TryWrite(batteryEvent);
        }
        catch (Exception) { PublishUnknownIfVisible(registration, timeProvider.GetUtcNow()); }
    }

    private void PublishUnknownIfVisible(Registration registration, DateTimeOffset observedAt)
    {
        ProviderFaulted? fault = null;
        lock (registration.Sync)
        {
            if (!registration.Visible || registration.Faulted) return;
            registration.Faulted = true;
            fault = new(registration.Key, registration.Generation, registration.NextSequence(), observedAt, "WGI_BATTERY_UNAVAILABLE", "Windows game controller battery report is unavailable.");
        }
        events?.TryWrite(fault);
    }

    private void PublishRemovedIfVisible(Registration registration)
    {
        DeviceRemoved? removed = null;
        lock (registration.Sync)
        {
            if (!registration.Visible) return;
            registration.Visible = false;
            removed = new(registration.Key, registration.Generation, registration.NextSequence(), timeProvider.GetUtcNow());
        }
        events?.TryWrite(removed);
    }

    private static DeviceKey CreateKey(string identity)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(identity));
        return new(Id, Convert.ToHexString(hash)[..24]);
    }

    private sealed class Registration(DeviceKey key, int generation, string displayName)
    {
        private long sequence;
        public object Sync { get; } = new();
        public DeviceKey Key { get; } = key;
        public int Generation { get; } = generation;
        public string DisplayName { get; } = displayName;
        public int? LastPercent { get; set; }
        public ChargingState LastCharging { get; set; } = ChargingState.Unknown;
        public bool Visible { get; set; }
        public bool Faulted { get; set; }
        public long NextSequence() => Interlocked.Increment(ref sequence);
    }
}
