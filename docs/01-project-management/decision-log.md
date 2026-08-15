# 의사결정 기록 (Decision Log)

- 프로젝트: Device Battery Widget
- 문서 버전: 1.2
- 상태: Requirements Baseline v1.2
- 최종 승인권자: 발주자

## 주요 결정사항

| ID | Gate/CHG | 결정 내용 | 상태 |
|---|---|---|---|
| DEC-001 | Gate 1 | Windows Desktop Utility Application으로 개발 | 확정 |
| DEC-002 | Gate 1 | Battery Gauge + % 표시 | 확정 |
| DEC-003 | Gate 1 | 충전 중 번개 아이콘 및 연두색 Gauge | 확정 |
| DEC-004 | Gate 1 | Widget Drag, Always On Top 제공 | 확정 |
| DEC-005 | Gate 1 | Event-driven 우선, 미지원 시 Polling | 확정 |
| DEC-006 | Gate 1 | Event 미지원 Polling 기본 30초 후보 | 확정 |
| DEC-007 | Gate 1 | 시작/신규 연결/절전 복귀 시 즉시 조회 | 확정 |
| DEC-008 | Gate 1 | 경량성·안정성·Memory Leak 방지 | 확정 |
| DEC-009 | Gate 2 | 약 8주/40영업일, 약 10% Buffer | 확정 |
| DEC-010 | Gate 2 | Git 형상관리 및 Gate 승인제 적용 | 확정 |
| DEC-011 | Gate 2 | Release 기준 Critical 0 / Major 0 | 확정 |
| DEC-012 | Gate 3 | Windows 10 22H2 이상 및 Windows 11 지원 | 확정 |
| DEC-013 | Gate 3 | 마우스/키보드/게임 컨트롤러/헤드셋 우선 지원 | 확정 |
| DEC-014 | Gate 3 | Battery 미지원 장치도 UI 표시 | 확정 |
| DEC-015 | Gate 3 | 개별 장치 숨김 및 상태 영속화 | 확정 |
| DEC-016 | Gate 3 | 숨긴 장치 목록 확인 및 개별 숨김 해제 | 확정 |
| DEC-017 | Gate 3 | 연결 해제 장치 즉시 제거 | 확정 |
| DEC-018 | Gate 3 | Widget 마지막 위치 저장/복원 | 확정 |
| DEC-019 | Gate 3 | Always On Top 기본값 OFF, 이후 상태 저장/복원 | 확정 |
| DEC-020 | Gate 3 | 동일 모델 장치를 개별 식별 | 확정 |
| DEC-021 | Gate 3 | Battery 일시 조회 실패 시 Unknown 표시 | 확정 |
| DEC-022 | Gate 3 | Widget X 버튼 클릭 시 Application 전체 종료 | 확정 |
| DEC-023 | Gate 3 | Windows 로그인 자동 실행 옵션 제공, 기본값 OFF | 확정 |
| DEC-024 | CHG-001 | System Tray 기능 추가 승인 | 확정 |
| DEC-025 | CHG-001 | Tray 메뉴에 Widget 표시/Always On Top/숨긴 장치 관리/자동 실행/종료 포함 | 확정 |
| DEC-026 | CHG-001 | Widget 숨김/최소화 시 Application과 Tray 유지, Tray에서 Widget 복원 | 확정 |
| DEC-027 | CHG-001 | 기존 OR-005 유지: Widget X 버튼은 Application 전체 종료 | 확정 |
| DEC-028 | CHG-001 | Requirements Baseline을 v1.1로 개정 | 확정 |
| DEC-029 | CHG-002 | v1.0 지원 장치를 Sony DualSense Bluetooth로 제한 | 확정 |
| DEC-030 | CHG-002 | Mouse/Keyboard/Headset/기타 Controller 지원을 vNext로 이관 | 확정 |
| DEC-031 | CHG-002 | Receiver POC 결과를 보존하고 B04 추가 조사를 동결 | 확정 |
| DEC-032 | CHG-002 | Requirements Baseline을 v1.2로 개정 | 확정 |

## 변경요청 현황

| CHG ID | 변경 내용 | 상태 |
|---|---|---|
| CHG-001 | System Tray 기능 추가 | **Approved** |
| CHG-002 | v1.0 DualSense 단일 장치 범위 | **Approved** |
