# POC-D01~D05 — System Tray / Lifecycle

작성일: 2026-08-15
상태: COMPLETE — PASS
기준: Requirements Baseline v1.2 / CHG-001 / CHG-002

## 목적

WinForms `NotifyIcon`을 사용해 tray icon, context menu, widget hide/restore,
두 종료 경로 및 tray resource 정리가 가능한지 검증한다.

## 비범위와 안전성

- Production widget UI가 아니다.
- 로그인 자동 실행 registry를 변경하지 않는다.
- 숨긴 장치 설정을 저장하지 않는다.
- Device/HID API 또는 vendor command를 사용하지 않는다.

## 판정

- PASS: D01~D05 두 종료 경로와 ghost icon 부재 확인
- PASS WITH LIMITATION: 기능은 동작하지만 ghost icon 또는 lifecycle 차이 존재
- NEED ALTERNATIVE: NotifyIcon으로 필수 lifecycle 구현 불가

## 실장비 결과 — 2026-08-15

- D01 tray icon 표시: PASS
- D02 context menu 및 Always On Top ON/OFF: PASS
- D03 최소화 hide, tray double-click 및 `Widget 표시` 복원: PASS
- D04 Widget X와 tray `종료` 모두 application 전체 종료: PASS
- D05 두 종료 경로에서 icon/menu/handler/widget dispose: PASS
- 종료 후 ghost icon 없음: PASS

최종 판정: **PASS**
