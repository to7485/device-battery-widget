using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace DeviceBattery.Infrastructure.Windows;

public sealed class BoundedFileTraceListener : TraceListener
{
    private readonly object sync = new();
    private readonly string directory;
    private readonly TimeSpan maxAge;
    private readonly long maxTotalBytes;
    private StreamWriter? writer;
    private DateOnly writerDate;

    public BoundedFileTraceListener(string? directory = null, TimeSpan? maxAge = null, long maxTotalBytes = 10 * 1024 * 1024)
    {
        this.directory = directory ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DeviceBatteryWidget", "logs");
        this.maxAge = maxAge ?? TimeSpan.FromDays(7);
        this.maxTotalBytes = maxTotalBytes > 0 ? maxTotalBytes : throw new ArgumentOutOfRangeException(nameof(maxTotalBytes));
        Directory.CreateDirectory(this.directory);
        Prune(DateTimeOffset.UtcNow);
    }

    public override void Write(string? message) => WriteLine(message);

    public override void WriteLine(string? message)
    {
        lock (sync)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            EnsureWriter(now);
            writer!.Write(now.ToString("O", CultureInfo.InvariantCulture));
            writer.Write(' ');
            writer.WriteLine(message ?? string.Empty);
            writer.Flush();
        }
    }

    public override void Flush()
    {
        lock (sync) writer?.Flush();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (sync)
            {
                writer?.Dispose();
                writer = null;
            }
        }
        base.Dispose(disposing);
    }

    private void EnsureWriter(DateTimeOffset now)
    {
        DateOnly today = DateOnly.FromDateTime(now.UtcDateTime);
        if (writer is not null && writerDate == today) return;
        writer?.Dispose();
        Prune(now);
        string path = Path.Combine(directory, $"device-battery-{today:yyyyMMdd}.log");
        writer = new StreamWriter(path, append: true, new UTF8Encoding(false));
        writerDate = today;
    }

    private void Prune(DateTimeOffset now)
    {
        FileInfo[] files = new DirectoryInfo(directory).GetFiles("device-battery-*.log")
            .OrderByDescending(file => file.LastWriteTimeUtc)
            .ToArray();
        DateTimeOffset cutoff = now - maxAge;
        foreach (FileInfo file in files.Where(file => file.LastWriteTimeUtc < cutoff.UtcDateTime))
            TryDelete(file);

        long total = 0;
        foreach (FileInfo file in new DirectoryInfo(directory).GetFiles("device-battery-*.log").OrderByDescending(file => file.LastWriteTimeUtc))
        {
            if (total + file.Length <= maxTotalBytes) { total += file.Length; continue; }
            TryDelete(file);
        }
    }

    private static void TryDelete(FileInfo file)
    {
        try { file.Delete(); }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }
}
