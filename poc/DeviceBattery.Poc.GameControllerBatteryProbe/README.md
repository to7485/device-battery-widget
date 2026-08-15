# POC-B03 — Windows.Gaming.Input Battery Probe

목적: DualSense 같은 게임 컨트롤러에 대해 비공개/역분석 HID 바이트를 사용하기 전에 Windows의 공개 `Windows.Gaming.Input` 배터리 경로를 먼저 검증한다.

Microsoft 문서상 `RawGameController`와 `Gamepad`는 `TryGetBatteryReport()`를 제공한다. 반환값은 `Windows.Devices.Power.BatteryReport`이며, 장치/드라이버가 제공하는 정보 수준에 따라 정확한 % 계산에 필요한 capacity 값이 없을 수 있다.

## 실행

DualSense를 Bluetooth로 연결한 뒤:

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.GameControllerBatteryProbe
dotnet clean
dotnet run --project .\DeviceBattery.Poc.GameControllerBatteryProbe.csproj
```

초기 Windows.Gaming.Input 열거가 잠깐 비어 있을 수 있어 프로그램은 약 2.5초 기다린 뒤 snapshot을 출력한다.

- `R`: 현재 컨트롤러 및 BatteryReport 재조회
- `Q` 또는 `Esc`: 종료

## 판정 기준

### PASS

DualSense가 `RawGameController` 또는 `Gamepad`에 표시되고 `TryGetBatteryReport()`가 non-null이며, `RemainingCapacityInMilliwattHours` / `FullChargeCapacityInMilliwattHours`로 유효한 %를 계산할 수 있다.

### PASS WITH LIMITATION

BatteryReport는 반환되지만 capacity가 `N/A`여서 정확한 %를 계산할 수 없고 Status 정도만 제공된다.

### NEED ALTERNATIVE

- DualSense가 Windows.Gaming.Input에 보이지만 `TryGetBatteryReport() = null`
- 또는 배터리 정보가 제품 요구사항에 충분하지 않음

그 경우 POC-B03b에서 Raw HID Input/Feature Report 경로를 별도로 검증한다. 이 단계에서는 DualSense 프로토콜의 특정 바이트 offset을 미리 가정하지 않는다.
