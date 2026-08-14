using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Devices.Enumeration;

internal static class Program
{
    private static readonly ConcurrentDictionary<string, DeviceSnapshot> Devices = new();
    private static readonly TaskCompletionSource<bool> InitialEnumerationCompleted =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private static readonly object LogLock = new();
    private static DeviceWatcher? _watcher;
    private static string _artifactDirectory = string.Empty;
    private static string _eventLogPath = string.Empty;

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        PrepareArtifacts();
        WriteSystemInfo();

        Log("Device Battery Widget - Gate 4 POC");
        Log("A01/A02/A03/A04: Enumeration / Added / Removed / Name");
        Log(new string('-', 90));

        _watcher = DeviceInformation.CreateWatcher();
        _watcher.Added += OnAdded;
        _watcher.Updated += OnUpdated;
        _watcher.Removed += OnRemoved;
        _watcher.EnumerationCompleted += OnEnumerationCompleted;
        _watcher.Stopped += OnStopped;
        _watcher.Start();

        await InitialEnumerationCompleted.Task;
        Log($"Initial enumeration completed. Entries: {Devices.Count}");
        Log($"Evidence directory: {_artifactDirectory}");
        Log("Connect/disconnect one test device. Press ENTER to stop.");
        Console.ReadLine();

        await StopWatcherAsync();
    }

    private static void PrepareArtifacts()
    {
        _artifactDirectory = Path.Combine(AppContext.BaseDirectory, "artifacts");
        Directory.CreateDirectory(_artifactDirectory);
        _eventLogPath = Path.Combine(_artifactDirectory, "device-events.log");
        File.WriteAllText(_eventLogPath, $"POC started: {DateTimeOffset.Now:O}{Environment.NewLine}", Encoding.UTF8);
    }

    private static void WriteSystemInfo()
    {
        var path = Path.Combine(_artifactDirectory, "system-info.txt");
        var content = $"CapturedAt={DateTimeOffset.Now:O}{Environment.NewLine}" +
                      $"OSDescription={RuntimeInformation.OSDescription}{Environment.NewLine}" +
                      $"OSArchitecture={RuntimeInformation.OSArchitecture}{Environment.NewLine}" +
                      $"ProcessArchitecture={RuntimeInformation.ProcessArchitecture}{Environment.NewLine}" +
                      $"Framework={RuntimeInformation.FrameworkDescription}{Environment.NewLine}" +
                      $"ProcessorCount={Environment.ProcessorCount}{Environment.NewLine}";
        File.WriteAllText(path, content, Encoding.UTF8);
    }

    private static void OnAdded(DeviceWatcher sender, DeviceInformation device)
    {
        Devices[device.Id] = new DeviceSnapshot(device.Id, device.Name, device.Kind.ToString(), device.IsEnabled, device.IsDefault);
        Log($"[ADDED] Name=\"{Safe(device.Name)}\" | Kind={device.Kind} | Enabled={device.IsEnabled} | Default={device.IsDefault}");
        Log($"        ID={device.Id}");
    }

    private static void OnUpdated(DeviceWatcher sender, DeviceInformationUpdate update) =>
        Log($"[UPDATED] ID={update.Id} | ChangedProperties={update.Properties.Count}");

    private static void OnRemoved(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        Devices.TryRemove(update.Id, out var removed);
        Log($"[REMOVED] Name=\"{Safe(removed?.Name)}\" | ID={update.Id}");
    }

    private static void OnEnumerationCompleted(DeviceWatcher sender, object args)
    {
        Log(new string('-', 90));
        Log("[ENUMERATION COMPLETED]");
        WriteInitialDeviceCsv();
        InitialEnumerationCompleted.TrySetResult(true);
    }

    private static void WriteInitialDeviceCsv()
    {
        var path = Path.Combine(_artifactDirectory, "initial-devices.csv");
        var sb = new StringBuilder("Name,Id,Kind,IsEnabled,IsDefault\r\n");
        foreach (var item in Devices.Values.OrderBy(x => x.Name).ThenBy(x => x.Id))
            sb.AppendLine($"{Csv(item.Name)},{Csv(item.Id)},{Csv(item.Kind)},{item.IsEnabled},{item.IsDefault}");
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
        Log($"[EVIDENCE] Initial device CSV written: {path}");
    }

    private static void OnStopped(DeviceWatcher sender, object args) => Log($"[WATCHER STOPPED] Status={sender.Status}");

    private static async Task StopWatcherAsync()
    {
        if (_watcher is null) return;
        try
        {
            if (_watcher.Status is DeviceWatcherStatus.Started or DeviceWatcherStatus.EnumerationCompleted)
            {
                _watcher.Stop();
                for (var i = 0; i < 40 && _watcher.Status != DeviceWatcherStatus.Stopped; i++)
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

    private static void Log(string message)
    {
        var line = $"[{DateTimeOffset.Now:HH:mm:ss.fff}] {message}";
        lock (LogLock)
        {
            Console.WriteLine(line);
            File.AppendAllText(_eventLogPath, line + Environment.NewLine, Encoding.UTF8);
        }
    }

    private static string Safe(string? value) => string.IsNullOrWhiteSpace(value) ? "(empty)" : value;
    private static string Csv(string? value) => "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    private sealed record DeviceSnapshot(string Id, string Name, string Kind, bool IsEnabled, bool IsDefault);
}
