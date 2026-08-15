# POC-C06 — DualSense Sleep / Resume

작성일: 2026-08-15
상태: COMPLETE — PASS
기준: Requirements Baseline v1.2 / CHG-002

## 목적

Windows 절전 전후에 DualSense Bluetooth read-only HID monitoring이 안전하게 복구되는지
검증한다. 완료된 `DeviceBattery.Poc.DualSenseLifecycleProbe`를 변경하거나 재구현하지 않고
동일 executable을 사용한다.

## 안전 범위

- targeted DualSense selector만 사용
- `FileAccessMode.Read`만 사용
- `DeviceClass.All` AQS 사용 금지
- Output/Feature/vendor command 없음
- 절전 진입과 해제는 사용자가 Windows UI와 PC 전원 버튼으로 수행

## 실행 및 실장비 절차

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.DualSenseLifecycleProbe
dotnet run -c Release --project .\DeviceBattery.Poc.DualSenseLifecycleProbe.csproj
```

1. DualSense를 Bluetooth로 연결하고 입력하여 `Available`을 확인한다.
2. `S`를 눌러 절전 전 summary를 남긴다.
3. Windows 시작 메뉴의 전원 메뉴에서 절전을 선택한다.
4. 최소 60초 후 PC를 깨운다.
5. DualSense를 켜고 버튼 또는 스틱 입력을 발생시킨다.
6. 자동 `REPORT_RECOVERED`와 `Available`을 최대 30초 기다린다.
7. 자동 복구가 없을 때만 `R`을 눌러 read-only reopen 결과를 기록한다.
8. `S`, `Q`를 눌러 최종 상태와 cleanup을 기록한다.

## 판정

- PASS: resume 후 30초 내 기존 watcher/session에서 자동 Available 복구 및 cleanup 정상
- PASS WITH LIMITATION: 자동 복구는 실패하지만 `R` read-only reopen으로 복구
- NEED ALTERNATIVE: watcher 재생성 또는 application restart 없이는 복구 불가

10초 freshness timeout은 연결 해제 판정이 아니라 POC 상태 신선도 후보일 뿐이다.

## 실장비 결과 — 2026-08-15

- 절전 전 `Available / 15% / NotCharging` 확인
- 절전/복귀 구간에서 stale percentage 제거 및 Unknown 전환
- resume 후 약 20초 내 기존 HID session에서 자동 `REPORT_RECOVERED`
- 수동 `R` reopen, watcher 재생성, application restart 불필요
- 최종 summary `Available / 15% / NotCharging`
- `Q` 종료 시 watcher, timer, handlers, HID devices dispose 확인

최종 판정: **PASS**
