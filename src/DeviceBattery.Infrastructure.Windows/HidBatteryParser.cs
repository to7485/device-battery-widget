using DeviceBattery.Domain;

namespace DeviceBattery.Infrastructure.Windows;

public interface IHidBatteryParser
{
    bool TryParse(
        ushort reportId,
        ReadOnlySpan<byte> report,
        DateTimeOffset observedAt,
        out BatteryObservation observation);
}

public sealed record BatteryObservation(
    BatteryState Battery,
    byte RawStatusByte,
    int StatusOffset,
    DualSenseReportLayout Layout);

public enum DualSenseReportLayout
{
    BluetoothFullReport,
    BluetoothPayload,
    UsbFullReport,
    UsbPayload
}
