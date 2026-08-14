# 요구사항 명세서

## 1. 문서 정보

| 항목 | 내용 |
|---|---|
| 프로젝트 | Device Battery Widget |
| 문서명 | 요구사항 명세서 |
| 문서 버전 | 1.0 |
| 상태 | Approved / Baseline |
| Gate | Gate 3 — 요구사항 정의 및 분석 |
| 최종 승인권자 | 발주자 |
| 변경관리 | 승인 이후 요구 변경은 `CHG-xxx` 절차 적용 |

---

## 2. 목적

본 문서는 Device Battery Widget Version 1.0의 기능, 비기능, UI, 인터페이스, 제약 및 운영 요구사항을 정의한다.

본 문서는 Gate 3 승인에 따라 `Requirements v1.0 Baseline`으로 동결되었다.

---

## 3. 요구사항 분류

| 분류 | ID | 설명 |
|---|---|---|
| 기능 요구사항 | FR-xxx | 프로그램이 수행해야 하는 기능 |
| UI 요구사항 | UIR-xxx | 화면 표시 및 사용자 조작 |
| 비기능 요구사항 | NFR-xxx | 성능, 안정성, 유지보수성 등 |
| 인터페이스 요구사항 | IR-xxx | Windows 및 장치 인터페이스 |
| 제약사항 | CR-xxx | 지원 환경과 기술적 원칙 |
| 운영 요구사항 | OR-xxx | 실행, 배포, 운영 관련 요구 |

---

## 4. 우선순위

- **Must**: Version 1.0에서 반드시 충족
- **Should**: 중요하지만 불가피한 경우 후속 버전으로 이관 가능
- **Could**: 있으면 유용하지만 핵심 기능은 아님
- **Won't**: Version 1.0 범위에서 구현하지 않음

---

# 5. 기능 요구사항

## FR-001 — 연결 장치 탐색
- 우선순위: Must
- Windows PC에 연결된 지원 대상 장치를 탐색한다.
- 우선 지원 장치:
  - 마우스
  - 키보드
  - 게임 컨트롤러
  - 헤드셋
- 수용 기준:
  1. 프로그램 실행 시 연결된 지원 대상 장치를 탐색한다.
  2. 신규 장치 연결을 감지한다.
  3. 장치 해제를 감지한다.
  4. 지원할 수 없는 장치로 인해 Application 전체가 비정상 종료되지 않는다.

## FR-002 — 장치 이름 획득
- 우선순위: Must
- 사용자 친화적인 장치 이름을 획득한다.
- Name Resolution 세부 규칙은 Gate 4 POC 후 확정한다.

## FR-003 — 배터리 상태 조회
- 우선순위: Must
- 상태 모델:
  - Available: 0~100% Battery 값을 정상 획득
  - Unsupported: Battery 정보를 제공하지 않음
  - Unknown: 원래 Battery 정보를 제공하지만 현재 일시적으로 조회 실패
- 수용 기준:
  1. 정상 값은 0~100%로 처리한다.
  2. Battery 미지원 장치에 임의 값을 생성하지 않는다.
  3. 정상적으로 읽던 값을 일시적으로 읽지 못하면 이전 값을 유지하지 않고 `Unknown`으로 변경한다.
  4. 다시 정상 조회되면 자동으로 정상 값으로 복구한다.

## FR-004 — 충전 상태 조회
- 우선순위: Must
- 상태: Charging / Not Charging / Unknown
- 알 수 없는 충전 상태를 임의로 Not Charging으로 판단하지 않는다.

## FR-005 — Event 기반 배터리 갱신
- 우선순위: Must
- Battery 상태 변경 Event를 제공하는 경우 Event-driven 방식으로 갱신한다.
- Polling보다 Event를 우선한다.

## FR-006 — Polling Fallback
- 우선순위: Must
- Event 미지원 장치는 Polling 방식으로 갱신한다.
- 기본 주기: 30초
- 최종 주기는 Gate 4 POC 결과에 따라 변경 가능하다.

## FR-007 — 프로그램 시작 시 즉시 조회
- 우선순위: Must
- Application 시작 시 Polling 주기와 관계없이 장치 및 Battery 상태를 즉시 조회한다.

## FR-008 — 신규 장치 연결 시 즉시 조회
- 우선순위: Must
- 실행 중 신규 장치가 연결되면 즉시 Battery 상태를 조회한다.

## FR-009 — 장치 해제 처리
- 우선순위: Must
- 장치 해제를 감지하면 해당 장치를 Widget 목록에서 즉시 제거한다.

## FR-010 — 절전 복귀 처리
- 우선순위: Must
- Windows 절전 복귀 시 장치 상태와 Battery 상태를 다시 확인한다.

## FR-011 — 개별 장치 숨기기
- 우선순위: Must
- 사용자는 개별 장치를 숨길 수 있다.
- 수용 기준:
  1. Battery 지원 여부와 관계없이 개별 장치를 숨길 수 있다.
  2. 다른 장치 표시 상태에는 영향을 주지 않는다.
  3. 숨김 상태는 Application 종료 후에도 저장된다.
  4. 재실행 시 동일 장치는 계속 숨김 상태를 유지한다.
  5. 동일 모델 장치도 각각 독립적으로 숨길 수 있다.

## FR-012 — Widget 위치 저장 및 복원
- 우선순위: Must
- 마지막 Widget 위치를 저장하고 재실행 시 복원한다.
- 저장 위치가 현재 화면 범위를 벗어나면 화면 내부로 보정한다.

## FR-013 — Always On Top 설정 저장 및 복원
- 우선순위: Must
- 최초 기본값: OFF
- 사용자가 변경한 마지막 상태를 저장하고 다음 실행 시 복원한다.

## FR-014 — 동일 모델 장치 개별 식별
- 우선순위: Must
- 동일 모델 장치가 여러 개 연결되어도 독립된 장치로 식별한다.
- 각 장치는 독립적인 Battery/충전/숨김 상태를 가져야 한다.
- 안정적인 Device Identifier 확보 방법은 Gate 4 POC에서 검증한다.

## FR-015 — Windows 로그인 자동 실행
- 우선순위: Should
- 사용자는 Windows 로그인 시 자동 실행 여부를 설정할 수 있다.
- 최초 기본값: OFF
- 사용자가 ON으로 변경한 경우에만 등록하며 다시 OFF로 변경하면 등록을 해제한다.

## FR-016 — 숨긴 장치 관리
- 우선순위: Must
- 사용자는 숨긴 장치 목록을 확인하고 개별 장치의 숨김 상태를 해제할 수 있어야 한다.
- 수용 기준:
  1. 숨긴 장치 목록을 확인할 수 있다.
  2. 특정 장치만 개별적으로 다시 표시할 수 있다.
  3. 동일 모델 장치도 각각 독립적으로 관리할 수 있다.
  4. 숨김 해제 시 해당 장치가 현재 연결 중이라면 Widget에 다시 표시된다.
  5. 다른 숨긴 장치의 상태에는 영향을 주지 않는다.

---

# 6. UI 요구사항

## UIR-001 — Widget 형태 표시
- Must
- 주요 장치 정보를 Windows Desktop Widget 형태로 제공한다.

## UIR-002 — 장치 이름 표시
- Must
- 각 장치 항목에 사용자 식별 가능한 장치 이름을 표시한다.

## UIR-003 — 배터리 막대 게이지
- Must
- Battery 잔량을 막대 형태 Gauge로 표현한다.

## UIR-004 — Battery Percentage
- Must
- Battery Gauge 오른쪽에 `%` 값을 표시한다.

## UIR-005 — 충전 아이콘
- Must
- 충전 중인 장치에 번개 모양을 표시한다.

## UIR-006 — 충전 중 Gauge 색상
- Must
- 충전 중 Battery Gauge는 연두색 계열로 표시한다.
- 정확한 색상은 UI/UX 설계 단계에서 확정한다.

## UIR-007 — Widget Drag
- Must
- 사용자는 마우스 Drag로 Widget을 이동할 수 있다.

## UIR-008 — Always On Top
- Must
- 사용자는 Always On Top을 ON/OFF 할 수 있다.
- 최초 기본값은 OFF이다.

## UIR-009 — 장치 없음 상태
- Must
- 표시 가능한 장치가 없을 경우 정상적인 Empty State를 제공한다.

## UIR-010 — 조회 중 상태
- Should
- 초기 장치 조회 시 조회 중 상태를 표현할 수 있어야 한다.

## UIR-011 — Battery 미지원 장치 표시
- Must
- Battery 정보를 제공하지 않는 장치도 표시하며 N/A 또는 이에 준하는 상태를 제공한다.

## UIR-012 — Battery Unknown 표시
- Must
- 일시적인 조회 실패 시 `Unknown` 상태를 사용자가 식별 가능하게 표시한다.

---

# 7. 비기능 요구사항

## NFR-PERF-001 — 유휴 CPU
- Must
- 정상 유휴 상태에서 5분 평균 CPU 사용률 1% 이하를 목표로 한다.

## NFR-PERF-002 — 작업 중 CPU
- Must
- 장치 검색/Event/Battery 갱신 시 순간 CPU 5% 이하를 목표로 한다.

## NFR-PERF-003 — Memory
- Must
- 정상 상태 Memory 100MB 이하를 목표로 한다.

## NFR-PERF-004 — Widget 표시 시간
- Must
- 실행 요청 후 Widget 표시까지 2초 이하를 목표로 한다.

## NFR-PERF-005 — 최초 장치 정보
- Must
- 정상 환경에서 실행 후 최초 장치 정보를 5초 이내 표시하는 것을 목표로 한다.

## NFR-STAB-001 — Memory Leak 방지
- Must
- 24시간 연속 실행에서 초기 안정화 이후 Memory가 지속 증가해서는 안 된다.
- 초기 목표: 기준 시점 대비 10MB 또는 10% 이내 증가.

## NFR-STAB-002 — 24시간 안정성
- Must
- 24시간 연속 실행 중 비정상 종료 0회.

## NFR-STAB-003 — 72시간 Soak Test
- Must
- Release Candidate에 대해 72시간 Soak Test 수행.

## NFR-STAB-004 — Resource 정리
- Must
- Application 종료/기능 해제 시 Event Handler, Timer, Task, Thread, Native Handle, Device Handle, Cancellation Resource를 정상 정리한다.

## NFR-STAB-005 — 예외 격리
- Must
- 특정 장치의 오류가 다른 장치 처리 또는 Application 전체 종료로 전파되지 않아야 한다.

## NFR-USAB-001 — 사용자 개입 최소화
- Must
- 장치 연결/해제 및 상태 변화가 자동 반영되어야 한다.

## NFR-MAINT-001 — 장치 유형 확장성
- Must
- 향후 Earbuds, Speaker, Stylus 등 장치 유형을 추가할 때 기존 핵심 기능의 대규모 수정을 최소화해야 한다.

## NFR-MAINT-002 — 장치별 Provider 분리 가능성
- Should
- 제조사/연결 방식/장치별 Battery 획득 방식 차이를 독립 구조로 분리할 수 있어야 한다.

---

# 8. 인터페이스 요구사항

## IR-001 — Windows Device Interface
- Must
- 적절한 Windows System Interface를 이용해 장치 정보를 조회한다.
- 구체적인 API는 Gate 4 POC에서 검증한다.

## IR-002 — Event Interface
- Must
- OS 또는 장치가 상태 변화 Event를 제공하는 경우 이를 사용할 수 있어야 한다.

## IR-003 — 연결 유형 기술검증
- Must
- Gate 4 POC 대상:
  - Bluetooth
  - USB
  - HID
  - 2.4GHz Dongle
- 네 방식 모두의 v1.0 완전 지원을 의미하지 않는다.

## IR-004 — 장치 고유 식별 검증
- Must
- 동일 모델 장치 구분을 위한 안정적 Identifier 확보 가능성을 POC에서 검증한다.
- 재연결 시 Identifier 유지 여부도 검증한다.

---

# 9. 제약사항

## CR-001 — 지원 운영체제
- Must
- Windows 10 22H2 이상
- Windows 11

## CR-002 — 경량성 우선
- Must
- 기술 선정 시 CPU, Memory, Startup, Runtime, 배포 크기, 안정성, Windows API 연동성을 평가한다.

## CR-003 — Event-driven 우선
- Must
- Event 사용 가능 시 Polling보다 Event-driven을 우선한다.

## CR-004 — 고빈도 Polling 제한
- Must
- 단순 실시간성을 목적으로 100ms, 1초 등의 고빈도 Polling을 기본 방식으로 사용하지 않는다.

---

# 10. 운영 요구사항

## OR-001 — 일반 사용자 실행
- Must
- 개발 도구가 설치되지 않은 지원 Windows 환경에서 Release 버전을 실행할 수 있어야 한다.

## OR-002 — Release Build
- Must
- 배포본은 Release Build를 사용한다.

## OR-003 — Version
- Must
- Release에 식별 가능한 Version을 부여한다.
- 첫 정식 Release 후보: `v1.0.0`

## OR-004 — 배포 방식 검토
- Should
- Installer와 Portable 방식을 검토하고 후속 단계에서 최종 확정한다.

## OR-005 — Widget Close 시 Application 종료
- Must
- 사용자가 Widget을 닫으면 Application 전체를 종료한다.
- 종료 과정에서 Monitoring, Event, Timer 및 Device Resource를 정리한다.

---

# 11. Version 1.0 Out of Scope

- Android
- iOS
- macOS
- Linux
- 원격 PC Battery Monitoring
- Cloud Sync
- 사용자 계정
- Web Service
- Firmware Update
- RGB Control
- Controller Button Mapping
- 제조사 전용 장치 설정
- 장치 제어 기능

---

# 12. Gate 4 POC 필수 검증 항목

1. Windows 연결 장치 Enumeration
2. 사용자 친화적인 장치 이름
3. Battery Percentage
4. Charging 상태
5. Battery Change Event
6. Device Added/Removed Event
7. Event 전달 지연
8. Event 누락 가능성
9. Bluetooth / USB / HID / 2.4GHz Dongle 차이
10. Polling Fallback
11. 동일 모델 장치 고유 식별
12. 재연결 시 Identifier 유지 여부
13. CPU / Memory / Handle / Thread 기본 사용량
14. Safety Polling 필요성
15. 기술 스택별 Runtime/배포 특성

---

# 13. Baseline 이후 변경관리

본 Requirements v1.0 승인 이후 신규 기능 또는 요구 변경은 `CHG-xxx` 변경요청으로 관리한다.

현재 승인 대기 변경요청:
- `CHG-001 — System Tray 기능 추가`

CHG-001은 발주자 승인 전이므로 본 v1.0 Baseline 요구사항에는 아직 포함하지 않는다.
