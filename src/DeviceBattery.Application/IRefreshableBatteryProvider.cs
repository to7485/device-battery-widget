namespace DeviceBattery.Application;

public interface IRefreshableBatteryProvider
{
    ValueTask RefreshAsync(CancellationToken cancellationToken);
}
