# POC-B Battery / Charging 검증 계획 — Update 04

## B01 — Generic Battery Controller 탐색

`Windows.Devices.Power.Battery.GetDeviceSelector()`로 Battery Controller를 열거한다.

성공 기준:
- 실제 주변장치와 대응되는 Battery Controller가 식별됨
- Full/Remaining Capacity 또는 실제 상태 값 확보 가능

판정:
- PASS: 목표 장치 Battery가 직접 대응됨
- PASS WITH LIMITATION: 일부 장치/연결 방식만 가능
- NEED ALTERNATIVE: API는 정상 동작하지만 목표 주변장치가 노출되지 않음

## B02 — Percentage

FullChargeCapacity와 RemainingCapacity가 제공되면 %를 계산한다.
값이 제공되지 않는 장치에서는 다른 Battery Level 신호를 검토한다.

## B03 — Unsupported / Unknown

- Controller가 없다는 이유만으로 즉시 제품의 Unsupported로 확정하지 않는다.
- Bluetooth GATT / HID / Vendor Provider를 추가 확인한다.
- 평소 지원되는 장치의 일시 조회 실패는 Unknown으로 처리한다.

## B04 — Charging

BatteryStatus 및 ChargeRate를 실제 장치 상태와 비교한다.
제품 상태는 Charging / Not Charging / Unknown 3상태를 유지한다.

## B05 — Alternative Provider 우선순위

1. AULA F87Pro Bluetooth: BLE GATT Battery Service `0x180F`
2. Generic HID Battery/Power property
3. HID Feature Report
4. Vendor-specific interface/protocol

## B06 — Event

Battery.ReportUpdated 또는 BLE GATT characteristic notification이 가능하면 event-first로 사용한다.
이벤트가 없거나 신뢰할 수 없으면 확인된 endpoint-level 상태 신호에 대해서만 저주기 polling을 적용한다.
Receiver 존재 자체를 polling하여 Peripheral Online을 추정하지 않는다.
