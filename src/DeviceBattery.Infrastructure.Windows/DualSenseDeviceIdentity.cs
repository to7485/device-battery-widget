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

    public static bool IsUsbEndpoint(string deviceId) =>
        !string.IsNullOrWhiteSpace(deviceId) &&
        deviceId.Contains("HID#VID_054C&PID_0CE6", StringComparison.OrdinalIgnoreCase);

    public static bool IsSupportedEndpoint(string deviceId) =>
        IsBluetoothEndpoint(deviceId) || IsUsbEndpoint(deviceId);

    public static bool UsesReportFreshnessTimeout(string deviceId) => IsBluetoothEndpoint(deviceId);

    public static DeviceKey CreateKey(string deviceId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId);
        byte[] normalized = Encoding.UTF8.GetBytes(deviceId.Trim().ToUpperInvariant());
        string transport = IsUsbEndpoint(deviceId) ? "USB" : "BT";
        string stableId = $"{transport}-{Convert.ToHexString(SHA256.HashData(normalized))[..24]}";
        return new(DualSenseHidBatteryParser.ProviderId, stableId);
    }
}
