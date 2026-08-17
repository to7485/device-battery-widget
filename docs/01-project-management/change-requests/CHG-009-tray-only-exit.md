# CHG-009 — Tray-only Exit Lifecycle

| 항목 | 내용 |
|---|---|
| 프로젝트 | Device Battery Widget |
| 요청 유형 | Lifecycle 요구사항 변경 |
| 요청자/승인권자 | 발주자 |
| 대상 버전 | Version 1.0 |
| 기준 Baseline | Requirements v1.8 |
| 승인 결과 | **Approved** |
| 반영 Baseline | **Requirements v1.9** |
| 승인일 | 2026-08-17 |

## 변경 내용

기존 OR-005의 Widget X 버튼 전체 종료 요구를 폐기한다. 위젯에는 종료 버튼을 표시하지 않고,
Application 전체 종료는 Tray 메뉴의 `종료`에서만 수행한다.

- Widget 숨김/표시 전환 중 Provider와 Tray lifecycle은 유지한다.
- Application 종료는 단일 `ShutdownAsync` 경로로 수렴한다.
- 종료 시 watcher, event handler, timer, provider, tray icon 및 widget resource를 정리한다.

## 근거

발주자가 frameless indicator UI와 Tray-only 종료 방식을 요구했고 최종 UI 및 lifecycle을
실장비에서 확인했다. 2026-08-17 재실행 검증에서 위젯 위치와 Always On Top 설정 복원도
확인했으며 Gate 6 종료를 승인했다.

