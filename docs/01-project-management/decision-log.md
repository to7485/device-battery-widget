# 의사결정 기록 (Decision Log)

- 프로젝트: Device Battery Widget
- 문서 버전: 1.0
- 상태: Gate 3 Approved
- Requirements Baseline: v1.0
- 최종 승인권자: 발주자

## 기록 원칙

- 승인된 Gate 및 요구사항 결정사항을 기록한다.
- Requirements v1.0 승인 이후 요구 변경은 `CHG-xxx` 변경요청 절차를 따른다.
- 승인되지 않은 변경안은 Decision Log의 확정 결정으로 기록하지 않고 Change Request 문서에서 관리한다.

## 결정사항

| ID | Gate | 결정 내용 | 상태 |
|---|---|---|---|
| DEC-001 | Gate 1 | 프로젝트는 Windows Desktop Utility Application으로 개발한다. | 확정 |
| DEC-002 | Gate 1 | 배터리 잔량은 막대 게이지와 %로 표시한다. | 확정 |
| DEC-003 | Gate 1 | 충전 중인 경우 번개 아이콘과 연두색 계열 게이지를 사용한다. | 확정 |
| DEC-004 | Gate 1 | Widget Drag와 Always On Top 기능을 제공한다. | 확정 |
| DEC-005 | Gate 1 | 경량성, 장시간 안정성, Memory Leak 방지를 핵심 품질 목표로 한다. | 확정 |
| DEC-006 | Gate 1 | 배터리 상태 갱신은 Event-driven을 우선하고, Event 미지원 장치만 Polling을 사용한다. | 확정 |
| DEC-007 | Gate 1 | Event 미지원 장치의 Polling 기본 주기는 30초 후보값으로 한다. | 확정 |
| DEC-008 | Gate 1 | 프로그램 시작, 신규 장치 연결, 절전 복귀 시 즉시 배터리 상태를 조회한다. | 확정 |
| DEC-009 | Gate 1 | 유휴 CPU 5분 평균 1% 이하를 초기 성능 목표로 한다. | 확정 |
| DEC-010 | Gate 1 | 정상 상태 Memory 100MB 이하를 초기 목표로 하며 POC 후 최종 확정한다. | 확정 |
| DEC-011 | Gate 1 | 24시간 안정성 시험과 Release 전 72시간 Soak Test를 수행한다. | 확정 |
| DEC-012 | Gate 2 | 전체 수행기간은 약 8주/40영업일, 관리 Buffer는 약 10%를 기준으로 한다. | 확정 |
| DEC-013 | Gate 2 | Git을 형상관리 도구로 사용한다. | 확정 |
| DEC-014 | Gate 2 | 단계별 Gate 승인제를 적용하고 승인된 산출물은 Baseline으로 관리한다. | 확정 |
| DEC-015 | Gate 2 | Release 기준은 Critical 0건, Major 0건으로 한다. | 확정 |
| DEC-016 | Gate 3 | 지원 OS는 Windows 10 22H2 이상 및 Windows 11로 한다. | 확정 |
| DEC-017 | Gate 3 | Version 1.0 우선 지원 장치는 마우스, 키보드, 게임 컨트롤러, 헤드셋으로 한다. | 확정 |
| DEC-018 | Gate 3 | 향후 새로운 장치 유형을 확장할 수 있는 구조를 요구한다. | 확정 |
| DEC-019 | Gate 3 | 배터리 정보를 얻을 수 없는 장치도 UI에 표시한다. | 확정 |
| DEC-020 | Gate 3 | 사용자는 개별 장치를 숨길 수 있다. | 확정 |
| DEC-021 | Gate 3 | 숨긴 장치는 Application 재실행 후에도 숨김 상태를 유지한다. | 확정 |
| DEC-022 | Gate 3 | 사용자는 숨긴 장치 목록을 확인하고 개별 장치의 숨김 상태를 해제할 수 있다. | 확정 |
| DEC-023 | Gate 3 | 연결 해제된 장치는 Widget 목록에서 즉시 제거한다. | 확정 |
| DEC-024 | Gate 3 | 마지막 Widget 위치를 저장하고 다음 실행 시 복원한다. | 확정 |
| DEC-025 | Gate 3 | Always On Top의 최초 기본값은 OFF이며 이후 마지막 설정을 저장/복원한다. | 확정 |
| DEC-026 | Gate 3 | 동일 모델 장치가 여러 개 연결되면 각각 독립적으로 식별한다. | 확정 |
| DEC-027 | Gate 3 | 정상적으로 읽던 Battery 값을 일시적으로 읽지 못하면 Unknown으로 표시한다. | 확정 |
| DEC-028 | Gate 3 | Widget을 닫으면 Application 전체를 종료한다. | 확정 |
| DEC-029 | Gate 3 | Windows 로그인 시 자동 실행 옵션을 제공한다. | 확정 |
| DEC-030 | Gate 3 | Windows 로그인 자동 실행 옵션의 기본값은 OFF로 한다. | 확정 |
| DEC-031 | Gate 3 | Requirements v1.0을 승인 Baseline으로 동결한다. | 확정 |

## 변경요청 현황

| CHG ID | 변경 내용 | 상태 |
|---|---|---|
| CHG-001 | System Tray 기능 추가 | Pending Approval |
