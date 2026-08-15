using System.Threading.Channels;

namespace DeviceBattery.Application;

public interface IBatteryProvider : IAsyncDisposable
{
    string ProviderId { get; }

    Task RunAsync(
        ChannelWriter<ProviderEvent> events,
        CancellationToken cancellationToken);
}
