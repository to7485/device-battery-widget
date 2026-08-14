# Device Battery Widget

Windows PC에 연결된 지원 장치의 배터리 상태를 표시하는 데스크톱 위젯 프로젝트입니다.

## Project Status

- Gate 1: 프로젝트 착수 승인 완료
- Gate 2: 프로젝트 수행계획 승인 완료
- Next: Gate 3 요구사항 정의 및 분석

## Repository Structure

```text
device-battery-widget/
├─ docs/
│  ├─ 01-project-management/
│  ├─ 02-requirements/
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

## Development Policy

- Windows Desktop Application
- Event-driven battery updates first
- Polling fallback for unsupported devices
- SI-style gate approval process
