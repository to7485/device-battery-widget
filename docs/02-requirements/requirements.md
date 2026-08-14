# 요구사항 명세서

## 1. 문서 정보

| 항목 | 내용 |
|---|---|
| 프로젝트 | Device Battery Widget |
| 문서명 | 요구사항 명세서 |
| 문서 버전 | 0.9 |
| 상태 | Draft — Gate 3 진행 중 |
| 기준 | Gate 1, Gate 2 승인 Baseline 및 Gate 3 발주자 결정사항 |
| 승인 예정 버전 | Requirements v1.0 |
| 최종 승인권자 | 발주자 |

## 2. 목적

본 문서는 Device Battery Widget Version 1.0의 기능, 비기능, UI, 인터페이스, 제약 및 운영 요구사항을 정의한다.
Gate 3 승인 후 본 문서는 `Requirements v1.0 Baseline`으로 동결하며, 이후 요구사항 변경은 별도의 변경요청(Change Request) 절차를 따른다.

## 3. 요구사항 분류

| 분류 | ID | 설명 |
|---|---|---|
| 기능 요구사항 | FR-xxx | 프로그램이 수행해야 하는 기능 |
| UI 요구사항 | UIR-xxx | 화면 표시 및 사용자 조작 |
| 비기능 요구사항 | NFR-xxx | 성능, 안정성, 유지보수성 등 |
| 인터페이스 요구사항 | IR-xxx | Windows 및 장치 인터페이스 |
| 제약사항 | CR-xxx | 지원 환경과 기술적 원칙 |
| 운영 요구사항 | OR-xxx | 실행, 배포, 운영 관련 요구 |

## 4. 우선순위 체계

- **Must**: Version 1.0에서 반드시 충족해야 한다.
- **Should**: 중요하지만 불가피한 경우 후속 버전으로 이관할 수 있다.
- **Could**: 있으면 유용하지만 핵심 기능은 아니다.
- **Won't**: Version 1.0 범위에서는 구현하지 않는다.

# 5. 기능 요구사항

## FR-001 — 연결 장치 탐색
- 우선순위: Must
- 설명: Windows PC에 연결되어 있으며 프로그램이 지원 가능한 장치를 탐색해야 한다.
- 우선 지원 장치: 마우스, 키보드, 게임 컨트롤러, 헤드셋
- 수용 기준:
  1. 프로그램 실행 시 연결된 지원 대상 장치를 탐색한다.
  2. 신규 장치 연결을 감지한다.
  3. 장치 해제를 감지한다.
  4. 지원할 수 없는 장치가 존재해도 프로그램 전체가 비정상 종료되지 않는다.

## FR-002 — 장치 이름 획득
- 우선순위: Must
- 설명: 사용자가 장치를 식별할 수 있는 이름을 획득해야 한다.
- 수용 기준:
  1. 가능한 경우 사용자 친화적인 장치 이름을 우선 사용한다.
  2. 이름 획득 실패가 전체 Application 종료로 이어지지 않는다.
  3. 구체적인 Name Resolution 규칙은 POC 후 확정한다.

## FR-003 — 배터리 상태 조회
- 우선순위: Must
- 설명: 지원 가능한 장치의 배터리 상태를 조회해야 한다.
- 상태 모델:
  - Available: 0~100% Battery 값을 정상적으로 획득
  - Unsupported: 해당 장치에서 Battery 정보를 제공하지 않음
  - Unknown: 원래 Battery 정보를 제공하는 장치이나 현재 일시적으로 읽지 못함
- 수용 기준:
  1. 정상 값은 0~100% 범위로 처리한다.
  2. Battery 정보를 제공하지 않는 장치에 임의 값을 생성하지 않는다.
  3. 정상적으로 읽던 Battery 값을 일시적으로 읽지 못하면 이전 값을 유지하지 않고 `Unknown`으로 변경한다.
  4. 다시 정상적으로 값을 읽으면 `Unknown`에서 정상 값으로 자동 복구한다.

## FR-004 — 충전 상태 조회
- 우선순위: Must
- 설명: 장치가 충전 상태 정보를 제공하는 경우 충전 중인지 확인해야 한다.
- 상태: Charging / Not Charging / Unknown
- 수용 기준:
  1. 장치가 제공하는 실제 상태를 반영한다.
  2. 충전 여부를 알 수 없는 경우 임의로 Not Charging으로 판단하지 않는다.

## FR-005 — Event 기반 배터리 갱신
- 우선순위: Must
- 설명: 장치 또는 운영체제가 Battery 상태 변경 Event를 제공하는 경우 Event-driven 방식으로 상태를 갱신해야 한다.
- 수용 기준:
  1. Battery Event 지원 장치는 Polling보다 Event-driven을 우선한다.
  2. 수신된 상태 변화는 Application 상태와 UI에 반영한다.
  3. Event 처리 실패가 전체 Application 종료로 이어지지 않는다.

## FR-006 — Polling Fallback
- 우선순위: Must
- 설명: Battery 변경 Event를 지원하지 않는 장치는 Polling 방식으로 상태를 갱신한다.
- 기본 주기: 30초
- 수용 기준:
  1. Event 미지원 장치에서 Polling이 동작한다.
  2. 기본 주기는 30초로 한다.
  3. POC 결과에 따라 발주자 승인 후 최종 주기를 변경할 수 있다.

## FR-007 — 프로그램 시작 시 즉시 상태 조회
- 우선순위: Must
- 설명: 프로그램 시작 시 Polling 주기를 기다리지 않고 장치 및 Battery 상태를 즉시 조회해야 한다.

## FR-008 — 신규 장치 연결 시 즉시 상태 조회
- 우선순위: Must
- 설명: 프로그램 실행 중 신규 장치가 연결되면 해당 장치의 Battery 상태를 즉시 조회해야 한다.

## FR-009 — 장치 해제 처리
- 우선순위: Must
- 설명: 장치 연결 해제가 감지되면 해당 장치를 Widget 목록에서 즉시 제거해야 한다.
- 수용 기준:
  1. 장치 해제 감지 후 목록에서 제거한다.
  2. Version 1.0에서는 별도의 `연결 끊김` 유지 상태를 제공하지 않는다.

## FR-010 — 절전 복귀 처리
- 우선순위: Must
- 설명: Windows가 절전 또는 유사한 저전력 상태에서 복귀하면 장치 목록과 Battery 상태를 다시 확인해야 한다.
- 수용 기준:
  1. 장치 상태를 재확인한다.
  2. Battery 상태를 즉시 재조회한다.

## FR-011 — 개별 장치 숨기기
- 우선순위: Must
- 설명: 사용자는 위젯에 표시되는 개별 장치를 숨길 수 있어야 한다.
- 수용 기준:
  1. Battery 지원 여부와 관계없이 사용자가 원하는 개별 장치를 숨길 수 있다.
  2. 다른 장치의 표시 상태에 영향을 주지 않는다.
  3. 숨김 상태는 Application 종료 후에도 저장된다.
  4. 다음 실행 시 동일한 장치는 계속 숨김 상태를 유지한다.
  5. 동일 모델 장치가 여러 개 있을 경우 특정 장치만 개별적으로 숨길 수 있어야 한다.

## FR-012 — Widget 위치 저장 및 복원
- 우선순위: Must
- 설명: 사용자가 Widget을 이동한 마지막 위치를 저장하고 다음 실행 시 복원해야 한다.
- 수용 기준:
  1. Widget 이동 후 최종 위치를 저장한다.
  2. 정상 종료 후 재실행하면 마지막 위치를 복원한다.
  3. 저장 위치가 현재 화면 범위를 벗어난 경우 화면 안쪽으로 보정한다.

## FR-013 — Always On Top 설정 저장 및 복원
- 우선순위: Must
- 설명: Always On Top 상태를 저장하고 다음 실행 시 복원해야 한다.
- 수용 기준:
  1. ON/OFF 상태를 저장한다.
  2. 재실행 시 마지막 상태를 복원한다.

## FR-014 — 동일 모델 장치 개별 식별
- 우선순위: Must
- 설명: 같은 모델의 장치가 여러 개 연결되어도 각각을 독립된 장치로 식별해야 한다.
- 수용 기준:
  1. 동일 모델 장치를 하나로 병합하지 않는다.
  2. 각 장치는 독립적인 Battery 상태를 가진다.
  3. 각 장치는 독립적인 숨김 설정을 가질 수 있다.
  4. 사용할 Device Identifier 방식은 POC에서 검증한다.

## FR-015 — Windows 로그인 자동 실행
- 우선순위: Should
- 설명: 사용자가 Windows 로그인 시 Device Battery Widget을 자동 실행하도록 설정할 수 있어야 한다.
- 기본값: OFF
- 수용 기준:
  1. 최초 기본 상태는 OFF이다.
  2. 사용자가 ON으로 변경한 경우에만 자동 실행을 활성화한다.
  3. 사용자가 다시 OFF로 변경하면 자동 실행 등록을 해제한다.
  4. 사용자 동작 없이 임의로 ON으로 변경해서는 안 된다.

# 6. UI 요구사항

## UIR-001 — Widget 형태 표시
- 우선순위: Must
- 주요 장치 정보를 Windows Desktop Widget 형태로 제공한다.

## UIR-002 — 장치 이름 표시
- 우선순위: Must
- 각 장치 항목에 사용자 식별 가능한 장치 이름을 표시한다.

## UIR-003 — 배터리 막대 게이지
- 우선순위: Must
- Battery 잔량을 막대 형태의 Gauge로 표현한다.

## UIR-004 — 배터리 Percentage 표시
- 우선순위: Must
- Battery Gauge 오른쪽에 Battery 값을 `%` 형식으로 표시한다.

## UIR-005 — 충전 아이콘
- 우선순위: Must
- 충전 중인 장치에 번개 모양의 시각적 표시를 제공한다.

## UIR-006 — 충전 중 Gauge 색상
- 우선순위: Must
- 충전 중인 장치의 Battery Gauge를 연두색 계열로 표시한다.
- 정확한 색상 값은 UI/UX 설계 단계에서 확정한다.

## UIR-007 — Widget Drag
- 우선순위: Must
- 사용자가 마우스 Drag를 이용하여 Widget 위치를 이동할 수 있어야 한다.

## UIR-008 — Always On Top
- 우선순위: Must
- 사용자가 Widget의 Always On Top을 ON/OFF 할 수 있어야 한다.

## UIR-009 — 장치 없음 상태
- 우선순위: Must
- 표시할 수 있는 장치가 없는 경우 오류 화면 대신 정상적인 Empty State를 제공한다.

## UIR-010 — 조회 중 상태
- 우선순위: Should
- 장치 정보를 초기 조회하는 동안 사용자가 Application이 멈춘 것으로 오해하지 않도록 조회 중 상태를 표현할 수 있어야 한다.

## UIR-011 — Battery 정보 미지원 장치 표시
- 우선순위: Must
- 장치는 탐색되었으나 Battery 정보를 제공하지 않는 경우에도 해당 장치를 표시한다.
- 최종 표현 문구는 UI/UX 설계 단계에서 확정한다.

## UIR-012 — Battery Unknown 표시
- 우선순위: Must
- 원래 Battery 정보를 제공하는 장치에서 일시적인 조회 실패가 발생한 경우 `Unknown` 상태를 사용자가 식별 가능하게 표시해야 한다.

# 7. 비기능 요구사항

## NFR-PERF-001 — 유휴 CPU 사용률
- 우선순위: Must
- 정상 유휴 상태에서 프로세스의 5분 평균 CPU 사용률은 1% 이하를 목표로 한다.
- 최종 Baseline은 POC 후 확정한다.

## NFR-PERF-002 — 작업 중 CPU 사용률
- 우선순위: Must
- 장치 검색, Event 처리, Battery 갱신 등 작업 시 순간 CPU 사용률은 5% 이하를 목표로 한다.
- 최종 Baseline은 POC 후 확정한다.

## NFR-PERF-003 — Memory 사용량
- 우선순위: Must
- 정상 상태 프로세스 Memory 사용량은 100MB 이하를 목표로 한다.
- Runtime 및 Framework 특성을 고려하여 POC 후 최종 Baseline을 확정한다.

## NFR-PERF-004 — Widget 표시 시간
- 우선순위: Must
- 프로그램 실행 요청 후 Widget 표시까지 2초 이하를 목표로 한다.

## NFR-PERF-005 — 최초 장치 정보 표시
- 우선순위: Must
- 정상적인 OS/API 응답 환경에서 프로그램 실행 후 최초 장치 정보가 5초 이내 표시되는 것을 목표로 한다.

## NFR-STAB-001 — Memory Leak 방지
- 우선순위: Must
- 24시간 연속 실행에서 초기 안정화 이후 Memory가 지속적으로 증가해서는 안 된다.
- 초기 목표는 기준 시점 대비 10MB 또는 10% 이내 증가이다.

## NFR-STAB-002 — 24시간 안정성
- 우선순위: Must
- 24시간 연속 실행 중 비정상 종료는 0회이어야 한다.

## NFR-STAB-003 — 72시간 Soak Test
- 우선순위: Must
- Release Candidate에 대해 72시간 연속 Soak Test를 수행한다.
- 주요 관찰 대상: CPU, Memory, Handle, Thread, Exception, Crash, Event, Device reconnect

## NFR-STAB-004 — Resource 정리
- 우선순위: Must
- Application 종료 또는 기능 해제 시 사용한 Resource를 정상적으로 반환해야 한다.
- 주요 대상: Event Handler, Timer, Task, Thread, Native Handle, Device Handle, Cancellation Resource

## NFR-STAB-005 — 예외 격리
- 우선순위: Must
- 특정 장치 하나의 오류가 다른 장치 처리 또는 전체 Application 종료로 전파되어서는 안 된다.

## NFR-USAB-001 — 사용자 개입 최소화
- 우선순위: Must
- Application 실행 후 사용자가 매번 수동으로 장치 검색을 수행하지 않아도 장치 연결/해제와 상태 변화가 자동 반영되어야 한다.

## NFR-MAINT-001 — 장치 유형 확장성
- 우선순위: Must
- 향후 Earbuds, Speaker, Stylus 등 새로운 장치 유형을 추가할 때 기존 핵심 기능의 대규모 수정을 최소화할 수 있도록 확장 가능한 구조로 설계해야 한다.

## NFR-MAINT-002 — 장치별 Provider 분리 가능성
- 우선순위: Should
- 제조사, 연결 방식, 장치 유형별 Battery 획득 방식이 다른 경우 독립된 Provider 또는 이에 준하는 구조로 분리할 수 있어야 한다.
- 구체적 Architecture는 Gate 6에서 확정한다.

# 8. 인터페이스 요구사항

## IR-001 — Windows 장치 정보 Interface
- 우선순위: Must
- Windows가 제공하는 적절한 시스템 Interface를 이용하여 장치 정보를 확인해야 한다.
- 구체적인 API는 Gate 4 POC에서 검증한다.

## IR-002 — Event Interface
- 우선순위: Must
- OS 또는 장치가 상태 변화 Event를 제공하는 경우 이를 이용할 수 있어야 한다.

## IR-003 — 연결 유형 기술검증
- 우선순위: Must
- 다음 연결 유형을 Gate 4 POC 검증 대상에 포함한다: Bluetooth, USB, HID, 2.4GHz Dongle.
- 본 항목은 네 방식 모두의 Version 1.0 완전 지원을 의미하지 않으며, POC 결과에 따라 공식 지원 범위를 확정한다.

## IR-004 — 장치 고유 식별 검증
- 우선순위: Must
- 동일 모델 장치를 서로 구분할 수 있는 안정적인 Identifier 확보 가능성을 POC에서 검증해야 한다.
- 재연결 시 Identifier 유지 여부도 검증 대상에 포함한다.

# 9. 제약사항

## CR-001 — 지원 운영체제
- 우선순위: Must
- Windows 10 22H2 이상 및 Windows 11을 공식 지원 대상으로 한다.

## CR-002 — 경량성 우선
- 우선순위: Must
- 기술 및 Framework 선정 시 개발 편의성뿐 아니라 CPU, Memory, Startup, Runtime, 배포 크기, 안정성, Windows API 연동성을 함께 평가한다.

## CR-003 — Event-driven 우선
- 우선순위: Must
- 상태 변화 Event를 사용할 수 있는 경우 Polling보다 Event-driven 방식을 우선한다.

## CR-004 — 불필요한 고빈도 Polling 제한
- 우선순위: Must
- 단순 실시간성을 목적으로 100ms, 1초 등 고빈도 Polling을 기본 방식으로 사용하지 않는다.
- 특별한 기술적 필요가 있는 경우 근거와 성능 영향을 검토하고 승인 후 적용한다.

# 10. 운영 및 배포 요구사항

## OR-001 — 일반 사용자 실행
- 우선순위: Must
- 개발 도구가 설치되지 않은 일반 Windows 환경에서 Release 버전을 실행할 수 있어야 한다.

## OR-002 — Release Build
- 우선순위: Must
- 배포본은 Debug Build가 아닌 Release Build를 제공해야 한다.

## OR-003 — 버전 관리
- 우선순위: Must
- Release에는 식별 가능한 Version을 부여해야 한다.
- 첫 정식 Release 후보는 `v1.0.0`이다.

## OR-004 — 배포 방식 검토
- 우선순위: Should
- Installer와 Portable 방식을 검토한다.
- 최종 배포 방식은 후속 설계/배포 단계에서 확정한다.

## OR-005 — Widget Close 시 Application 완전 종료
- 우선순위: Must
- 사용자가 Widget을 닫으면 Window만 숨기지 않고 Application 전체를 종료해야 한다.
- 종료 과정에서 Background Monitoring, Event Handler, Timer, Device Resource 등을 정상적으로 정리해야 한다.

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

# 12. Gate 4 POC 필수 검증 항목

1. Windows 연결 장치 Enumeration 가능 여부
2. 사용자 친화적인 장치 이름 획득 가능 여부
3. Battery Percentage 획득 가능 여부
4. Charging 상태 획득 가능 여부
5. Battery Change Event 지원 여부
6. Device Added/Removed Event 지원 여부
7. Event 전달 지연
8. Event 누락 가능성
9. Bluetooth / USB / HID / 2.4GHz Dongle 차이
10. Event 미지원 장치 Polling 가능 여부
11. 동일 모델 장치 고유 식별 가능 여부
12. 장치 재연결 시 Identifier 유지 여부
13. CPU / Memory / Handle / Thread 기본 사용량
14. Event 기반 장치의 Safety Polling 필요성
15. 기술 스택별 Runtime/배포 특성

# 13. Gate 3 미확정 사항

- C++ / C# 등 개발 언어
- WPF / WinUI 3 / Win32 등 UI Framework
- 구체적인 Windows Device API
- Device Identifier 선정 방식
- Safety Polling 적용 여부 및 주기
- Installer / Portable 최종 선택
- 설정 저장 위치와 형식
- Logging 구현 방식
- 세부 UI 디자인 및 색상 코드

해당 항목은 Gate 4 이후 분석·설계 단계에서 결정한다.
