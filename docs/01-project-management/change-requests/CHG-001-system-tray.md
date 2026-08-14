# CHG-001 — System Tray 기능 추가

## 1. 변경요청 정보

| 항목 | 내용 |
|---|---|
| Change Request ID | CHG-001 |
| 프로젝트 | Device Battery Widget |
| 요청 유형 | 기능 추가 |
| 요청자 | 발주자 |
| 대상 버전 | Version 1.0 제안 |
| 기준 Baseline | Requirements v1.0 |
| 현재 상태 | **Pending Approval** |

---

## 2. 변경 목적

Windows System Tray(알림 영역)에 Device Battery Widget 아이콘을 제공하여
사용자가 Widget 표시, 주요 설정, Application 종료 기능에 빠르게 접근할 수 있도록 한다.

---

## 3. 변경 요구사항 제안

### FR-017 — System Tray 제공
- 제안 우선순위: Must
- Application 실행 중 Windows System Tray에 Application 아이콘을 표시한다.
- 수용 기준:
  1. Application 실행 시 Tray 아이콘이 표시된다.
  2. Application 종료 시 Tray 아이콘이 제거된다.
  3. Tray를 통해 주요 Application 기능에 접근할 수 있다.
  4. Tray 처리 오류가 Application 전체의 비정상 종료를 유발해서는 안 된다.

### UIR-013 — System Tray Context Menu
- 제안 우선순위: Must
- Tray 아이콘의 Context Menu에서 다음 기능을 제공하는 것을 제안한다.
  - Widget 표시
  - Always On Top ON/OFF
  - 숨긴 장치 관리
  - Windows 로그인 자동 실행 ON/OFF
  - 종료

---

## 4. Application Lifecycle 제안

기존 Requirements v1.0의 `OR-005 — Widget Close 시 Application 종료`는 유지한다.

제안 동작:

```text
Widget의 X 버튼
→ Application 전체 종료
→ Tray Icon 제거

Widget 숨김/최소화 기능
→ Widget만 숨김
→ Application과 Tray는 계속 실행

Tray → Widget 표시
→ Widget 복원

Tray → 종료
→ Application 전체 종료
→ Event/Timer/Device Resource/Tray Icon 정리
```

> Widget을 Tray로 숨기는 구체적인 UX(최소화 버튼, 별도 숨기기 메뉴 등)는 UI/UX 설계 단계에서 확정한다.

---

## 5. 변경 영향도 분석

| 영향 영역 | 영향 내용 | 영향 수준 |
|---|---|---|
| 요구사항 | FR/UIR 신규 추가 | 중 |
| UI/UX | Tray Icon 및 Context Menu 설계 필요 | 중 |
| Architecture | Application/Widget Lifecycle 상태 관리 필요 | 중 |
| 설정 | Always On Top, Auto Start, Hidden Device 관리 진입점 연계 | 낮음~중 |
| 구현 | Tray Icon 및 Menu Event 처리 추가 | 중 |
| 테스트 | Tray 표시/복원/종료/설정 연계 테스트 추가 | 중 |
| 안정성 | Tray Event/Native Resource 정리 검증 필요 | 중 |
| 성능 | 유휴 CPU/Memory에 소폭 영향 가능 | 낮음 |
| 일정 | 개발 및 테스트 일정 소폭 증가 예상 | 낮음~중 |

---

## 6. 주요 테스트 추가 제안

- TC-TRAY-001: Application 실행 시 Tray Icon 표시
- TC-TRAY-002: 정상 종료 시 Tray Icon 제거
- TC-TRAY-003: Widget 숨김 시 Tray 유지
- TC-TRAY-004: Tray에서 Widget 다시 표시
- TC-TRAY-005: Tray에서 Always On Top 변경 및 저장
- TC-TRAY-006: Tray에서 자동 실행 ON/OFF 변경
- TC-TRAY-007: Tray에서 숨긴 장치 관리 진입
- TC-TRAY-008: Tray에서 종료 시 Application 및 Resource 완전 종료
- TC-TRAY-009: 반복 실행/종료 후 Ghost Tray Icon 및 Resource Leak 여부 확인

---

## 7. Baseline 반영 규칙

현재 CHG-001은 **발주자 승인 전**이므로 Requirements v1.0 Baseline을 수정하지 않는다.

발주자 승인 시 다음 작업을 수행한다.

1. CHG-001 상태를 `Approved`로 변경
2. Requirements 문서를 v1.1로 개정
3. FR-017 및 UIR-013 정식 요구사항 반영
4. RTM v1.1에 신규 요구사항 추가
5. Decision Log에 CHG-001 승인 결정 기록
6. README의 Open Change Request 상태 갱신
