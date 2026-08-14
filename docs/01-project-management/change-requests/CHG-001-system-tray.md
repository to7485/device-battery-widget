# CHG-001 — System Tray 기능 추가

## 변경요청 정보

| 항목 | 내용 |
|---|---|
| Change Request ID | CHG-001 |
| 프로젝트 | Device Battery Widget |
| 요청 유형 | 기능 추가 |
| 요청자 | 발주자 |
| 대상 버전 | Version 1.0 |
| 기준 Baseline | Requirements v1.0 |
| 승인 결과 | **Approved** |
| 반영 Baseline | **Requirements v1.1** |

## 승인 내용

### FR-017 — System Tray 제공
- 우선순위: Must
- Application 실행 중 Windows System Tray에 Application 아이콘을 표시한다.

### UIR-013 — System Tray Context Menu
- 우선순위: Must
- 최소 메뉴:
  - Widget 표시
  - Always On Top ON/OFF
  - 숨긴 장치 관리
  - Windows 로그인 자동 실행 ON/OFF
  - 종료

## Lifecycle

기존 OR-005를 유지한다.

```text
Widget X 버튼
→ Application 전체 종료
→ Tray Icon 제거

Widget 숨김/최소화
→ Application 계속 실행
→ Tray 유지

Tray → Widget 표시
→ Widget 복원

Tray → 종료
→ Application 전체 종료
→ Event/Timer/Device Resource/Tray Icon 정리
```

## 영향도

| 영역 | 영향 |
|---|---|
| 요구사항 | FR-017, UIR-013 추가 |
| UI/UX | Tray Icon 및 Context Menu 설계 필요 |
| Architecture | Widget/Application Lifecycle 관리 필요 |
| 구현 | Tray Icon 및 Menu Event 처리 추가 |
| 테스트 | Tray 표시/복원/종료/설정 연계 테스트 추가 |
| 안정성 | Tray Resource 정리 및 Ghost Icon 확인 필요 |
| 일정 | 소폭 증가 가능 |

## 상태

CHG-001은 발주자 승인 완료되었으며 Requirements v1.1에 반영되었다.
