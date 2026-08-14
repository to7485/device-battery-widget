# Device Battery Widget

Windows PC에 연결된 주변 장치의 배터리 상태를 표시하는 경량 데스크톱 위젯 프로젝트입니다.

## Project Status

- Gate 1 — 프로젝트 착수: **Approved**
- Gate 2 — 프로젝트 수행계획: **Approved**
- Gate 3 — 요구사항 정의 및 분석: **Approved**
- Requirements Baseline: **v1.1**
- CHG-001 — System Tray 기능 추가: **Approved**
- Gate 4 — POC / 기술 타당성 검증: **In Progress**

## Core Scope

- 지원 OS: Windows 10 22H2 이상, Windows 11
- 우선 지원 장치: 마우스, 키보드, 게임 컨트롤러, 헤드셋
- 향후 장치 유형 확장 가능 구조
- Battery Event 지원 시 Event-driven 우선
- Event 미지원 시 Polling Fallback
- 프로그램 시작/신규 연결/절전 복귀 시 즉시 상태 조회
- Battery 미지원 장치도 UI 표시 가능
- 개별 장치 숨김 및 숨김 상태 영속화
- 숨긴 장치 목록 확인 및 개별 숨김 해제
- Widget 마지막 위치 저장/복원
- Always On Top 기본값 OFF 및 설정 영속화
- Windows 로그인 자동 실행 옵션 제공, 기본값 OFF
- 동일 모델 장치 개별 식별
- Widget X 버튼 클릭 시 Application 전체 종료
- System Tray 제공

## System Tray

Tray Context Menu 최소 기능:

- Widget 표시
- Always On Top ON/OFF
- 숨긴 장치 관리
- Windows 로그인 자동 실행 ON/OFF
- 종료

Lifecycle:

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
```

## Performance Targets

| 항목 | 목표 |
|---|---:|
| 유휴 CPU | 5분 평균 1% 이하 |
| 작업 중 CPU | 순간 5% 이하 목표 |
| Memory | 정상 상태 100MB 이하 목표 |
| 24시간 Memory 증가 | 10MB 또는 10% 이내 목표 |
| 24시간 안정성 | 비정상 종료 0회 |
| Release Candidate | 72시간 Soak Test |
| Event 미지원 장치 Polling | 기본 30초 |
| Widget 표시 | 실행 후 2초 이하 |
| 최초 장치 정보 표시 | 실행 후 5초 이내 목표 |

> CPU/Memory, Polling, Event 신뢰성 관련 최종 Baseline은 Gate 4 POC 결과 후 발주자 승인으로 확정합니다.
