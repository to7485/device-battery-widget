# 요구사항 추적 매트릭스 (RTM)

- 프로젝트: Device Battery Widget
- 문서 버전: **1.1**
- Requirements Baseline: **v1.1**
- 상태: Approved

> 현재는 설계/구현 전이므로 설계·구현·테스트 칼럼의 TBD가 정상이다.

| 요구사항 ID | 요구사항 요약 | 우선순위 | 설계 ID | 구현 대상 | 테스트 ID | 상태 |
|---|---|---|---|---|---|---|
| FR-001 | 연결 장치 탐색 | Must | TBD | TBD | TBD | Baseline |
| FR-002 | 장치 이름 획득 | Must | TBD | TBD | TBD | Baseline |
| FR-003 | Battery 상태 조회 | Must | TBD | TBD | TBD | Baseline |
| FR-004 | 충전 상태 조회 | Must | TBD | TBD | TBD | Baseline |
| FR-005 | Event 기반 Battery 갱신 | Must | TBD | TBD | TBD | Baseline |
| FR-006 | Polling Fallback | Must | TBD | TBD | TBD | Baseline |
| FR-007 | 시작 시 즉시 조회 | Must | TBD | TBD | TBD | Baseline |
| FR-008 | 신규 장치 즉시 조회 | Must | TBD | TBD | TBD | Baseline |
| FR-009 | 장치 해제 즉시 제거 | Must | TBD | TBD | TBD | Baseline |
| FR-010 | 절전 복귀 처리 | Must | TBD | TBD | TBD | Baseline |
| FR-011 | 개별 장치 숨김/영속화 | Must | TBD | TBD | TBD | Baseline |
| FR-012 | Widget 위치 저장/복원 | Must | TBD | TBD | TBD | Baseline |
| FR-013 | Always On Top 저장/복원, 기본 OFF | Must | TBD | TBD | TBD | Baseline |
| FR-014 | 동일 모델 장치 개별 식별 | Must | TBD | TBD | TBD | Baseline |
| FR-015 | Windows 로그인 자동 실행, 기본 OFF | Should | TBD | TBD | TBD | Baseline |
| FR-016 | 숨긴 장치 목록/숨김 해제 | Must | TBD | TBD | TBD | Baseline |
| FR-017 | System Tray 제공 | Must | TBD | TBD | TBD | Baseline |
| UIR-001 | Widget 형태 표시 | Must | TBD | TBD | TBD | Baseline |
| UIR-002 | 장치 이름 표시 | Must | TBD | TBD | TBD | Baseline |
| UIR-003 | Battery Gauge | Must | TBD | TBD | TBD | Baseline |
| UIR-004 | Battery % 표시 | Must | TBD | TBD | TBD | Baseline |
| UIR-005 | 충전 아이콘 | Must | TBD | TBD | TBD | Baseline |
| UIR-006 | 충전 중 연두색 Gauge | Must | TBD | TBD | TBD | Baseline |
| UIR-007 | Widget Drag | Must | TBD | TBD | TBD | Baseline |
| UIR-008 | Always On Top UI | Must | TBD | TBD | TBD | Baseline |
| UIR-009 | Empty State | Must | TBD | TBD | TBD | Baseline |
| UIR-010 | 조회 중 상태 | Should | TBD | TBD | TBD | Baseline |
| UIR-011 | Battery 미지원 장치 표시 | Must | TBD | TBD | TBD | Baseline |
| UIR-012 | Battery Unknown 표시 | Must | TBD | TBD | TBD | Baseline |
| UIR-013 | System Tray Context Menu | Must | TBD | TBD | TBD | Baseline |
| NFR-PERF-001 | 유휴 CPU 목표 | Must | TBD | TBD | TBD | Baseline |
| NFR-PERF-002 | 작업 중 CPU 목표 | Must | TBD | TBD | TBD | Baseline |
| NFR-PERF-003 | Memory 목표 | Must | TBD | TBD | TBD | Baseline |
| NFR-PERF-004 | Widget 표시 시간 | Must | TBD | TBD | TBD | Baseline |
| NFR-PERF-005 | 최초 장치 정보 시간 | Must | TBD | TBD | TBD | Baseline |
| NFR-STAB-001 | Memory Leak 방지 | Must | TBD | TBD | TBD | Baseline |
| NFR-STAB-002 | 24시간 안정성 | Must | TBD | TBD | TBD | Baseline |
| NFR-STAB-003 | 72시간 Soak Test | Must | TBD | TBD | TBD | Baseline |
| NFR-STAB-004 | Resource 정리 | Must | TBD | TBD | TBD | Baseline |
| NFR-STAB-005 | 예외 격리 | Must | TBD | TBD | TBD | Baseline |
| NFR-USAB-001 | 사용자 개입 최소화 | Must | TBD | TBD | TBD | Baseline |
| NFR-MAINT-001 | 장치 유형 확장성 | Must | TBD | TBD | TBD | Baseline |
| NFR-MAINT-002 | 장치별 Provider 분리 | Should | TBD | TBD | TBD | Baseline |
| IR-001 | Windows Device Interface | Must | TBD | TBD | TBD | Baseline |
| IR-002 | Event Interface | Must | TBD | TBD | TBD | Baseline |
| IR-003 | 연결 유형 POC 검증 | Must | TBD | TBD | TBD | Baseline |
| IR-004 | 장치 고유 식별 POC 검증 | Must | TBD | TBD | TBD | Baseline |
| CR-001 | 지원 Windows | Must | TBD | TBD | TBD | Baseline |
| CR-002 | 경량성 우선 | Must | TBD | TBD | TBD | Baseline |
| CR-003 | Event-driven 우선 | Must | TBD | TBD | TBD | Baseline |
| CR-004 | 고빈도 Polling 제한 | Must | TBD | TBD | TBD | Baseline |
| OR-001 | 일반 사용자 실행 | Must | TBD | TBD | TBD | Baseline |
| OR-002 | Release Build | Must | TBD | TBD | TBD | Baseline |
| OR-003 | Version 관리 | Must | TBD | TBD | TBD | Baseline |
| OR-004 | Installer/Portable 검토 | Should | TBD | TBD | TBD | Baseline |
| OR-005 | Widget Close 시 Application 종료 | Must | TBD | TBD | TBD | Baseline |
