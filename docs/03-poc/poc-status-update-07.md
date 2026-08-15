# Gate 4 POC 상태 업데이트 — POC-B03-1 결과 / POC-B03-2 시작

작성일: 2026-08-15
상태: Gate 4 진행 중

## 1. POC-B03-1 Windows.Gaming.Input 실측 결과

DualSense를 Bluetooth로 연결한 상태에서 `RawGameController` / `Gamepad` 공개 배터리 경로를 검증했다.

관찰 결과:

```text
RawGameController count = 1
Gamepad count           = 1

DisplayName       = HID 규격 게임 컨트롤러
VendorId          = 0x054C
ProductId         = 0x0CE6
IsWireless        = True
TryGetBatteryReport = null

[GAMEPAD #1]
IsWireless = True
Raw.VID/PID = 0x054C/0x0CE6
TryGetBatteryReport = null
```

판정:

- DualSense 장치 인식: PASS
- VID/PID 식별: PASS
- Bluetooth/무선 여부: PASS
- RawGameController BatteryReport: NEED ALTERNATIVE (`null`)
- Gamepad BatteryReport: NEED ALTERNATIVE (`null`)

**POC-B03-1 최종: NEED ALTERNATIVE**

Windows.Gaming.Input은 이 환경에서 DualSense의 정확한 배터리 상태를 제품 요구사항에 사용할 수 있는 형태로 노출하지 않았다.

## 2. POC-B03-2 HID fallback

다음 단계는 HID Input Report를 읽는 방식으로 진행한다.

새 프로젝트:

```text
poc/DeviceBattery.Poc.DualSenseHidBatteryProbe
```

목표:

- VID `054C` / PID `0CE6` HID top-level collection 탐색
- read-only로 HID 열기
- `InputReportReceived` 수신 검증
- Bluetooth full report `0x31` 확인
- DualSense status byte에서 배터리 bucket과 charging code 해석
- 상태 변화 시에만 출력하여 이벤트 기반 Provider 가능성 확인

## 3. 기술 근거

Windows HID API는 `HidDevice.FromIdAsync`로 HID 장치를 열고 `InputReportReceived` 및 `HidInputReport.Data`를 통해 input report를 읽을 수 있다.

DualSense report layout/배터리 비트는 upstream Linux `drivers/hid/hid-playstation.c` 구현을 검증 근거로 사용한다. 해당 구현은 Bluetooth full report `0x31`과 DualSense `status[0]`의 battery capacity / charging nibble을 명시한다.

## 4. Gate 상태

- Gate 1: APPROVED
- Gate 2: APPROVED
- Gate 3: APPROVED / requirements baseline v1.1
- Gate 4: IN PROGRESS
  - POC-A: 결과 동결
  - POC-B01 Generic Battery: FAIL / NEED ALTERNATIVE
  - POC-B02 BLE GATT Battery: PASS
  - POC-B03-1 Windows.Gaming.Input: NEED ALTERNATIVE
  - POC-B03-2 DualSense HID Battery: READY FOR EXECUTION
  - POC-B04 Receiver/Vendor: NOT TESTED
  - POC-B05 Normalized BatteryState: NOT TESTED
