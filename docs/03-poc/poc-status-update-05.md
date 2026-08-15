# Gate 4 POC 상태 업데이트 — POC-B01 종료 / POC-B02 준비

작성일: 2026-08-15
상태: Gate 4 진행 중

## 1. POC-B01 Generic Battery API 결과

실제 테스트 장치들을 연결한 상태에서 `Windows.Devices.Power.Battery` 기반 BatteryProbe를 실행했다.

관찰 결과:

```text
[AGGREGATE BATTERY]
DeviceId=AggregateBattery
Status=NotPresent
...

Enumerating individual battery controllers...
Battery controller DeviceInformation count: 0

ReportUpdated listeners are active.
```

판정:

- AggregateBattery는 주변기기 Battery Provider가 아니다.
- 개별 Battery Controller는 0개였다.
- `ReportUpdated listeners are active`는 리스너 구독 코드가 활성 상태라는 의미이며 주변기기 Battery가 발견되었다는 의미가 아니다.
- 현재 POC 환경에서 `Windows.Devices.Power.Battery`를 범용 Peripheral Battery Provider로 채택하지 않는다.
- POC-B01: `FAIL / NEED ALTERNATIVE`.

## 2. Provider 검증 방향

특정 장치 모델이 아니라 연결 기술별 Provider를 검증한다.

1. POC-B02 — BLE GATT Battery Provider
2. POC-B03 — HID Battery / Power Provider
3. POC-B04 — 2.4GHz Receiver / Vendor Provider
4. POC-B05 — Normalized BatteryState 통합

## 3. POC-B02 준비

`DeviceBattery.Poc.BleBatteryProbe` 프로젝트를 추가했다.

검증 대상:

- BLE Battery Service `0x180F`
- Battery Level Characteristic `0x2A19`
- Uncached Read
- Notify/Indicate 및 ValueChanged
- 종료 시 구독/Service 자원 해제

AULA F87Pro는 POC-A에서 `0x180F` 노출 단서가 있었기 때문에 대표 장비로 사용한다.
제품 요구사항이나 Provider 아키텍처를 키보드에 고정하지 않는다.

## 4. Gate 상태

- Gate 1: APPROVED
- Gate 2: APPROVED
- Gate 3: APPROVED / requirements baseline v1.1
- Gate 4: IN PROGRESS
  - POC-A: 결과 동결
  - POC-B01: FAIL / NEED ALTERNATIVE
  - POC-B02: READY FOR EXECUTION
  - POC-B03~B05: NOT TESTED
