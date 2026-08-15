using DeviceBattery.Domain;

namespace DeviceBattery.Application;

public enum ReductionOutcome
{
    Applied,
    Removed,
    IgnoredUnknownDevice,
    IgnoredOlderGeneration,
    IgnoredOutOfOrderSequence
}

public sealed record ReductionResult(
    ReductionOutcome Outcome,
    DeviceSnapshot? Snapshot = null,
    DeviceKey? RemovedKey = null);

public sealed class DeviceStateReducer
{
    private readonly Dictionary<DeviceKey, Entry> entries = [];

    public IReadOnlyCollection<DeviceSnapshot> Snapshots =>
        entries.Values.Select(entry => entry.Snapshot).ToArray();

    public ReductionResult Apply(ProviderEvent providerEvent)
    {
        ArgumentNullException.ThrowIfNull(providerEvent);
        ValidateEnvelope(providerEvent);

        if (providerEvent is DeviceDiscovered discovered)
            return ApplyDiscovered(discovered);

        if (!entries.TryGetValue(providerEvent.DeviceKey, out Entry? entry))
            return new(ReductionOutcome.IgnoredUnknownDevice);
        if (providerEvent.SessionGeneration < entry.SessionGeneration)
            return new(ReductionOutcome.IgnoredOlderGeneration, entry.Snapshot);
        if (providerEvent.SessionGeneration > entry.SessionGeneration)
            return new(ReductionOutcome.IgnoredUnknownDevice, entry.Snapshot);
        if (providerEvent.ProviderSequence <= entry.ProviderSequence)
            return new(ReductionOutcome.IgnoredOutOfOrderSequence, entry.Snapshot);

        return ApplyCurrent(providerEvent, entry);
    }

    public bool TryGetSnapshot(DeviceKey key, out DeviceSnapshot? snapshot)
    {
        ArgumentNullException.ThrowIfNull(key);
        if (entries.TryGetValue(key, out Entry? entry))
        {
            snapshot = entry.Snapshot;
            return true;
        }

        snapshot = null;
        return false;
    }

    private ReductionResult ApplyDiscovered(DeviceDiscovered discovered)
    {
        if (string.IsNullOrWhiteSpace(discovered.DisplayName))
            throw new ArgumentException("Display name must not be empty.", nameof(discovered));

        if (entries.TryGetValue(discovered.DeviceKey, out Entry? current))
        {
            if (discovered.SessionGeneration < current.SessionGeneration)
                return new(ReductionOutcome.IgnoredOlderGeneration, current.Snapshot);
            if (discovered.SessionGeneration == current.SessionGeneration &&
                discovered.ProviderSequence <= current.ProviderSequence)
                return new(ReductionOutcome.IgnoredOutOfOrderSequence, current.Snapshot);
        }

        long revision = current?.Snapshot.Revision + 1 ?? 1;
        BatteryState waiting = BatteryState.Unknown(
            discovered.OccurredAt,
            discovered.DeviceKey.ProviderId,
            "Waiting for first valid battery report");
        DeviceSnapshot snapshot = new(
            discovered.DeviceKey,
            discovered.DisplayName.Trim(),
            waiting,
            true,
            revision);

        entries[discovered.DeviceKey] = new(
            discovered.SessionGeneration,
            discovered.ProviderSequence,
            snapshot.Validate());
        return new(ReductionOutcome.Applied, snapshot);
    }

    private ReductionResult ApplyCurrent(ProviderEvent providerEvent, Entry entry)
    {
        if (providerEvent is DeviceRemoved)
        {
            entries.Remove(providerEvent.DeviceKey);
            return new(ReductionOutcome.Removed, RemovedKey: providerEvent.DeviceKey);
        }

        BatteryState battery = entry.Snapshot.Battery;
        bool visible = entry.Snapshot.IsVisible;

        switch (providerEvent)
        {
            case BatteryChanged changed:
                battery = RequireMatchingBattery(changed.Battery, providerEvent.DeviceKey);
                visible = true;
                break;
            case ReportRecovered recovered:
                battery = RequireMatchingBattery(recovered.Battery, providerEvent.DeviceKey);
                visible = true;
                break;
            case FreshnessExpired expired:
                battery = BatteryState.Unknown(
                    expired.OccurredAt,
                    expired.DeviceKey.ProviderId,
                    RequireText(expired.Reason, nameof(expired.Reason)));
                break;
            case DeviceOffline:
                battery = BatteryState.Unknown(
                    providerEvent.OccurredAt,
                    providerEvent.DeviceKey.ProviderId,
                    "Device offline grace expired");
                visible = false;
                break;
            case ProviderFaulted faulted:
                battery = BatteryState.Unknown(
                    faulted.OccurredAt,
                    faulted.DeviceKey.ProviderId,
                    RequireText(faulted.Message, nameof(faulted.Message)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(providerEvent));
        }

        DeviceSnapshot snapshot = (entry.Snapshot with
        {
            Battery = battery,
            IsVisible = visible,
            Revision = entry.Snapshot.Revision + 1
        }).Validate();
        entries[providerEvent.DeviceKey] = new(
            providerEvent.SessionGeneration,
            providerEvent.ProviderSequence,
            snapshot);
        return new(ReductionOutcome.Applied, snapshot);
    }

    private static BatteryState RequireMatchingBattery(BatteryState battery, DeviceKey key)
    {
        ArgumentNullException.ThrowIfNull(battery);
        if (!string.Equals(battery.SourceProvider, key.ProviderId, StringComparison.Ordinal))
            throw new ArgumentException("Battery source provider must match the device key provider.", nameof(battery));
        return battery;
    }

    private static string RequireText(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value must not be empty.", parameterName)
            : value.Trim();

    private static void ValidateEnvelope(ProviderEvent providerEvent)
    {
        if (providerEvent.SessionGeneration < 0)
            throw new ArgumentOutOfRangeException(nameof(providerEvent.SessionGeneration));
        if (providerEvent.ProviderSequence < 0)
            throw new ArgumentOutOfRangeException(nameof(providerEvent.ProviderSequence));
    }

    private sealed record Entry(
        int SessionGeneration,
        long ProviderSequence,
        DeviceSnapshot Snapshot);
}
