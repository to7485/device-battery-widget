internal static class Program
{
    private static int Main()
    {
        Console.WriteLine("Gate 4 POC-B05 — DualSense Normalized BatteryState");
        Console.WriteLine(new string('-', 88));
        Console.WriteLine("Scope: deterministic normalization of validated DualSense HID status bytes.");
        Console.WriteLine("This POC does not enumerate or send commands to a device.");
        Console.WriteLine();

        DateTimeOffset t0 = new(2026, 8, 15, 13, 30, 0, TimeSpan.FromHours(9));
        List<TestCase> tests =
        [
            new("Discharging bucket 0", () => DualSenseBatteryNormalizer.FromStatusByte(0x00, t0), state =>
                state.Availability == BatteryAvailability.Available &&
                state.Percent == 5 &&
                state.Charging == ChargingState.NotCharging &&
                state.Precision == BatteryPrecision.TenPercentBucket &&
                state.IsEstimated),

            new("Charging bucket 0", () => DualSenseBatteryNormalizer.FromStatusByte(0x10, t0.AddSeconds(1)), state =>
                state.Availability == BatteryAvailability.Available &&
                state.Percent == 5 &&
                state.Charging == ChargingState.Charging &&
                state.IsEstimated),

            new("Full", () => DualSenseBatteryNormalizer.FromStatusByte(0x2A, t0.AddSeconds(2)), state =>
                state.Availability == BatteryAvailability.Available &&
                state.Percent == 100 &&
                state.Charging == ChargingState.NotCharging &&
                state.Precision == BatteryPrecision.Full &&
                !state.IsEstimated),

            new("Controller error code", () => DualSenseBatteryNormalizer.FromStatusByte(0xA0, t0.AddSeconds(3)), state =>
                IsUnknownWithoutStaleValue(state) && state.Reason == "DualSense status error code 0xA"),

            new("Invalid bucket", () => DualSenseBatteryNormalizer.FromStatusByte(0x0B, t0.AddSeconds(4)), state =>
                IsUnknownWithoutStaleValue(state) && state.Reason == "Invalid DualSense battery bucket 11"),

            new("Transient read failure clears stale value", () =>
            {
                BatteryState previous = DualSenseBatteryNormalizer.FromStatusByte(0x05, t0.AddSeconds(5));
                return BatteryStateTransitions.ReadFailed(previous, t0.AddSeconds(6), "Input report stream interrupted");
            }, state => IsUnknownWithoutStaleValue(state) && state.Reason == "Input report stream interrupted"),

            new("Unsupported is distinct from Unknown", () =>
                BatteryStateTransitions.Unsupported(t0.AddSeconds(7), "Device profile is not supported"), state =>
                    state.Availability == BatteryAvailability.Unsupported &&
                    state.Percent is null &&
                    state.Charging == ChargingState.Unknown),

            new("Valid report recovers from Unknown", () =>
            {
                BatteryState previous = BatteryStateTransitions.ReadFailed(
                    DualSenseBatteryNormalizer.FromStatusByte(0x00, t0),
                    t0.AddSeconds(8),
                    "Temporary failure");
                return BatteryStateTransitions.ReportRecovered(previous, 0x11, t0.AddSeconds(9));
            }, state =>
                state.Availability == BatteryAvailability.Available &&
                state.Percent == 15 &&
                state.Charging == ChargingState.Charging)
        ];

        int failed = 0;
        foreach (TestCase test in tests)
        {
            BatteryState state = test.Act();
            bool passed = test.Assert(state);
            failed += passed ? 0 : 1;

            Console.WriteLine($"[{(passed ? "PASS" : "FAIL")}] {test.Name}");
            PrintState(state);
        }

        Console.WriteLine(new string('-', 88));
        Console.WriteLine($"RESULT = {(failed == 0 ? "PASS" : "FAIL")} ({tests.Count - failed}/{tests.Count} cases)");
        return failed == 0 ? 0 : 1;
    }

    private static bool IsUnknownWithoutStaleValue(BatteryState state) =>
        state.Availability == BatteryAvailability.Unknown &&
        state.Percent is null &&
        state.Charging == ChargingState.Unknown &&
        state.Precision == BatteryPrecision.Unknown &&
        !state.IsEstimated;

    private static void PrintState(BatteryState state)
    {
        Console.WriteLine($"  Availability = {state.Availability}");
        Console.WriteLine($"  Percent      = {(state.Percent.HasValue ? $"{state.Percent.Value}%" : "null")}");
        Console.WriteLine($"  Charging     = {state.Charging}");
        Console.WriteLine($"  Precision    = {state.Precision}");
        Console.WriteLine($"  IsEstimated  = {state.IsEstimated}");
        Console.WriteLine($"  UpdatedAt    = {state.LastUpdatedAt:O}");
        Console.WriteLine($"  Reason       = {state.Reason ?? "(none)"}");
    }

    private sealed record TestCase(
        string Name,
        Func<BatteryState> Act,
        Func<BatteryState, bool> Assert);
}

internal enum BatteryAvailability
{
    Available,
    Unsupported,
    Unknown
}

internal enum ChargingState
{
    Charging,
    NotCharging,
    Unknown
}

internal enum BatteryPrecision
{
    Unknown,
    TenPercentBucket,
    Full
}

internal sealed record BatteryState(
    BatteryAvailability Availability,
    int? Percent,
    ChargingState Charging,
    BatteryPrecision Precision,
    bool IsEstimated,
    string SourceProvider,
    bool IsEventDriven,
    DateTimeOffset LastUpdatedAt,
    string? Reason);

internal static class DualSenseBatteryNormalizer
{
    private const string Provider = "DualSenseHid";

    public static BatteryState FromStatusByte(byte statusByte, DateTimeOffset observedAt)
    {
        int bucket = statusByte & 0x0F;
        int chargingCode = (statusByte >> 4) & 0x0F;

        if (chargingCode is 0xA or 0xB or 0xF)
        {
            return Unknown(observedAt, $"DualSense status error code 0x{chargingCode:X1}");
        }

        if (chargingCode is not (0x0 or 0x1 or 0x2))
        {
            return Unknown(observedAt, $"Unknown DualSense charging code 0x{chargingCode:X1}");
        }

        if (bucket is < 0 or > 10)
        {
            return Unknown(observedAt, $"Invalid DualSense battery bucket {bucket}");
        }

        bool full = chargingCode == 0x2 || bucket == 10;
        int percent = full ? 100 : bucket * 10 + 5;
        ChargingState charging = chargingCode == 0x1
            ? ChargingState.Charging
            : ChargingState.NotCharging;

        return new BatteryState(
            BatteryAvailability.Available,
            percent,
            charging,
            full ? BatteryPrecision.Full : BatteryPrecision.TenPercentBucket,
            IsEstimated: !full,
            Provider,
            IsEventDriven: true,
            observedAt,
            Reason: null);
    }

    private static BatteryState Unknown(DateTimeOffset observedAt, string reason) =>
        new(
            BatteryAvailability.Unknown,
            Percent: null,
            ChargingState.Unknown,
            BatteryPrecision.Unknown,
            IsEstimated: false,
            Provider,
            IsEventDriven: true,
            observedAt,
            reason);
}

internal static class BatteryStateTransitions
{
    public static BatteryState ReadFailed(BatteryState previous, DateTimeOffset failedAt, string reason) =>
        previous with
        {
            Availability = BatteryAvailability.Unknown,
            Percent = null,
            Charging = ChargingState.Unknown,
            Precision = BatteryPrecision.Unknown,
            IsEstimated = false,
            LastUpdatedAt = failedAt,
            Reason = reason
        };

    public static BatteryState Unsupported(DateTimeOffset observedAt, string reason) =>
        new(
            BatteryAvailability.Unsupported,
            Percent: null,
            ChargingState.Unknown,
            BatteryPrecision.Unknown,
            IsEstimated: false,
            SourceProvider: "None",
            IsEventDriven: false,
            observedAt,
            reason);

    public static BatteryState ReportRecovered(BatteryState previous, byte statusByte, DateTimeOffset observedAt)
    {
        _ = previous;
        return DualSenseBatteryNormalizer.FromStatusByte(statusByte, observedAt);
    }
}
