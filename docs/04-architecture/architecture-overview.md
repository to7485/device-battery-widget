# Device Battery Widget — Architecture Overview 초안

문서 버전: 0.1
상태: APPROVED WITH CONDITIONS
범위: v1.0 DualSense Bluetooth + USB

## 1. 기술 스택

- C# / .NET 10 Windows Desktop
- WPF presentation 후보
- Windows Runtime targeted HID discovery/open
- `System.Windows.Forms.NotifyIcon` tray adapter
- framework-dependent 또는 self-contained win-x64 배포

WPF 선택과 배포 mode는 Gate 5에서 최종 승인한다.

## 2. 논리 구성요소

| Design ID | 구성요소 | 책임 |
|---|---|---|
| ARC-001 | Domain | DeviceKey, BatteryState, precision/charging/availability invariant |
| ARC-002 | DualSense HID Provider | targeted Bluetooth/USB discovery, read-only session, report parsing/liveness |
| ARC-003 | State Coordinator | provider event 직렬화, immutable snapshot, UI state 결정 |
| ARC-004 | Freshness Policy | stale/unknown/offline 후보 시간 정책 |
| ARC-005 | Identity Policy | provider-owned stable key와 reconnect correlation |
| ARC-006 | WPF Presentation | widget/view-model, estimated battery 표현, dispatcher boundary |
| ARC-007 | App/Tray Lifecycle | hide/restore/X/Tray Exit 및 전체 resource cleanup |
| ARC-008 | Settings | widget 위치, topmost, hidden devices, autostart 상태 영속화 |
| ARC-009 | Diagnostics/Stability | 예외 격리, structured log, resource/soak 관측 |
| ARC-010 | Deployment | FDD/SCD, version, packaging, OS compatibility |

## 3. 제안 Solution 구조

```text
src/
├─ DeviceBattery.Domain/
├─ DeviceBattery.Application/
├─ DeviceBattery.Infrastructure.Windows/
├─ DeviceBattery.Presentation.Wpf/
└─ DeviceBattery.App/

tests/
├─ DeviceBattery.Domain.Tests/
├─ DeviceBattery.Application.Tests/
└─ DeviceBattery.Infrastructure.Windows.Tests/
```

Domain/Application은 Windows Runtime, WPF, WinForms를 참조하지 않는다.

## 4. Device/Battery 흐름

```text
Targeted DualSense selector
→ HidDevice.FromIdAsync(Read)
→ InputReportReceived
→ DualSense parser
→ status byte가 변경되거나 recovery일 때만 ProviderEvent 생성
→ single-reader State Coordinator
→ immutable DeviceSnapshot
→ WPF Dispatcher에서 ViewModel 반영
```

초당 수백 건의 raw report를 UI queue에 전달하지 않는다. Provider는 모든 valid report에서
`LastValidReportAt`만 원자적으로 갱신하고 battery status가 같으면 event를 생략한다.

## 5. Concurrency 정책

- Provider callback은 blocking UI 작업을 하지 않는다.
- 모든 의미 있는 상태 변경은 single-reader channel에서 순서화한다.
- timer는 직접 UI/state를 변경하지 않고 coordinator command를 발행한다.
- session generation을 event에 포함해 dispose된 이전 session의 late callback을 폐기한다.
- cleanup 순서: watcher stop → timer cancel → callback detach → HID dispose → provider 종료 → channel complete/drain → tray dispose.

이 정책은 POC에서 관찰된 동일 시각 `RECOVERED/TIMEOUT` 로그 경합을 제거한다.

## 6. Freshness / Offline 제안

Gate 5 검토 후보:

1. 마지막 valid report 후 10초: `Availability=Unknown`, `Percent=null`로 stale 값 제거
2. 30초: active widget 목록에서 제거하되 provider session은 유지
3. valid report 복구: 즉시 active 목록에 재추가하고 `Available`
4. Windows Removed: grace 없이 session dispose 및 목록 제거
5. sleep/resume: 기존 session 자동 복구를 30초 기다리고 실패 시 read-only reopen

10초/30초는 Architecture 승인 전 후보값이다. device polling은 하지 않는다.

## 7. Battery 표시 정책 제안

- raw bucket 0..9: 대표값 5..95, `IsEstimated=true`, `Precision=TenPercentBucket`
- raw bucket 10/full: 100, `IsEstimated=false`
- UI는 estimated 상태를 `약 15%`처럼 명시
- Unknown은 이전 percent를 표시하지 않음
- Charging은 번개 아이콘과 연두색 gauge

## 8. Identity 정책

범용 ContainerId 단독 키를 금지한다. v1 provider가 `DeviceKey`를 소유하며 Bluetooth
HID interface identity, transport, VID/PID와 유효한 ContainerId를 조합한다. reconnect
correlation 실패 시 새 device instance로 취급하고 stale instance를 grace 후 제거한다.

## 9. Tray / Application Lifecycle

- Minimize/Hide: WPF window만 숨기고 provider와 tray 유지
- Tray Show: 기존 window 복원
- Widget X: 전체 application 종료
- Tray Exit: 전체 application 종료
- 단일 shutdown coordinator만 실제 dispose를 수행해 이중 정리를 방지

## 10. Settings / Deployment

- 사용자 설정은 `%LocalAppData%/DeviceBatteryWidget/settings.json` 후보
- temp file + atomic replace 방식
- autostart는 기본 OFF이며 packaging 방식 확정 후 adapter 선택
- FDD/SCD 모두 가능; Windows 11 및 packaging 검증 후 운영 mode 결정

## 11. Review Recommendations

- WPF + WinForms NotifyIcon 채택
- 10초 Unknown / 30초 Dormant 정책
- self-contained win-x64 기본 배포, FDD 보조 profile
- unpackaged v1 HKCU Run / packaged StartupTask adapter
- 기본 local log 7일 또는 총 10 MiB, raw HID/전체 Device ID 금지

세부 근거는 `open-decisions-review.md`에 있다. 이 문서는 Production 구현 승인이 아닌
Gate 5 검토 초안이다.
