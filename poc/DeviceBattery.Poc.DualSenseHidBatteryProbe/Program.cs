using System.Text;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

internal static class Program
{
    private const ushort GenericDesktopUsagePage = 0x0001;
    private const ushort JoystickUsageId = 0x0004;
    private const ushort GamepadUsageId = 0x0005;
    private const ushort SonyVendorId = 0x054C;
    private const ushort DualSenseProductId = 0x0CE6;

    private static readonly object ConsoleLock = new();
    private static readonly List<HidDevice> OpenDevices = new();
    private static readonly HashSet<string> OpenDeviceIds = new(StringComparer.OrdinalIgnoreCase);

    private static byte? _lastStatusByte;
    private static int _receivedReportCount;
    private static bool _printedFirstRawReport;

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Gate 4 POC-B03-2 — DualSense HID Battery Probe");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine("Target: Sony DualSense VID=0x054C PID=0x0CE6");
        Console.WriteLine("Purpose: verify whether the controller's HID input report exposes battery/charging state.");
        Console.WriteLine("This probe is read-only. It does not send output/feature reports to the controller.");
        Console.WriteLine();

        try
        {
            await EnumerateAndOpenAsync(GenericDesktopUsagePage, GamepadUsageId, "Game Pad");
            await EnumerateAndOpenAsync(GenericDesktopUsagePage, JoystickUsageId, "Joystick fallback");

            if (OpenDevices.Count == 0)
            {
                Console.WriteLine();
                Console.WriteLine("RESULT = No matching DualSense HID top-level collection could be opened.");
                Console.WriteLine("Send this complete output back for the next fallback decision.");
                return;
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"Open HID device count = {OpenDevices.Count}");
            Console.WriteLine("Listening for input reports...");
            Console.WriteLine("Move a stick or press a button if no report appears immediately.");
            Console.WriteLine();
            Console.WriteLine("Commands: S = print current summary, Q = quit");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                {
                    break;
                }

                if (key.Key == ConsoleKey.S)
                {
                    lock (ConsoleLock)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] SUMMARY Reports={_receivedReportCount}, LastStatusByte={FormatByte(_lastStatusByte)}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine();
            Console.WriteLine($"[FATAL] {ex.GetType().Name}: {ex.Message}");
            Console.WriteLine(ex.StackTrace);
        }
        finally
        {
            foreach (HidDevice device in OpenDevices)
            {
                device.InputReportReceived -= OnInputReportReceived;
                device.Dispose();
            }

            OpenDevices.Clear();
            OpenDeviceIds.Clear();
        }
    }

    private static async Task EnumerateAndOpenAsync(ushort usagePage, ushort usageId, string label)
    {
        string selector = HidDevice.GetDeviceSelector(usagePage, usageId, SonyVendorId, DualSenseProductId);
        DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);

        Console.WriteLine($"[{label}] DeviceInformation count = {devices.Count}");

        foreach (DeviceInformation info in devices)
        {
            Console.WriteLine($"  Name = {Empty(info.Name)}");
            Console.WriteLine($"  Id   = {info.Id}");

            if (!OpenDeviceIds.Add(info.Id))
            {
                Console.WriteLine("  OPEN = skipped (already opened by another selector)");
                continue;
            }

            try
            {
                HidDevice? hid = await HidDevice.FromIdAsync(info.Id, FileAccessMode.Read);
                if (hid is null)
                {
                    Console.WriteLine("  OPEN = null (Windows did not grant/open this HID collection)");
                    continue;
                }

                hid.InputReportReceived += OnInputReportReceived;
                OpenDevices.Add(hid);

                Console.WriteLine("  OPEN = Success");
                Console.WriteLine($"  ProductId = 0x{hid.ProductId:X4}");
                Console.WriteLine($"  VendorId  = 0x{hid.VendorId:X4}");
                Console.WriteLine($"  Version   = 0x{hid.Version:X4}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  OPEN ERROR = {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static void OnInputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
    {
        try
        {
            HidInputReport report = args.Report;
            byte[] data = ReadBytes(report.Data);
            DualSenseBatterySample? battery = TryParseDualSenseBattery(report.Id, data);

            lock (ConsoleLock)
            {
                _receivedReportCount++;

                if (!_printedFirstRawReport)
                {
                    _printedFirstRawReport = true;
                    Console.WriteLine();
                    Console.WriteLine(new string('=', 100));
                    Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] FIRST INPUT REPORT");
                    Console.WriteLine($"ReportId     = 0x{report.Id:X2}");
                    Console.WriteLine($"BufferLength = {data.Length}");
                    Console.WriteLine($"Head         = {Hex(data, 0, Math.Min(20, data.Length))}");
                    if (data.Length > 20)
                    {
                        Console.WriteLine($"Tail         = {Hex(data, Math.Max(0, data.Length - 16), Math.Min(16, data.Length))}");
                    }
                }

                if (battery is null)
                {
                    if (_receivedReportCount <= 3)
                    {
                        Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] Report 0x{report.Id:X2}, len={data.Length}: not a supported full DualSense report layout.");
                    }
                    return;
                }

                if (_lastStatusByte == battery.StatusByte)
                {
                    return;
                }

                _lastStatusByte = battery.StatusByte;

                Console.WriteLine();
                Console.WriteLine(new string('=', 100));
                Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] DUALSENSE BATTERY SAMPLE");
                Console.WriteLine($"ReportId              = 0x{report.Id:X2}");
                Console.WriteLine($"BufferLength          = {data.Length}");
                Console.WriteLine($"Layout                = {battery.Layout}");
                Console.WriteLine($"StatusOffset          = {battery.StatusOffset}");
                Console.WriteLine($"StatusByte            = 0x{battery.StatusByte:X2}");
                Console.WriteLine($"BatteryBucketRaw      = {battery.BatteryBucket}");
                Console.WriteLine($"ChargingCodeRaw       = 0x{battery.ChargingCode:X1}");
                Console.WriteLine($"EstimatedPercent      = {(battery.EstimatedPercent.HasValue ? $"{battery.EstimatedPercent.Value}%" : "Unknown")}");
                Console.WriteLine($"ChargingState         = {battery.ChargingState}");
                Console.WriteLine("NOTE: DualSense reports battery in coarse 10% buckets; the estimate follows the upstream hid-playstation mapping.");
            }
        }
        catch (Exception ex)
        {
            lock (ConsoleLock)
            {
                Console.WriteLine($"[INPUT ERROR] {ex.GetType().Name}: {ex.Message}");
            }
        }
    }

    private static DualSenseBatterySample? TryParseDualSenseBattery(ushort reportId, byte[] data)
    {
        int commonStart;
        string layout;

        if (reportId == 0x31)
        {
            // Upstream hid-playstation: BT full report is 78 bytes including report ID,
            // with dualsense_input_report beginning at packet byte 2.
            if (data.Length >= 78 && data[0] == 0x31)
            {
                commonStart = 2;
                layout = "Bluetooth full report; Data includes report ID";
            }
            else if (data.Length >= 77)
            {
                // WinRT exposes Report.Id separately on some HID stacks; handle payload-only data too.
                commonStart = 1;
                layout = "Bluetooth full report; report ID exposed separately";
            }
            else
            {
                return null;
            }
        }
        else if (reportId == 0x01)
        {
            // Upstream hid-playstation: USB full report is 64 bytes including report ID.
            // Bluetooth may also emit a minimal report ID 0x01; reject short layouts.
            if (data.Length >= 64 && data[0] == 0x01)
            {
                commonStart = 1;
                layout = "USB full report; Data includes report ID";
            }
            else if (data.Length >= 63)
            {
                commonStart = 0;
                layout = "USB full report; report ID exposed separately";
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }

        const int Status0OffsetInCommonReport = 52;
        int statusOffset = commonStart + Status0OffsetInCommonReport;
        if (statusOffset >= data.Length)
        {
            return null;
        }

        byte status = data[statusOffset];
        int batteryBucket = status & 0x0F;
        int chargingCode = (status >> 4) & 0x0F;

        int? percent;
        string chargingState;

        switch (chargingCode)
        {
            case 0x0:
                percent = Math.Min(batteryBucket * 10 + 5, 100);
                chargingState = "Not Charging / Discharging";
                break;
            case 0x1:
                percent = Math.Min(batteryBucket * 10 + 5, 100);
                chargingState = "Charging";
                break;
            case 0x2:
                percent = 100;
                chargingState = "Full";
                break;
            case 0xA:
                percent = null;
                chargingState = "Unknown (voltage/temperature out of range)";
                break;
            case 0xB:
                percent = null;
                chargingState = "Unknown (temperature error)";
                break;
            case 0xF:
                percent = null;
                chargingState = "Unknown (charging error)";
                break;
            default:
                percent = null;
                chargingState = $"Unknown (code 0x{chargingCode:X1})";
                break;
        }

        return new DualSenseBatterySample(
            status,
            batteryBucket,
            chargingCode,
            percent,
            chargingState,
            statusOffset,
            layout);
    }

    private static byte[] ReadBytes(IBuffer buffer)
    {
        using DataReader reader = DataReader.FromBuffer(buffer);
        byte[] data = new byte[reader.UnconsumedBufferLength];
        reader.ReadBytes(data);
        return data;
    }

    private static string Hex(byte[] data, int offset, int count)
    {
        if (count <= 0 || offset < 0 || offset >= data.Length)
        {
            return "(empty)";
        }

        int safeCount = Math.Min(count, data.Length - offset);
        return Convert.ToHexString(data, offset, safeCount);
    }

    private static string Empty(string? value) => string.IsNullOrWhiteSpace(value) ? "(empty)" : value;

    private static string FormatByte(byte? value) => value.HasValue ? $"0x{value.Value:X2}" : "(none)";

    private sealed record DualSenseBatterySample(
        byte StatusByte,
        int BatteryBucket,
        int ChargingCode,
        int? EstimatedPercent,
        string ChargingState,
        int StatusOffset,
        string Layout);
}
