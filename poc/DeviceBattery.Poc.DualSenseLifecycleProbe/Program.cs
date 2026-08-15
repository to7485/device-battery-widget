using System.Collections.Concurrent;
using System.Text;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

internal static class Program
{
    private const ushort GenericDesktopUsagePage = 0x0001;
    private const ushort GamepadUsageId = 0x0005;
    private const ushort SonyVendorId = 0x054C;
    private const ushort DualSenseProductId = 0x0CE6;
    private static readonly TimeSpan ReportTimeout = TimeSpan.FromSeconds(10);

    private static readonly object ConsoleLock = new();
    private static readonly ConcurrentDictionary<string, DeviceSession> Sessions =
        new(StringComparer.OrdinalIgnoreCase);

    private static DeviceWatcher? _watcher;
    private static CancellationTokenSource? _monitorCancellation;
    private static Task? _monitorTask;
    private static bool _enumerationCompleted;

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Gate 4 — DualSense Lifecycle / Timeout Probe");
        Console.WriteLine(new string('-', 96));
        Console.WriteLine("Target: Sony DualSense Bluetooth VID=0x054C PID=0x0CE6");
        Console.WriteLine("Safety: targeted HID watcher + FileAccessMode.Read only.");
        Console.WriteLine("DeviceClass.All AQS, output reports, feature reports, and vendor commands are not used.");
        Console.WriteLine($"POC report timeout = {ReportTimeout.TotalSeconds:0} seconds (candidate only, not Production policy).");
        Console.WriteLine();

        try
        {
            StartWatcher();
            _monitorCancellation = new CancellationTokenSource();
            _monitorTask = MonitorTimeoutsAsync(_monitorCancellation.Token);

            Console.WriteLine("Commands: S = summary, R = read-only reopen, Q = quit");
            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                if (key.Key is ConsoleKey.Q or ConsoleKey.Escape)
                {
                    break;
                }

                if (key.Key == ConsoleKey.S)
                {
                    PrintSummary();
                }
                else if (key.Key == ConsoleKey.R)
                {
                    await ReopenKnownDevicesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Log($"[FATAL] {ex.GetType().Name}: {ex.Message}");
            Log(ex.StackTrace ?? "(no stack trace)");
        }
        finally
        {
            await StopAsync();
        }
    }

    private static void StartWatcher()
    {
        string selector = HidDevice.GetDeviceSelector(
            GenericDesktopUsagePage,
            GamepadUsageId,
            SonyVendorId,
            DualSenseProductId);

        _watcher = DeviceInformation.CreateWatcher(selector);
        _watcher.Added += OnDeviceAdded;
        _watcher.Removed += OnDeviceRemoved;
        _watcher.Updated += OnDeviceUpdated;
        _watcher.EnumerationCompleted += OnEnumerationCompleted;
        _watcher.Stopped += OnWatcherStopped;
        _watcher.Start();

        Log("[WATCHER] Started with targeted DualSense HID selector.");
    }

    private static async void OnDeviceAdded(DeviceWatcher sender, DeviceInformation info)
    {
        try
        {
            Log($"[WATCHER ADDED] Name={Empty(info.Name)}, Id={info.Id}");
            await OpenReadOnlyAsync(info.Id, info.Name, "Added");
        }
        catch (Exception ex)
        {
            Log($"[ADDED ERROR] {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void OnDeviceRemoved(DeviceWatcher sender, DeviceInformationUpdate update)
    {
        Log($"[WATCHER REMOVED] Id={update.Id}");
        if (Sessions.TryRemove(update.Id, out DeviceSession? session))
        {
            session.Dispose();
            LogState(update.Id, "REMOVED_FROM_UI", state: null);
        }
    }

    private static void OnDeviceUpdated(DeviceWatcher sender, DeviceInformationUpdate update) =>
        Log($"[WATCHER UPDATED] Id={update.Id}");

    private static void OnEnumerationCompleted(DeviceWatcher sender, object args)
    {
        _enumerationCompleted = true;
        Log($"[WATCHER] EnumerationCompleted; Sessions={Sessions.Count}");
    }

    private static void OnWatcherStopped(DeviceWatcher sender, object args) =>
        Log($"[WATCHER] Stopped; Status={sender.Status}");

    private static async Task OpenReadOnlyAsync(string id, string? name, string reason)
    {
        if (Sessions.ContainsKey(id))
        {
            Log($"[OPEN] skipped existing session; Reason={reason}, Id={id}");
            return;
        }

        HidDevice? device;
        try
        {
            device = await HidDevice.FromIdAsync(id, FileAccessMode.Read);
        }
        catch (Exception ex)
        {
            Log($"[OPEN ERROR] Reason={reason}, {ex.GetType().Name}: {ex.Message}, Id={id}");
            return;
        }

        if (device is null)
        {
            Log($"[OPEN NULL] Reason={reason}, Id={id}");
            return;
        }

        DeviceSession session = new(id, name, device);
        if (!Sessions.TryAdd(id, session))
        {
            session.Dispose();
            return;
        }

        device.InputReportReceived += session.OnInputReportReceived;
        Log($"[OPEN SUCCESS] Reason={reason}, Name={Empty(name)}, Id={id}");
        LogState(id, "CONNECTED_WAITING_FOR_REPORT", session.State);
    }

    private static async Task ReopenKnownDevicesAsync()
    {
        Log("[MANUAL REOPEN] Starting targeted FindAllAsync.");
        string selector = HidDevice.GetDeviceSelector(
            GenericDesktopUsagePage,
            GamepadUsageId,
            SonyVendorId,
            DualSenseProductId);

        DeviceInformationCollection infos = await DeviceInformation.FindAllAsync(selector);
        Log($"[MANUAL REOPEN] Found={infos.Count}");
        foreach (DeviceInformation info in infos)
        {
            if (Sessions.TryRemove(info.Id, out DeviceSession? previous))
            {
                previous.Dispose();
            }

            await OpenReadOnlyAsync(info.Id, info.Name, "ManualReopen");
        }
    }

    private static async Task MonitorTimeoutsAsync(CancellationToken cancellationToken)
    {
        using PeriodicTimer timer = new(TimeSpan.FromSeconds(1));
        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                DateTimeOffset now = DateTimeOffset.Now;
                foreach (DeviceSession session in Sessions.Values)
                {
                    session.ApplyTimeoutIfNeeded(now, ReportTimeout);
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private static void PrintSummary()
    {
        lock (ConsoleLock)
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] SUMMARY Watcher={_watcher?.Status}, EnumerationCompleted={_enumerationCompleted}, Sessions={Sessions.Count}");
            if (Sessions.Count == 0)
            {
                Console.WriteLine("  No connected/open DualSense HID session.");
            }

            foreach (DeviceSession session in Sessions.Values.OrderBy(x => x.Id))
            {
                BatteryState state = session.State;
                Console.WriteLine($"  Id={session.Id}");
                Console.WriteLine($"    Reports={session.ReportCount}, LastReportAt={FormatTime(session.LastReportAt)}");
                Console.WriteLine($"    Availability={state.Availability}, Percent={FormatPercent(state.Percent)}, Charging={state.Charging}, Precision={state.Precision}, Estimated={state.IsEstimated}");
                Console.WriteLine($"    Reason={state.Reason ?? "(none)"}");
            }
        }
    }

    private static async Task StopAsync()
    {
        if (_watcher is not null)
        {
            _watcher.Added -= OnDeviceAdded;
            _watcher.Removed -= OnDeviceRemoved;
            _watcher.Updated -= OnDeviceUpdated;
            _watcher.EnumerationCompleted -= OnEnumerationCompleted;
            _watcher.Stopped -= OnWatcherStopped;

            if (_watcher.Status is DeviceWatcherStatus.Started or DeviceWatcherStatus.EnumerationCompleted)
            {
                _watcher.Stop();
            }

            _watcher = null;
        }

        if (_monitorCancellation is not null)
        {
            _monitorCancellation.Cancel();
        }

        if (_monitorTask is not null)
        {
            await _monitorTask;
        }

        _monitorCancellation?.Dispose();
        _monitorCancellation = null;
        _monitorTask = null;

        foreach ((string id, DeviceSession session) in Sessions.ToArray())
        {
            if (Sessions.TryRemove(id, out _))
            {
                session.Dispose();
            }
        }

        Log("[CLEANUP] Watcher, timer, handlers, and HID devices disposed.");
    }

    internal static void LogState(string id, string transition, BatteryState? state)
    {
        if (state is null)
        {
            Log($"[STATE] Transition={transition}, Id={id}");
            return;
        }

        Log($"[STATE] Transition={transition}, Availability={state.Availability}, Percent={FormatPercent(state.Percent)}, Charging={state.Charging}, Precision={state.Precision}, Estimated={state.IsEstimated}, Reason={state.Reason ?? "(none)"}, Id={id}");
    }

    internal static void Log(string message)
    {
        lock (ConsoleLock)
        {
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] {message}");
        }
    }

    private static string Empty(string? value) => string.IsNullOrWhiteSpace(value) ? "(empty)" : value;
    private static string FormatPercent(int? value) => value.HasValue ? $"{value.Value}%" : "null";
    private static string FormatTime(DateTimeOffset? value) => value.HasValue ? value.Value.ToString("O") : "(none)";
}

internal sealed class DeviceSession : IDisposable
{
    private readonly object _stateLock = new();
    private readonly HidDevice _device;
    private BatteryState _state;
    private DateTimeOffset? _lastReportAt;
    private int _reportCount;
    private bool _timeoutApplied;
    private bool _disposed;
    private bool _shapeLogged;
    private byte? _lastInvalidStatus;

    public DeviceSession(string id, string? name, HidDevice device)
    {
        Id = id;
        Name = name;
        _device = device;
        _state = BatteryState.Unknown(DateTimeOffset.Now, "Waiting for first valid DualSense report");
    }

    public string Id { get; }
    public string? Name { get; }
    public int ReportCount { get { lock (_stateLock) return _reportCount; } }
    public DateTimeOffset? LastReportAt { get { lock (_stateLock) return _lastReportAt; } }
    public BatteryState State { get { lock (_stateLock) return _state; } }

    public void OnInputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
    {
        try
        {
            byte[] data = ReadBytes(args.Report.Data);
            LogReportShapeOnce(args.Report.Id, data);
            BatteryState? parsed = DualSenseNormalizer.TryNormalize(args.Report.Id, data, DateTimeOffset.Now);
            if (parsed is null)
            {
                return;
            }

            if (parsed.Availability != BatteryAvailability.Available)
            {
                bool shouldLog;
                lock (_stateLock)
                {
                    shouldLog = _lastInvalidStatus != parsed.StatusByte;
                    _lastInvalidStatus = parsed.StatusByte;
                }

                if (shouldLog)
                {
                    Program.LogState(Id, "INVALID_REPORT_IGNORED", parsed);
                }

                return;
            }

            string transition;
            lock (_stateLock)
            {
                _reportCount++;
                _lastReportAt = parsed.LastUpdatedAt;
                transition = _state.Availability != BatteryAvailability.Available
                    ? "REPORT_RECOVERED"
                    : _state.StatusByte == parsed.StatusByte ? "REPORT" : "BATTERY_CHANGED";
                _state = parsed;
                _timeoutApplied = false;
                _lastInvalidStatus = null;
            }

            if (transition != "REPORT")
            {
                Program.LogState(Id, transition, parsed);
            }
        }
        catch (Exception ex)
        {
            Program.Log($"[INPUT ERROR] {ex.GetType().Name}: {ex.Message}, Id={Id}");
        }
    }

    private void LogReportShapeOnce(ushort reportId, byte[] data)
    {
        lock (_stateLock)
        {
            if (_shapeLogged || reportId is not (0x31 or 0x01))
            {
                return;
            }

            _shapeLogged = true;
        }

        static string At(byte[] bytes, int offset) =>
            offset < bytes.Length ? $"0x{bytes[offset]:X2}" : "n/a";

        string head = Convert.ToHexString(data.AsSpan(0, Math.Min(data.Length, 16)));
        Program.Log(
            $"[REPORT SHAPE] ReportId=0x{reportId:X2}, DataLength={data.Length}, " +
            $"Data[0]={At(data, 0)}, Offset52={At(data, 52)}, Offset53={At(data, 53)}, " +
            $"Offset54={At(data, 54)}, Offset55={At(data, 55)}, Head={head}, Id={Id}");
    }

    public void ApplyTimeoutIfNeeded(DateTimeOffset now, TimeSpan timeout)
    {
        BatteryState? timedOutState = null;
        lock (_stateLock)
        {
            DateTimeOffset reference = _lastReportAt ?? _state.LastUpdatedAt;
            if (!_timeoutApplied && now - reference >= timeout)
            {
                _state = BatteryState.Unknown(now, $"No valid DualSense report for {timeout.TotalSeconds:0} seconds");
                _timeoutApplied = true;
                timedOutState = _state;
            }
        }

        if (timedOutState is not null)
        {
            Program.LogState(Id, "REPORT_TIMEOUT", timedOutState);
        }
    }

    public void Dispose()
    {
        lock (_stateLock)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
        }

        _device.InputReportReceived -= OnInputReportReceived;
        _device.Dispose();
    }

    private static byte[] ReadBytes(IBuffer buffer)
    {
        using DataReader reader = DataReader.FromBuffer(buffer);
        byte[] data = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(data);
        return data;
    }
}

internal enum BatteryAvailability { Available, Unknown }
internal enum ChargingState { Charging, NotCharging, Unknown }
internal enum BatteryPrecision { Unknown, TenPercentBucket, Full }

internal sealed record BatteryState(
    BatteryAvailability Availability,
    int? Percent,
    ChargingState Charging,
    BatteryPrecision Precision,
    bool IsEstimated,
    DateTimeOffset LastUpdatedAt,
    byte? StatusByte,
    string? Reason)
{
    public static BatteryState Unknown(DateTimeOffset at, string reason) =>
        new(BatteryAvailability.Unknown, null, ChargingState.Unknown, BatteryPrecision.Unknown, false, at, null, reason);
}

internal static class DualSenseNormalizer
{
    public static BatteryState? TryNormalize(ushort reportId, byte[] data, DateTimeOffset observedAt)
    {
        int commonStart;
        // On the tested Windows Bluetooth HID stack, WinRT exposes the 78-byte
        // full Bluetooth packet as Report.Id 0x01 / Data[0] 0x01. Therefore
        // packet length must take precedence over Report.Id when choosing the
        // Bluetooth-versus-USB layout. The validated battery byte remains at
        // packet offset 54 (common report offset 52 + BT prefix length 2).
        if (data.Length >= 78 && reportId is 0x31 or 0x01)
        {
            commonStart = 2;
        }
        else if (reportId == 0x31 && data.Length >= 77)
        {
            commonStart = 1;
        }
        else if (reportId == 0x01)
        {
            if (data.Length >= 64 && data[0] == 0x01) commonStart = 1;
            else if (data.Length >= 63) commonStart = 0;
            else return null;
        }
        else
        {
            return null;
        }

        int statusOffset = commonStart + 52;
        if (statusOffset >= data.Length) return null;

        byte status = data[statusOffset];
        int bucket = status & 0x0F;
        int chargingCode = (status >> 4) & 0x0F;
        if (bucket > 10 || chargingCode is not (0x0 or 0x1 or 0x2))
        {
            return BatteryState.Unknown(observedAt, $"Invalid status byte 0x{status:X2}") with { StatusByte = status };
        }

        bool full = chargingCode == 0x2 || bucket == 10;
        return new BatteryState(
            BatteryAvailability.Available,
            full ? 100 : bucket * 10 + 5,
            chargingCode == 0x1 ? ChargingState.Charging : ChargingState.NotCharging,
            full ? BatteryPrecision.Full : BatteryPrecision.TenPercentBucket,
            IsEstimated: !full,
            observedAt,
            status,
            Reason: null);
    }
}
