# POC-B Battery / Charging 검증 계획 — Update 05

## B01 — Generic Battery Controller 탐색

`Windows.Devices.Power.Battery.GetDeviceSelector()`로 Battery Controller를 열거한다.

실측 결과:
- AggregateBattery: `Status=NotPresent`
- Individual Battery Controller count: `0`

판정: **FAIL / NEED ALTERNATIVE**

현재 POC 환경에서 이 API를 범용 Peripheral Battery Provider로 채택하지 않는다.

## B02 — BLE GATT Battery Provider

표준 Bluetooth Battery Service `0x180F`와 Battery Level `0x2A19`를 직접 조회한다.

실측 결과(AULA F87Pro Bluetooth):
- Battery Service 발견: PASS
- Battery Level 발견: PASS
- Uncached Read: `100%` PASS
- `Read, Notify`: 확인
- Notify Subscribe: PASS
- 전원 OFF/ON 재연결 후 재조회: PASS
- 실제 ValueChanged 발생 관찰: POC-C로 이관

판정: **PASS**

DualSense Bluetooth는 `0x180F` 검색에 나타나지 않았으므로 다른 Provider로 이관한다.

## B03 — Game Controller / HID Battery Provider

DualSense Bluetooth를 대표 샘플로 사용한다.

먼저 공개 Windows API를 우선한다.

1. `Windows.Gaming.Input.RawGameController.TryGetBatteryReport()`
2. `Windows.Gaming.Input.Gamepad.TryGetBatteryReport()`
3. 위 API가 제품 요구사항에 충분하지 않을 때만 Raw HID Input/Feature Report fallback

Raw HID 단계에서도 undocumented byte offset을 사전 가정하지 않고 실제 evidence를 먼저 수집한다.

## B04 — 2.4GHz Receiver / Vendor Provider

상태: **DEFERRED TO VNEXT / CHG-002**

Receiver 자체의 연결 상태와 실제 Peripheral Online/Battery 상태를 분리한다.

G703/Corsair 실측에서 Receiver가 유지된 상태의 본체 OFF/ON은 Generic DeviceWatcher 이벤트가 없었다.
따라서 다음 후보를 검증한다.

- HID Feature Report
- Receiver-specific endpoint/report
- Vendor-specific protocol/API

Receiver 존재 자체를 polling하여 Peripheral Online을 추정하지 않는다.

## B05 — Normalized BatteryState

Provider가 달라도 애플리케이션 Core에는 동일 모델로 전달한다.

후보 필드:
- Availability: Available / Unsupported / Unknown
- Percentage: nullable 0~100
- ChargingState: Charging / Not Charging / Unknown
- SourceProvider
- LastUpdatedAt
- IsEventDriven

## B06 — Event-first / Poll fallback

- BLE Notify/Indicate 또는 Provider 이벤트가 있으면 event-first
- 이벤트가 없지만 신뢰 가능한 실제 endpoint battery signal이 있으면 저주기 polling
- 일시 조회 실패 시 stale %를 유지하지 않고 Unknown
- Generic Receiver 연결 상태를 실제 Peripheral battery/online 상태로 대체하지 않음

## 2026-08-15 status addendum

- B01 Windows.Devices.Power peripheral battery: FAIL / NEED ALTERNATIVE
- B02 BLE GATT Battery: PASS (AULA F87Pro read + Notify subscribe + reconnect re-read)
- B03-1 Windows.Gaming.Input: NEED ALTERNATIVE for Bluetooth DualSense (`TryGetBatteryReport() == null`)
- B03-2 DualSense HID: PASS WITH LIMITATION (battery bucket + charging state event verified)
- B04-1 Receiver HID Discovery: PASS WITH LIMITATION / evidence frozen
- B04-2 Receiver Battery Correlation: DEFERRED TO VNEXT / CHG-002
