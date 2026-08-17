# 요구사항 추적 매트릭스 (RTM)

- 프로젝트: Device Battery Widget
- 문서 버전: **1.9**
- Requirements Baseline: **v1.9**
- 상태: Approved

> Gate 5에서 승인된 ARC ID를 매핑했다. Gate 6에서 구현된 항목부터 구현 대상과 자동 사양 ID를 연결하며, TBD는 아직 Production 미착수 상태를 의미한다.

| 요구사항 ID | 요구사항 요약 | 우선순위 | 설계 ID | 구현 대상 | 테스트 ID | 상태 |
|---|---|---|---|---|---|---|
| FR-001 | DualSense, 표준 BLE Battery, Xbox WGI Battery 탐색 | Must | ARC-002 | targeted HID + Battery Service watcher + Gamepad events | SPEC-DS/BLE/WGI + G6-CHG006-007-01 | Implemented / Integration PASS |
| FR-002 | 장치 이름 획득 | Must | ARC-002, ARC-006 | DualSenseHidProvider DeviceDiscovered | G6-SMOKE-01 | Implemented / Integration PASS |
| FR-003 | Battery 상태 조회 | Must | ARC-001~004 | BatteryState, parser, reducer/coordinator, transport freshness | SPEC-DOM/APP/COORD/DS + G6-CHG003-USB-01 | Implemented / Integration PASS |
| FR-004 | 충전 상태 조회 | Must | ARC-001~003 | DualSense charging parse; BLE Battery Level은 Unknown 유지 | SPEC-DOM, SPEC-DS-PARSER | Implemented |
| FR-005 | Event 기반 Battery 갱신 | Must | ARC-002, ARC-003 | ProviderEvent, DeviceStateCoordinator | SPEC-APP, SPEC-COORD + real-device lifecycle | Implemented / Integration PASS |
| FR-006 | Polling Fallback | Must | ARC-002, ARC-004 | BLE notify 미지원 및 WGI BatteryReport 30초 polling | SPEC-WGI + real-device projection | Implemented / PASS WITH LIMITATION |
| FR-007 | 시작 시 즉시 조회 | Must | ARC-002, ARC-003 | watcher enumeration + read-only open | G6-SMOKE-01 | Implemented / Integration PASS |
| FR-008 | 신규 장치 즉시 조회 | Must | ARC-002, ARC-003 | watcher Added + read-only open | G6-CHG006-007-01 Xbox/AULA ON | Implemented / Integration PASS |
| FR-009 | 장치 해제 즉시 제거 | Must | ARC-003, ARC-004 | DeviceStateReducer + provider removal events | SPEC-APP + G6-CHG006-007-01 Xbox/AULA OFF | Implemented / Integration PASS |
| FR-010 | 절전 복귀 처리 | Must | ARC-002, ARC-004 | PowerMode resume + delayed IRefreshableBatteryProvider read-only refresh | G6-SLEEP-RESUME-01 | Implemented / Integration PASS |
| FR-011 | 개별 장치 숨김/영속화 | Must | ARC-006, ARC-008 | WidgetViewModel hidden projection + JsonWidgetSettingsStore | SPEC-WPF + G6-VISIBILITY-01 | Implemented / Integration PASS |
| FR-012 | Widget 위치 저장/복원 | Must | ARC-006, ARC-008 | JsonWidgetSettingsStore + WindowPositionPolicy | SPEC-WPF + owner restart verification | Implemented / Integration PASS |
| FR-013 | Always On Top 저장/복원, 기본 OFF | Must | ARC-006, ARC-008 | JsonWidgetSettingsStore + tray projection | SPEC-WPF + owner restart verification | Implemented / Integration PASS |
| FR-014 | 동일 모델 장치 개별 식별 | Must | ARC-005 | provider-owned stable hashed keys | SPEC-DS-PARSER | Implemented / PASS WITH LIMITATION (two identical devices not tested) |
| FR-015 | Windows 로그인 자동 실행, 기본 OFF | Should | ARC-008, ARC-010 | RegistryRunAutoStartService + tray toggle | SPEC-AUTOSTART + G6-AUTOSTART-01 | Implemented / Integration PASS |
| FR-016 | 숨긴 장치 목록/숨김 해제 | Must | ARC-006, ARC-008 | tray device visibility submenu | SPEC-WPF + G6-VISIBILITY-01 | Implemented / Integration PASS |
| FR-017 | System Tray 제공 | Must | ARC-007 | TrayIconController | G6-APP-SMOKE-01 + owner lifecycle approval | Implemented / Integration PASS |
| UIR-001 | 장치별 인디케이터 표시 | Must | ARC-006 | approved frameless visual + DualSense USB priority + BLE/WGI arbitration | SPEC-WPF + G6-CHG006-007-01 + owner visual approval | Implemented / Integration PASS |
| UIR-002 | 장치 이름 표시 | Must | ARC-006 | DeviceCardViewModel/WidgetWindow | SPEC-WPF + owner visual approval | Implemented / Integration PASS |
| UIR-003 | Battery Gauge | Must | ARC-006 | DeviceCardViewModel/ProgressBar | SPEC-WPF + owner visual approval | Implemented / Integration PASS |
| UIR-004 | Battery % 표시 | Must | ARC-006 | estimated-aware BatteryText | SPEC-WPF + real-device display | Implemented / Integration PASS |
| UIR-005 | 충전 표시 | Must | ARC-006 | charging status projection | SPEC-WPF + DualSense USB smoke | Implemented / Integration PASS |
| UIR-006 | 충전 중 green Gauge | Must | ARC-006 | WidgetWindow gauge baseline | owner visual approval | Implemented / Integration PASS |
| UIR-007 | Widget Drag | Must | ARC-006, ARC-008 | WidgetWindow DragMove | owner restart verification | Implemented / Integration PASS |
| UIR-008 | Always On Top Tray UI | Must | ARC-006, ARC-008 | Tray menu binding, default OFF | SPEC-WPF + owner restart verification | Implemented / Integration PASS |
| UIR-009 | Empty State | Must | ARC-006 | WidgetViewModel IsEmpty | SPEC-WPF + real-device removal | Implemented / Integration PASS |
| UIR-010 | 조회 중 상태 | Should | ARC-006 | waiting projection | SPEC-WPF | Implemented / Spec PASS |
| UIR-011 | Battery 미지원 장치 표시 | Deferred | TBD | TBD | TBD | vNext / CHG-002 |
| UIR-012 | Battery Unknown 표시 | Must | ARC-001, ARC-006 | stale-clearing DeviceCardViewModel | SPEC-WPF | Implemented / Spec PASS |
| UIR-013 | Tray-only 실행 presence | Must | ARC-007 | ShowInTaskbar=False + Show/Topmost/Exit tray | App smoke + owner approval | Implemented / Integration PASS |
| NFR-PERF-001 | 유휴 CPU 목표 | Must | ARC-002~004, ARC-009 | current multi-provider Production app | G6-PERF-05M-01 avg 0.358% | Integration PASS |
| NFR-PERF-002 | 작업 중 CPU 목표 | Must | ARC-002~004, ARC-009 | current multi-provider Production app | G6-PERF-05M-01 max 1.679% | Integration PASS |
| NFR-PERF-003 | Working Set 150 MiB / Private Memory 100 MiB | Must | ARC-007, ARC-009 | current multi-provider Production app | G6-PERF-05M-01 138.16/71.43 MiB + CHG-008 | Integration PASS |
| NFR-PERF-004 | Widget 표시 시간 | Must | ARC-006, ARC-007, ARC-009 | Production WPF/tray startup | G6-STARTUP-10X-01 max/P95 939.1 ms | Integration PASS |
| NFR-PERF-005 | 최초 장치 정보 시간 | Must | ARC-002~004, ARC-009 | first normalized Available projection | G6-STARTUP-10X-01 max/P95 849.6 ms | Integration PASS |
| NFR-STAB-001 | Memory Leak 방지 | Must | ARC-007, ARC-009 | Production soak | partial 7.07h; G6-SOAK-DEFERRED | Deferred / Owner residual-risk acceptance |
| NFR-STAB-002 | 24시간 안정성 | Must | ARC-009 | Production soak | incomplete attempts; G6-SOAK-DEFERRED | Deferred / Owner residual-risk acceptance |
| NFR-STAB-003 | 72시간 Soak Test | Must | ARC-009, ARC-010 | Production soak | incomplete attempts; G6-SOAK-DEFERRED | Deferred / Owner residual-risk acceptance |
| NFR-STAB-004 | Resource 정리 | Must | ARC-002, ARC-007, ARC-009 | App.ShutdownAsync ordered cancellation/drain/disposal | G6-CLEANUP-01 | Implemented / Integration PASS |
| NFR-STAB-005 | 예외 격리 | Must | ARC-002, ARC-003, ARC-009 | ProviderRunner + DeviceStateCoordinator | SPEC-PROVIDER-ISOLATION + SPEC-COORD | Implemented / Spec PASS |
| NFR-USAB-001 | 사용자 개입 최소화 | Must | ARC-002~008 | automatic discovery/lifecycle + tray settings | real-device lifecycle smokes | Implemented / Integration PASS |
| NFR-MAINT-001 | 장치 유형 확장성 | Must | ARC-001~005 | provider abstraction + BLE/WGI extensions | SPEC-APP + multi-provider smoke | Implemented / Integration PASS |
| NFR-MAINT-002 | 장치별 Provider 분리 | Should | ARC-002, ARC-005 | IBatteryProvider + HID/BLE/WGI Providers | SPEC-APP + Release build | Implemented / Spec PASS |
| IR-001 | Windows Device Interface | Must | ARC-002 | targeted DeviceWatcher, HidDevice FromIdAsync(Read) | static safety scan + G6-SMOKE-01 | Implemented / Integration PASS |
| IR-002 | Event Interface | Must | ARC-002, ARC-003 | IBatteryProvider, ProviderEvent, DeviceStateCoordinator | SPEC-APP, SPEC-COORD | Implemented / Spec PASS |
| IR-003 | 연결 유형 POC 검증 | Must | ARC-002 | DualSenseHidBatteryParser | SPEC-DS-PARSER + Gate 4/6 real-device evidence | Implemented / Integration PASS |
| IR-004 | 장치 고유 식별 POC 검증 | Must | ARC-005 | provider-owned stable hashed keys | SPEC-DS-PARSER | Implemented / PASS WITH LIMITATION |
| CR-001 | 지원 Windows | Must | ARC-002, ARC-006, ARC-007, ARC-010 | net10.0-windows10.0.19041.0 app | Windows 10 evidence; Windows 11 pending | PASS WITH LIMITATION / Gate 7 condition |
| CR-002 | 경량성 우선 | Must | ARC-002~004, ARC-009 | event-first multi-provider app | G6-PERF-05M-01 + CHG-008 | Integration PASS |
| CR-003 | Event-driven 우선 | Must | ARC-002~004 | provider events + coordinator | SPEC-APP/COORD + real-device lifecycle | Implemented / Integration PASS |
| CR-004 | 고빈도 Polling 제한 | Must | ARC-002, ARC-004 | event-first + 30-second fallback | implementation review | Implemented / Spec PASS |
| OR-001 | 일반 사용자 실행 | Must | ARC-007, ARC-010 | unpackaged per-user app + HKCU autostart | G6-AUTOSTART-01 + real-device runs | Integration PASS |
| OR-002 | Release Build | Must | ARC-010 | DeviceBatteryWidget.slnx Release | 2026-08-17 warnings 0/errors 0 | PASS |
| OR-003 | Version 관리 | Must | ARC-010 | TBD | TBD | Architecture Draft |
| OR-004 | Installer/Portable 검토 | Should | ARC-010 | TBD | TBD | Architecture Draft |
| OR-005 | Tray 메뉴에서 Application 종료 | Must | ARC-007 | Tray Exit + App single ShutdownAsync path | G6-CLEANUP-01 + CHG-009 owner approval | Implemented / Integration PASS |
