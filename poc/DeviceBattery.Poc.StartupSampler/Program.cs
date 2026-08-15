using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;

Dictionary<string, string> options = ParseOptions(args);
if (!options.TryGetValue("exe", out string? exe) || !File.Exists(exe))
    return Usage("--exe must reference an existing executable.");

int iterations = ReadInt(options, "iterations", 10, 1, 100);
int timeoutMs = ReadInt(options, "timeout-ms", 10000, 1000, 60000);
int shutdownTimeoutMs = ReadInt(options, "shutdown-timeout-ms", 15000, 1000, 60000);
string stage = options.GetValueOrDefault("stage", "STARTUP");
string executableArguments = options.GetValueOrDefault("arguments", string.Empty);
string outputDirectory = options.GetValueOrDefault(
    "output", Path.Combine(Environment.CurrentDirectory, "artifacts", "startup"));

Directory.CreateDirectory(outputDirectory);
string stamp = DateTimeOffset.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
string csvPath = Path.GetFullPath(Path.Combine(outputDirectory, $"startup-{stage}-{stamp}.csv"));
var samples = new List<double>();

Console.WriteLine("Gate 4 POC-E04 Startup Sampler");
Console.WriteLine($"Executable={Path.GetFullPath(exe)}");
Console.WriteLine($"Iterations={iterations}, ReadyTimeout={timeoutMs}ms");
Console.WriteLine($"ShutdownTimeout={shutdownTimeoutMs}ms");
Console.WriteLine($"Arguments={executableArguments}");
Console.WriteLine("Readiness=first visible top-level window owned by target PID\n");

using var writer = new StreamWriter(csvPath, false, new System.Text.UTF8Encoding(false));
writer.WriteLine("Iteration,ReadyMilliseconds,Result");

for (int i = 1; i <= iterations; i++)
{
    using Process? process = Process.Start(new ProcessStartInfo
    {
        FileName = Path.GetFullPath(exe),
        Arguments = executableArguments,
        UseShellExecute = true
    });
    if (process is null)
    {
        writer.WriteLine($"{i},,START_FAILED");
        continue;
    }

    var stopwatch = Stopwatch.StartNew();
    IntPtr window = IntPtr.Zero;
    while (stopwatch.ElapsedMilliseconds < timeoutMs && !process.HasExited)
    {
        window = NativeMethods.FindFirstVisibleWindow(process.Id);
        if (window != IntPtr.Zero) break;
        await Task.Delay(10);
    }
    stopwatch.Stop();
    bool ready = window != IntPtr.Zero;

    if (ready)
    {
        samples.Add(stopwatch.Elapsed.TotalMilliseconds);
        writer.WriteLine($"{i},{stopwatch.Elapsed.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture)},READY");
        Console.WriteLine($"[{i:00}] READY {stopwatch.Elapsed.TotalMilliseconds:F1} ms");
    }
    else
    {
        writer.WriteLine($"{i},,TIMEOUT");
        Console.WriteLine($"[{i:00}] TIMEOUT");
    }
    writer.Flush();

    if (!process.HasExited)
    {
        if (window != IntPtr.Zero)
            NativeMethods.PostMessage(window, NativeMethods.WmClose, IntPtr.Zero, IntPtr.Zero);
        if (!process.WaitForExit(shutdownTimeoutMs))
        {
            Console.WriteLine($"[{i:00}] CloseMainWindow did not exit; stop the run and inspect the POC.");
            return 4;
        }
    }
    await Task.Delay(500);
}

if (samples.Count == 0)
{
    Console.WriteLine("RESULT = INCOMPLETE (no ready samples)");
    return 3;
}

samples.Sort();
double percentile95 = samples[(int)Math.Ceiling(samples.Count * 0.95) - 1];
Console.WriteLine("\nSUMMARY");
Console.WriteLine($"Ready={samples.Count}/{iterations}");
Console.WriteLine($"Average={samples.Average():F1} ms");
Console.WriteLine($"Min={samples.Min():F1} ms, Max={samples.Max():F1} ms, P95={percentile95:F1} ms");
Console.WriteLine($"CSV={csvPath}");
Console.WriteLine($"RESULT = {(samples.Count == iterations ? "PASS" : "PASS WITH LIMITATION")}");
return samples.Count == iterations ? 0 : 2;

static Dictionary<string, string> ParseOptions(string[] arguments)
{
    var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    for (int i = 0; i < arguments.Length; i++)
    {
        if (!arguments[i].StartsWith("--", StringComparison.Ordinal) || i + 1 >= arguments.Length) continue;
        result[arguments[i][2..]] = arguments[++i];
    }
    return result;
}

static int ReadInt(Dictionary<string, string> options, string key, int fallback, int min, int max)
{
    if (!options.TryGetValue(key, out string? text) || !int.TryParse(text, out int value)) return fallback;
    if (value < min || value > max) throw new ArgumentOutOfRangeException(key);
    return value;
}

static int Usage(string error)
{
    Console.Error.WriteLine(error);
    Console.Error.WriteLine("Usage: --exe <path> [--arguments text] [--stage name] [--iterations 10] [--timeout-ms 10000] [--shutdown-timeout-ms 15000]");
    return 1;
}

internal static class NativeMethods
{
    public const uint WmClose = 0x0010;
    private delegate bool EnumWindowsProc(IntPtr window, IntPtr parameter);

    public static IntPtr FindFirstVisibleWindow(int processId)
    {
        IntPtr found = IntPtr.Zero;
        EnumWindows((window, _) =>
        {
            GetWindowThreadProcessId(window, out uint ownerProcessId);
            if (ownerProcessId == (uint)processId && IsWindowVisible(window))
            {
                found = window;
                return false;
            }
            return true;
        }, IntPtr.Zero);
        return found;
    }

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc callback, IntPtr parameter);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr window);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr window, out uint processId);

    [DllImport("user32.dll")]
    public static extern bool PostMessage(IntPtr window, uint message, IntPtr wParam, IntPtr lParam);
}
