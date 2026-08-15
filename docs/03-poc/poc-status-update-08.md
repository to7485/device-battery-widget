# Gate 4 POC 상태 업데이트 — POC-B03-2 결과 / POC-B04 시작

> 이 문서의 B04 `READY FOR EXECUTION` 표기는 당시 시점 기록이다. 최신 상태는 CHG-002에 따라 B04-2 `DEFERRED TO VNEXT`이다.

작성일: 2026-08-15
상태: Gate 4 진행 중

## 1. POC-B03-2 DualSense HID 실측 결과

Bluetooth DualSense를 대상으로 HID Input Report를 검증했다.

실측:

```text
Name = DualSense Wireless Controller
VID  = 0x054C
PID  = 0x0CE6
OPEN = Success

ReportId     = 0x31
BufferLength = 78
```

배터리/충전 샘플:

```text
StatusByte       = 0x00
BatteryBucketRaw = 0
EstimatedPercent = 5%
ChargingState    = Not Charging / Discharging

(실제로 USB 충전 케이블 연결)

StatusByte       = 0x10
BatteryBucketRaw = 0
EstimatedPercent = 5%
ChargingState    = Charging
```

사용자가 실제로 충전 케이블을 연결한 시점과 `Not Charging -> Charging` 변화가 일치했다.

판정:

- HID collection open: PASS
- Bluetooth report ID `0x31`: PASS
- 78-byte full report: PASS
- battery bucket parse: PASS
- charging state parse: PASS
- charging state event-driven change: PASS
- exact 1% precision: limitation (10% bucket 기반 대표값)

**POC-B03-2 최종: PASS WITH LIMITATION**

정밀도 표현 정책은 UI/UX 설계 단계로 이관한다. 제품 코드에서는 DualSense 전용 실행 프로젝트가 아니라 공통 HID Provider + 장치별 Parser 구조를 후보로 한다.

## 2. POC-B04 2.4 GHz Receiver/Vendor 시작

실장비 POC-A에서 다음이 이미 확인되었다.

- Logitech G703: receiver가 PC에 남아 있는 동안 마우스 본체 OFF/ON은 Generic DeviceWatcher 이벤트가 없었다.
- Corsair VOID WIRELESS V2: receiver가 PC에 남아 있는 동안 헤드셋 본체 OFF/ON은 Generic DeviceWatcher 이벤트가 없었다.
- receiver 자체 제거/삽입은 Windows interface state 변화로 감지 가능했다.

따라서 `receiver present == actual peripheral online`으로 볼 수 없다.

B04는 바로 vendor protocol을 하드코딩하지 않고 두 단계로 진행한다.

### B04-1 Receiver HID Discovery

- HID device-interface class 열거
- G703 receiver VID/PID `046D/C539` 필터
- Corsair receiver VID/PID `1B1C/2A08` 필터
- top-level collection UsagePage/Usage 확인
- input/output/feature report length 확인
- vendor-defined UsagePage 후보 식별
- WinRT HidDevice read-only open 가능성 확인

### B04-2 Vendor protocol / battery path

B04-1 결과를 보고 필요한 경우에만 진행한다.

가능한 결과:

- passive input/status report에서 battery/online signal 존재 -> event-driven Provider 후보
- feature report read로 정보 획득 가능 -> low-frequency query 후보
- host request/response vendor protocol 필요 -> vendor-specific Provider 후보
- Windows user-mode 경로에서 접근 불가 -> limitation/지원정책 후보

## 3. Gate 상태

- Gate 1: APPROVED
- Gate 2: APPROVED
- Gate 3: APPROVED / requirements baseline v1.1
- Gate 4: IN PROGRESS
  - POC-A: 결과 동결
  - POC-B01 Generic Battery: FAIL / NEED ALTERNATIVE
  - POC-B02 BLE GATT Battery: PASS
  - POC-B03-1 Windows.Gaming.Input: NEED ALTERNATIVE
  - POC-B03-2 DualSense HID Battery: PASS WITH LIMITATION
  - POC-B04-1 Receiver HID Discovery: PASS WITH LIMITATION (TLC/open PASS, passive online candidate 2회 재현)
  - POC-B04-2 Vendor Battery: READY FOR EXECUTION (passive correlation first)
  - POC-B05 Normalized BatteryState: NOT TESTED
