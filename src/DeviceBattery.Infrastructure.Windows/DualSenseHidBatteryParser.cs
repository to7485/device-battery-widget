using DeviceBattery.Domain;

namespace DeviceBattery.Infrastructure.Windows;

public sealed class DualSenseHidBatteryParser : IHidBatteryParser
{
    public const string ProviderId = "DualSenseHid";
    private const int StatusOffsetInCommonReport = 52;

    public bool TryParse(
        ushort reportId,
        ReadOnlySpan<byte> report,
        DateTimeOffset observedAt,
        out BatteryObservation observation)
    {
        observation = null!;
        if (!TryResolveLayout(reportId, report, out int commonStart, out DualSenseReportLayout layout))
            return false;

        int statusOffset = commonStart + StatusOffsetInCommonReport;
        if ((uint)statusOffset >= (uint)report.Length)
            return false;

        byte status = report[statusOffset];
        int bucket = status & 0x0F;
        int chargingCode = status >> 4;
        if (bucket > 10 || chargingCode is not (0x0 or 0x1 or 0x2))
            return false;

        bool full = chargingCode == 0x2 || bucket == 10;
        int percent = full ? 100 : bucket * 10 + 5;
        ChargingState charging = chargingCode == 0x1
            ? ChargingState.Charging
            : ChargingState.NotCharging;
        BatteryPrecision precision = full
            ? BatteryPrecision.ExactPercent
            : BatteryPrecision.TenPercentBucket;

        BatteryState battery = BatteryState.Available(
            percent,
            charging,
            precision,
            observedAt,
            ProviderId);
        observation = new(battery, status, statusOffset, layout);
        return true;
    }

    private static bool TryResolveLayout(
        ushort reportId,
        ReadOnlySpan<byte> report,
        out int commonStart,
        out DualSenseReportLayout layout)
    {
        // The tested WinRT Bluetooth stack exposes a 78-byte packet as
        // Report.Id 0x01 and Data[0] 0x01. Length must therefore win over ID.
        if (report.Length >= 78 && reportId is 0x31 or 0x01)
        {
            commonStart = 2;
            layout = DualSenseReportLayout.BluetoothFullReport;
            return true;
        }

        if (reportId == 0x31 && report.Length >= 77)
        {
            commonStart = 1;
            layout = DualSenseReportLayout.BluetoothPayload;
            return true;
        }

        if (reportId == 0x01 && report.Length >= 64 && report[0] == 0x01)
        {
            commonStart = 1;
            layout = DualSenseReportLayout.UsbFullReport;
            return true;
        }

        if (reportId == 0x01 && report.Length >= 63)
        {
            commonStart = 0;
            layout = DualSenseReportLayout.UsbPayload;
            return true;
        }

        commonStart = 0;
        layout = default;
        return false;
    }
}
