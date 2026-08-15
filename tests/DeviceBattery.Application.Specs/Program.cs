using DeviceBattery.Application;
using DeviceBattery.Domain;

DateTimeOffset now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);
DeviceKey key = new("DualSenseHid", "device-1");

var specs = new (string Name, Action Run)[]
{
    ("Discovery creates visible waiting snapshot", () =>
    {
        var reducer = new DeviceStateReducer();
        ReductionResult result = reducer.Apply(new DeviceDiscovered(key, 1, 1, now, "DualSense"));
        Equal(ReductionOutcome.Applied, result.Outcome);
        Equal(true, result.Snapshot!.IsVisible);
        Equal(BatteryAvailability.Unknown, result.Snapshot.Battery.Availability);
        Equal(1L, result.Snapshot.Revision);
    }),
    ("Battery event makes state available", () =>
    {
        var reducer = Started();
        BatteryState battery = BatteryState.Available(15, ChargingState.NotCharging, BatteryPrecision.TenPercentBucket, now, key.ProviderId);
        ReductionResult result = reducer.Apply(new BatteryChanged(key, 1, 2, now, battery));
        Equal(15, result.Snapshot!.Battery.Percent);
        Equal(2L, result.Snapshot.Revision);
    }),
    ("Freshness expiry clears stale percent", () =>
    {
        var reducer = Available();
        ReductionResult result = reducer.Apply(new FreshnessExpired(key, 1, 3, now, "No valid report for 10 seconds"));
        Equal(BatteryAvailability.Unknown, result.Snapshot!.Battery.Availability);
        Equal<int?>(null, result.Snapshot.Battery.Percent);
        Equal(true, result.Snapshot.IsVisible);
    }),
    ("Offline grace hides device", () =>
    {
        var reducer = Available();
        ReductionResult result = reducer.Apply(new DeviceOffline(key, 1, 3, now));
        Equal(false, result.Snapshot!.IsVisible);
        Equal<int?>(null, result.Snapshot.Battery.Percent);
    }),
    ("Recovery re-adds dormant device", () =>
    {
        var reducer = Available();
        reducer.Apply(new DeviceOffline(key, 1, 3, now));
        BatteryState battery = BatteryState.Available(25, ChargingState.Charging, BatteryPrecision.TenPercentBucket, now, key.ProviderId);
        ReductionResult result = reducer.Apply(new ReportRecovered(key, 1, 4, now, battery));
        Equal(true, result.Snapshot!.IsVisible);
        Equal(25, result.Snapshot.Battery.Percent);
    }),
    ("Older generation cannot overwrite state", () =>
    {
        var reducer = Available();
        ReductionResult result = reducer.Apply(new FreshnessExpired(key, 0, 100, now, "old callback"));
        Equal(ReductionOutcome.IgnoredOlderGeneration, result.Outcome);
        Equal(15, result.Snapshot!.Battery.Percent);
    }),
    ("Duplicate sequence is ignored", () =>
    {
        var reducer = Available();
        ReductionResult result = reducer.Apply(new DeviceOffline(key, 1, 2, now));
        Equal(ReductionOutcome.IgnoredOutOfOrderSequence, result.Outcome);
        Equal(true, result.Snapshot!.IsVisible);
    }),
    ("New generation requires discovery", () =>
    {
        var reducer = Available();
        BatteryState battery = BatteryState.Available(25, ChargingState.Charging, BatteryPrecision.TenPercentBucket, now, key.ProviderId);
        ReductionResult result = reducer.Apply(new BatteryChanged(key, 2, 1, now, battery));
        Equal(ReductionOutcome.IgnoredUnknownDevice, result.Outcome);
        Equal(15, result.Snapshot!.Battery.Percent);
    }),
    ("New discovery replaces generation", () =>
    {
        var reducer = Available();
        ReductionResult result = reducer.Apply(new DeviceDiscovered(key, 2, 1, now, "DualSense Wireless Controller"));
        Equal(ReductionOutcome.Applied, result.Outcome);
        Equal(BatteryAvailability.Unknown, result.Snapshot!.Battery.Availability);
        Equal(3L, result.Snapshot.Revision);
    }),
    ("Removed device leaves no snapshot", () =>
    {
        var reducer = Available();
        ReductionResult result = reducer.Apply(new DeviceRemoved(key, 1, 3, now));
        Equal(ReductionOutcome.Removed, result.Outcome);
        Equal(false, reducer.TryGetSnapshot(key, out _));
    }),
    ("Battery provider must match device provider", () =>
    {
        var reducer = Started();
        BatteryState battery = BatteryState.Available(50, ChargingState.NotCharging, BatteryPrecision.ExactPercent, now, "OtherProvider");
        Throws<ArgumentException>(() => reducer.Apply(new BatteryChanged(key, 1, 2, now, battery)));
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

DeviceStateReducer Started()
{
    var reducer = new DeviceStateReducer();
    reducer.Apply(new DeviceDiscovered(key, 1, 1, now, "DualSense"));
    return reducer;
}

DeviceStateReducer Available()
{
    DeviceStateReducer reducer = Started();
    BatteryState battery = BatteryState.Available(15, ChargingState.NotCharging, BatteryPrecision.TenPercentBucket, now, key.ProviderId);
    reducer.Apply(new BatteryChanged(key, 1, 2, now, battery));
    return reducer;
}

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
