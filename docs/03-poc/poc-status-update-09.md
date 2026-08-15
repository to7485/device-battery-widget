# Gate 4 POC 상태 업데이트 — POC-B04-1 완료 / POC-B04-2 승인

작성일: 2026-08-15
상태: Gate 4 진행 중

## 1. POC-B04-1 실장비 결과

두 receiver의 HID top-level collection을 read-only로 열거하고 passive input report를 관찰했다.

| 대상 | Native TLC | Vendor TLC | WinRT Read Open | Passive Online 후보 |
|---|---:|---:|---:|---|
| Logitech G703 receiver `046D:C539` | 7 | 4 | 6/7 | `FF00/0001`, Report `0x10`, offset 4 `0x62 OFF / 0xA2 ON` |
| Corsair VOID WIRELESS V2 receiver `1B1C:2A08` | 5 | 3 | 5/5 | `FF42/0002`, Report `0x03`, OFF/transition/initialized sequence |

Logitech과 Corsair 모두 2회의 본체 OFF/ON에서 같은 passive 상태 변화가 재현됐다. Receiver는 계속 PC에 연결되어 있었으므로 Generic receiver presence와 실제 peripheral online 상태가 분리됨을 다시 확인했다.

## 2. 안전성

수행한 작업:

- HID metadata/capability 조회
- WinRT `FileAccessMode.Read` open
- passive `InputReportReceived` 관찰

수행하지 않은 작업:

- Output report 전송
- Feature report get/set
- vendor-specific request/command
- 장치 상태 변경 명령

## 3. 판정

```text
POC-B04-1 = PASS WITH LIMITATION
```

- TLC discovery와 read-only open 자체는 PASS
- passive online 후보는 2회 재현
- 앱 재시작, receiver 재연결, 장시간 안정성은 limitation
- Battery 값/charging 해석은 아직 NOT TESTED

## 4. 다음 승인 범위 — POC-B04-2

B04-2 진입이 승인됐다. 다음 순서를 유지한다.

1. passive report와 제조사 앱의 실제 battery 표시값 상관관계
2. 여러 battery 값/시간대에서 동일 offset 재현
3. passive 경로가 부족할 때만 read-only feature 가능성 재검토
4. request/response 또는 output command가 필요하면 별도 승인 전 중단

현재 collection의 `FeatureReportLength`는 모두 0이므로 근거 없이 Feature API를 호출하지 않는다.

### Passive correlation 기준값 1

사용자가 제조사 앱에서 확인한 동시점 표시값:

```text
Logitech G HUB / G703              = 91%
Corsair iCUE / VOID WIRELESS V2   = 33%
```

기존 passive report에서 Logitech의 `0x5B`(91) 및 Corsair의 `0x21`(33)은 해당 장치의 독립 byte 값으로 발견되지 않았다. 따라서 현재 프로토콜은 raw 0~100 한 byte라고 가정하지 않는다. 후보 offset은 제조사 앱 조회 전후 traffic 및 다른 실제 battery 값에서 반복 상관관계를 확인해야 한다.

## 5. Gate 상태

- Gate 1: APPROVED
- Gate 2: APPROVED
- Gate 3: APPROVED / requirements baseline v1.1
- Gate 4: IN PROGRESS
  - POC-B04-1 Receiver HID Discovery: PASS WITH LIMITATION / 결과 동결
  - POC-B04-2 Passive Battery Correlation: DEFERRED TO VNEXT / CHG-002
  - POC-B05 Normalized BatteryState: NOT TESTED

다음 Gate 또는 Production 구현은 시작하지 않는다.

## 6. CHG-002 범위 변경

발주자 승인으로 v1.0 지원 대상을 Sony DualSense Bluetooth 하나로 제한했다. G703/Corsair Receiver Battery 추가 조사는 중단하고 현재 증거를 후속 릴리스용으로 동결한다.
