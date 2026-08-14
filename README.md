# Device Battery Widget

Windows PC에 연결된 주변 장치의 배터리 상태를 표시하는 경량 데스크톱 위젯 프로젝트입니다.

## Project Status

- Gate 1 — 프로젝트 착수: **Approved**
- Gate 2 — 프로젝트 수행계획: **Approved**
- Gate 3 — 요구사항 정의 및 분석: **In Progress**
- Gate 4 — POC / 기술 타당성 검증: **Pending**

## Project Goal

Windows 10 22H2 이상 및 Windows 11 환경에서 마우스, 키보드, 게임 컨트롤러, 헤드셋을 우선 지원하며, 지원 가능한 장치의 이름, 배터리 잔량, 충전 상태를 위젯 형태로 표시합니다.

배터리 상태 갱신은 **Event-driven 방식을 우선**하고, 이벤트를 지원하지 않는 장치에 대해서만 Polling 방식을 사용합니다.

## Repository Structure

```text
device-battery-widget/
├─ docs/
│  ├─ 01-project-management/
│  │  └─ decision-log.md
│  ├─ 02-requirements/
│  │  ├─ requirements.md
│  │  └─ rtm.md
│  ├─ 03-poc/
│  ├─ 04-analysis/
│  ├─ 05-architecture/
│  ├─ 06-design/
│  └─ 07-test/
├─ poc/
├─ src/
├─ tests/
├─ tools/
├─ .gitignore
└─ README.md
```

## Development Principles

- SI 방식의 단계별 Gate 승인 절차 적용
- 승인된 산출물은 Baseline으로 관리
- 요구사항 → 설계 → 구현 → 테스트 추적성 유지
- Event-driven 우선, Event 미지원 시 Polling Fallback
- 경량성, 장시간 안정성, Resource Leak 방지를 핵심 품질 목표로 설정
- 요구사항 Baseline 승인 이후 변경은 Change Request 절차로 관리

## Current Performance Targets

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

> CPU/Memory 및 Polling 관련 최종 Baseline은 Gate 4 POC 결과 후 발주자 승인으로 확정합니다.
