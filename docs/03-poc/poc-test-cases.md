# Gate 4 POC 테스트 케이스

- 문서 버전: 0.2
- 기준: Requirements v1.1

| TC ID | POC | 검증 목적 | 성공 기준 | 결과 |
|---|---|---|---|---|
| POC-TC-001 | A01 | 초기 장치 Enumeration | Device ID/Name 확보 | NOT TESTED |
| POC-TC-002 | A02 | Device Added | 재시작 없이 연결 감지 | NOT TESTED |
| POC-TC-003 | A03 | Device Removed | 재시작 없이 해제 감지 | NOT TESTED |
| POC-TC-004 | A04 | Friendly Name | 사용자 식별 가능한 이름 확보 가능성 판단 | NOT TESTED |
| POC-TC-005 | A05 | 동일 모델 구분 | 두 장치 Identifier 구분 | NOT TESTED |
| POC-TC-006 | A06 | 재연결 Identity | 동일 물리 장치 추적 가능성 판단 | NOT TESTED |
| POC-TC-007 | B01 | Battery % | 지원 장치에서 0~100 확보 | NOT TESTED |
| POC-TC-008 | B02 | Unsupported | 미지원과 오류 구분 | NOT TESTED |
| POC-TC-009 | B03 | Unknown | 조회 실패 시 Unknown 처리 가능 | NOT TESTED |
| POC-TC-010 | B04 | Charging | Charging 상태 판단 | NOT TESTED |
| POC-TC-011 | C01 | Battery Event | Event 수신 여부 | NOT TESTED |
| POC-TC-012 | C02 | Event Latency | 지연시간 확보 | NOT TESTED |
| POC-TC-013 | C03 | Event 누락 | 발생/수신 비교 | NOT TESTED |
| POC-TC-014 | C04 | Polling | Fallback 가능 | NOT TESTED |
| POC-TC-015 | C05 | Safety Polling | 필요 여부 판단 | NOT TESTED |
| POC-TC-016 | C06 | Sleep/Resume | 복귀 후 재조회 | NOT TESTED |
| POC-TC-017 | D01 | Tray Icon | Tray 표시 | PASS |
| POC-TC-018 | D02 | Tray Menu | Context Menu 동작 | PASS |
| POC-TC-019 | D03 | Hide/Restore | Tray에서 Widget 복원 | PASS |
| POC-TC-020 | D04 | X 종료 | Application 전체 종료 | PASS |
| POC-TC-021 | D05 | Tray 정리 | Ghost Icon/Leak 없음 | PASS |
| POC-TC-022 | E01 | Idle CPU | 5분 평균 측정값 확보 | PASS (Tray 0.001%, BT Battery 0.144%) |
| POC-TC-023 | E02 | Memory | Working Set/Private Memory 확보 | PASS (최대 46.43/12.74 MiB) |
| POC-TC-024 | E03 | Handle/Thread | 지속 증가 여부 확인 | PASS (5분 지속 증가 없음) |
| POC-TC-025 | E04 | Startup | 시작시간 측정 | NOT TESTED |
| POC-TC-026 | E05 | OS | Win10/11 차이 기록 | NOT TESTED |
| POC-TC-027 | E06 | Runtime/Deploy | 배포 특성 확인 | NOT TESTED |
| POC-TC-028 | E07 | 기술 적합성 | 채택/조건부/대체 권고 | NOT TESTED |
| **POC-TC-029** | **E08** | **기능별 Resource 증가량** | **기능 추가 단계별 CPU/Memory/Handle/Thread 변화량 확보** | **PASS WITH LIMITATION (별도 POC 간 1차 비교; 동일-host pending)** |
| POC-TC-030 | B05 | DualSense 상태 정규화 | bucket/charging code를 BatteryState로 변환 | PASS |
| POC-TC-031 | B05 | Stale percent 제거 | 일시 실패 시 Percent=null, Availability=Unknown | PASS |
| POC-TC-032 | B05 | Unknown 복구 | 정상 report 재수신 시 Available 복귀 | PASS |
| POC-TC-033 | B05 | Unsupported 구분 | Unsupported와 일시 실패 Unknown 구분 | PASS |
| POC-TC-034 | B05-1 | DualSense OFF/ON lifecycle | OFF/ON 3회에서 remove/add/open/report recovery 확인 | PASS WITH LIMITATION (동일 session recovery; Removed/Added 미발생) |
| POC-TC-035 | B05-1 | Report timeout | 10초 미수신 시 stale percent 제거와 Unknown 전환 | PASS |
| POC-TC-036 | B05-1 | Cleanup | Q 종료 시 watcher/timer/handler/HID handle 정리 | PASS |
| POC-TC-037 | B05-1 | DualSense USB charging transition | 64-byte report에서 NotCharging -> Charging 전환 | PASS |
| POC-TC-038 | B05-1 | Bluetooth report recovery | 동일 HID session에서 Unknown -> Available 복구 | PASS (3회) |
| POC-TC-039 | B05-1 | Timer/input concurrency | 동시 callback의 상태 및 UI 전달 순서 확인 | PASS WITH LIMITATION (직렬화 필요) |
| POC-TC-040 | B06 | Event-first selection | DualSense HID provider가 event-only 선택 | PASS |
| POC-TC-041 | B06 | No device polling | DualSense timer device-read 호출 0회 | PASS |
| POC-TC-042 | B06 | Poll fallback guard | reliable read endpoint가 있을 때만 polling 허용 | PASS |
| POC-TC-043 | B06 | Freshness recovery | timeout stale 제거 후 event로 Available 복구 | PASS |
| POC-TC-044 | C06 | Sleep entry | Available 상태에서 Windows 절전 진입 | PASS |
| POC-TC-045 | C06 | Automatic resume recovery | resume 후 30초 내 자동 Available 복구 | PASS (약 20초) |
| POC-TC-046 | C06 | Read-only reopen fallback | 자동 복구 실패 시 R reopen 결과 | NOT REQUIRED (자동 복구 성공) |
| POC-TC-047 | C06 | Resume cleanup | resume 검증 후 watcher/timer/HID 정리 | PASS |

## Evidence
- Console Log
- `initial-devices.csv`
- OS / .NET 정보
- 장치 모델 및 연결 방식
- Resource CSV
- 재현 절차
