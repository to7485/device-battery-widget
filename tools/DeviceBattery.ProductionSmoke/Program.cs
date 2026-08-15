using DeviceBattery.Application;
using DeviceBattery.Domain;
using DeviceBattery.Infrastructure.Windows;

int durationSeconds = ParseDuration(args);
using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));
ConsoleCancelEventHandler cancelHandler = (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};
Console.CancelKeyPress += cancelHandler;

var reducer = new DeviceStateReducer();
var coordinator = new DeviceStateCoordinator(
    reducer,
    PrintResult,
    (providerEvent, error) =>
    {
        Console.WriteLine(
            $"[{DateTimeOffset.Now:HH:mm:ss.fff}] EVENT_ERROR " +
            $"Type={providerEvent.GetType().Name}, Error={error.GetType().Name}");
        return ValueTask.CompletedTask;
    });
await using var provider = new DualSenseHidProvider();

Console.WriteLine("Gate 6 — Production Pipeline Read-Only Smoke");
Console.WriteLine($"Duration={durationSeconds}s, Provider={provider.ProviderId}");
Console.WriteLine("Safety=Targeted Bluetooth DualSense watcher + FileAccessMode.Read only");
Console.WriteLine("Raw HID IDs, Output/Feature reports, vendor commands, and DeviceClass.All are not used.");

Task coordinatorTask = coordinator.RunAsync();
Exception? providerFailure = null;
try
{
    await provider.RunAsync(coordinator.Events, cancellation.Token);
}
catch (Exception ex)
{
    providerFailure = ex;
    Console.WriteLine($"PROVIDER_FAILURE Type={ex.GetType().Name}, Message={ex.Message}");
}
finally
{
    coordinator.Complete();
    await coordinatorTask;
    Console.CancelKeyPress -= cancelHandler;
}

Console.WriteLine();
Console.WriteLine(
    $"SUMMARY Processed={coordinator.ProcessedCount}, Faulted={coordinator.FaultedCount}, " +
    $"Snapshots={reducer.Snapshots.Count}");
foreach (DeviceSnapshot snapshot in reducer.Snapshots.OrderBy(value => value.Key.StableId))
{
    Console.WriteLine(
        $"  Key={snapshot.Key}, Name={snapshot.DisplayName}, Visible={snapshot.IsVisible}, " +
        $"Availability={snapshot.Battery.Availability}, Percent={FormatPercent(snapshot.Battery.Percent)}, " +
        $"Charging={snapshot.Battery.Charging}, Precision={snapshot.Battery.Precision}, " +
        $"Estimated={snapshot.Battery.IsEstimated}, Revision={snapshot.Revision}");
}
Console.WriteLine("CLEANUP = COMPLETE");

return providerFailure is null && coordinator.FaultedCount == 0 ? 0 : 1;

static ValueTask PrintResult(ReductionResult result)
{
    string timestamp = DateTimeOffset.Now.ToString("HH:mm:ss.fff");
    if (result.Outcome == ReductionOutcome.Removed)
    {
        Console.WriteLine($"[{timestamp}] REMOVED Key={result.RemovedKey}");
        return ValueTask.CompletedTask;
    }

    if (result.Snapshot is not null && result.Outcome == ReductionOutcome.Applied)
    {
        DeviceSnapshot snapshot = result.Snapshot;
        Console.WriteLine(
            $"[{timestamp}] STATE Key={snapshot.Key}, Visible={snapshot.IsVisible}, " +
            $"Availability={snapshot.Battery.Availability}, Percent={FormatPercent(snapshot.Battery.Percent)}, " +
            $"Charging={snapshot.Battery.Charging}, Revision={snapshot.Revision}");
    }

    return ValueTask.CompletedTask;
}

static int ParseDuration(string[] values)
{
    if (values.Length == 0)
        return 15;
    if (values.Length == 1 && int.TryParse(values[0], out int seconds) && seconds is >= 5 and <= 600)
        return seconds;
    throw new ArgumentException("Duration must be one integer from 5 to 600 seconds.");
}

static string FormatPercent(int? percent) => percent.HasValue ? $"{percent.Value}%" : "null";
