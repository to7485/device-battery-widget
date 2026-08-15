# Gate 4 POC 계획서

## 문서 정보
- 프로젝트: Device Battery Widget
- 단계: Gate 4 — POC / 기술 타당성 검증
- 문서 버전: 0.2
- 상태: In Progress
- 기준: Requirements Baseline v1.2 (CHG-002 DualSense-only v1.0 scope)

## POC 목적
본개발 전에 핵심 기술의 구현 가능성, 장치별 제약, 실행 중 자원 사용량을 실제 Windows 환경에서 검증한다.

설치 파일 크기는 본 Gate의 핵심 성능 판단 기준으로 사용하지 않는다. 핵심 성능 항목은 실행 중 프로세스의 CPU, Working Set, Private Memory, Handle, Thread 및 장시간 실행 시 지속 증가 여부이다.

## POC Track
### A — Device Enumeration / Identity
- A01 초기 Device Enumeration
- A02 Device Added
- A03 Device Removed
- A04 Friendly Name
- A05 동일 모델 개별 식별
- A06 재연결 Identity

### B — Battery / Charging
- B01 Battery Percentage
- B02 Unsupported 판별
- B03 Unknown 처리 가능성
- B04 Charging 상태

### C — Event / Polling
- C01 Battery Event
- C02 Event Latency
- C03 Event 누락
- C04 Polling Fallback
- C05 Safety Polling 필요성
- C06 Sleep / Resume

### D — Desktop / Tray
- D01 Tray Icon
- D02 Tray Context Menu
- D03 Widget Hide / Restore
- D04 X 버튼 Application 종료
- D05 Tray Resource 정리

### E — Technology / Performance
- E01 Idle CPU
- E02 Memory Baseline
- E03 Handle / Thread
- E04 Startup Time
- E05 Windows 10 22H2 / Windows 11
- E06 Runtime / Deployment 특성
- E07 기술 스택 종합 적합성
- **E08 기능별 Resource Increment**

## POC-E08 — 기능별 Resource Increment
최종 프로그램만 한 번 측정하지 않고 기능을 단계적으로 추가하여 어느 기능이 자원을 증가시키는지 확인한다.

| Stage | 기능 | CPU | Working Set | Private Memory | Handles | Threads |
|---|---|---:|---:|---:|---:|---:|
| PERF-BASE | 최소 Desktop App | TBD | TBD | TBD | TBD | TBD |
| PERF-DEVICE | + DeviceWatcher | TBD | TBD | TBD | TBD | TBD |
| PERF-BATTERY | + Battery Monitor | TBD | TBD | TBD | TBD | TBD |
| PERF-EVENT | + Battery Event | TBD | TBD | TBD | TBD | TBD |
| PERF-TRAY | + System Tray | TBD | TBD | TBD | TBD | TBD |
| PERF-IDLE | 전체 POC 유휴 | TBD | TBD | TBD | TBD | TBD |

## 판단 원칙
- Self-contained 여부 자체를 Runtime Memory 증가 원인으로 단정하지 않는다.
- 실제 프로세스 수치로 판단한다.
- 절대 Memory 사용량과 Memory Leak을 구분한다.
- Memory가 일정 수준에서 안정화되면 허용 가능성을 검토한다.
- 시간이 지날수록 지속 증가하면 심각한 안정성 Risk로 본다.

## Gate 1 성능 목표
| 항목 | 목표 |
|---|---:|
| 유휴 CPU | 5분 평균 1% 이하 |
| 작업 순간 CPU | 5% 이하 목표 |
| 정상 Memory | 100MB 이하 목표 |
| 24시간 Memory 증가 | 10MB 또는 10% 이내 목표 |
| 24시간 비정상 종료 | 0회 |
| Release Candidate | 72시간 Soak Test |

## 1차 기술 후보
- C#
- .NET 10
- WPF 후보
- Windows Runtime `Windows.Devices.*`
- Tray 후보: `System.Windows.Forms.NotifyIcon`
- 필요 시 Win32 / HID Interop

아직 기술 스택 승인 상태가 아니다.

## 현재 실행 대상
1. POC-A01 — 초기 Enumeration
2. POC-A02 — Added
3. POC-A03 — Removed
4. POC-A04 — Name / ID 확인

이 결과를 확보한 뒤 A05/A06 Identity 검증으로 진행한다.
