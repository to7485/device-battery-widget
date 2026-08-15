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

검증:
- 서비스 열거
- Battery Level uncached read
- 0~100% 파싱
- Notify/Indicate 지원 여부
- ValueChanged 이벤트
- cleanup

대표 테스트 장치로 AULA F87Pro Bluetooth를 사용하되, 구현은 AULA 전용이 아니라 표준 BLE Provider로 작성한다.

## B03 — HID Battery / Power Provider

USB/HID 또는 Bluetooth HID 장치에서 다음을 조사한다.

- Windows HID/Device property에 Battery/Power 관련 표준 속성이 있는지
- HID Feature/Input Report에 Battery Level이 있는지
- DualSense 등 HID 기반 장치를 대표 샘플로 검증

## B04 — 2.4GHz Receiver / Vendor Provider

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
