# 요구사항 추적 매트릭스 (RTM)

- 프로젝트: Device Battery Widget
- 문서 버전: **1.3**
- Requirements Baseline: **v1.3**
- 상태: Approved

> Gate 5에서 승인된 ARC ID를 매핑했다. Gate 6에서 구현된 항목부터 구현 대상과 자동 사양 ID를 연결하며, TBD는 아직 Production 미착수 상태를 의미한다.

| 요구사항 ID | 요구사항 요약 | 우선순위 | 설계 ID | 구현 대상 | 테스트 ID | 상태 |
|---|---|---|---|---|---|---|
| FR-001 | DualSense Bluetooth/USB 탐색 | Must | ARC-002 | targeted watcher/transport filter | SPEC-DS-PARSER + G6-CHG003-USB-01 | Implemented / Integration PASS |
| FR-002 | 장치 이름 획득 | Must | ARC-002, ARC-006 | DualSenseHidProvider DeviceDiscovered | G6-SMOKE-01 | Implemented / Integration PASS |
| FR-003 | Battery 상태 조회 | Must | ARC-001~004 | Domain BatteryState, DualSenseHidBatteryParser, reducer/coordinator, ReportFreshnessTracker | SPEC-DOM, SPEC-APP, SPEC-COORD, SPEC-DS-PARSER, SPEC-DS-FRESHNESS | In Implementation |
| FR-004 | 충전 상태 조회 | Must | ARC-001~003 | Domain BatteryState, DualSenseHidBatteryParser | SPEC-DOM, SPEC-DS-PARSER | In Implementation |
| FR-005 | Event 기반 Battery 갱신 | Must | ARC-002, ARC-003 | ProviderEvent, DeviceStateCoordinator | SPEC-APP, SPEC-COORD | In Implementation |
| FR-006 | Polling Fallback | Must | ARC-002, ARC-004 | TBD | TBD | Architecture Draft |
| FR-007 | 시작 시 즉시 조회 | Must | ARC-002, ARC-003 | watcher enumeration + read-only open | G6-SMOKE-01 | Implemented / Integration PASS |
| FR-008 | 신규 장치 즉시 조회 | Must | ARC-002, ARC-003 | watcher Added + read-only open | 실장비 통합 TBD | In Implementation |
| FR-009 | 장치 해제 즉시 제거 | Must | ARC-003, ARC-004 | DeviceStateReducer | SPEC-APP | In Implementation |
| FR-010 | 절전 복귀 처리 | Must | ARC-002, ARC-004 | TBD | TBD | Architecture Draft |
| FR-011 | 개별 장치 숨김/영속화 | Must | ARC-006, ARC-008 | TBD | TBD | Architecture Draft |
| FR-012 | Widget 위치 저장/복원 | Must | ARC-006, ARC-008 | TBD | TBD | Architecture Draft |
| FR-013 | Always On Top 저장/복원, 기본 OFF | Must | ARC-006, ARC-008 | TBD | TBD | Architecture Draft |
| FR-014 | 동일 모델 장치 개별 식별 | Must | ARC-005 | DualSenseDeviceIdentity provider-owned hash key | SPEC-DS-PARSER | In Implementation |
| FR-015 | Windows 로그인 자동 실행, 기본 OFF | Should | ARC-008, ARC-010 | TBD | TBD | Architecture Draft |
| FR-016 | 숨긴 장치 목록/숨김 해제 | Must | ARC-006, ARC-008 | TBD | TBD | Architecture Draft |
| FR-017 | System Tray 제공 | Must | ARC-007 | TrayIconController | G6-APP-SMOKE-01 + manual lifecycle TBD | In Implementation |
| UIR-001 | 단일 인디케이터 표시 | Must | ARC-006 | compact WidgetWindow + USB-priority projection | build + SPEC-WPF | In Implementation |
| UIR-002 | 장치 이름 표시 | Must | ARC-006 | DeviceCardViewModel/WidgetWindow | SPEC-WPF | In Implementation |
| UIR-003 | Battery Gauge | Must | ARC-006 | DeviceCardViewModel/ProgressBar | SPEC-WPF | In Implementation |
| UIR-004 | Battery % 표시 | Must | ARC-006 | estimated-aware BatteryText | SPEC-WPF | In Implementation |
| UIR-005 | 충전 아이콘 | Must | ARC-006 | charging status projection | SPEC-WPF | In Implementation |
| UIR-006 | 충전 중 연두색 Gauge | Must | ARC-006 | WidgetWindow gauge baseline | visual verification TBD | In Implementation |
| UIR-007 | Widget Drag | Must | ARC-006, ARC-008 | WidgetWindow DragMove | manual UI verification TBD | In Implementation |
| UIR-008 | Always On Top Tray UI | Must | ARC-006, ARC-008 | Tray menu binding, default OFF | SPEC-WPF + manual tray TBD | In Implementation |
| UIR-009 | Empty State | Must | ARC-006 | WidgetViewModel IsEmpty | SPEC-WPF | In Implementation |
| UIR-010 | 조회 중 상태 | Should | ARC-006 | waiting projection | SPEC-WPF | In Implementation |
| UIR-011 | Battery 미지원 장치 표시 | Deferred | TBD | TBD | TBD | vNext / CHG-002 |
| UIR-012 | Battery Unknown 표시 | Must | ARC-001, ARC-006 | stale-clearing DeviceCardViewModel | SPEC-WPF | In Implementation |
| UIR-013 | System Tray Context Menu | Must | ARC-007 | Show/Topmost/Exit menu baseline | G6-APP-SMOKE-01 + manual verification TBD | In Implementation |
| NFR-PERF-001 | 유휴 CPU 목표 | Must | ARC-002~004, ARC-009 | TBD | TBD | Architecture Draft |
| NFR-PERF-002 | 작업 중 CPU 목표 | Must | ARC-002~004, ARC-009 | TBD | TBD | Architecture Draft |
| NFR-PERF-003 | Memory 목표 | Must | ARC-007, ARC-009 | TBD | TBD | Architecture Draft |
| NFR-PERF-004 | Widget 표시 시간 | Must | ARC-006, ARC-007, ARC-009 | TBD | TBD | Architecture Draft |
| NFR-PERF-005 | 최초 장치 정보 시간 | Must | ARC-002~004, ARC-009 | TBD | TBD | Architecture Draft |
| NFR-STAB-001 | Memory Leak 방지 | Must | ARC-007, ARC-009 | TBD | TBD | Architecture Draft |
| NFR-STAB-002 | 24시간 안정성 | Must | ARC-009 | TBD | TBD | Architecture Draft |
| NFR-STAB-003 | 72시간 Soak Test | Must | ARC-009, ARC-010 | TBD | TBD | Architecture Draft |
| NFR-STAB-004 | Resource 정리 | Must | ARC-002, ARC-007, ARC-009 | TBD | TBD | Architecture Draft |
| NFR-STAB-005 | 예외 격리 | Must | ARC-002, ARC-003, ARC-009 | DeviceStateCoordinator | SPEC-COORD | In Implementation |
| NFR-USAB-001 | 사용자 개입 최소화 | Must | ARC-002~008 | TBD | TBD | Architecture Draft |
| NFR-MAINT-001 | 장치 유형 확장성 | Must | ARC-001~005 | TBD | TBD | Architecture Draft |
| NFR-MAINT-002 | 장치별 Provider 분리 | Should | ARC-002, ARC-005 | IBatteryProvider, DualSenseHidProvider | SPEC-APP + build | In Implementation |
| IR-001 | Windows Device Interface | Must | ARC-002 | targeted DeviceWatcher, HidDevice FromIdAsync(Read) | static safety scan + G6-SMOKE-01 | Implemented / Integration PASS |
| IR-002 | Event Interface | Must | ARC-002, ARC-003 | IBatteryProvider, ProviderEvent, DeviceStateCoordinator | SPEC-APP, SPEC-COORD | In Implementation |
| IR-003 | 연결 유형 POC 검증 | Must | ARC-002 | DualSenseHidBatteryParser | SPEC-DS-PARSER + Gate 4 실장비 POC | In Implementation |
| IR-004 | 장치 고유 식별 POC 검증 | Must | ARC-005 | TBD | TBD | Architecture Draft |
| CR-001 | 지원 Windows | Must | ARC-002, ARC-006, ARC-007, ARC-010 | TBD | TBD | Architecture Draft |
| CR-002 | 경량성 우선 | Must | ARC-002~004, ARC-009 | TBD | TBD | Architecture Draft |
| CR-003 | Event-driven 우선 | Must | ARC-002~004 | TBD | TBD | Architecture Draft |
| CR-004 | 고빈도 Polling 제한 | Must | ARC-002, ARC-004 | TBD | TBD | Architecture Draft |
| OR-001 | 일반 사용자 실행 | Must | ARC-007, ARC-010 | TBD | TBD | Architecture Draft |
| OR-002 | Release Build | Must | ARC-010 | TBD | TBD | Architecture Draft |
| OR-003 | Version 관리 | Must | ARC-010 | TBD | TBD | Architecture Draft |
| OR-004 | Installer/Portable 검토 | Should | ARC-010 | TBD | TBD | Architecture Draft |
| OR-005 | Widget Close 시 Application 종료 | Must | ARC-007 | App single ShutdownAsync path | G6-APP-SMOKE-01 + manual X verification TBD | In Implementation |
