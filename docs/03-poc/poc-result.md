# Gate 4 POC 결과서

- 상태: Gate 4 APPROVED WITH CONDITIONS
- 기준: Requirements Baseline v1.2 / CHG-002

| Track | 판정 | 핵심 결과 | 제약 |
|---|---|---|---|
| A Device / Identity | PASS WITH LIMITATION | targeted enumeration, name, interface lifecycle, reconnect identity 확인 | ContainerId 단독 범용 키 불가; paired endpoint 유지 가능 |
| B Battery / Charging | PASS WITH LIMITATION | DualSense BT/USB HID battery bucket 및 charging, normalized state 확인 | exact 1%가 아닌 10% bucket 대표값 |
| C Event / Polling | PASS WITH LIMITATION | event-first, sequence 136,859건 연속/누락 0, device polling 0회, timeout/recovery 3회, sleep/resume 자동 복구 | Production timeout/콜백 직렬화 설계 필요 |
| D Tray / Lifecycle | PASS | tray/menu/hide/restore/두 종료 경로/cleanup/ghost icon 없음 | Production UI 및 실제 설정 저장은 후속 단계 |
| E Performance / Technology | PASS WITH LIMITATION | 5분 CPU/Memory 목표 통과, startup 10/10, FDD/SCD publish 실행 | Win10만 검증; 장시간 soak/Win11/packaging 미실시 |

## Known Limitations

- v1.0 지원 범위는 DualSense Bluetooth `054C:0CE6`이며 다른 장치는 vNext다.
- 배터리는 10% bucket 기반 대표값이며 UI에 estimated precision 표현이 필요하다.
- paired Bluetooth HID endpoint는 controller OFF 시 Removed/Added를 내지 않을 수 있다.
- 10초 freshness timeout은 POC 값이며 Production 연결 해제 정책이 아니다.
- timer/input callback 상태 전달을 직렬화해 UI flicker를 방지해야 한다.
- `DeviceInformation.GetAqsFilterFromDeviceClass(DeviceClass.All)` 경로는 이 환경에서 native AccessViolation 이력이 있어 사용하지 않는다.
- Windows 11, 24시간 leak, 72시간 soak 및 installer/portable packaging은 미검증이다.

## 성능 Baseline 조정 제안

현재 목표를 유지한다. 5분 실측은 다음과 같다.

- Tray: 평균 CPU 0.001%, Working Set 42.16 MiB
- DualSense Bluetooth monitor: 평균 CPU 0.144%, Working Set 46.43 MiB

목표를 상향 조정할 근거는 없다. 24시간/72시간 결과 전에는 leak 기준을 확정하지 않는다.

## 기술 스택 권고

**조건부 채택 권고**

- C# / .NET 10 Windows Desktop
- targeted `Windows.Devices.HumanInterfaceDevice` read-only provider
- provider-independent normalized `BatteryState`
- event-first, 신뢰 가능한 read endpoint가 있을 때만 poll fallback
- `System.Windows.Forms.NotifyIcon` tray integration
- WPF는 Production UI 후보이며 Architecture Gate에서 최종 승인
- framework-dependent와 self-contained 모두 가능하며 배포 방식은 운영정책에서 선택

## Gate 4 판정 제안

**APPROVE WITH CONDITIONS**

조건:

1. v1.0은 CHG-002대로 DualSense Bluetooth만 지원한다.
2. Production 설계에서 timeout, callback 직렬화, estimated precision UI를 확정한다.
3. Release 전 Windows 11, 장시간 soak 및 packaging 검증을 수행한다.
4. Receiver/vendor 결과는 vNext 증적으로 보존하며 v1.0에 재도입하지 않는다.

2026-08-15 발주자 승인에 따라 Gate 4는 `APPROVED WITH CONDITIONS`로 종료됐다.
조건은 Gate 5 Architecture와 Release 전 검증 항목으로 이관한다.
