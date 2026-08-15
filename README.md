# Gate 4 POC Update 04

POC-A 실장비 결과를 반영하고 POC-B Battery/Charging 검증으로 이동하기 위한 업데이트입니다.

## 다음 실행

AULA F87Pro를 Bluetooth로 연결한 상태에서:

```powershell
cd .\poc\DeviceBattery.Poc.BatteryProbe
dotnet clean
dotnet run
```

출력 전체를 저장해서 분석합니다.
주변장치 Battery Controller가 없더라도 정상적인 POC 결과입니다.

## Update 05 — POC-B02 BLE GATT Battery Probe

POC-B01에서 Windows Generic Battery Controller가 주변기기를 0개 반환하여 해당 경로를 `FAIL / NEED ALTERNATIVE`로 기록했습니다.

새 프로젝트:

```text
poc/DeviceBattery.Poc.BleBatteryProbe
```

실행:

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.BleBatteryProbe
dotnet clean
dotnet run
```

이 POC는 표준 BLE Battery Service `0x180F`와 Battery Level `0x2A19`를 읽고 Notify/Indicate 가능 여부를 확인합니다.

## Update 06 — POC-B02 PASS / POC-B03 Windows.Gaming.Input Battery Probe

AULA F87Pro Bluetooth에서 BLE GATT `0x180F -> 0x2A19` 배터리 100% Read와 Notify 구독이 재연결 후에도 재현되어 POC-B02를 PASS로 기록했습니다.

DualSense Bluetooth는 표준 BLE Battery Service 검색에 나타나지 않았습니다. Raw HID 역분석을 바로 시작하지 않고, 먼저 Microsoft 공개 API인 `Windows.Gaming.Input.RawGameController/Gamepad.TryGetBatteryReport()`를 검증합니다.

새 프로젝트:

```text
poc/DeviceBattery.Poc.GameControllerBatteryProbe
```

실행:

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.GameControllerBatteryProbe
dotnet clean
dotnet run --project .\DeviceBattery.Poc.GameControllerBatteryProbe.csproj
```

DualSense를 Bluetooth로 연결한 상태에서 전체 출력을 저장합니다.

## Update 08 — POC-B03-2 DualSense HID Battery

Validated B03-1 result: Bluetooth DualSense is visible through `Windows.Gaming.Input`, but both RawGameController and Gamepad `TryGetBatteryReport()` returned `null`.

Added:

- `poc/DeviceBattery.Poc.DualSenseHidBatteryProbe`
- `docs/03-poc/dualsense-hid-battery-poc-plan.md`
- `docs/03-poc/poc-status-update-07.md`

The new probe opens the DualSense HID collection read-only and parses the battery/charging status from incoming full HID reports using the upstream `hid-playstation` report layout as technical evidence.
