using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

const ushort UsagePage = 0x0001;
const ushort UsageId = 0x0005;
const ushort Vid = 0x054C;
const ushort Pid = 0x0CE6;

Console.WriteLine("Gate 4 POC-C03 — DualSense Event Continuity Probe");
Console.WriteLine("Read-only: targeted selector, FileAccessMode.Read, no Output/Feature/vendor command.");
Console.WriteLine("Bluetooth 78-byte reports only. Candidate sequence offsets 7 and 8 are compared.\n");

string selector = HidDevice.GetDeviceSelector(UsagePage, UsageId, Vid, Pid);
DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(selector);
var sessions = new List<ContinuitySession>();

foreach (DeviceInformation info in devices)
{
    if (!info.Id.Contains("{00001124-0000-1000-8000-00805f9b34fb}", StringComparison.OrdinalIgnoreCase))
        continue;

    HidDevice? hid = await HidDevice.FromIdAsync(info.Id, FileAccessMode.Read);
    if (hid is null)
    {
        Console.WriteLine($"[OPEN FAILED] {info.Id}");
        continue;
    }

    var session = new ContinuitySession(info.Id, hid);
    sessions.Add(session);
    Console.WriteLine($"[OPEN SUCCESS] Name={info.Name}, Id={info.Id}");
}

if (sessions.Count == 0)
{
    Console.WriteLine("RESULT = INCOMPLETE (no Bluetooth DualSense HID session opened)");
    return 2;
}

Console.WriteLine("Commands: S = summary, Q = summary and quit");
bool quit = false;
while (!quit)
{
    ConsoleKey key = Console.ReadKey(intercept: true).Key;
    switch (key)
    {
        case ConsoleKey.S:
            foreach (ContinuitySession session in sessions) session.PrintSummary();
            break;
        case ConsoleKey.Q:
            foreach (ContinuitySession session in sessions) session.PrintSummary();
            quit = true;
            break;
    }
}

foreach (ContinuitySession session in sessions) session.Dispose();
Console.WriteLine($"[{DateTimeOffset.Now:HH:mm:ss.fff}] [CLEANUP] Input handlers and HID devices disposed.");
return 0;

internal sealed class ContinuitySession : IDisposable
{
    private readonly object _lock = new();
    private readonly HidDevice _device;
    private readonly SequenceTracker _offset7 = new("Offset7Raw", 256);
    private readonly SequenceTracker _offset7High6 = new("Offset7High6(value>>2)", 64);
    private readonly SequenceTracker _offset7Bits2To5 = new("Offset7Bits2To5((value>>2)&0x0F)", 16);
    private readonly SequenceTracker _offset8 = new("Offset8Raw", 256);
    private long _reports;
    private long _unsupportedShapes;
    private DateTimeOffset? _firstAt;
    private DateTimeOffset? _lastAt;
    private double _maxGapMs;
    private bool _disposed;

    public ContinuitySession(string id, HidDevice device)
    {
        Id = id;
        _device = device;
        _device.InputReportReceived += OnInputReportReceived;
    }

    public string Id { get; }

    private void OnInputReportReceived(HidDevice sender, HidInputReportReceivedEventArgs args)
    {
        byte[] data = ReadBytes(args.Report.Data);
        if (data.Length < 78)
        {
            lock (_lock) _unsupportedShapes++;
            return;
        }

        DateTimeOffset now = DateTimeOffset.Now;
        lock (_lock)
        {
            if (_lastAt is not null)
                _maxGapMs = Math.Max(_maxGapMs, (now - _lastAt.Value).TotalMilliseconds);
            _firstAt ??= now;
            _lastAt = now;
            _reports++;
            _offset7.Observe(data[7]);
            _offset7High6.Observe((byte)(data[7] >> 2));
            _offset7Bits2To5.Observe((byte)((data[7] >> 2) & 0x0F));
            _offset8.Observe(data[8]);
        }
    }

    public void PrintSummary()
    {
        lock (_lock)
        {
            double seconds = _firstAt is null || _lastAt is null ? 0 : (_lastAt.Value - _firstAt.Value).TotalSeconds;
            Console.WriteLine($"\n[{DateTimeOffset.Now:HH:mm:ss.fff}] CONTINUITY SUMMARY");
            Console.WriteLine($"Id={Id}");
            Console.WriteLine($"Reports={_reports}, UnsupportedShapes={_unsupportedShapes}, Duration={seconds:F3}s, MaxInterArrivalGap={_maxGapMs:F3}ms");
            _offset7.Print();
            _offset7High6.Print();
            _offset7Bits2To5.Print();
            _offset8.Print();
            Console.WriteLine("Interpretation: the real counter candidate should have a dominant Sequential ratio; MissingEstimate is valid only for that candidate.");
        }
    }

    public void Dispose()
    {
        lock (_lock)
        {
            if (_disposed) return;
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

internal sealed class SequenceTracker(string name, int modulus)
{
    private byte? _previous;
    private long _transitions;
    private long _sequential;
    private long _duplicates;
    private long _gapTransitions;
    private long _missingEstimate;
    private long _resetsOrLargeJumps;

    public void Observe(byte current)
    {
        if (_previous is null)
        {
            _previous = current;
            return;
        }

        int delta = (current - _previous.Value + modulus) % modulus;
        _transitions++;
        if (delta == 0) _duplicates++;
        else if (delta == 1) _sequential++;
        else if (delta <= Math.Max(2, modulus / 4))
        {
            _gapTransitions++;
            _missingEstimate += delta - 1;
        }
        else _resetsOrLargeJumps++;
        _previous = current;
    }

    public void Print()
    {
        double sequentialRatio = _transitions == 0 ? 0 : _sequential * 100d / _transitions;
        Console.WriteLine($"{name}: Transitions={_transitions}, Sequential={_sequential} ({sequentialRatio:F3}%), Duplicates={_duplicates}, GapTransitions={_gapTransitions}, MissingEstimate={_missingEstimate}, ResetsOrLargeJumps={_resetsOrLargeJumps}");
    }
}
