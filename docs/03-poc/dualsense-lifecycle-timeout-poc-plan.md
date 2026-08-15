# DualSense Lifecycle / Timeout POC 계획

작성일: 2026-08-15
상태: COMPLETE — PASS WITH LIMITATION
기준: Requirements Baseline v1.2 / CHG-002

## 목적

DualSense-only v1.0에서 연결 해제, 재연결, valid HID report 중단 및 복구를 안전하게 상태 모델과 UI lifecycle로 연결할 수 있는지 검증한다.

## 판정 항목

- targeted DeviceWatcher 시작 및 초기 enumeration
- read-only HID open
- valid report에서 Available/estimated percent/charging 출력
- 해제 시 session dispose 및 UI 제거 의미 출력
- report timeout 시 stale percent 제거와 Unknown
- 재연결/정상 report 시 Available 복구
- 종료 시 watcher/timer/handler/HID device 정리

## 안전 규칙

`DeviceClass.All` AQS를 사용하지 않는다. Output/Feature/vendor command를 전송하지 않는다.

## 판정

- PASS: OFF/ON 3회와 종료 cleanup이 안정적으로 재현
- PASS WITH LIMITATION: watcher 또는 timeout 중 일부 경로만 재현되거나 POC timeout 조정 필요
- NEED ALTERNATIVE: 재연결 후 read-only session 복구가 되지 않거나 stale state 제거가 불가능

## 실장비 중간 결과 — 2026-08-15

- Windows Bluetooth HID: 78-byte full packet이 WinRT `Report.Id=0x01`, `Data[0]=0x01`로 노출됨
- Bluetooth packet length 우선 판별 후 검증된 status offset 54 사용
- USB HID: 64-byte full report, status offset 53 사용
- USB 연결 시 약 15%에서 `NotCharging -> Charging` 상태 전환과 `BATTERY_CHANGED` 확인
- USB report layout 및 충전 전환: PASS
- Bluetooth 78-byte report에서 offset 54의 `0x01`을 약 15% / NotCharging으로 정상 변환
- Bluetooth valid report 중단 후 10초 timeout에서 stale percent 제거 및 Unknown 전환 확인
- 동일 HID session에서 report 재수신 후 Available / 약 15% 복구 확인 (1회)
- paired Bluetooth HID endpoint가 유지되어 watcher Removed/Added는 관찰되지 않음
- Bluetooth timeout/recovery 정상 경로 3회 확인
- timer tick과 input callback이 같은 시각에 경합할 때 콘솔 transition 출력 순서가
  `RECOVERED -> TIMEOUT -> RECOVERED`로 교차될 수 있음을 확인
- Production에서는 provider state transition을 단일 직렬화 경로로 전달하고 UI flicker를 방지해야 함
- 최종 summary에서 Available / 약 15% / NotCharging 확인
- `Q` 종료 시 watcher, timer, handlers, HID devices dispose 확인

## 최종 판정

**PASS WITH LIMITATION**

read-only HID에서 USB/Bluetooth battery 상태, timeout 시 stale 값 제거,
동일 Bluetooth session의 report recovery, 종료 cleanup이 실장비로 검증됐다.
다만 paired Bluetooth endpoint에서는 watcher Removed/Added가 관찰되지 않았고,
10초 timeout은 POC 값일 뿐 Production 정책으로 채택하지 않는다. Production 설계에서는
timer/input 상태 전달 직렬화와 UI flicker 방지 정책이 필요하다.
