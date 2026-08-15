using System.Security.Cryptography;
using System.Text;
using DeviceBattery.Domain;

namespace DeviceBattery.Infrastructure.Windows;

public static class DualSenseDeviceIdentity
{
    public const string BluetoothHidServiceId = "{00001124-0000-1000-8000-00805f9b34fb}";

    public static bool IsBluetoothEndpoint(string deviceId) =>
        !string.IsNullOrWhiteSpace(deviceId) &&
        deviceId.Contains(BluetoothHidServiceId, StringComparison.OrdinalIgnoreCase);

    public static DeviceKey CreateKey(string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        byte[] normalized = Encoding.UTF8.GetBytes(deviceId.Trim().ToUpperInvariant());
        string stableId = Convert.ToHexString(SHA256.HashData(normalized))[..24];
        return new(DualSenseHidBatteryParser.ProviderId, stableId);
    }
}
