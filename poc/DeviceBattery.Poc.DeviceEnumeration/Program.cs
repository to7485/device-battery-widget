using System.Collections.Concurrent;
using System.Text;
using Windows.Devices.Enumeration;

internal static class Program
{
    private static readonly ConcurrentDictionary<string, DeviceSnapshot> Devices = new();
    private static readonly TaskCompletionSource<bool> InitialEnumerationCompleted =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    private static DeviceWatcher? _watcher;

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Device Battery Widget - Gate 4 POC");
        Console.WriteLine("A01/A02/A03/A04: Enumeration / Added / Removed / Name");
        Console.WriteLine(new string('-', 80));

        _watcher = DeviceInformation.CreateWatcher();

        _watcher.Added += OnAdded;
        _watcher.Updated += OnUpdated;
        _watcher.Removed += OnRemoved;
        _watcher.EnumerationCompleted += OnEnumerationCompleted;
        _watcher.Stopped += OnStopped;

        _watcher.Start();
        await InitialEnumerationCompleted.Task;

        Console.WriteLine();
        Console.WriteLine($"Initial enumeration completed. Entries: {Devices.Count}");
        Console.WriteLine("Connect/disconnect a test device and watch the log.");
        Console.WriteLine("Press ENTER to stop.");
        Console.ReadLine();

        await StopWatcherAsync();
    }

    private static void OnAdded(DeviceWatcher sender, DeviceInformation device)
    {
        Devices[device.Id] = new DeviceSnapshot(
            device.Id, device.Name, device.Kind.ToString(),
            device.IsEnabled, device.IsDefault);

        Console.WriteLine(
            $"[ADDED] Name=\"{Safe(device.Name)}\" | Kind={device.Kind} | " +
            $"Enabled={device.IsEnabled} | Default={device.IsDefault}");
        Console.WriteLine($"        ID={device.Id}");
    }

    private static void OnUpdated(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        Console.WriteLine($"[UPDATED] ID={update.Id} | ChangedProperties={update.Properties.Count}");
    }

    private static void OnRemoved(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        Devices.TryRemove(update.Id, out var removed);
        Console.WriteLine($"[REMOVED] Name=\"{Safe(removed?.Name)}\" | ID={update.Id}");
    }

    private static void OnEnumerationCompleted(DeviceWatcher sender, object args)
    {
        Console.WriteLine(new string('-', 80));
        Console.WriteLine("[ENUMERATION COMPLETED]");
        InitialEnumerationCompleted.TrySetResult(true);
    }

    private static void OnStopped(DeviceWatcher sender, object args)
    {
        Console.WriteLine($"[WATCHER STOPPED] Status={sender.Status}");
    }

    private static async Task StopWatcherAsync()
    {
        if (_watcher is null) return;

        try
        {
            if (_watcher.Status is DeviceWatcherStatus.Started
                or DeviceWatcherStatus.EnumerationCompleted)
            {
                _watcher.Stop();

                for (var i = 0; i < 20 && _watcher.Status != DeviceWatcherStatus.Stopped; i++)
                    await Task.Delay(50);
            }
        }
        finally
        {
            _watcher.Added -= OnAdded;
            _watcher.Updated -= OnUpdated;
            _watcher.Removed -= OnRemoved;
            _watcher.EnumerationCompleted -= OnEnumerationCompleted;
            _watcher.Stopped -= OnStopped;
            _watcher = null;
        }
    }

    private static string Safe(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "(empty)" : value;

    private sealed record DeviceSnapshot(
        string Id,
        string Name,
        string Kind,
        bool IsEnabled,
        bool IsDefault);
}
