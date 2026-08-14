# 요구사항 추적 매트릭스 (RTM)

- 프로젝트: Device Battery Widget
- 문서 버전: 0.9
- 상태: Gate 3 초기본
- 목적: 요구사항이 설계, 구현, 테스트까지 누락 없이 추적되도록 관리한다.

> 현재는 설계/구현 전이므로 관련 칼럼의 `TBD`가 정상 상태이다.

| 요구사항 ID | 요구사항 요약 | 우선순위 | 설계 ID | 구현 대상 | 테스트 ID | 상태 |
|---|---|---|---|---|---|---|
| FR-001 | 연결 장치 탐색 | Must | TBD | TBD | TBD | 정의 |
| FR-002 | 장치 이름 획득 | Must | TBD | TBD | TBD | 정의 |
| FR-003 | 배터리 상태 조회 및 상태 모델 | Must | TBD | TBD | TBD | 정의 |
| FR-004 | 충전 상태 조회 | Must | TBD | TBD | TBD | 정의 |
| FR-005 | Event 기반 Battery 갱신 | Must | TBD | TBD | TBD | 정의 |
| FR-006 | Polling Fallback | Must | TBD | TBD | TBD | 정의 |
| FR-007 | 시작 시 즉시 조회 | Must | TBD | TBD | TBD | 정의 |
| FR-008 | 신규 장치 즉시 조회 | Must | TBD | TBD | TBD | 정의 |
| FR-009 | 장치 해제 즉시 제거 | Must | TBD | TBD | TBD | 정의 |
| FR-010 | 절전 복귀 처리 | Must | TBD | TBD | TBD | 정의 |
| FR-011 | 개별 장치 숨김 및 영속화 | Must | TBD | TBD | TBD | 정의 |
| FR-012 | Widget 위치 저장/복원 | Must | TBD | TBD | TBD | 정의 |
| FR-013 | Always On Top 저장/복원 | Must | TBD | TBD | TBD | 정의 |
| FR-014 | 동일 모델 장치 개별 식별 | Must | TBD | TBD | TBD | 정의 |
| FR-015 | Windows 로그인 자동 실행 | Should | TBD | TBD | TBD | 정의 |
| UIR-001 | Widget 형태 표시 | Must | TBD | TBD | TBD | 정의 |
| UIR-002 | 장치 이름 표시 | Must | TBD | TBD | TBD | 정의 |
| UIR-003 | Battery Gauge | Must | TBD | TBD | TBD | 정의 |
| UIR-004 | Battery % 표시 | Must | TBD | TBD | TBD | 정의 |
| UIR-005 | 충전 아이콘 | Must | TBD | TBD | TBD | 정의 |
| UIR-006 | 충전 중 연두색 Gauge | Must | TBD | TBD | TBD | 정의 |
| UIR-007 | Widget Drag | Must | TBD | TBD | TBD | 정의 |
| UIR-008 | Always On Top UI | Must | TBD | TBD | TBD | 정의 |
| UIR-009 | 장치 없음 상태 | Must | TBD | TBD | TBD | 정의 |
| UIR-010 | 조회 중 상태 | Should | TBD | TBD | TBD | 정의 |
| UIR-011 | Battery 미지원 장치 표시 | Must | TBD | TBD | TBD | 정의 |
| UIR-012 | Battery Unknown 표시 | Must | TBD | TBD | TBD | 정의 |
| NFR-PERF-001 | 유휴 CPU 5분 평균 1% 이하 목표 | Must | TBD | TBD | TBD | 정의 |
| NFR-PERF-002 | 작업 중 순간 CPU 5% 이하 목표 | Must | TBD | TBD | TBD | 정의 |
| NFR-PERF-003 | Memory 100MB 이하 목표 | Must | TBD | TBD | TBD | 정의 |
| NFR-PERF-004 | Widget 2초 이하 표시 목표 | Must | TBD | TBD | TBD | 정의 |
| NFR-PERF-005 | 최초 장치 정보 5초 이내 목표 | Must | TBD | TBD | TBD | 정의 |
| NFR-STAB-001 | Memory Leak 방지 | Must | TBD | TBD | TBD | 정의 |
| NFR-STAB-002 | 24시간 Crash 0회 | Must | TBD | TBD | TBD | 정의 |
| NFR-STAB-003 | 72시간 Soak Test | Must | TBD | TBD | TBD | 정의 |
| NFR-STAB-004 | Resource 정리 | Must | TBD | TBD | TBD | 정의 |
| NFR-STAB-005 | 장치별 예외 격리 | Must | TBD | TBD | TBD | 정의 |
| NFR-USAB-001 | 사용자 개입 최소화 | Must | TBD | TBD | TBD | 정의 |
| NFR-MAINT-001 | 장치 유형 확장성 | Must | TBD | TBD | TBD | 정의 |
| NFR-MAINT-002 | 장치별 Provider 분리 가능성 | Should | TBD | TBD | TBD | 정의 |
| IR-001 | Windows 장치 Interface | Must | TBD | TBD | TBD | 정의 |
| IR-002 | Event Interface | Must | TBD | TBD | TBD | 정의 |
| IR-003 | 연결 유형 POC 검증 | Must | TBD | TBD | TBD | 정의 |
| IR-004 | 장치 고유 식별 POC 검증 | Must | TBD | TBD | TBD | 정의 |
| CR-001 | Windows 10 22H2 이상 + Windows 11 | Must | TBD | TBD | TBD | 정의 |
| CR-002 | 경량성 우선 | Must | TBD | TBD | TBD | 정의 |
| CR-003 | Event-driven 우선 | Must | TBD | TBD | TBD | 정의 |
| CR-004 | 고빈도 Polling 제한 | Must | TBD | TBD | TBD | 정의 |
| OR-001 | 일반 Windows 사용자 실행 | Must | TBD | TBD | TBD | 정의 |
| OR-002 | Release Build | Must | TBD | TBD | TBD | 정의 |
| OR-003 | Version 부여 | Must | TBD | TBD | TBD | 정의 |
| OR-004 | Installer/Portable 검토 | Should | TBD | TBD | TBD | 정의 |
| OR-005 | Widget Close 시 Application 종료 | Must | TBD | TBD | TBD | 정의 |

## 향후 갱신 예시

```text
FR-003
  ↓
DES-BAT-001
  ↓
BatteryProvider / BatteryMonitor
  ↓
TC-FR003-001 ~ TC-FR003-00N
```

Gate 5~8에서 설계 ID를 연결하고, Gate 9~13에서 구현 및 테스트 ID를 채운다.
