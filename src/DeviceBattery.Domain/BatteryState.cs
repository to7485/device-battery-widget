namespace DeviceBattery.Domain;

public enum BatteryAvailability { Available, Unsupported, Unknown }
public enum ChargingState { Charging, NotCharging, Unknown }
public enum BatteryPrecision { ExactPercent, TenPercentBucket, GranularLevel, Unknown }

public sealed record BatteryState
{
    private BatteryState(
        BatteryAvailability availability,
        int? percent,
        ChargingState charging,
        BatteryPrecision precision,
        bool isEstimated,
        DateTimeOffset observedAt,
        string sourceProvider,
        string? reason)
    {
        Availability = availability;
        Percent = percent;
        Charging = charging;
        Precision = precision;
        IsEstimated = isEstimated;
        ObservedAt = observedAt;
        SourceProvider = RequireProvider(sourceProvider);
        Reason = reason;
    }

    public BatteryAvailability Availability { get; }
    public int? Percent { get; }
    public ChargingState Charging { get; }
    public BatteryPrecision Precision { get; }
    public bool IsEstimated { get; }
    public DateTimeOffset ObservedAt { get; }
    public string SourceProvider { get; }
    public string? Reason { get; }

    public static BatteryState Available(
        int percent,
        ChargingState charging,
        BatteryPrecision precision,
        DateTimeOffset observedAt,
        string sourceProvider)
    {
        if (percent is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(percent));
        if (precision == BatteryPrecision.Unknown)
            throw new ArgumentException("Available state requires known precision.", nameof(precision));

        bool estimated = precision is BatteryPrecision.TenPercentBucket or BatteryPrecision.GranularLevel && percent < 100;
        return new(
            BatteryAvailability.Available,
            percent,
            charging,
            precision,
            estimated,
            observedAt,
            sourceProvider,
            null);
    }

    public static BatteryState Unknown(DateTimeOffset observedAt, string sourceProvider, string reason) =>
        new(
            BatteryAvailability.Unknown,
            null,
            ChargingState.Unknown,
            BatteryPrecision.Unknown,
            false,
            observedAt,
            sourceProvider,
            RequireReason(reason));

    public static BatteryState Unsupported(DateTimeOffset observedAt, string sourceProvider, string reason) =>
        new(
            BatteryAvailability.Unsupported,
            null,
            ChargingState.Unknown,
            BatteryPrecision.Unknown,
            false,
            observedAt,
            sourceProvider,
            RequireReason(reason));

    private static string RequireProvider(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Source provider must not be empty.", nameof(value))
            : value.Trim();

    private static string RequireReason(string value) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Reason must not be empty.", nameof(value))
            : value.Trim();
}
