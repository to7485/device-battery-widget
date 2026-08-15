using DeviceBattery.Application;
using DeviceBattery.Domain;

DateTimeOffset now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);
DeviceKey key = new("DualSenseHid", "device-1");
var specs = new (string Name, Func<Task> Run)[]
{
    ("Completed mailbox drains in order", async () =>
    {
        var reducer = new DeviceStateReducer();
        var revisions = new List<long>();
        var coordinator = new DeviceStateCoordinator(reducer, result =>
        {
            if (result.Snapshot is not null)
                revisions.Add(result.Snapshot.Revision);
            return ValueTask.CompletedTask;
        });

        coordinator.TryPublish(new DeviceDiscovered(key, 1, 1, now, "DualSense"));
        for (int sequence = 2; sequence <= 101; sequence++)
        {
            BatteryState battery = BatteryState.Available(
                sequence % 101,
                ChargingState.NotCharging,
                BatteryPrecision.ExactPercent,
                now,
                key.ProviderId);
            coordinator.TryPublish(new BatteryChanged(key, 1, sequence, now, battery));
        }

        coordinator.Complete();
        await coordinator.RunAsync();
        Equal(101L, coordinator.ProcessedCount);
        Equal(0L, coordinator.FaultedCount);
        SequenceEqual(Enumerable.Range(1, 101).Select(value => (long)value), revisions);
    }),
    ("Malformed event is isolated", async () =>
    {
        var reducer = new DeviceStateReducer();
        var errors = new List<Exception>();
        var coordinator = new DeviceStateCoordinator(
            reducer,
            errorSink: (_, error) =>
            {
                errors.Add(error);
                return ValueTask.CompletedTask;
            });

        coordinator.TryPublish(new DeviceDiscovered(key, 1, 1, now, "DualSense"));
        coordinator.TryPublish(new FreshnessExpired(key, 1, 2, now, " "));
        BatteryState recovered = BatteryState.Available(
            25,
            ChargingState.Charging,
            BatteryPrecision.TenPercentBucket,
            now,
            key.ProviderId);
        coordinator.TryPublish(new ReportRecovered(key, 1, 3, now, recovered));
        coordinator.Complete();

        await coordinator.RunAsync();
        Equal(2L, coordinator.ProcessedCount);
        Equal(1L, coordinator.FaultedCount);
        Equal(1, errors.Count);
        Equal(true, reducer.TryGetSnapshot(key, out DeviceSnapshot? snapshot));
        Equal(25, snapshot!.Battery.Percent);
    }),
    ("Writes are rejected after completion", async () =>
    {
        var coordinator = new DeviceStateCoordinator(new DeviceStateReducer());
        coordinator.Complete();
        Equal(false, coordinator.TryPublish(new DeviceDiscovered(key, 1, 1, now, "DualSense")));
        await coordinator.RunAsync();
    }),
    ("Coordinator cannot be run twice", async () =>
    {
        var coordinator = new DeviceStateCoordinator(new DeviceStateReducer());
        coordinator.Complete();
        await coordinator.RunAsync();
        await ThrowsAsync<InvalidOperationException>(() => coordinator.RunAsync());
    })
};

int passed = 0;
foreach ((string name, Func<Task> run) in specs)
{
    try
    {
        await run();
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

static void SequenceEqual<T>(IEnumerable<T> expected, IEnumerable<T> actual)
{
    if (!expected.SequenceEqual(actual))
        throw new InvalidOperationException("Sequences are not equal.");
}

static async Task ThrowsAsync<TException>(Func<Task> action) where TException : Exception
{
    try
    {
        await action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Expected {typeof(TException).Name}.");
}
