# POC 기술 스택 평가서

## Candidate A
- Language: C#
- Runtime: .NET 10
- Desktop UI: WPF 후보
- Device API: Windows.Devices.*
- Tray: System.Windows.Forms.NotifyIcon 후보

| 평가 항목 | 가중치(초안) | 결과 | 비고 |
|---|---:|---|---|
| Device API 접근성 | 20 | 조건부 적합 | targeted HID read-only 성공; DeviceClass.All 금지 |
| Battery/Charging | 20 | 조건부 적합 | DualSense bucket/charging 성공; exact 1% 아님 |
| Event 처리 | 15 | 조건부 적합 | event-first/recovery/sleep 성공; 직렬화 필요 |
| Device Identity | 10 | 조건부 적합 | provider fallback 필요 |
| CPU/Memory | 10 | 적합 | 5분 목표 통과 |
| Resource 관리 | 10 | 조건부 적합 | cleanup 통과; 장시간 soak 필요 |
| Tray/Desktop | 5 | 적합 | NotifyIcon lifecycle PASS |
| 배포 | 5 | 조건부 적합 | FDD/SCD 실행 성공; packaging/Win11 필요 |
| 확장성 | 5 | 적합 | provider/parser/normalized state 경계 검증 |

## 최종 권고
- [ ] 채택
- [x] 조건부 채택
- [ ] 대체 기술 검토

조건부 채택은 Gate 4에서 승인됐으며 Gate 5 Architecture 입력이다. Production 구현은
Gate 5 Architecture 승인 전 시작하지 않는다.
