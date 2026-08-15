using DeviceBattery.Domain;

namespace DeviceBattery.Application;

public abstract record ProviderEvent(
    DeviceKey DeviceKey,
    int SessionGeneration,
    long ProviderSequence,
    DateTimeOffset OccurredAt);

public sealed record DeviceDiscovered(
    DeviceKey DeviceKey,
    int SessionGeneration,
    long ProviderSequence,
    DateTimeOffset OccurredAt,
    string DisplayName)
    : ProviderEvent(DeviceKey, SessionGeneration, ProviderSequence, OccurredAt);

public sealed record BatteryChanged(
    DeviceKey DeviceKey,
    int SessionGeneration,
    long ProviderSequence,
    DateTimeOffset OccurredAt,
    BatteryState Battery)
    : ProviderEvent(DeviceKey, SessionGeneration, ProviderSequence, OccurredAt);

public sealed record ReportRecovered(
    DeviceKey DeviceKey,
    int SessionGeneration,
    long ProviderSequence,
    DateTimeOffset OccurredAt,
    BatteryState Battery)
    : ProviderEvent(DeviceKey, SessionGeneration, ProviderSequence, OccurredAt);

public sealed record FreshnessExpired(
    DeviceKey DeviceKey,
    int SessionGeneration,
    long ProviderSequence,
    DateTimeOffset OccurredAt,
    string Reason)
    : ProviderEvent(DeviceKey, SessionGeneration, ProviderSequence, OccurredAt);

public sealed record DeviceOffline(
    DeviceKey DeviceKey,
    int SessionGeneration,
    long ProviderSequence,
    DateTimeOffset OccurredAt)
    : ProviderEvent(DeviceKey, SessionGeneration, ProviderSequence, OccurredAt);

public sealed record DeviceRemoved(
    DeviceKey DeviceKey,
    int SessionGeneration,
    long ProviderSequence,
    DateTimeOffset OccurredAt)
    : ProviderEvent(DeviceKey, SessionGeneration, ProviderSequence, OccurredAt);

public sealed record ProviderFaulted(
    DeviceKey DeviceKey,
    int SessionGeneration,
    long ProviderSequence,
    DateTimeOffset OccurredAt,
    string ErrorFingerprint,
    string Message)
    : ProviderEvent(DeviceKey, SessionGeneration, ProviderSequence, OccurredAt);
