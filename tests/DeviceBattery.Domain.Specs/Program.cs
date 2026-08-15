using DeviceBattery.Domain;

DateTimeOffset now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);
var specs = new (string Name, Action Run)[]
{
    ("DeviceKey trims values", () =>
        Equal("DualSenseHid:device-1", new DeviceKey(" DualSenseHid ", " device-1 ").ToString())),
    ("DeviceKey rejects empty provider", () =>
        Throws<ArgumentException>(() => new DeviceKey(" ", "device-1"))),
    ("Available exact state", () =>
    {
        BatteryState state = BatteryState.Available(91, ChargingState.NotCharging, BatteryPrecision.ExactPercent, now, "Test");
        Equal(BatteryAvailability.Available, state.Availability);
        Equal(false, state.IsEstimated);
    }),
    ("Available permits unknown charging for standard battery level", () =>
    {
        BatteryState state = BatteryState.Available(73, ChargingState.Unknown, BatteryPrecision.ExactPercent, now, "BleGattBattery");
        Equal(BatteryAvailability.Available, state.Availability);
        Equal(ChargingState.Unknown, state.Charging);
    }),
    ("Bucket state is estimated below full", () =>
    {
        BatteryState state = BatteryState.Available(15, ChargingState.NotCharging, BatteryPrecision.TenPercentBucket, now, "DualSenseHid");
        Equal(true, state.IsEstimated);
    }),
    ("Bucket full is not estimated", () =>
        Equal(false, BatteryState.Available(100, ChargingState.Charging, BatteryPrecision.TenPercentBucket, now, "DualSenseHid").IsEstimated)),
    ("Granular game-controller level is estimated", () =>
        Equal(true, BatteryState.Available(40, ChargingState.NotCharging, BatteryPrecision.GranularLevel, now, "WindowsGamingInputBattery").IsEstimated)),
    ("Available rejects invalid percent", () =>
        Throws<ArgumentOutOfRangeException>(() => BatteryState.Available(101, ChargingState.NotCharging, BatteryPrecision.ExactPercent, now, "Test"))),
    ("Unknown clears stale values", () =>
    {
        BatteryState state = BatteryState.Unknown(now, "DualSenseHid", "freshness expired");
        Equal(null, state.Percent);
        Equal(ChargingState.Unknown, state.Charging);
        Equal(BatteryPrecision.Unknown, state.Precision);
    }),
    ("Snapshot rejects negative revision", () =>
    {
        BatteryState battery = BatteryState.Unknown(now, "DualSenseHid", "waiting");
        Throws<ArgumentOutOfRangeException>(() =>
            new DeviceSnapshot(new DeviceKey("DualSenseHid", "device-1"), "DualSense", battery, true, -1).Validate());
    })
};

int passed = 0;
foreach ((string name, Action run) in specs)
{
    try
    {
        run();
        Console.WriteLine($"[PASS] {name}");
        passed++;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[FAIL] {name}: {ex.Message}");
    }
}

Console.WriteLine($"RESULT = {(passed == specs.Length ? "PASS" : "FAIL")} ({passed}/{specs.Length})");
return passed == specs.Length ? 0 : 1;

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, actual {actual}.");
}

static void Throws<TException>(Action action) where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }
    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
