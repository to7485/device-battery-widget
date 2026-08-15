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
| DEC-033 | Gate 4 | Technical Feasibility POC를 APPROVED WITH CONDITIONS로 승인 | 확정 |
| DEC-034 | Gate 4 | C#/.NET 10, targeted read-only HID, event-first, normalized state, NotifyIcon을 Architecture 입력으로 채택 | 확정 |
| DEC-035 | Gate 5 | Architecture baseline과 ADR-001~010 조건부 승인 | 확정 |
| DEC-036 | Gate 5 | WPF+NotifyIcon, 10초 Unknown/30초 Dormant, SCD win-x64 기본 정책 채택 | 확정 |
| DEC-037 | Gate 6 | Domain/Application foundation부터 Production 구현 시작 | 확정 |
| DEC-038 | Gate 6 | Provider semantic event를 순수 single-reader reducer에서 세대/순번 검증 후 상태로 반영 | 확정 |
| DEC-039 | Gate 6 | multi-producer Channel을 single-reader coordinator가 drain하며 개별 event 오류를 격리 | 확정 |
| DEC-040 | Gate 6 | Production DualSense parser는 실측 78-byte BT 길이를 ID보다 우선하고 invalid status를 상태 변경 없이 거부 | 확정 |
| DEC-041 | Gate 6 | Production Provider는 targeted selector + Bluetooth endpoint filter + FileAccessMode.Read만 사용 | 확정 |
| DEC-042 | Gate 6 | 실제 UI 연결 전 제한 시간 smoke host로 Production Provider→Coordinator→Reducer 통합 경로 검증 | 확정 |
| DEC-043 | Gate 6 | 10초 Unknown/30초 Dormant 판단을 monotonic TimeProvider 기반 독립 freshness policy로 분리 | 확정 |
| DEC-044 | Gate 6 | WPF shell은 revision-aware ViewModel projection부터 구현하고 최종 visual style은 기능 연결 후 승인 | 확정 |
| DEC-045 | Gate 6 | 실행 App은 Provider→Coordinator→Dispatcher projection과 Tray/X 단일 shutdown 경로로 구성 | 확정 |
| DEC-046 | CHG-003 | DualSense USB 지원, 단일 compact indicator, Tray-only Topmost로 Requirements v1.3 승인 | 확정 |
| DEC-047 | CHG-004 | USB 연결 중 valid charging 유지, Bluetooth freshness 유지, frameless indicator로 Requirements v1.4 승인 | 확정 |
| DEC-048 | CHG-005 | compact typography와 ShowInTaskbar=False Tray-only presence로 Requirements v1.5 승인 | 확정 |
| DEC-049 | Gate 6 UI | DualSense white/black/blue visual language와 green charging feedback를 1차 시안으로 구현 | 확정 |
| DEC-050 | Gate 6 UI | 2차 시안에서 brand label 제거, 14px gauge, charging-only centered lightning 적용 | 확정 |
| DEC-051 | Gate 6 UI | 3차 시안에서 360px 폭 유지, 기기명-게이지 수평 배치, 10px gap/상하 5px spacing 적용 | 확정 |
| DEC-052 | Gate 6 UI | 4차 시안에서 배터리 형태 게이지와 중앙 잔량 표시를 적용하고 번개 및 별도 충전 문구 제거 | 확정 |
| DEC-053 | Gate 6 UI | 5차 시안에서 창 내부 종료 UI 제거, 사용자 종료를 Tray Exit로 한정, 기기 항목 수에 따른 높이 자동 확장 적용 | 확정 |
| DEC-054 | Gate 6 UI | 6차 시안에서 animation 없이 충전 중에만 기기명 옆 고정형 초록색 `● 충전` 표시 적용 | 확정 |
| DEC-055 | Gate 6 UI | 7차 시안에서 gauge 폭을 축소하고 기기명 가변 영역과 충전 표시 고정 영역을 분리하여 text clipping 방지 | 확정 |
| DEC-056 | Gate 6 | 승인된 UI 시안을 visual baseline으로 확정하고 위치/Topmost 설정 영속화 구현으로 진행 | 확정 |
| DEC-057 | CHG-006 | 표준 BLE GATT Battery Provider와 장치별 indicator를 Requirements v1.6 Production 범위로 승인 | 확정 |
| DEC-058 | Gate 6 UI | 일반 battery gauge 색상을 owner 지정 blue `#4F71E0`으로 조정 | 확정 |
| DEC-059 | Gate 6 UI | battery Available 상태의 gauge 중앙 잔량 text를 white로 표시 | 확정 |
| DEC-060 | POC-B03 | Xbox 0x045E/0x0B13 Windows.Gaming.Input BatteryReport 10%/Discharging 실측을 PASS WITH LIMITATION으로 기록 | 확정 |
| DEC-061 | CHG-007 | Xbox Windows.Gaming.Input BatteryReport Provider를 Requirements v1.7 Production 범위로 승인 | 확정 |
| DEC-062 | Gate 6 | Xbox가 BLE와 WGI에서 중복 검출되면 실측 정상값인 BLE를 우선하고 WGI를 fallback으로 projection | 확정 |
| DEC-063 | Gate 6 | 트레이 장치 표시 메뉴에서 개별 indicator를 숨김/복원하고 여러 장치를 연속 설정하며 선택 상태를 로컬 저장 | 확정 |
| DEC-064 | Gate 6 | unpackaged 앱 자동실행은 기본 OFF, 사용자 명시 선택 시 HKCU Run의 앱 전용 값만 생성/삭제 | 확정 |
| DEC-065 | Gate 6 | 절전 복귀 후 기존 이벤트 복구를 30초 우선하고 Provider별 표준 read-only refresh를 1회 수행 | 확정 |
| DEC-066 | Gate 6 | 기본 진단은 UTC 일별 파일, 7일/총 10 MiB 제한, short-hash DeviceKey와 정규화 상태만 기록 | 확정 |
| DEC-067 | Gate 6 | Production 8초 timed smoke 정상 종료와 APP_STOP을 자원 정리 PASS로 기록하고 성능 수치는 별도 NFR 측정으로 분리 | 확정 |

## 변경요청 현황

| CHG ID | 변경 내용 | 상태 |
|---|---|---|
| CHG-001 | System Tray 기능 추가 | **Approved** |
| CHG-002 | v1.0 DualSense 단일 장치 범위 | **Approved** |
| CHG-006 | 표준 BLE GATT Battery Production 지원 | **Approved** |
| CHG-007 | Xbox Windows.Gaming.Input Battery Production 지원 | **Approved** |

## Gate 현황

| Gate | 상태 |
|---|---|
| Gate 4 Technical Feasibility POC | **Approved With Conditions** |
| Gate 5 Architecture Design | **Approved With Conditions** |
| Gate 6 Production Implementation | **In Progress** |
