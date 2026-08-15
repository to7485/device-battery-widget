using System.Diagnostics;
using System.Globalization;

Dictionary<string, string> options = ParseOptions(args);
if (!TryReadInt(options, "pid", out int pid) || pid <= 0)
{
    return Usage("--pid must be a positive process ID.");
}

string stage = options.GetValueOrDefault("stage", "UNSPECIFIED");
int durationSeconds = ReadInt(options, "duration", 300, 10, 86400);
int intervalSeconds = ReadInt(options, "interval", 1, 1, 60);
string outputDirectory = options.GetValueOrDefault(
    "output",
    Path.Combine(Environment.CurrentDirectory, "artifacts", "resource"));

Process target;
try
{
    target = Process.GetProcessById(pid);
    target.Refresh();
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Cannot open PID {pid}: {ex.Message}");
    return 2;
}

Directory.CreateDirectory(outputDirectory);
string safeStage = string.Concat(stage.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '_'));
string timestamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
string csvPath = Path.GetFullPath(Path.Combine(outputDirectory, $"resource-{safeStage}-{pid}-{timestamp}.csv"));

Console.WriteLine("Gate 4 POC-E Resource Sampler");
Console.WriteLine($"Stage={stage}, PID={pid}, Process={target.ProcessName}");
Console.WriteLine($"Duration={durationSeconds}s, Interval={intervalSeconds}s, LogicalProcessors={Environment.ProcessorCount}");
Console.WriteLine($"CSV={csvPath}");
Console.WriteLine("Read-only observation: the target process is not stopped or modified.\n");

var samples = new List<ResourceSample>();
using var writer = new StreamWriter(csvPath, false, new System.Text.UTF8Encoding(false));
writer.WriteLine("Timestamp,ElapsedSeconds,CpuPercent,WorkingSetBytes,PrivateMemoryBytes,HandleCount,ThreadCount");

DateTimeOffset startedAt = DateTimeOffset.Now;
DateTimeOffset previousAt = startedAt;
TimeSpan previousCpu = target.TotalProcessorTime;
using var timer = new PeriodicTimer(TimeSpan.FromSeconds(intervalSeconds));
using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(durationSeconds));

try
{
    while (await timer.WaitForNextTickAsync(cancellation.Token))
    {
        if (target.HasExited)
        {
            Console.WriteLine("Target exited before measurement completed.");
            break;
        }

        target.Refresh();
        DateTimeOffset now = DateTimeOffset.Now;
        TimeSpan cpu = target.TotalProcessorTime;
        double wallSeconds = (now - previousAt).TotalSeconds;
        double cpuPercent = wallSeconds <= 0
            ? 0
            : (cpu - previousCpu).TotalSeconds / wallSeconds / Environment.ProcessorCount * 100.0;

        var sample = new ResourceSample(
            now,
            (now - startedAt).TotalSeconds,
            cpuPercent,
            target.WorkingSet64,
            target.PrivateMemorySize64,
            target.HandleCount,
            target.Threads.Count);
        samples.Add(sample);
        writer.WriteLine(sample.ToCsv());
        writer.Flush();

        previousAt = now;
        previousCpu = cpu;
    }
}
catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
{
}
finally
{
    target.Dispose();
}

if (samples.Count == 0)
{
    Console.WriteLine("RESULT = INCOMPLETE (no samples)");
    return 3;
}

ResourceSample first = samples[0];
ResourceSample last = samples[^1];
Console.WriteLine("\nSUMMARY");
Console.WriteLine($"Samples={samples.Count}");
Console.WriteLine($"CPU Avg={samples.Average(x => x.CpuPercent):F3}%, Max={samples.Max(x => x.CpuPercent):F3}%");
Console.WriteLine($"WorkingSet Last={ToMiB(last.WorkingSetBytes):F2} MiB, Delta={ToMiB(last.WorkingSetBytes - first.WorkingSetBytes):F2} MiB");
Console.WriteLine($"PrivateMemory Last={ToMiB(last.PrivateMemoryBytes):F2} MiB, Delta={ToMiB(last.PrivateMemoryBytes - first.PrivateMemoryBytes):F2} MiB");
Console.WriteLine($"Handles Last={last.HandleCount}, Delta={last.HandleCount - first.HandleCount}");
Console.WriteLine($"Threads Last={last.ThreadCount}, Delta={last.ThreadCount - first.ThreadCount}");
Console.WriteLine($"RESULT = COMPLETE ({samples.Count} samples)");
return 0;

static Dictionary<string, string> ParseOptions(string[] arguments)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < arguments.Length; i++)
    {
        if (!arguments[i].StartsWith("--", StringComparison.Ordinal) || i + 1 >= arguments.Length)
            continue;
        result[arguments[i][2..]] = arguments[++i];
    }
    return result;
}

static bool TryReadInt(Dictionary<string, string> options, string key, out int value)
{
    value = 0;
    return options.TryGetValue(key, out string? text) && int.TryParse(text, out value);
}

static int ReadInt(Dictionary<string, string> options, string key, int fallback, int min, int max)
{
    if (!TryReadInt(options, key, out int value)) return fallback;
    if (value < min || value > max)
        throw new ArgumentOutOfRangeException($"--{key}", $"Expected {min}..{max}.");
    return value;
}

static int Usage(string error)
{
    Console.Error.WriteLine(error);
    Console.Error.WriteLine("Usage: --pid <id> --stage <name> [--duration 300] [--interval 1] [--output <directory>]");
    return 1;
}

static double ToMiB(long bytes) => bytes / 1024d / 1024d;

internal sealed record ResourceSample(
    DateTimeOffset Timestamp,
    double ElapsedSeconds,
    double CpuPercent,
    long WorkingSetBytes,
    long PrivateMemoryBytes,
    int HandleCount,
    int ThreadCount)
{
    public string ToCsv() => string.Join(",",
        Timestamp.ToString("O", CultureInfo.InvariantCulture),
        ElapsedSeconds.ToString("F3", CultureInfo.InvariantCulture),
        CpuPercent.ToString("F6", CultureInfo.InvariantCulture),
        WorkingSetBytes.ToString(CultureInfo.InvariantCulture),
        PrivateMemoryBytes.ToString(CultureInfo.InvariantCulture),
        HandleCount.ToString(CultureInfo.InvariantCulture),
        ThreadCount.ToString(CultureInfo.InvariantCulture));
}
