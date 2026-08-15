using DeviceBattery.Domain;
using DeviceBattery.Infrastructure.Windows;
using Windows.System.Power;

DateTimeOffset now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);
var parser = new DualSenseHidBatteryParser();
var specs = new (string Name, Action Run)[]
{
    ("BLE battery level accepts exact percent", () => { Equal(true, BleBatteryLevelParser.TryParse([73], out int percent)); Equal(73, percent); }),
    ("BLE battery level rejects empty value", () => Equal(false, BleBatteryLevelParser.TryParse([], out _))),
    ("BLE battery level rejects value above 100", () => Equal(false, BleBatteryLevelParser.TryParse([101], out _))),
    ("Gaming Input battery maps capacity and status", () => { Equal(true, GamingInputBatteryMapper.TryCreate(100, 1000, BatteryStatus.Discharging, now, GamingInputBatteryProvider.Id, out BatteryState battery)); Equal(10, battery.Percent); Equal(ChargingState.NotCharging, battery.Charging); Equal(BatteryPrecision.GranularLevel, battery.Precision); }),
    ("Gaming Input battery rejects missing capacity", () => Equal(false, GamingInputBatteryMapper.TryCreate(null, 1000, BatteryStatus.Discharging, now, GamingInputBatteryProvider.Id, out _))),
    ("Gaming Input battery rejects unknown status", () => Equal(false, GamingInputBatteryMapper.TryCreate(100, 1000, BatteryStatus.NotPresent, now, GamingInputBatteryProvider.Id, out _))),
    ("Auto-start command quotes executable path", () => Equal("\"C:\\Program Files\\Device Battery Widget\\DeviceBatteryWidget.exe\" --autostart", RegistryRunAutoStartService.FormatCommand(@"C:\Program Files\Device Battery Widget\DeviceBatteryWidget.exe"))),
    ("Diagnostic log prunes expired files", () =>
    {
        string directory = Path.Combine(Path.GetTempPath(), $"DeviceBatterySpecs-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            string expired = Path.Combine(directory, "device-battery-20200101.log");
            File.WriteAllText(expired, "expired");
            File.SetLastWriteTimeUtc(expired, DateTime.UtcNow.AddDays(-8));
            using var listener = new BoundedFileTraceListener(directory, TimeSpan.FromDays(7), 1024);
            listener.WriteLine("APP_START");
            Equal(false, File.Exists(expired));
            Equal(1, Directory.GetFiles(directory, "device-battery-*.log").Length);
        }
        finally { Directory.Delete(directory, recursive: true); }
    }),
    ("Tested WinRT Bluetooth layout uses offset 54", () =>
    {
        byte[] report = Report(78, 54, 0x01, 0x01);
        Equal(true, parser.TryParse(0x01, report, now, out BatteryObservation observation));
        Equal(DualSenseReportLayout.BluetoothFullReport, observation.Layout);
        Equal(54, observation.StatusOffset);
        Equal(15, observation.Battery.Percent);
        Equal(ChargingState.NotCharging, observation.Battery.Charging);
    }),
    ("Bluetooth length takes precedence over report ID", () =>
    {
        byte[] report = Report(78, 54, 0x11, 0x01);
        Equal(true, parser.TryParse(0x01, report, now, out BatteryObservation observation));
        Equal(ChargingState.Charging, observation.Battery.Charging);
        Equal(15, observation.Battery.Percent);
    }),
    ("Canonical Bluetooth report ID parses", () =>
    {
        byte[] report = Report(78, 54, 0x00, 0x31);
        Equal(true, parser.TryParse(0x31, report, now, out BatteryObservation observation));
        Equal(5, observation.Battery.Percent);
    }),
    ("Bluetooth payload-only layout uses offset 53", () =>
    {
        byte[] report = Report(77, 53, 0x09);
        Equal(true, parser.TryParse(0x31, report, now, out BatteryObservation observation));
        Equal(DualSenseReportLayout.BluetoothPayload, observation.Layout);
        Equal(95, observation.Battery.Percent);
    }),
    ("USB full layout uses offset 53", () =>
    {
        byte[] report = Report(64, 53, 0x11, 0x01);
        Equal(true, parser.TryParse(0x01, report, now, out BatteryObservation observation));
        Equal(DualSenseReportLayout.UsbFullReport, observation.Layout);
        Equal(15, observation.Battery.Percent);
        Equal(ChargingState.Charging, observation.Battery.Charging);
    }),
    ("USB payload-only layout uses offset 52", () =>
    {
        byte[] report = Report(63, 52, 0x02);
        Equal(true, parser.TryParse(0x01, report, now, out BatteryObservation observation));
        Equal(DualSenseReportLayout.UsbPayload, observation.Layout);
        Equal(25, observation.Battery.Percent);
    }),
    ("Full code produces exact 100 percent", () =>
    {
        byte[] report = Report(78, 54, 0x2A, 0x01);
        Equal(true, parser.TryParse(0x01, report, now, out BatteryObservation observation));
        Equal(100, observation.Battery.Percent);
        Equal(BatteryPrecision.ExactPercent, observation.Battery.Precision);
        Equal(false, observation.Battery.IsEstimated);
    }),
    ("Invalid charging status is rejected", () =>
    {
        byte[] report = Report(78, 54, 0xDF, 0x01);
        Equal(false, parser.TryParse(0x01, report, now, out _));
    }),
    ("Invalid bucket is rejected", () =>
    {
        byte[] report = Report(78, 54, 0x0B, 0x01);
        Equal(false, parser.TryParse(0x01, report, now, out _));
    }),
    ("Short and unrelated reports are rejected", () =>
    {
        Equal(false, parser.TryParse(0x01, new byte[62], now, out _));
        Equal(false, parser.TryParse(0x02, new byte[78], now, out _));
    }),
    ("Bluetooth endpoint filter excludes USB", () =>
    {
        const string bluetooth = @"\\?\HID#{00001124-0000-1000-8000-00805f9b34fb}_VID&0002054C_PID&0CE6#device";
        const string usb = @"\\?\HID#VID_054C&PID_0CE6&MI_03#device";
        Equal(true, DualSenseDeviceIdentity.IsBluetoothEndpoint(bluetooth));
        Equal(false, DualSenseDeviceIdentity.IsBluetoothEndpoint(usb));
        Equal(true, DualSenseDeviceIdentity.IsUsbEndpoint(usb));
        Equal(true, DualSenseDeviceIdentity.IsSupportedEndpoint(bluetooth));
        Equal(true, DualSenseDeviceIdentity.IsSupportedEndpoint(usb));
        Equal(true, DualSenseDeviceIdentity.UsesReportFreshnessTimeout(bluetooth));
        Equal(false, DualSenseDeviceIdentity.UsesReportFreshnessTimeout(usb));
    }),
    ("Stable key is deterministic and does not expose device ID", () =>
    {
        const string id = @"\\?\HID#{00001124-0000-1000-8000-00805f9b34fb}_VID&0002054C_PID&0CE6#device";
        DeviceKey first = DualSenseDeviceIdentity.CreateKey(id);
        DeviceKey second = DualSenseDeviceIdentity.CreateKey(id.ToLowerInvariant());
        Equal(first, second);
        Equal(DualSenseHidBatteryParser.ProviderId, first.ProviderId);
        Equal(true, first.StableId.StartsWith("BT-", StringComparison.Ordinal));
        Equal(false, first.StableId.Contains("HID", StringComparison.OrdinalIgnoreCase));
    }),
    ("Freshness remains active before 10 seconds", () =>
    {
        var time = new ManualTimeProvider();
        var tracker = new ReportFreshnessTracker(time, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
        tracker.MarkValidReport();
        time.Advance(TimeSpan.FromMilliseconds(9999));
        Equal(new FreshnessEvaluation(false, false), tracker.Evaluate());
    }),
    ("Freshness expires exactly at 10 seconds once", () =>
    {
        var time = new ManualTimeProvider();
        var tracker = new ReportFreshnessTracker(time, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
        tracker.MarkValidReport();
        time.Advance(TimeSpan.FromSeconds(10));
        Equal(new FreshnessEvaluation(true, false), tracker.Evaluate());
        Equal(new FreshnessEvaluation(false, false), tracker.Evaluate());
    }),
    ("Freshness becomes dormant exactly at 30 seconds", () =>
    {
        var time = new ManualTimeProvider();
        var tracker = new ReportFreshnessTracker(time, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
        tracker.MarkValidReport();
        time.Advance(TimeSpan.FromSeconds(30));
        Equal(new FreshnessEvaluation(true, true), tracker.Evaluate());
    }),
    ("Valid report after expiry is recovery and resets clock", () =>
    {
        var time = new ManualTimeProvider();
        var tracker = new ReportFreshnessTracker(time, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30));
        Equal(true, tracker.MarkValidReport());
        time.Advance(TimeSpan.FromSeconds(10));
        tracker.Evaluate();
        Equal(true, tracker.MarkValidReport());
        time.Advance(TimeSpan.FromSeconds(9));
        Equal(new FreshnessEvaluation(false, false), tracker.Evaluate());
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

static byte[] Report(int length, int statusOffset, byte status, byte? first = null)
{
    var report = new byte[length];
    if (first.HasValue)
        report[0] = first.Value;
    report[statusOffset] = status;
    return report;
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Expected {expected}, actual {actual}.");
}

sealed class ManualTimeProvider : TimeProvider
{
    private long timestamp;

    public override long TimestampFrequency => 1_000;
    public override long GetTimestamp() => timestamp;
    public void Advance(TimeSpan duration) =>
        timestamp = checked(timestamp + (long)(duration.TotalSeconds * TimestampFrequency));
}
