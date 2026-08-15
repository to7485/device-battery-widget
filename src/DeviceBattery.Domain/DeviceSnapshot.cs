namespace DeviceBattery.Domain;

public sealed record DeviceSnapshot(
    DeviceKey Key,
    string DisplayName,
    BatteryState Battery,
    bool IsVisible,
    long Revision)
{
    public DeviceSnapshot Validate()
    {
        if (string.IsNullOrWhiteSpace(DisplayName))
            throw new ArgumentException("Display name must not be empty.", nameof(DisplayName));
        if (Revision < 0)
            throw new ArgumentOutOfRangeException(nameof(Revision));
        return this;
    }
}
