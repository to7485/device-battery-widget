Console.WriteLine("Gate 4 POC-B06 — Event-first / Poll Fallback Policy");
Console.WriteLine("Scope: deterministic policy verification; no device access or HID commands.\n");

DateTimeOffset t0 = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);
var cases = new[]
{
    new PolicyCase("DualSense selects event-only",
        () => MonitorPolicy.Select(new(true, false)) == MonitorMode.EventOnly),
    new PolicyCase("Readable non-event provider selects poll fallback",
        () => MonitorPolicy.Select(new(false, true)) == MonitorMode.PollFallback),
    new PolicyCase("Provider with event and read still prefers event",
        () => MonitorPolicy.Select(new(true, true)) == MonitorMode.EventOnly),
    new PolicyCase("Unsupported provider selects none",
        () => MonitorPolicy.Select(new(false, false)) == MonitorMode.None),
    new PolicyCase("DualSense timer never invokes device read",
        () =>
        {
            var monitor = new PolicyMonitor(new(true, false), t0);
            monitor.OnBatteryEvent(0x05, t0.AddSeconds(1));
            monitor.OnTimer(t0.AddSeconds(30));
            return monitor.DeviceReadAttempts == 0;
        }),
    new PolicyCase("Timeout clears stale percentage",
        () =>
        {
            var monitor = new PolicyMonitor(new(true, false), t0);
            monitor.OnBatteryEvent(0x05, t0.AddSeconds(1));
            monitor.OnTimer(t0.AddSeconds(12));
            return monitor.State is { Availability: Availability.Unknown, Percent: null };
        }),
    new PolicyCase("Event recovery restores available state",
        () =>
        {
            var monitor = new PolicyMonitor(new(true, false), t0);
            monitor.OnBatteryEvent(0x05, t0.AddSeconds(1));
            monitor.OnTimer(t0.AddSeconds(12));
            monitor.OnBatteryEvent(0x06, t0.AddSeconds(13));
            return monitor.State is { Availability: Availability.Available, Percent: 65 };
        }),
    new PolicyCase("Poll fallback invokes only readable provider",
        () =>
        {
            var monitor = new PolicyMonitor(new(false, true), t0);
            monitor.OnTimer(t0.AddMinutes(5));
            return monitor.DeviceReadAttempts == 1;
        })
};

int passed = 0;
foreach (PolicyCase test in cases)
{
    bool result = test.Run();
    Console.WriteLine($"[{(result ? "PASS" : "FAIL")}] {test.Name}");
    if (result) passed++;
}

Console.WriteLine($"\nRESULT = {(passed == cases.Length ? "PASS" : "FAIL")} ({passed}/{cases.Length})");
return passed == cases.Length ? 0 : 1;

internal sealed record PolicyCase(string Name, Func<bool> Run);
internal sealed record ProviderCapabilities(bool HasBatteryEvent, bool HasReliableReadEndpoint);
internal enum MonitorMode { EventOnly, PollFallback, None }
internal enum Availability { Available, Unknown }
internal sealed record State(Availability Availability, int? Percent, DateTimeOffset UpdatedAt);

internal static class MonitorPolicy
{
    public static MonitorMode Select(ProviderCapabilities capabilities) => capabilities switch
    {
        { HasBatteryEvent: true } => MonitorMode.EventOnly,
        { HasReliableReadEndpoint: true } => MonitorMode.PollFallback,
        _ => MonitorMode.None
    };
}

internal sealed class PolicyMonitor
{
    private static readonly TimeSpan PocFreshnessTimeout = TimeSpan.FromSeconds(10);
    private readonly MonitorMode _mode;
    private DateTimeOffset? _lastEventAt;

    public PolicyMonitor(ProviderCapabilities capabilities, DateTimeOffset createdAt)
    {
        _mode = MonitorPolicy.Select(capabilities);
        State = new(Availability.Unknown, null, createdAt);
    }

    public State State { get; private set; }
    public int DeviceReadAttempts { get; private set; }

    public void OnBatteryEvent(byte status, DateTimeOffset at)
    {
        int bucket = status & 0x0F;
        if (bucket > 10) return;
        State = new(Availability.Available, bucket == 10 ? 100 : bucket * 10 + 5, at);
        _lastEventAt = at;
    }

    public void OnTimer(DateTimeOffset at)
    {
        if (_mode == MonitorMode.PollFallback)
        {
            DeviceReadAttempts++;
            return;
        }

        if (_mode == MonitorMode.EventOnly &&
            _lastEventAt is not null &&
            at - _lastEventAt >= PocFreshnessTimeout)
        {
            State = new(Availability.Unknown, null, at);
        }
    }
}
