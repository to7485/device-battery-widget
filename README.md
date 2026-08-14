# Device Battery Widget

Windows PC에 연결된 주변 장치의 배터리 상태를 표시하는 경량 데스크톱 위젯 프로젝트입니다.

## Project Status

- Gate 1 — 프로젝트 착수: **Approved**
- Gate 2 — 프로젝트 수행계획: **Approved**
- Gate 3 — 요구사항 정의 및 분석: **Approved**
- Requirements Baseline: **v1.0**
- Gate 4 — POC / 기술 타당성 검증: **Pending**
- Open Change Request: **CHG-001 — System Tray 기능 추가 (Pending Approval)**

## Project Goal

Windows 10 22H2 이상 및 Windows 11 환경에서 마우스, 키보드, 게임 컨트롤러, 헤드셋을 우선 지원하며,
지원 가능한 장치의 이름, 배터리 잔량, 충전 상태를 데스크톱 위젯으로 표시합니다.

배터리 갱신은 **Event-driven 방식 우선**, 이벤트 미지원 장치에 대해서만 **Polling Fallback**을 사용합니다.

## Core Decisions

- 지원 OS: Windows 10 22H2 이상, Windows 11
- 우선 지원 장치: 마우스, 키보드, 게임 컨트롤러, 헤드셋
- 향후 장치 유형 확장 가능 구조
- Battery Event 지원 시 Event-driven 우선
- Event 미지원 시 기본 30초 Polling
- 프로그램 시작/신규 연결/절전 복귀 시 즉시 상태 조회
- 배터리 미지원 장치도 UI에 표시
- 사용자는 개별 장치를 숨길 수 있으며 숨김 상태는 영속화
- 사용자는 숨긴 장치 목록을 확인하고 개별 숨김을 해제 가능
- Widget 마지막 위치 저장/복원
- Always On Top 기본값 OFF, 이후 마지막 설정 저장/복원
- Windows 로그인 자동 실행 옵션 제공, 기본값 OFF
- Widget Close 시 Application 전체 종료
- 동일 모델 장치도 개별 장치로 식별

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

## Change Management

Requirements v1.0은 Gate 3 승인 Baseline입니다.

Gate 3 승인 이후 신규 요구사항 또는 기존 요구사항 변경은 `CHG-xxx` 변경요청으로 관리합니다.
현재 `CHG-001 — System Tray 기능 추가`가 발주자 승인 대기 상태입니다.
