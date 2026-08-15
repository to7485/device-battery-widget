# POC-B03 — Game Controller Battery Provider 검증 계획

작성일: 2026-08-15
상태: READY FOR EXECUTION

## 1. 목적

DualSense Bluetooth를 대표 장비로 사용하여 Windows의 공개 게임 컨트롤러 API가 주변기기 BatteryReport를 제공하는지 확인한다.

POC-B02에서 DualSense는 BLE 표준 Battery Service `0x180F`로 발견되지 않았다. 그러나 그 사실만으로 배터리 조회 불가를 결론내리지 않는다.

## 2. 검증 순서

### B03-1 Windows.Gaming.Input 공개 API

먼저 다음을 검증한다.

- `RawGameController.RawGameControllers`
- `RawGameController.DisplayName`
- Hardware VID/PID
- `RawGameController.IsWireless`
- `RawGameController.TryGetBatteryReport()`
- `Gamepad.Gamepads`
- `Gamepad.TryGetBatteryReport()`

BatteryReport가 반환되면 다음을 기록한다.

- Status
- ChargeRateInMilliwatts
- DesignCapacityInMilliwattHours
- FullChargeCapacityInMilliwattHours
- RemainingCapacityInMilliwattHours
- 계산 가능한 경우 Percentage

### B03-2 Raw HID fallback

B03-1이 제품 요구사항에 충분하지 않을 때만 진행한다.

- HID interface 식별
- Input Report 관찰
- Feature Report 접근 가능성
- battery/power 관련 데이터 존재 여부

DualSense 전용 undocumented byte offset을 근거 없이 하드코딩하지 않는다. 실제 POC evidence 또는 신뢰 가능한 기술 근거가 확보된 뒤 해석한다.

## 3. 판정

- `PASS`: 공개 API로 정확한 %까지 확보
- `PASS WITH LIMITATION`: BatteryReport/Status는 얻지만 정확한 % 부족
- `NEED ALTERNATIVE`: BatteryReport가 null 또는 제품 요구사항에 부족하여 Raw HID/provider fallback 필요

## 4. 설계 영향

성공 시 `GameControllerBatteryProvider` 후보를 추가할 수 있다.

```text
BatteryProvider
  ├─ BleGattBatteryProvider
  ├─ GameControllerBatteryProvider
  ├─ HidBatteryProvider (fallback)
  └─ Receiver/VendorBatteryProvider
```
