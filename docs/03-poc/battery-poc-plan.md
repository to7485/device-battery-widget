# POC-B Battery / Charging 검증 계획

## B01 — Battery Percentage

성공 기준:
- 실제 Battery Controller에서 Full/Remaining Capacity를 얻을 수 있음
- 두 값이 제공될 경우 % 계산 가능

판정 예:
- PASS: 실제 목표 주변장치 Battery가 정확하게 대응됨
- PASS WITH LIMITATION: 일부 장치/연결 방식만 가능
- NEED ALTERNATIVE: Battery API는 동작하지만 목표 주변장치가 노출되지 않음

## B02 — Unsupported 판별

Battery Controller가 보이지 않는 경우를 즉시 Unsupported로 확정하지 않는다.

먼저 다른 Windows Device Property / Bluetooth / HID 경로 존재 여부를 확인한다.

## B03 — Unknown

지원 장치의 조회가 일시 실패하는 시나리오를 실제 장치 테스트에서 관찰한다.

제품에서는 마지막 값을 무기한 유지하지 않고 Unknown으로 전환한다.

## B04 — Charging

BatteryStatus와 ChargeRate를 함께 기록한다.

실제 장치와 비교하여 Charging / Not Charging / Unknown 매핑을 검증한다.

## Event

Battery.ReportUpdated가 발생하면 Timestamp와 새 Report를 기록한다.
Event Latency/누락 검증은 POC-C에서 별도로 수행한다.
