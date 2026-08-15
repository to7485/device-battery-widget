using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Win32.SafeHandles;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

internal static class Program
{
    private const ushort LogitechVid = 0x046D;
    private const ushort LogitechReceiverPid = 0xC539;
    private const ushort CorsairVid = 0x1B1C;
    private const ushort CorsairReceiverPid = 0x2A08;

    private const uint CmGetDeviceInterfaceListPresent = 0x00000000;
    private const uint FileShareRead = 0x00000001;
    private const uint FileShareWrite = 0x00000002;
    private const uint OpenExisting = 3;
    private const uint CrBufferSmall = 0x0000001A;
    private const int HidpStatusSuccess = 0x00110000;

    private static readonly object ConsoleLock = new();
    private static readonly List<HidDevice> OpenHidDevices = new();
    private static readonly ConcurrentDictionary<HidDevice, OpenedCollectionContext> OpenedContexts = new();
    private static readonly HashSet<string> OpenDeviceIds = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, int> InputCounts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> FirstReportPrinted = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<string> ReportingDeviceIds = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, byte[]> LastReports = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, int> MarkerReportCounts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, int> MarkerChangedReportCounts = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, int> MarkerPrintedChangeCounts = new(StringComparer.OrdinalIgnoreCase);
    private const int MaxChangedReportsPerShapePerMarker = 20;
    private static int MarkerSequence;
    private static string CurrentMarker = "0:BASELINE";
    private static readonly Dictionary<(ushort Vid, ushort Pid), TargetOpenStats> TargetOpenStatistics = new()
    {
        [(LogitechVid, LogitechReceiverPid)] = new(),
        [(CorsairVid, CorsairReceiverPid)] = new()
    };

    private static async Task Main()
    {
        Console.OutputEncoding = Encoding.UTF8;

        Console.WriteLine("Gate 4 POC-B04-1 — 2.4 GHz Receiver HID Discovery Probe");
        Console.WriteLine(new string('-', 100));
        Console.WriteLine("Purpose: identify HID top-level collections exposed by the tested 2.4 GHz receivers before any vendor-protocol implementation.");
        Console.WriteLine("Targets:");
        Console.WriteLine("  Logitech receiver VID=0x046D PID=0xC539 (G703 test setup)");
        Console.WriteLine("  Corsair receiver  VID=0x1B1C PID=0x2A08 (VOID WIRELESS V2 test setup)");
        Console.WriteLine("This probe does not send output reports or vendor commands.");
        Console.WriteLine();

        try
        {
            List<NativeHidCollection> collections = EnumerateTargetHidCollections();

            if (collections.Count == 0)
            {
                Console.WriteLine("RESULT = No target receiver HID collections were found.");
                return;
            }

            PrintNativeCollections(collections);
            await OpenWinRtCollectionsAsync(collections);
            PrintDiscoverySummary(collections);

            Console.WriteLine();
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"WinRT HID collections opened = {OpenHidDevices.Count}");
            Console.WriteLine("The probe will print the first input report observed for each opened collection.");
            Console.WriteLine("Keep the receiver connected. You may move/use the peripheral to generate reports.");
            Console.WriteLine();
            Console.WriteLine("Commands:");
            Console.WriteLine("  1 = G703_OFF, 2 = G703_ON");
            Console.WriteLine("  3 = CORSAIR_OFF, 4 = CORSAIR_ON");
            Console.WriteLine("  5 = APPS_CLOSED_BASELINE");
            Console.WriteLine("  6 = GHUB_BATTERY_SCREEN");
            Console.WriteLine("  7 = ICUE_BATTERY_SCREEN");
            Console.WriteLine("  S = summary, Q = quit");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Q || key.Key == ConsoleKey.Escape)
                {
                    break;
                }

                if (key.Key == ConsoleKey.D1 || key.Key == ConsoleKey.NumPad1)
                {
                    PrintMarker("G703_OFF");
                }
                else if (key.Key == ConsoleKey.D2 || key.Key == ConsoleKey.NumPad2)
                {
                    PrintMarker("G703_ON");
                }
                else if (key.Key == ConsoleKey.D3 || key.Key == ConsoleKey.NumPad3)
                {
                    PrintMarker("CORSAIR_OFF");
                }
                else if (key.Key == ConsoleKey.D4 || key.Key == ConsoleKey.NumPad4)
                {
                    PrintMarker("CORSAIR_ON");
                }
                else if (key.Key == ConsoleKey.D5 || key.Key == ConsoleKey.NumPad5)
                {
                    PrintMarker("APPS_CLOSED_BASELINE");
                }
                else if (key.Key == ConsoleKey.D6 || key.Key == ConsoleKey.NumPad6)
                {
                    PrintMarker("GHUB_BATTERY_SCREEN");
                }
                else if (key.Key == ConsoleKey.D7 || key.Key == ConsoleKey.NumPad7)
                {
                    PrintMarker("ICUE_BATTERY_SCREEN");
                }
                else if (key.Key == ConsoleKey.S)
                {
                    PrintSummary();
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
            foreach (HidDevice device in OpenHidDevices)
            {
                device.InputReportReceived -= OnInputReportReceived;
                device.Dispose();
            }

            OpenHidDevices.Clear();
            OpenedContexts.Clear();
            OpenDeviceIds.Clear();
        }
    }

    private static List<NativeHidCollection> EnumerateTargetHidCollections()
    {
        HidD_GetHidGuid(out Guid hidGuid);

        char[] multiSz = GetPresentHidInterfaceList(ref hidGuid);

        List<NativeHidCollection> results = new();

        foreach (string path in ParseMultiSz(multiSz))
        {
            using SafeFileHandle handle = CreateFileW(
                path,
                0,
                FileShareRead | FileShareWrite,
                IntPtr.Zero,
                OpenExisting,
                0,
                IntPtr.Zero);

            if (handle.IsInvalid)
            {
                if (LooksLikeTargetPath(path))
                {
                    Console.WriteLine($"[NATIVE OPEN ERROR] Win32={Marshal.GetLastWin32Error()}, DevicePath={path}");
                }
                continue;
            }

            HIDD_ATTRIBUTES attributes = new()
            {
                Size = Marshal.SizeOf<HIDD_ATTRIBUTES>()
            };

            if (!HidD_GetAttributes(handle, ref attributes))
            {
                if (LooksLikeTargetPath(path))
                {
                    Console.WriteLine($"[HID ATTRIBUTES ERROR] Win32={Marshal.GetLastWin32Error()}, DevicePath={path}");
                }
                continue;
            }

            if (!IsTarget(attributes.VendorID, attributes.ProductID))
            {
                continue;
            }

            HIDP_CAPS caps = default;
            bool hasCaps = false;
            if (HidD_GetPreparsedData(handle, out IntPtr preparsedData))
            {
                try
                {
                    int status = HidP_GetCaps(preparsedData, out caps);
                    hasCaps = status == HidpStatusSuccess;
                }
                finally
                {
                    HidD_FreePreparsedData(preparsedData);
                }
            }
            else
            {
                Console.WriteLine($"[HID CAPS ERROR] Win32={Marshal.GetLastWin32Error()}, DevicePath={path}");
            }

            results.Add(new NativeHidCollection(
                path,
                attributes.VendorID,
                attributes.ProductID,
                attributes.VersionNumber,
                GetHidString(handle, HidStringKind.Product),
                GetHidString(handle, HidStringKind.Manufacturer),
                GetHidString(handle, HidStringKind.SerialNumber),
                hasCaps,
                caps.UsagePage,
                caps.Usage,
                caps.InputReportByteLength,
                caps.OutputReportByteLength,
                caps.FeatureReportByteLength));
        }

        return results
            .OrderBy(x => x.VendorId)
            .ThenBy(x => x.ProductId)
            .ThenBy(x => x.UsagePage)
            .ThenBy(x => x.Usage)
            .ThenBy(x => x.DevicePath, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static char[] GetPresentHidInterfaceList(ref Guid hidGuid)
    {
        for (int attempt = 1; attempt <= 3; attempt++)
        {
            uint sizeResult = CM_Get_Device_Interface_List_SizeW(
                out uint charCount,
                ref hidGuid,
                null,
                CmGetDeviceInterfaceListPresent);

            if (sizeResult != 0)
            {
                throw new InvalidOperationException($"CM_Get_Device_Interface_List_SizeW failed: CONFIGRET=0x{sizeResult:X8}");
            }

            if (charCount <= 1)
            {
                return new char[charCount];
            }

            char[] buffer = new char[charCount];
            uint listResult = CM_Get_Device_Interface_ListW(
                ref hidGuid,
                null,
                buffer,
                charCount,
                CmGetDeviceInterfaceListPresent);

            if (listResult == 0)
            {
                return buffer;
            }

            if (listResult != CrBufferSmall || attempt == 3)
            {
                throw new InvalidOperationException($"CM_Get_Device_Interface_ListW failed: CONFIGRET=0x{listResult:X8}, Attempt={attempt}");
            }
        }

        throw new InvalidOperationException("HID interface enumeration retry limit reached.");
    }

    private static void PrintNativeCollections(IReadOnlyList<NativeHidCollection> collections)
    {
        Console.WriteLine($"Target HID collection count = {collections.Count}");

        for (int i = 0; i < collections.Count; i++)
        {
            NativeHidCollection c = collections[i];
            Console.WriteLine();
            Console.WriteLine(new string('=', 100));
            Console.WriteLine($"[NATIVE HID COLLECTION #{i + 1}]");
            Console.WriteLine($"Target             = {TargetName(c.VendorId, c.ProductId)}");
            Console.WriteLine($"VID/PID            = 0x{c.VendorId:X4}/0x{c.ProductId:X4}");
            Console.WriteLine($"Version            = 0x{c.VersionNumber:X4}");
            Console.WriteLine($"ProductString      = {Empty(c.ProductString)}");
            Console.WriteLine($"ManufacturerString = {Empty(c.ManufacturerString)}");
            Console.WriteLine($"SerialNumber       = {Empty(c.SerialNumber)}");
            Console.WriteLine($"DevicePath         = {c.DevicePath}");

            if (c.HasCaps)
            {
                Console.WriteLine($"UsagePage          = 0x{c.UsagePage:X4}");
                Console.WriteLine($"Usage              = 0x{c.Usage:X4}");
                Console.WriteLine($"InputReportLength  = {c.InputReportByteLength}");
                Console.WriteLine($"OutputReportLength = {c.OutputReportByteLength}");
                Console.WriteLine($"FeatureReportLength= {c.FeatureReportByteLength}");
                Console.WriteLine($"CollectionClass    = {ClassifyUsage(c.UsagePage)}");
            }
            else
            {
                Console.WriteLine("HID capabilities   = unavailable");
            }
        }
    }

    private static async Task OpenWinRtCollectionsAsync(IEnumerable<NativeHidCollection> collections)
    {
        HashSet<(ushort UsagePage, ushort Usage, ushort Vid, ushort Pid)> selectors = new();

        foreach (NativeHidCollection c in collections.Where(x => x.HasCaps))
        {
            selectors.Add((c.UsagePage, c.Usage, c.VendorId, c.ProductId));
        }

        foreach ((ushort usagePage, ushort usage, ushort vid, ushort pid) in selectors
                     .OrderBy(x => x.Vid)
                     .ThenBy(x => x.Pid)
                     .ThenBy(x => x.UsagePage)
                     .ThenBy(x => x.Usage))
        {
            string selector = HidDevice.GetDeviceSelector(usagePage, usage, vid, pid);
            TargetOpenStats stats = TargetOpenStatistics[(vid, pid)];
            DeviceInformationCollection infos;

            try
            {
                infos = await DeviceInformation.FindAllAsync(selector);
            }
            catch (Exception ex)
            {
                stats.SelectorErrors++;
                Console.WriteLine();
                Console.WriteLine(new string('-', 100));
                Console.WriteLine($"[WINRT SELECTOR ERROR] VID/PID=0x{vid:X4}/0x{pid:X4}, UsagePage=0x{usagePage:X4}, Usage=0x{usage:X4}");
                Console.WriteLine($"  ERROR = {ex.GetType().Name}: {ex.Message}");
                continue;
            }

            Console.WriteLine();
            Console.WriteLine(new string('-', 100));
            Console.WriteLine($"[WINRT SELECTOR] VID/PID=0x{vid:X4}/0x{pid:X4}, UsagePage=0x{usagePage:X4}, Usage=0x{usage:X4}, Count={infos.Count}");

            foreach (DeviceInformation info in infos)
            {
                stats.SelectorMatches++;
                Console.WriteLine($"  Name = {Empty(info.Name)}");
                Console.WriteLine($"  Id   = {info.Id}");

                if (!OpenDeviceIds.Add(info.Id))
                {
                    Console.WriteLine("  OPEN = skipped (already opened)");
                    continue;
                }

                try
                {
                    stats.OpenAttempts++;
                    HidDevice? device = await HidDevice.FromIdAsync(info.Id, FileAccessMode.Read);
                    if (device is null)
                    {
                        stats.OpenNull++;
                        Console.WriteLine("  OPEN = null (protected/inaccessible collection or access not granted)");
                        continue;
                    }

                    OpenedCollectionContext context = new(
                        info.Id,
                        info.Name,
                        vid,
                        pid,
                        usagePage,
                        usage,
                        collections.First(x =>
                            x.VendorId == vid &&
                            x.ProductId == pid &&
                            x.UsagePage == usagePage &&
                            x.Usage == usage).InputReportByteLength);

                    if (!OpenedContexts.TryAdd(device, context))
                    {
                        device.Dispose();
                        stats.OpenErrors++;
                        Console.WriteLine("  OPEN ERROR = collection context registration failed");
                        continue;
                    }
                    device.InputReportReceived += OnInputReportReceived;
                    OpenHidDevices.Add(device);
                    stats.OpenSuccess++;
                    Console.WriteLine("  OPEN = Success");
                }
                catch (Exception ex)
                {
                    stats.OpenErrors++;
                    Console.WriteLine($"  OPEN ERROR = {ex.GetType().Name}: {ex.Message}");
                }
            }
        }
    }

    private static void PrintDiscoverySummary(IReadOnlyCollection<NativeHidCollection> collections)
    {
        Console.WriteLine();
        Console.WriteLine(new string('-', 100));
        Console.WriteLine("DISCOVERY SUMMARY BY TARGET");

        PrintTargetDiscoverySummary(LogitechVid, LogitechReceiverPid, collections);
        PrintTargetDiscoverySummary(CorsairVid, CorsairReceiverPid, collections);
    }

    private static void PrintTargetDiscoverySummary(
        ushort vid,
        ushort pid,
        IReadOnlyCollection<NativeHidCollection> collections)
    {
        NativeHidCollection[] targetCollections = collections
            .Where(x => x.VendorId == vid && x.ProductId == pid)
            .ToArray();
        OpenedCollectionContext[] opened = OpenedContexts.Values
            .Where(x => x.VendorId == vid && x.ProductId == pid)
            .ToArray();
        TargetOpenStats stats = TargetOpenStatistics[(vid, pid)];

        Console.WriteLine($"  {TargetName(vid, pid)} (0x{vid:X4}/0x{pid:X4})");
        Console.WriteLine($"    NativeCollections       = {targetCollections.Length}");
        Console.WriteLine($"    VendorDefinedCollections= {targetCollections.Count(x => x.HasCaps && x.UsagePage >= 0xFF00)}");
        Console.WriteLine($"    WinRtSelectorMatches    = {stats.SelectorMatches}");
        Console.WriteLine($"    WinRtSelectorErrors     = {stats.SelectorErrors}");
        Console.WriteLine($"    WinRtOpenAttempts       = {stats.OpenAttempts}");
        Console.WriteLine($"    WinRtReadOpened         = {stats.OpenSuccess} (tracked contexts: {opened.Length})");
        Console.WriteLine($"    WinRtOpenNull           = {stats.OpenNull}");
        Console.WriteLine($"    WinRtOpenErrors         = {stats.OpenErrors}");
        if (targetCollections.Length == 0)
        {
            Console.WriteLine("    WARNING                 = Target was not discovered.");
        }
    }

    private static void OnInputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
    {
        try
        {
            byte[] data = ReadBytes(args.Report.Data);
            if (!OpenedContexts.TryGetValue(sender, out OpenedCollectionContext? context))
            {
                throw new InvalidOperationException("No collection context was registered for the input-report sender.");
            }

            string key = $"{context.DeviceId}|{context.UsagePage:X4}:{context.Usage:X4}|{args.Report.Id:X2}|{data.Length}";

            lock (ConsoleLock)
            {
                InputCounts.TryGetValue(key, out int count);
                InputCounts[key] = count + 1;
                ReportingDeviceIds.Add(context.DeviceId);

                string markerKey = $"{CurrentMarker}|{key}";
                MarkerReportCounts.TryGetValue(markerKey, out int markerCount);
                MarkerReportCounts[markerKey] = markerCount + 1;

                bool isFirstShape = FirstReportPrinted.Add(key);
                bool hasPrevious = LastReports.TryGetValue(key, out byte[]? previous);
                bool isChanged = hasPrevious && !data.AsSpan().SequenceEqual(previous);
                LastReports[key] = data;

                if (isChanged)
                {
                    MarkerChangedReportCounts.TryGetValue(markerKey, out int changedCount);
                    MarkerChangedReportCounts[markerKey] = changedCount + 1;
                }

                if (!isFirstShape && !isChanged)
                {
                    return;
                }

                if (isChanged)
                {
                    MarkerPrintedChangeCounts.TryGetValue(markerKey, out int printedCount);
                    if (printedCount >= MaxChangedReportsPerShapePerMarker)
                    {
                        return;
                    }

                    MarkerPrintedChangeCounts[markerKey] = printedCount + 1;
                }

                Console.WriteLine();
                Console.WriteLine(new string('=', 100));
                Console.WriteLine(isFirstShape
                    ? $"[{DateTimeOffset.Now:HH:mm:ss.fff}] FIRST INPUT REPORT FOR SHAPE — Marker={CurrentMarker}"
                    : $"[{DateTimeOffset.Now:HH:mm:ss.fff}] CHANGED INPUT REPORT — Marker={CurrentMarker}");
                Console.WriteLine($"VID/PID      = 0x{sender.VendorId:X4}/0x{sender.ProductId:X4}");
                Console.WriteLine($"DeviceName   = {Empty(context.DeviceName)}");
                Console.WriteLine($"DeviceId     = {context.DeviceId}");
                Console.WriteLine($"UsagePage    = 0x{context.UsagePage:X4}");
                Console.WriteLine($"Usage        = 0x{context.Usage:X4}");
                Console.WriteLine($"Class        = {ClassifyUsage(context.UsagePage)}");
                Console.WriteLine($"ReportId     = 0x{args.Report.Id:X2}");
                Console.WriteLine($"NativeMaxLen = {context.NativeInputReportByteLength}");
                Console.WriteLine($"BufferLength = {data.Length}");
                if (isChanged && previous is not null)
                {
                    Console.WriteLine($"ChangedBytes = {DescribeChanges(previous, data)}");
                }
                Console.WriteLine($"Data         = {Convert.ToHexString(data)}");
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

    private static void PrintMarker(string marker)
    {
        lock (ConsoleLock)
        {
            MarkerSequence++;
            CurrentMarker = $"{MarkerSequence}:{marker}";
            Console.WriteLine();
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] ==================== MARKER {CurrentMarker} ====================");
            Console.WriteLine($"Changed-report output cap = {MaxChangedReportsPerShapePerMarker} per report shape for this marker.");
        }
    }

    private static void PrintSummary()
    {
        lock (ConsoleLock)
        {
            Console.WriteLine();
            Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] INPUT REPORT SUMMARY");
            if (InputCounts.Count == 0)
            {
                Console.WriteLine("  No input reports observed through opened WinRT HID collections.");
                return;
            }

            foreach ((string key, int count) in InputCounts.OrderBy(x => x.Key))
            {
                Console.WriteLine($"  {key} -> {count}");
            }

            Console.WriteLine($"  Input-producing collections = {ReportingDeviceIds.Count}");

            Console.WriteLine();
            Console.WriteLine("MARKER SEGMENT SUMMARY");
            foreach ((string markerKey, int count) in MarkerReportCounts.OrderBy(x => x.Key))
            {
                MarkerChangedReportCounts.TryGetValue(markerKey, out int changedCount);
                Console.WriteLine($"  {markerKey} -> Total={count}, Changed={changedCount}");
            }
        }
    }

    private static bool IsTarget(ushort vid, ushort pid) =>
        (vid == LogitechVid && pid == LogitechReceiverPid) ||
        (vid == CorsairVid && pid == CorsairReceiverPid);

    private static bool LooksLikeTargetPath(string path) =>
        path.Contains("vid_046d&pid_c539", StringComparison.OrdinalIgnoreCase) ||
        path.Contains("vid_1b1c&pid_2a08", StringComparison.OrdinalIgnoreCase);

    private static string TargetName(ushort vid, ushort pid)
    {
        if (vid == LogitechVid && pid == LogitechReceiverPid)
        {
            return "Logitech G703 receiver path";
        }

        if (vid == CorsairVid && pid == CorsairReceiverPid)
        {
            return "Corsair VOID WIRELESS V2 receiver path";
        }

        return "Unknown target";
    }

    private static string ClassifyUsage(ushort usagePage)
    {
        if (usagePage >= 0xFF00)
        {
            return "Vendor-defined usage page (high-value candidate for receiver/vendor protocol)";
        }

        return usagePage switch
        {
            0x0001 => "Generic Desktop Controls",
            0x0007 => "Keyboard/Keypad",
            0x000C => "Consumer",
            _ => "Standard/other usage page"
        };
    }

    private static IEnumerable<string> ParseMultiSz(char[] buffer)
    {
        string all = new(buffer);
        return all.Split('\0', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }

    private static string? GetHidString(SafeFileHandle handle, HidStringKind kind)
    {
        byte[] buffer = new byte[512];
        bool success = kind switch
        {
            HidStringKind.Product => HidD_GetProductString(handle, buffer, buffer.Length),
            HidStringKind.Manufacturer => HidD_GetManufacturerString(handle, buffer, buffer.Length),
            HidStringKind.SerialNumber => HidD_GetSerialNumberString(handle, buffer, buffer.Length),
            _ => false
        };

        if (!success)
        {
            return null;
        }

        string text = Encoding.Unicode.GetString(buffer);
        int terminator = text.IndexOf('\0');
        return (terminator >= 0 ? text[..terminator] : text).Trim();
    }

    private static byte[] ReadBytes(IBuffer buffer)
    {
        byte[] bytes = new byte[(int)buffer.Length];
        using DataReader reader = DataReader.FromBuffer(buffer);
        reader.ReadBytes(bytes);
        return bytes;
    }

    private static string Hex(byte[] data, int offset, int count) =>
        Convert.ToHexString(data.AsSpan(offset, count));

    private static string DescribeChanges(byte[] previous, byte[] current)
    {
        List<string> changes = new();
        int commonLength = Math.Min(previous.Length, current.Length);

        for (int i = 0; i < commonLength; i++)
        {
            if (previous[i] != current[i])
            {
                changes.Add($"[{i}] {previous[i]:X2}->{current[i]:X2}");
            }
        }

        if (previous.Length != current.Length)
        {
            changes.Add($"Length {previous.Length}->{current.Length}");
        }

        return changes.Count == 0 ? "(none)" : string.Join(", ", changes);
    }

    private static string Empty(string? value) => string.IsNullOrWhiteSpace(value) ? "(empty)" : value;

    private enum HidStringKind
    {
        Product,
        Manufacturer,
        SerialNumber
    }

    private sealed record NativeHidCollection(
        string DevicePath,
        ushort VendorId,
        ushort ProductId,
        ushort VersionNumber,
        string? ProductString,
        string? ManufacturerString,
        string? SerialNumber,
        bool HasCaps,
        ushort UsagePage,
        ushort Usage,
        ushort InputReportByteLength,
        ushort OutputReportByteLength,
        ushort FeatureReportByteLength);

    private sealed record OpenedCollectionContext(
        string DeviceId,
        string? DeviceName,
        ushort VendorId,
        ushort ProductId,
        ushort UsagePage,
        ushort Usage,
        ushort NativeInputReportByteLength);

    private sealed class TargetOpenStats
    {
        public int SelectorMatches { get; set; }
        public int SelectorErrors { get; set; }
        public int OpenAttempts { get; set; }
        public int OpenSuccess { get; set; }
        public int OpenNull { get; set; }
        public int OpenErrors { get; set; }
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct HIDD_ATTRIBUTES
    {
        public int Size;
        public ushort VendorID;
        public ushort ProductID;
        public ushort VersionNumber;
    }

    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct HIDP_CAPS
    {
        public ushort Usage;
        public ushort UsagePage;
        public ushort InputReportByteLength;
        public ushort OutputReportByteLength;
        public ushort FeatureReportByteLength;
        public fixed ushort Reserved[17];
        public ushort NumberLinkCollectionNodes;
        public ushort NumberInputButtonCaps;
        public ushort NumberInputValueCaps;
        public ushort NumberInputDataIndices;
        public ushort NumberOutputButtonCaps;
        public ushort NumberOutputValueCaps;
        public ushort NumberOutputDataIndices;
        public ushort NumberFeatureButtonCaps;
        public ushort NumberFeatureValueCaps;
        public ushort NumberFeatureDataIndices;
    }

    [DllImport("hid.dll")]
    private static extern void HidD_GetHidGuid(out Guid hidGuid);

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_GetAttributes(SafeFileHandle hidDeviceObject, ref HIDD_ATTRIBUTES attributes);

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_GetPreparsedData(SafeFileHandle hidDeviceObject, out IntPtr preparsedData);

    [DllImport("hid.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_FreePreparsedData(IntPtr preparsedData);

    [DllImport("hid.dll")]
    private static extern int HidP_GetCaps(IntPtr preparsedData, out HIDP_CAPS capabilities);

    [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_GetProductString(SafeFileHandle hidDeviceObject, byte[] buffer, int bufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_GetManufacturerString(SafeFileHandle hidDeviceObject, byte[] buffer, int bufferLength);

    [DllImport("hid.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool HidD_GetSerialNumberString(SafeFileHandle hidDeviceObject, byte[] buffer, int bufferLength);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern uint CM_Get_Device_Interface_List_SizeW(
        out uint bufferLength,
        ref Guid interfaceClassGuid,
        string? deviceId,
        uint flags);

    [DllImport("cfgmgr32.dll", CharSet = CharSet.Unicode)]
    private static extern uint CM_Get_Device_Interface_ListW(
        ref Guid interfaceClassGuid,
        string? deviceId,
        [Out] char[] buffer,
        uint bufferLength,
        uint flags);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern SafeFileHandle CreateFileW(
        string fileName,
        uint desiredAccess,
        uint shareMode,
        IntPtr securityAttributes,
        uint creationDisposition,
        uint flagsAndAttributes,
        IntPtr templateFile);
}
