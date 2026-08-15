using System.Threading.Channels;

namespace DeviceBattery.Application;

public sealed class DeviceStateCoordinator
{
    private readonly Channel<ProviderEvent> mailbox;
    private readonly DeviceStateReducer reducer;
    private readonly Func<ReductionResult, ValueTask> projection;
    private readonly Func<ProviderEvent, Exception, ValueTask> errorSink;
    private int runState;

    public DeviceStateCoordinator(
        DeviceStateReducer reducer,
        Func<ReductionResult, ValueTask>? projection = null,
        Func<ProviderEvent, Exception, ValueTask>? errorSink = null)
    {
        this.reducer = reducer ?? throw new ArgumentNullException(nameof(reducer));
        this.projection = projection ?? (_ => ValueTask.CompletedTask);
        this.errorSink = errorSink ?? ((_, _) => ValueTask.CompletedTask);
        mailbox = Channel.CreateUnbounded<ProviderEvent>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
    }

    public ChannelWriter<ProviderEvent> Events => mailbox.Writer;

    public long ProcessedCount { get; private set; }
    public long FaultedCount { get; private set; }

    public bool TryPublish(ProviderEvent providerEvent)
    {
        ArgumentNullException.ThrowIfNull(providerEvent);
        return mailbox.Writer.TryWrite(providerEvent);
    }

    public ValueTask PublishAsync(
        ProviderEvent providerEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(providerEvent);
        return mailbox.Writer.WriteAsync(providerEvent, cancellationToken);
    }

    public bool Complete(Exception? error = null) => mailbox.Writer.TryComplete(error);

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        if (Interlocked.CompareExchange(ref runState, 1, 0) != 0)
            throw new InvalidOperationException("The coordinator can only be run once.");

        try
        {
            await foreach (ProviderEvent providerEvent in
                mailbox.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
            {
                try
                {
                    ReductionResult result = reducer.Apply(providerEvent);
                    ProcessedCount++;
                    await projection(result).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    FaultedCount++;
                    try
                    {
                        await errorSink(providerEvent, ex).ConfigureAwait(false);
                    }
                    catch (Exception sinkException) when (sinkException is not OperationCanceledException)
                    {
                        // Diagnostics must not stop state serialization.
                    }
                }
            }
        }
        finally
        {
            Interlocked.Exchange(ref runState, 2);
        }
    }
}
