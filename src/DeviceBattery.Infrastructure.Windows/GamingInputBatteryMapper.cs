using DeviceBattery.Domain;
using Windows.System.Power;

namespace DeviceBattery.Infrastructure.Windows;

public static class GamingInputBatteryMapper
{
    public static bool TryCreate(
        int? remainingCapacity,
        int? fullChargeCapacity,
        BatteryStatus status,
        DateTimeOffset observedAt,
        string providerId,
        out BatteryState battery)
    {
        battery = null!;
        if (!remainingCapacity.HasValue || !fullChargeCapacity.HasValue || fullChargeCapacity.Value <= 0)
            return false;
        if (remainingCapacity.Value < 0 || remainingCapacity.Value > fullChargeCapacity.Value)
            return false;

        ChargingState charging = status switch
        {
            BatteryStatus.Charging => ChargingState.Charging,
            BatteryStatus.Discharging or BatteryStatus.Idle => ChargingState.NotCharging,
            _ => ChargingState.Unknown
        };
        if (charging == ChargingState.Unknown) return false;

        int percent = Math.Clamp(
            (int)Math.Round(remainingCapacity.Value * 100.0 / fullChargeCapacity.Value, MidpointRounding.AwayFromZero),
            0,
            100);
        battery = BatteryState.Available(percent, charging, BatteryPrecision.GranularLevel, observedAt, providerId);
        return true;
    }
}
