namespace DeviceBattery.Application;

public interface IAutoStartService
{
    bool IsEnabled();
    void SetEnabled(bool enabled);
}
