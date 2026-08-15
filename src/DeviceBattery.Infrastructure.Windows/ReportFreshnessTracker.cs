namespace DeviceBattery.Infrastructure.Windows;

public readonly record struct FreshnessEvaluation(bool ExpiredNow, bool DormantNow);

public sealed class ReportFreshnessTracker
{
    private readonly TimeProvider timeProvider;
    private readonly TimeSpan unknownAfter;
    private readonly TimeSpan dormantAfter;
    private long lastValidTimestamp;
    private bool hasValidReport;
    private bool expired;
    private bool dormant;

    public ReportFreshnessTracker(
        TimeProvider timeProvider,
        TimeSpan unknownAfter,
        TimeSpan dormantAfter)
    {
        this.timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
        if (unknownAfter <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(unknownAfter));
        if (dormantAfter <= unknownAfter)
            throw new ArgumentOutOfRangeException(nameof(dormantAfter));

        this.unknownAfter = unknownAfter;
        this.dormantAfter = dormantAfter;
        lastValidTimestamp = timeProvider.GetTimestamp();
    }

    public bool MarkValidReport()
    {
        bool recovered = !hasValidReport || expired || dormant;
        hasValidReport = true;
        expired = false;
        dormant = false;
        lastValidTimestamp = timeProvider.GetTimestamp();
        return recovered;
    }

    public FreshnessEvaluation Evaluate()
    {
        TimeSpan elapsed = timeProvider.GetElapsedTime(lastValidTimestamp, timeProvider.GetTimestamp());
        bool expiredNow = !expired && elapsed >= unknownAfter;
        bool dormantNow = !dormant && elapsed >= dormantAfter;
        expired |= expiredNow;
        dormant |= dormantNow;
        return new(expiredNow, dormantNow);
    }
}
