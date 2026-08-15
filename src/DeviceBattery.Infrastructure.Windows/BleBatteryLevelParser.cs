namespace DeviceBattery.Infrastructure.Windows;

public static class BleBatteryLevelParser
{
    public static bool TryParse(ReadOnlySpan<byte> value, out int percent)
    {
        percent = 0;
        if (value.Length < 1 || value[0] > 100) return false;
        percent = value[0];
        return true;
    }
}
