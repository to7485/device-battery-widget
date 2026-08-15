using DeviceBattery.Application;
using DeviceBattery.Domain;
using DeviceBattery.Presentation.Wpf;

DateTimeOffset now = new(2026, 8, 15, 0, 0, 0, TimeSpan.Zero);
DeviceKey key = new("DualSenseHid", "device-1");
var specs = new (string Name, Action Run)[]
{
    ("Widget starts empty and topmost off", () => { var vm = new WidgetViewModel(); Equal(true, vm.IsEmpty); Equal(false, vm.IsTopmost); }),
    ("Waiting has no stale percent", () => { var vm = new WidgetViewModel(); vm.Apply(Applied(Snapshot(BatteryState.Unknown(now, key.ProviderId, "Waiting for first valid battery report"), 1))); Equal("—", vm.Devices[0].BatteryText); Equal("조회 중", vm.Devices[0].StatusText); }),
    ("Estimated battery is marked", () => { var card = Card(BatteryState.Available(15, ChargingState.NotCharging, BatteryPrecision.TenPercentBucket, now, key.ProviderId), 1); Equal("약 15%", card.BatteryText); Equal(15, card.GaugePercent); }),
    ("Charging state is projected", () => { var card = Card(BatteryState.Available(25, ChargingState.Charging, BatteryPrecision.TenPercentBucket, now, key.ProviderId), 1); Equal(true, card.IsCharging); Equal("⚡ 충전 중", card.StatusText); }),
    ("Unknown clears gauge and percent", () => { var card = Card(BatteryState.Available(15, ChargingState.NotCharging, BatteryPrecision.TenPercentBucket, now, key.ProviderId), 1); card.Apply(Snapshot(BatteryState.Unknown(now, key.ProviderId, "freshness expired"), 2)); Equal(0, card.GaugePercent); Equal("—", card.BatteryText); }),
    ("Older revision is ignored", () => { var card = Card(BatteryState.Available(25, ChargingState.Charging, BatteryPrecision.TenPercentBucket, now, key.ProviderId), 2); Equal(false, card.Apply(Snapshot(BatteryState.Available(15, ChargingState.NotCharging, BatteryPrecision.TenPercentBucket, now, key.ProviderId), 1))); Equal("약 25%", card.BatteryText); }),
    ("Dormant snapshot restores empty state", () => { var vm = new WidgetViewModel(); vm.Apply(Applied(Snapshot(BatteryState.Unknown(now, key.ProviderId, "waiting"), 1))); vm.Apply(Applied(Snapshot(BatteryState.Unknown(now, key.ProviderId, "offline"), 2, false))); Equal(true, vm.IsEmpty); })
    ,("USB indicator takes priority over Bluetooth", () => { var vm = new WidgetViewModel(); DeviceKey bt = new("DualSenseHid", "BT-one"); DeviceKey usb = new("DualSenseHid", "USB-one"); BatteryState battery = BatteryState.Available(15, ChargingState.NotCharging, BatteryPrecision.TenPercentBucket, now, key.ProviderId); vm.Apply(Applied(new(bt, "DualSense Wireless Controller", battery, true, 1))); vm.Apply(Applied(new(usb, "DualSense Controller (USB)", battery, true, 1))); Equal(1, vm.Devices.Count); Equal(usb, vm.Devices[0].Key); })
};

int passed = 0;
foreach ((string name, Action run) in specs) { try { run(); Console.WriteLine($"[PASS] {name}"); passed++; } catch (Exception ex) { Console.WriteLine($"[FAIL] {name}: {ex.Message}"); } }
Console.WriteLine($"RESULT = {(passed == specs.Length ? "PASS" : "FAIL")} ({passed}/{specs.Length})");
return passed == specs.Length ? 0 : 1;

DeviceSnapshot Snapshot(BatteryState battery, long revision, bool visible = true) => new(key, "DualSense Wireless Controller", battery, visible, revision);
DeviceCardViewModel Card(BatteryState battery, long revision) => new(Snapshot(battery, revision));
static ReductionResult Applied(DeviceSnapshot snapshot) => new(ReductionOutcome.Applied, snapshot);
static void Equal<T>(T expected, T actual) { if (!EqualityComparer<T>.Default.Equals(expected, actual)) throw new InvalidOperationException($"Expected {expected}, actual {actual}."); }
