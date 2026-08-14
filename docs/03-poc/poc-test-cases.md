# Gate 4 POC 테스트 케이스

| TC ID | POC | 검증 목적 | 성공 기준 | 결과 |
|---|---|---|---|---|
| POC-TC-001 | A01 | 초기 장치 Enumeration | Device ID/Name 확보 | NOT TESTED |
| POC-TC-002 | A02 | Device Added | 재시작 없이 연결 감지 | NOT TESTED |
| POC-TC-003 | A03 | Device Removed | 재시작 없이 해제 감지 | NOT TESTED |
| POC-TC-004 | A04 | Friendly Name | 사용자 식별 가능한 이름 확보 | NOT TESTED |
| POC-TC-005 | A05 | 동일 모델 구분 | 두 장치 Identifier 구분 | NOT TESTED |
| POC-TC-006 | A06 | 재연결 Identity | 동일 장치 추적 가능성 판단 | NOT TESTED |
| POC-TC-007 | B01 | Battery % | 지원 장치에서 0~100 확보 | NOT TESTED |
| POC-TC-008 | B02 | Unsupported | 미지원과 오류 구분 | NOT TESTED |
| POC-TC-009 | B03 | Unknown | 조회 실패 시 Unknown 처리 가능 | NOT TESTED |
| POC-TC-010 | B04 | Charging | Charging 상태 판단 | NOT TESTED |
| POC-TC-011 | C01 | Battery Event | Event 수신 여부 | NOT TESTED |
| POC-TC-012 | C02 | Event Latency | 지연시간 수치 확보 | NOT TESTED |
| POC-TC-013 | C03 | Event 누락 | 발생/수신 횟수 비교 | NOT TESTED |
| POC-TC-014 | C04 | Polling | Fallback 가능 | NOT TESTED |
| POC-TC-015 | C05 | Safety Polling | 필요 여부 판단 | NOT TESTED |
| POC-TC-016 | C06 | Sleep/Resume | 복귀 후 재조회 | NOT TESTED |
| POC-TC-017 | D01 | Tray Icon | Tray 표시 | NOT TESTED |
| POC-TC-018 | D02 | Tray Menu | Context Menu 동작 | NOT TESTED |
| POC-TC-019 | D03 | Hide/Restore | Tray에서 Widget 복원 | NOT TESTED |
| POC-TC-020 | D04 | X 종료 | Application 전체 종료 | NOT TESTED |
| POC-TC-021 | D05 | Tray 정리 | Ghost Icon/Leak 없음 | NOT TESTED |
| POC-TC-022 | E01 | Idle CPU | Baseline 확보 | NOT TESTED |
| POC-TC-023 | E02 | Memory | Baseline 확보 | NOT TESTED |
| POC-TC-024 | E03 | Handle/Thread | 지속 증가 여부 확인 | NOT TESTED |
| POC-TC-025 | E04 | Startup | 시작 시간 확보 | NOT TESTED |
| POC-TC-026 | E05 | OS | Win10/11 차이 기록 | NOT TESTED |
| POC-TC-027 | E06 | Runtime/Deploy | 배포 옵션 비교 | NOT TESTED |
| POC-TC-028 | E07 | 기술 적합성 | 채택/조건부/대체 권고 | NOT TESTED |

## Evidence
- Console 로그
- Screenshot
- OS Build
- 장치 모델/연결 방식
- CPU/Memory/Handle/Thread 측정값
- 재현 절차
