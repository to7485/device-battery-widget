# 요구사항 명세서

## 1. 문서 정보

| 항목 | 내용 |
|---|---|
| 프로젝트 | Device Battery Widget |
| 문서명 | 요구사항 명세서 |
| 문서 버전 | **1.3** |
| 상태 | **Approved / Baseline** |
| 기준 | Gate 3 승인 + CHG-001 + CHG-002 + CHG-003 승인 |
| 최종 승인권자 | 발주자 |

---

## 2. Baseline 변경이력

| 버전 | 변경 내용 |
|---|---|
| v1.0 | Gate 3 요구사항 Baseline 승인 |
| v1.1 | CHG-001 System Tray 기능 추가 반영 |
| v1.2 | CHG-002 v1.0 지원 범위를 Sony DualSense Bluetooth 단일 장치로 축소 |
| v1.3 | CHG-003 DualSense USB 지원, 단일 인디케이터, Tray-only Always On Top |

---

# 3. 기능 요구사항

## FR-001 — 연결 장치 탐색
- 우선순위: Must
- Windows PC에 연결된 지원 대상 장치를 탐색한다.
- v1.0 지원 장치: Sony DualSense Bluetooth/USB (`VID 0x054C / PID 0x0CE6`)
- Mouse, Keyboard, Headset 및 기타 Game Controller는 후속 릴리스 확장 범위로 이관한다.
- v1.0 범위 밖 장치는 Widget 장치 목록에 표시하지 않는다.

## FR-002 — 장치 이름 획득
- 우선순위: Must
- 사용자 친화적인 장치 이름을 획득한다.

## FR-003 — 배터리 상태 조회
- 우선순위: Must
- 상태 모델:
  - Available
  - Unsupported
  - Unknown
- 일시적 조회 실패 시 이전 값을 유지하지 않고 Unknown으로 변경한다.

## FR-004 — 충전 상태 조회
- 우선순위: Must
- Charging / Not Charging / Unknown을 구분한다.

## FR-005 — Event 기반 배터리 갱신
- 우선순위: Must
- Battery 상태 변경 Event를 제공하는 경우 Event-driven 방식으로 갱신한다.

## FR-006 — Polling Fallback
- 우선순위: Must
- Event 미지원 장치는 기본 30초 Polling을 사용한다.
- 최종 주기는 Gate 4 POC 후 확정 가능하다.

## FR-007 — 프로그램 시작 시 즉시 조회
- 우선순위: Must

## FR-008 — 신규 장치 연결 시 즉시 조회
- 우선순위: Must

## FR-009 — 장치 해제 처리
- 우선순위: Must
- 장치 해제 시 Widget 목록에서 즉시 제거한다.

## FR-010 — 절전 복귀 처리
- 우선순위: Must
- 절전 복귀 시 장치 및 Battery 상태를 재확인한다.

## FR-011 — 개별 장치 숨기기
- 우선순위: Must
- 숨김 상태는 Application 종료 후에도 유지한다.

## FR-012 — Widget 위치 저장 및 복원
- 우선순위: Must
- 저장 위치가 화면 밖이면 화면 내부로 보정한다.

## FR-013 — Always On Top 설정 저장 및 복원
- 우선순위: Must
- 최초 기본값: OFF

## FR-014 — 동일 모델 장치 개별 식별
- 우선순위: Must
- 동일 모델 장치도 각각 독립적인 상태와 숨김 설정을 가져야 한다.

## FR-015 — Windows 로그인 자동 실행
- 우선순위: Should
- 최초 기본값: OFF

## FR-016 — 숨긴 장치 관리
- 우선순위: Must
- 숨긴 장치 목록 확인 및 개별 숨김 해제를 제공한다.

## FR-017 — System Tray 제공
- 우선순위: Must
- Application 실행 중 Windows System Tray에 Application 아이콘을 표시한다.
- 수용 기준:
  1. Application 실행 시 Tray Icon 표시
  2. Application 종료 시 Tray Icon 제거
  3. Tray에서 주요 기능 접근 가능
  4. Tray 처리 오류가 전체 Application 비정상 종료로 이어지지 않음

---

# 4. UI 요구사항

## UIR-001 — Widget 형태 표시
- Must
- 한 개의 간단한 배터리 인디케이터로 표시한다.
- Bluetooth/USB가 함께 있으면 USB 상태를 우선 표시한다.

## UIR-002 — 장치 이름 표시
- Must

## UIR-003 — Battery Gauge
- Must

## UIR-004 — Battery %
- Must

## UIR-005 — 충전 아이콘
- Must

## UIR-006 — 충전 중 연두색 Gauge
- Must

## UIR-007 — Widget Drag
- Must

## UIR-008 — Always On Top UI
- Must
- 기본값 OFF
- Widget 내부에는 조작 UI를 두지 않고 Tray 메뉴에서만 변경한다.

## UIR-009 — Empty State
- Must

## UIR-010 — 조회 중 상태
- Should

## UIR-011 — Battery 미지원 장치 표시
- Deferred to vNext (CHG-002)
- v1.0은 지원 대상 DualSense만 장치 목록에 표시한다.

## UIR-012 — Battery Unknown 표시
- Must

## UIR-013 — System Tray Context Menu
- 우선순위: Must
- 최소 메뉴:
  - Widget 표시
  - Always On Top ON/OFF
  - 숨긴 장치 관리
  - Windows 로그인 자동 실행 ON/OFF
  - 종료

---

# 5. 비기능 요구사항

## NFR-PERF-001 — 유휴 CPU
- Must
- 5분 평균 1% 이하 목표

## NFR-PERF-002 — 작업 중 CPU
- Must
- 순간 5% 이하 목표

## NFR-PERF-003 — Memory
- Must
- 정상 상태 100MB 이하 목표

## NFR-PERF-004 — Widget 표시 시간
- Must
- 2초 이하 목표

## NFR-PERF-005 — 최초 장치 정보 표시
- Must
- 5초 이내 목표

## NFR-STAB-001 — Memory Leak 방지
- Must
- 24시간 기준 10MB 또는 10% 이내 증가 목표

## NFR-STAB-002 — 24시간 안정성
- Must
- 비정상 종료 0회

## NFR-STAB-003 — 72시간 Soak Test
- Must

## NFR-STAB-004 — Resource 정리
- Must

## NFR-STAB-005 — 예외 격리
- Must

## NFR-USAB-001 — 사용자 개입 최소화
- Must

## NFR-MAINT-001 — 장치 유형 확장성
- Must

## NFR-MAINT-002 — 장치별 Provider 분리 가능성
- Should

---

# 6. 인터페이스 요구사항

## IR-001 — Windows Device Interface
- Must

## IR-002 — Event Interface
- Must

## IR-003 — 연결 유형 기술검증
- Must
- Bluetooth / USB / HID / 2.4GHz Dongle

## IR-004 — 장치 고유 식별 검증
- Must

---

# 7. 제약사항

## CR-001 — 지원 운영체제
- Must
- Windows 10 22H2 이상
- Windows 11

## CR-002 — 경량성 우선
- Must

## CR-003 — Event-driven 우선
- Must

## CR-004 — 고빈도 Polling 제한
- Must

---

# 8. 운영 요구사항

## OR-001 — 일반 사용자 실행
- Must

## OR-002 — Release Build
- Must

## OR-003 — Version 관리
- Must
- 첫 정식 Release 후보 v1.0.0

## OR-004 — Installer/Portable 검토
- Should

## OR-005 — Widget Close 시 Application 종료
- Must
- Widget X 버튼 클릭 시 Application 전체 종료
- Tray Icon도 제거
- Event/Timer/Device Resource 정상 정리

---

# 9. System Tray Lifecycle

```text
Widget X 버튼
→ Application 전체 종료
→ Tray Icon 제거

Widget 숨김/최소화
→ Widget만 숨김
→ Application 계속 실행
→ Tray 유지

Tray → Widget 표시
→ Widget 복원

Tray → 종료
→ Application 전체 종료
→ Resource 정리
```

---

# 10. Version 1.0 Out of Scope

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

# 11. Gate 4 POC 필수 검증 항목

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
16. System Tray 구현 가능성 및 Resource 정리 특성
