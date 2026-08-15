using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Devices.Enumeration;

internal static class Program
{
    private static readonly string[] RequestedProperties =
    [
        "System.Devices.ContainerId",
        "System.Devices.DeviceInstanceId",
        "System.Devices.FriendlyName",
        "System.Devices.DeviceManufacturer",
        "System.Devices.ModelName",
        "System.Devices.Connected"
    ];

    private static readonly ConcurrentDictionary<string, DeviceInformation> Devices = new();

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
        Log("A01/A02/A03/A04: Enumeration / Added / Removed / Updated Property Refresh");
        Log(new string('-', 100));

        /*
         * 중요:
         * GetAqsFilterFromDeviceClass(DeviceClass.All)은 사용하지 않는다.
         *
         * 이전 POC에서 parameterless CreateWatcher()는 정상 동작했고
         * 실제 장치 변경 이벤트도 수신했다.
         *
         * 상세 Property는 이벤트 발생 후 CreateFromIdAsync()로 재조회한다.
         */
        _watcher = DeviceInformation.CreateWatcher();

        _watcher.Added += OnAdded;
        _watcher.Updated += OnUpdated;
        _watcher.Removed += OnRemoved;
        _watcher.EnumerationCompleted += OnEnumerationCompleted;
        _watcher.Stopped += OnStopped;

        Log("Starting DeviceWatcher...");
        _watcher.Start();

        await InitialEnumerationCompleted.Task;

        Log(string.Empty);
        Log($"Initial enumeration completed. Entries: {Devices.Count}");
        Log($"Evidence directory: {_artifactDirectory}");
        Log("Connect/disconnect DualSense and watch ADDED / UPDATED / REMOVED.");
        Log("Press ENTER to stop.");
        Console.ReadLine();

        await StopWatcherAsync();

        Log("POC stopped.");
    }

    private static void PrepareArtifacts()
    {
        _artifactDirectory = Path.Combine(
            AppContext.BaseDirectory,
            "artifacts");

        Directory.CreateDirectory(_artifactDirectory);

        _eventLogPath = Path.Combine(
            _artifactDirectory,
            "device-events.log");

        File.WriteAllText(
            _eventLogPath,
            $"POC started: {DateTimeOffset.Now:O}{Environment.NewLine}",
            Encoding.UTF8);
    }

    private static void WriteSystemInfo()
    {
        var path = Path.Combine(
            _artifactDirectory,
            "system-info.txt");

        var content = $"""
        CapturedAt={DateTimeOffset.Now:O}
        OSDescription={RuntimeInformation.OSDescription}
        OSArchitecture={RuntimeInformation.OSArchitecture}
        ProcessArchitecture={RuntimeInformation.ProcessArchitecture}
        Framework={RuntimeInformation.FrameworkDescription}
        ProcessorCount={Environment.ProcessorCount}
        MachineName={Environment.MachineName}
        """;

        File.WriteAllText(
            path,
            content,
            Encoding.UTF8);
    }

    private static void OnAdded(
        DeviceWatcher sender,
        DeviceInformation device)
    {
        Devices[device.Id] = device;

        Log(new string('-', 100));
        Log($"[ADDED] Name=\"{Safe(device.Name)}\"");
        Log($"        Kind={device.Kind}");
        Log($"        Enabled={device.IsEnabled}");
        Log($"        Default={device.IsDefault}");
        Log($"        ID={device.Id}");

        /*
         * parameterless watcher의 최초 DeviceInformation에는
         * RequestedProperties가 포함되지 않을 수 있으므로,
         * 상세 Property는 비동기로 별도 조회한다.
         */
        _ = RefreshAndLogDeviceAsync(
            device.Id,
            device.Kind,
            "ADDED REFRESH");
    }

    private static void OnUpdated(
        DeviceWatcher sender,
        DeviceInformationUpdate update)
    {
        Log(new string('-', 100));
        Log($"[UPDATED] ID={update.Id}");
        Log($"          ChangedProperties={update.Properties.Count}");

        if (update.Properties.Count == 0)
        {
            Log("          (No changed properties were supplied.)");
        }
        else
        {
            foreach (var property in update.Properties.OrderBy(x => x.Key))
            {
                Log(
                    $"          CHANGED {property.Key} = " +
                    $"{FormatValue(property.Value)}");
            }
        }

        /*
         * Update에 Connected 같은 관심 Property가 직접 포함되지 않아도
         * 현재 DeviceInformation을 다시 조회해 최신 상태를 확인한다.
         */
        var kind = Devices.TryGetValue(update.Id, out var existing)
            ? existing.Kind
            : DeviceInformationKind.DeviceInterface;

        _ = RefreshAndLogDeviceAsync(
            update.Id,
            kind,
            "UPDATED REFRESH");
    }

    private static void OnRemoved(
        DeviceWatcher sender,
        DeviceInformationUpdate update)
    {
        Devices.TryRemove(
            update.Id,
            out var removed);

        Log(new string('-', 100));
        Log($"[REMOVED] Name=\"{Safe(removed?.Name)}\"");
        Log($"          ID={update.Id}");

        if (removed is not null)
        {
            Log($"          Kind={removed.Kind}");
        }

        if (update.Properties.Count > 0)
        {
            foreach (var property in update.Properties.OrderBy(x => x.Key))
            {
                Log(
                    $"          REMOVED PROPERTY {property.Key} = " +
                    $"{FormatValue(property.Value)}");
            }
        }

        /*
         * REMOVED 이후에는 CreateFromIdAsync()가 null 또는 예외가 될 수도 있다.
         * 그것 자체가 해당 Interface가 더 이상 열거되지 않는다는 증거가 된다.
         */
        var kind = removed?.Kind
                   ?? DeviceInformationKind.DeviceInterface;

        _ = RefreshAndLogDeviceAsync(
            update.Id,
            kind,
            "REMOVED REFRESH");
    }

    private static async Task RefreshAndLogDeviceAsync(
        string deviceId,
        DeviceInformationKind kind,
        string reason)
    {
        try
        {
            var refreshed =
                await DeviceInformation.CreateFromIdAsync(
                    deviceId,
                    RequestedProperties,
                    kind);

            if (refreshed is null)
            {
                Log($"          [{reason}] Result=null");
                return;
            }

            Devices[deviceId] = refreshed;

            Log($"          [{reason}]");
            Log($"          Name={Safe(refreshed.Name)}");
            Log($"          Kind={refreshed.Kind}");

            Log(
                $"          Connected=" +
                $"{GetProperty(refreshed, "System.Devices.Connected")}");

            Log(
                $"          ContainerId=" +
                $"{GetProperty(refreshed, "System.Devices.ContainerId")}");

            Log(
                $"          DeviceInstanceId=" +
                $"{GetProperty(refreshed, "System.Devices.DeviceInstanceId")}");

            Log(
                $"          FriendlyName=" +
                $"{GetProperty(refreshed, "System.Devices.FriendlyName")}");

            Log(
                $"          Manufacturer=" +
                $"{GetProperty(refreshed, "System.Devices.DeviceManufacturer")}");

            Log(
                $"          ModelName=" +
                $"{GetProperty(refreshed, "System.Devices.ModelName")}");
        }
        catch (Exception ex)
        {
            Log(
                $"          [{reason}] ERROR: " +
                $"{ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void OnEnumerationCompleted(
        DeviceWatcher sender,
        object args)
    {
        Log(new string('-', 100));
        Log("[ENUMERATION COMPLETED]");

        WriteInitialDeviceCsv();

        InitialEnumerationCompleted.TrySetResult(true);
    }

    private static void WriteInitialDeviceCsv()
    {
        var path = Path.Combine(
            _artifactDirectory,
            "initial-devices.csv");

        var sb = new StringBuilder();

        sb.AppendLine(
            "Name,Id,Kind,IsEnabled,IsDefault");

        foreach (var device in Devices.Values
                     .OrderBy(x => x.Name)
                     .ThenBy(x => x.Id))
        {
            sb.AppendLine(
                string.Join(",",
                    Csv(device.Name),
                    Csv(device.Id),
                    Csv(device.Kind.ToString()),
                    device.IsEnabled,
                    device.IsDefault));
        }

        File.WriteAllText(
            path,
            sb.ToString(),
            new UTF8Encoding(true));

        Log(
            $"[EVIDENCE] Initial device CSV written: {path}");
    }

    private static void OnStopped(
        DeviceWatcher sender,
        object args)
    {
        Log(
            $"[WATCHER STOPPED] Status={sender.Status}");
    }

    private static async Task StopWatcherAsync()
    {
        if (_watcher is null)
        {
            return;
        }

        try
        {
            if (_watcher.Status is
                DeviceWatcherStatus.Started
                or DeviceWatcherStatus.EnumerationCompleted)
            {
                _watcher.Stop();

                for (var i = 0;
                     i < 40 &&
                     _watcher.Status != DeviceWatcherStatus.Stopped;
                     i++)
                {
                    await Task.Delay(50);
                }
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

            Devices.Clear();
        }
    }

    private static string GetProperty(
        DeviceInformation device,
        string key)
    {
        if (!device.Properties.TryGetValue(
                key,
                out var value))
        {
            return "(not supplied)";
        }

        return FormatValue(value);
    }

    private static string FormatValue(object? value)
    {
        if (value is null)
        {
            return "(null)";
        }

        return value switch
        {
            Guid guid =>
                guid.ToString("D"),

            bool boolean =>
                boolean ? "True" : "False",

            string text when string.IsNullOrWhiteSpace(text) =>
                "(empty)",

            _ =>
                value.ToString() ?? "(null)"
        };
    }

    private static void Log(string message)
    {
        var line =
            $"[{DateTimeOffset.Now:HH:mm:ss.fff}] {message}";

        lock (LogLock)
        {
            Console.WriteLine(line);

            File.AppendAllText(
                _eventLogPath,
                line + Environment.NewLine,
                Encoding.UTF8);
        }
    }

    private static string Safe(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? "(empty)"
            : value;
    }

    private static string Csv(string? value)
    {
        var text = value ?? string.Empty;

        return "\"" +
               text.Replace("\"", "\"\"") +
               "\"";
    }
}
