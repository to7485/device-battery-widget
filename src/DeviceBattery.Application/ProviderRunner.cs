using System.Threading.Channels;

namespace DeviceBattery.Application;

public static class ProviderRunner
{
    public static async Task RunIsolatedAsync(
        IBatteryProvider provider,
        ChannelWriter<ProviderEvent> events,
        CancellationToken cancellationToken,
        Func<IBatteryProvider, Exception, ValueTask> onFailure)
    {
        ArgumentNullException.ThrowIfNull(provider);
        ArgumentNullException.ThrowIfNull(events);
        ArgumentNullException.ThrowIfNull(onFailure);
        try
        {
            await provider.RunAsync(events, cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception error)
        {
            await onFailure(provider, error).ConfigureAwait(false);
        }
    }
}
