# Gate 4 POC 계획서

## 문서 정보
- 프로젝트: Device Battery Widget
- 단계: Gate 4 — POC / 기술 타당성 검증
- 버전: 0.1
- 상태: In Progress
- 기준: Requirements Baseline v1.1

## 목적
본개발 전에 핵심 기술 가능성과 제약을 실제 코드와 장치로 검증한다.

## 핵심 검증 질문
1. Windows에서 마우스, 키보드, 게임 컨트롤러, 헤드셋을 탐색할 수 있는가?
2. 사용자 친화적인 장치 이름을 얻을 수 있는가?
3. Battery %와 Charging 상태를 얻을 수 있는가?
4. Battery 변경 Event를 받을 수 있는가?
5. Event 미지원 장치에서 Polling Fallback이 가능한가?
6. 동일 모델 장치를 개별 식별할 수 있는가?
7. 재연결 후 동일 장치를 추적할 수 있는가?
8. System Tray를 안정적으로 구현할 수 있는가?
9. 후보 기술 스택이 CPU/Memory 목표에 적합한가?

## POC Track
### A — Device Enumeration / Identity
- A01 초기 장치 Enumeration
- A02 Device Added
- A03 Device Removed
- A04 Friendly Name
- A05 동일 모델 개별 식별
- A06 재연결 Identity

### B — Battery / Charging
- B01 Battery %
- B02 Unsupported 판별
- B03 Unknown 처리
- B04 Charging 상태

### C — Event / Polling
- C01 Battery Event
- C02 Event Latency
- C03 Event 누락
- C04 Polling Fallback
- C05 Safety Polling 필요성
- C06 Sleep/Resume

### D — Desktop / Tray
- D01 Tray Icon
- D02 Tray Context Menu
- D03 Widget Hide/Restore
- D04 X 버튼 종료
- D05 Tray Resource 정리

### E — Technology / Performance
- E01 CPU
- E02 Memory
- E03 Handle/Thread
- E04 Startup
- E05 Windows 10 22H2 / Windows 11
- E06 Runtime/Deploy
- E07 기술 적합성

## 1차 기술 후보
- C#
- .NET 10
- WPF 후보
- Windows Runtime `Windows.Devices.*`
- System Tray 후보: `System.Windows.Forms.NotifyIcon`
- 필요 시 Win32/HID Interop

> 최종 기술 스택이 아니라 POC 1차 후보이다.

## 판정
- PASS
- PASS WITH LIMITATION
- FAIL
- NEED ALTERNATIVE

## Entry Criteria
- Gate 1~3 승인
- Requirements v1.1
- CHG-001 승인
- Git Repository 준비
- Windows 테스트 PC 확보

## Exit Criteria
- POC 테스트 결과
- Device Matrix
- Battery/Event 지원 여부
- Device Identity 분석
- Tray 결과
- CPU/Memory/Handle/Thread 측정
- 기술 스택 평가
- Known Limitations
- Gate 4 검토자료

## Gate 1 성능 목표
| 항목 | 목표 |
|---|---:|
| 유휴 CPU | 5분 평균 1% 이하 |
| 작업 CPU | 순간 5% 이하 목표 |
| Memory | 100MB 이하 목표 |
| 24h Memory 증가 | 10MB 또는 10% 이내 |
| Widget 표시 | 2초 이하 |
| 최초 장치 정보 | 5초 이내 |
