# POC-B03-2 — DualSense HID Battery 검증 계획

작성일: 2026-08-15
상태: READY FOR EXECUTION

## 1. 목적

Bluetooth DualSense가 `Windows.Gaming.Input.TryGetBatteryReport()`에서 `null`을 반환하므로 HID Input Report fallback의 기술적 타당성을 검증한다.

## 2. 검증 대상

- Sony VID: `0x054C`
- DualSense PID: `0x0CE6`
- HID Generic Desktop / Game Pad selector 우선
- Joystick selector fallback

## 3. 검증 항목

### B03-2-01 HID 열거

- `HidDevice.GetDeviceSelector()` 기반 대상 검색
- DeviceInformation 이름/ID 기록

### B03-2-02 HID Open

- `HidDevice.FromIdAsync(..., FileAccessMode.Read)` 성공 여부
- 실패 시 권한/collection/WinRT 제한을 별도 원인으로 기록

### B03-2-03 Input Report

- `InputReportReceived` 수신 여부
- Report ID와 Data length 기록
- Bluetooth full report `0x31` 여부 확인

### B03-2-04 Battery Parse

upstream `hid-playstation` 기준:

- common report의 `status[0]` lower nibble: battery bucket
- common report의 `status[0]` upper nibble: charging code
- charging code `0x0`: discharging
- `0x1`: charging
- `0x2`: full
- `0xA`, `0xB`, `0xF` 및 기타: error/unknown으로 제품 모델에 정규화

### B03-2-05 Event behavior

input report에서 status byte 변화가 관찰될 경우 polling 없이 event-driven 갱신 후보로 기록한다.

## 4. 판정 기준

- PASS: HID report 수신 + battery bucket/charging state 해석 성공
- PASS WITH LIMITATION: HID report는 수신하지만 배터리 해석이 부분적/10% 단위 등 제한 존재
- NEED ALTERNATIVE: HID collection을 열지 못하거나 full report를 수신하지 못함

## 5. 제품 설계 영향

성공 시 DualSense는 generic `Windows.Gaming.Input` Provider가 아니라 `HidBatteryProvider` 또는 장치 프로파일 기반 HID Provider가 담당한다.
