# DeviceBattery.Poc.DualSenseHidBatteryProbe

Gate 4 POC-B03-2: DualSense HID input-report battery probe.

## Why this exists

`Windows.Gaming.Input` successfully recognized the Bluetooth DualSense (VID `054C`, PID `0CE6`) but both `RawGameController.TryGetBatteryReport()` and `Gamepad.TryGetBatteryReport()` returned `null` in the validated POC environment.

This project therefore tests the HID input-report fallback.

## Safety / scope

- Read-only HID open (`FileAccessMode.Read`).
- Does not send output reports.
- Does not send feature reports.
- Logs the first raw input report layout and only prints subsequent battery output when the raw status byte changes.

## Technical basis

The upstream Linux `hid-playstation` driver, whose DualSense driver header is copyright Sony Interactive Entertainment, defines:

- Bluetooth full input report ID: `0x31`, total size 78 bytes.
- USB full input report ID: `0x01`, total size 64 bytes.
- Battery capacity: lower nibble of DualSense `status[0]`.
- Charging status: upper nibble of DualSense `status[0]`.
- Battery capacity unit represents a 10% bucket. The upstream driver maps buckets to midpoint values (5%, 15%, ... 95%, 100%).

This POC handles both possible WinRT buffer layouts: report ID included in `HidInputReport.Data` or exposed separately through `HidInputReport.Id`.

## Run

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.DualSenseHidBatteryProbe
dotnet clean
dotnet run --project .\DeviceBattery.Poc.DualSenseHidBatteryProbe.csproj
```

Keep DualSense connected through Bluetooth. If no input report appears, move an analog stick or press a button.

Expected success shape:

```text
DUALSENSE BATTERY SAMPLE
ReportId         = 0x31
StatusByte       = 0x??
BatteryBucketRaw = ?
ChargingCodeRaw  = 0x?
EstimatedPercent = ??%
ChargingState    = ...
```

Send the complete output back for POC-B03-2 classification.
