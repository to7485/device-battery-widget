# Gate 5 Open Decisions Review

작성일: 2026-08-15
상태: REVIEW RECOMMENDATION

## 1. WPF + NotifyIcon

권고: **채택**

- Widget, binding, drag/topmost 및 상태 표현은 WPF가 담당한다.
- Tray는 검증된 `System.Windows.Forms.NotifyIcon` adapter가 담당한다.
- WinForms UI를 WPF 안에 host하지 않고 NotifyIcon component만 shell boundary에서 소유한다.
- 두 UI 기술의 message processing/interop은 Microsoft가 공식 지원한다.

근거:

- Gate 4 NotifyIcon lifecycle/cleanup/ghost icon PASS
- Microsoft WPF/WinForms interop 문서:
  https://learn.microsoft.com/en-us/dotnet/desktop/wpf/advanced/windows-forms-and-wpf-interoperability-input-architecture

## 2. Freshness / Offline

권고: **10초 Unknown / 30초 Dormant(UI 제거)**

- 10초: stale percent 제거, Unknown 표시
- 30초: active 목록에서 제거하되 read-only session은 Dormant로 유지
- report recovery: 즉시 Available/목록 복구
- Windows Removed: 즉시 dispose/제거
- sleep resume 자동 복구가 30초 내 없을 때 read-only reopen 1회

근거:

- active input 136,859 transition 연속, 최대 callback gap 47.232ms
- OFF/ON timeout/recovery 3회 PASS
- sleep/resume 약 20초 자동 복구 PASS

Risk control:

- 값은 내부 policy option으로 정의해 통합/soak 결과로 조정 가능하게 한다.
- 사용자 설정으로 노출하지 않는다.
- duration은 monotonic TimeProvider로 계산한다.

## 3. Deployment

권고: **v1 기본은 self-contained win-x64 + signed installer**

- 일반 사용자 PC에 .NET 10 Desktop Runtime 사전 설치를 요구하지 않는다.
- Gate 4에서 FDD/SCD 모두 10/10 startup PASS였고 runtime resource 차이는 핵심 blocker가 아니었다.
- SCD 117.08 MiB 증가는 수용하되 installer/업데이트 정책으로 관리한다.
- runtime 보안 수정은 앱을 새 runtime으로 republish해야 하므로 Release 운영 책임으로 둔다.

대안:

- FDD는 관리형 환경/개발 배포 profile로 유지한다.
- single-file/trimming은 WPF/WinRT 호환성 검증 전 기본 적용하지 않는다.

근거:

- Microsoft .NET publishing overview:
  https://learn.microsoft.com/en-us/dotnet/core/deploying/deploy-with-cli
- Microsoft .NET runtime-specific deployment behavior:
  https://learn.microsoft.com/en-us/dotnet/core/compatibility/sdk/8.0/runtimespecific-app-default
- Microsoft single-file deployment:
  https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview

## 4. Windows Login Auto Start

권고: **adapter 분리, unpackaged v1은 HKCU Run**

```text
IAutoStartService
├─ RegistryRunAutoStartService (unpackaged/signed installer)
└─ PackagedStartupTaskService (future MSIX)
```

- 기본 OFF
- 사용자가 UI에서 명시적으로 ON/OFF
- 현재 사용자 `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`만 사용
- 관리자 권한 불필요
- 실행 경로를 quote하고 자신의 value만 생성/삭제
- Task Manager 등 외부에서 비활성화된 상태를 다시 읽어 UI에 반영
- packaged mode에서는 manifest startupTask/StartupTask API adapter 사용

Microsoft는 Run key 실행 시점이 지연될 수 있고 순서를 보장하지 않는다고 명시한다.
따라서 로그인 직후 즉시 장치 표시를 보장하는 요구로 해석하지 않는다.

근거:

- Run/RunOnce:
  https://learn.microsoft.com/en-us/windows/win32/setupapi/run-and-runonce-registry-keys
- Packaged desktop startup task:
  https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/desktop-to-uwp-extensions

## 5. Local Diagnostics

권고: **기본 최소 로그 + 제한 보존**

- `%LocalAppData%/DeviceBatteryWidget/logs`
- 기본 7일 또는 총 10 MiB 중 먼저 도달한 기준으로 rotation
- raw HID report, MAC, 전체 DeviceInformation.Id 저장 금지
- DeviceKey는 진단용 short hash로 기록
- battery percent/state, lifecycle transition, exception fingerprint, resource summary만 기록
- 상세 debug log는 사용자가 명시적으로 활성화하며 자동 만료

## 6. 결론

위 다섯 권고를 Architecture baseline으로 승인할 것을 제안한다. 승인 전 상태는
`Recommended`이며 Production 구현을 시작하지 않는다.
