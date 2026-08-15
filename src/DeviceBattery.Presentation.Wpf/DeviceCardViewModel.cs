using DeviceBattery.Domain;

namespace DeviceBattery.Presentation.Wpf;

public sealed class DeviceCardViewModel : ObservableObject
{
    private string displayName = "";
    private string batteryText = "";
    private string statusText = "";
    private int gaugePercent;
    private bool isAvailable;
    private bool isCharging;
    private long revision;

    public DeviceCardViewModel(DeviceSnapshot snapshot) { Key = snapshot.Key; Apply(snapshot); }
    public DeviceKey Key { get; }
    public string DisplayName { get => displayName; private set => SetProperty(ref displayName, value); }
    public string BatteryText { get => batteryText; private set => SetProperty(ref batteryText, value); }
    public string StatusText { get => statusText; private set => SetProperty(ref statusText, value); }
    public int GaugePercent { get => gaugePercent; private set => SetProperty(ref gaugePercent, value); }
    public bool IsAvailable { get => isAvailable; private set => SetProperty(ref isAvailable, value); }
    public bool IsCharging { get => isCharging; private set => SetProperty(ref isCharging, value); }
    public long Revision { get => revision; private set => SetProperty(ref revision, value); }

    public bool Apply(DeviceSnapshot snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.Key != Key) throw new ArgumentException("Snapshot key does not match the card.", nameof(snapshot));
        if (snapshot.Revision <= Revision) return false;
        BatteryState battery = snapshot.Battery;
        DisplayName = snapshot.DisplayName;
        Revision = snapshot.Revision;
        IsAvailable = battery.Availability == BatteryAvailability.Available;
        IsCharging = battery.Charging == ChargingState.Charging;
        GaugePercent = battery.Percent ?? 0;
        BatteryText = battery.Availability switch
        {
            BatteryAvailability.Available when battery.IsEstimated => $"약 {battery.Percent}%",
            BatteryAvailability.Available => $"{battery.Percent}%",
            BatteryAvailability.Unsupported => "지원하지 않음",
            _ => "—"
        };
        StatusText = FormatStatus(battery);
        return true;
    }

    private static string FormatStatus(BatteryState battery)
    {
        if (battery.Availability == BatteryAvailability.Unknown)
            return battery.Reason?.StartsWith("Waiting", StringComparison.OrdinalIgnoreCase) == true ? "조회 중" : "배터리 상태 알 수 없음";
        if (battery.Availability == BatteryAvailability.Unsupported) return "배터리 정보를 지원하지 않는 장치";
        return battery.Charging == ChargingState.Charging ? "⚡ 충전 중" : "배터리 사용 중";
    }
}
