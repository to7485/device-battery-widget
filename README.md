# Gate 4 POC Update 04

POC-A 실장비 결과를 반영하고 POC-B Battery/Charging 검증으로 이동하기 위한 업데이트입니다.

## 다음 실행

AULA F87Pro를 Bluetooth로 연결한 상태에서:

```powershell
cd .\poc\DeviceBattery.Poc.BatteryProbe
dotnet clean
dotnet run
```

출력 전체를 저장해서 분석합니다.
주변장치 Battery Controller가 없더라도 정상적인 POC 결과입니다.

## Update 05 — POC-B02 BLE GATT Battery Probe

POC-B01에서 Windows Generic Battery Controller가 주변기기를 0개 반환하여 해당 경로를 `FAIL / NEED ALTERNATIVE`로 기록했습니다.

새 프로젝트:

```text
poc/DeviceBattery.Poc.BleBatteryProbe
```

실행:

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.BleBatteryProbe
dotnet clean
dotnet run
```

이 POC는 표준 BLE Battery Service `0x180F`와 Battery Level `0x2A19`를 읽고 Notify/Indicate 가능 여부를 확인합니다.

## Update 06 — POC-B02 PASS / POC-B03 Windows.Gaming.Input Battery Probe

AULA F87Pro Bluetooth에서 BLE GATT `0x180F -> 0x2A19` 배터리 100% Read와 Notify 구독이 재연결 후에도 재현되어 POC-B02를 PASS로 기록했습니다.

DualSense Bluetooth는 표준 BLE Battery Service 검색에 나타나지 않았습니다. Raw HID 역분석을 바로 시작하지 않고, 먼저 Microsoft 공개 API인 `Windows.Gaming.Input.RawGameController/Gamepad.TryGetBatteryReport()`를 검증합니다.

새 프로젝트:

```text
poc/DeviceBattery.Poc.GameControllerBatteryProbe
```

실행:

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.GameControllerBatteryProbe
dotnet clean
dotnet run --project .\DeviceBattery.Poc.GameControllerBatteryProbe.csproj
```

DualSense를 Bluetooth로 연결한 상태에서 전체 출력을 저장합니다.

## Update 08 — POC-B03-2 DualSense HID Battery

Validated B03-1 result: Bluetooth DualSense is visible through `Windows.Gaming.Input`, but both RawGameController and Gamepad `TryGetBatteryReport()` returned `null`.

Added:

- `poc/DeviceBattery.Poc.DualSenseHidBatteryProbe`
- `docs/03-poc/dualsense-hid-battery-poc-plan.md`
- `docs/03-poc/poc-status-update-07.md`

The new probe opens the DualSense HID collection read-only and parses the battery/charging status from incoming full HID reports using the upstream `hid-playstation` report layout as technical evidence.

## Update 09 — POC-B03-2 validated / POC-B04-1 Receiver HID Discovery

DualSense Bluetooth HID POC-B03-2 was validated on real hardware:

- Report ID `0x31`
- 78-byte Bluetooth full input report
- battery bucket parsed
- real USB charging cable connection changed charging code from `0x0` to `0x1`

Final B03-2 status: `PASS WITH LIMITATION` because battery percentage is a coarse bucket estimate.

Added:

```text
poc/DeviceBattery.Poc.ReceiverHidProbe
docs/03-poc/receiver-hid-poc-plan.md
docs/03-poc/poc-status-update-08.md
```

Run B04-1 with the G703/Corsair receivers connected and send the complete first-run output before doing additional power-cycle tests.

## Update 10 — POC-B04-1 validated / POC-B04-2 approved

Real-hardware read-only discovery found 7 Logitech and 5 Corsair HID top-level collections. Both receivers exposed readable vendor-defined collections and passive reports correlated with peripheral OFF/ON transitions.

- Logitech `FF00/0001`, report `0x10`: byte offset 4 changed `0x62 (OFF) <-> 0xA2 (ON)` twice.
- Corsair `FF42/0002`, report `0x03`: repeated `OFF -> ON transition -> ON initialized` report sequence twice.

B04-1 is frozen as `PASS WITH LIMITATION`; B04-2 is approved to begin with passive battery correlation. No output report, feature command, or vendor request has been sent.

## Update 11 — CHG-002 / v1.0 DualSense-only scope

The approved v1.0 device scope is now Sony DualSense over Bluetooth (`054C:0CE6`) only. Mouse, keyboard, headset, other controllers, and receiver battery work are deferred to a later release. Existing BLE and receiver POC evidence remains preserved; B04-2 is stopped without sending vendor commands.

## Update 12 — POC-B05 Normalized BatteryState

Added `poc/DeviceBattery.Poc.NormalizedBatteryState` for the DualSense-only v1.0 scope. All 8 deterministic normalization/state-transition cases passed, including stale-percent removal on read failure and recovery from Unknown. B05 is `PASS WITH LIMITATION` pending real-hardware lifecycle/timeout integration tests.

## Update 13 — DualSense Lifecycle / Timeout Probe

Added `poc/DeviceBattery.Poc.DualSenseLifecycleProbe`. Read-only hardware testing passed USB charging transitions, Bluetooth timeout/recovery across three cycles, stale-percent removal, and cleanup. Final result is `PASS WITH LIMITATION`: paired Bluetooth did not emit Removed/Added, the 10-second timeout is POC-only, and Production state delivery must serialize timer/input callbacks.

## Update 14 — POC-B06 Event-first Policy

Added `poc/DeviceBattery.Poc.EventFirstPolicy`. The deterministic policy matrix passed 8/8: DualSense selects event-only monitoring, freshness timers perform zero device reads, stale percentage is cleared on timeout, and polling is permitted only for providers with a reliable read endpoint. Result: `PASS WITH LIMITATION` pending Production timeout and callback-serialization design.

## Update 15 — POC-C06 Sleep / Resume Ready

The approved sleep/resume hardware test reuses the completed read-only `DeviceBattery.Poc.DualSenseLifecycleProbe`. It first checks automatic event recovery after Windows resume and uses the existing `R` read-only reopen only as a fallback. No new device command path was introduced.

Hardware result: `PASS`. After Windows resume, the existing Bluetooth HID session recovered automatically in about 20 seconds without `R`, watcher recreation, or application restart. Final Available state and cleanup were verified.

## Update 16 — POC-D System Tray / Lifecycle

Added `poc/DeviceBattery.Poc.SystemTrayLifecycle`. Hardware testing passed tray display, context menu, minimize/hide, double-click and menu restore, Always On Top, Widget X exit, tray-menu exit, explicit cleanup, and absence of a ghost icon. Final result: `PASS`.

## Update 17 — POC-E Resource Baselines

Added `poc/DeviceBattery.Poc.ResourceSampler`. Five-minute Release measurements passed the idle targets: Tray averaged 0.001% CPU with 42.16 MiB Working Set; Bluetooth-only DualSense monitoring averaged 0.144% CPU with 46.43 MiB Working Set. Neither run showed Handle/Thread growth. Overall result is `PASS WITH LIMITATION` because long soak, startup, OS, deployment, and same-host incremental measurements remain.

## Update 18 — POC-E04/E06 Startup and Deployment

Added `poc/DeviceBattery.Poc.StartupSampler`. On Windows 10 22H2, framework-dependent and self-contained win-x64 tray publishes both reached a visible window in 10/10 runs, averaging 68.8 ms and 67.5 ms. Publish sizes were 0.18 MiB and 117.08 MiB. Result: `PASS WITH LIMITATION` pending Windows 11 and installer/portable packaging validation.

## Update 19 — Gate 4 Technical Evaluation Draft

The Gate 4 result and technology evaluation were reconciled against hardware evidence. At this checkpoint the recommendation was `APPROVE WITH CONDITIONS`; the later approval is recorded in Update 21. Production implementation had not started.

## Update 20 — POC-C03 Event Continuity

Added `poc/DeviceBattery.Poc.DualSenseEventContinuityProbe`. Passive Bluetooth HID testing identified the WinRT continuity counter as offset 7 bits 2–5. Across 288.675 seconds and 136,859 transitions, the modulo-16 sequence was 100% continuous with zero duplicates, gaps, estimated missing reports, or unsupported shapes. Result: `PASS`.

## Update 21 — Gate 4 Approved / Gate 5 Started

Gate 4 is `APPROVED WITH CONDITIONS`. Gate 5 Architecture Design is now in progress. Production implementation remains blocked until the architecture baseline is explicitly approved.

## Update 22 — Gate 5 Architecture Ready for Review

Provider contracts, state machine, concurrency, test strategy, RTM mappings, and open-decision recommendations are complete drafts. The recommendation is WPF + NotifyIcon, 10-second Unknown / 30-second Dormant, self-contained win-x64 default deployment, an autostart adapter using HKCU Run for unpackaged v1, and privacy-limited local diagnostics. Production remains blocked pending owner approval.

## Update 23 — Gate 5 Approved / Gate 6 Foundation

Gate 5 is `APPROVED WITH CONDITIONS`, ADR-001~010 are Accepted, and Gate 6 Production Implementation has started with the platform-independent Domain/Application foundation. Windows HID and WPF code remain subsequent increments.

## Update 24 — Gate 6 State Reducer

The platform-independent state reducer now serializes semantic provider events, rejects stale generations and out-of-order sequences, clears stale battery values, hides dormant devices, restores recovered devices, and removes Windows-removed devices. Windows HID access is not part of this increment.

## Update 25 — Gate 6 Single-Reader Coordinator

The application mailbox now has a single-reader coordinator with multi-producer writes, ordered drain-on-completion, per-event fault isolation, and late-write rejection. It feeds the deterministic reducer without introducing Windows HID or UI dependencies.

## Update 26 — Gate 6 DualSense Parser

The Production pure parser recognizes the POC-verified 78-byte Bluetooth and 64-byte USB report layouts, normalizes valid battery/charging status into the Domain model, and rejects invalid shapes/status codes without overwriting prior state. At this historical increment the Provider was Bluetooth-only; CHG-003 later added USB.

## Update 27 — Gate 6 Read-Only DualSense Provider

The Windows provider now uses the targeted DualSense gamepad selector, accepts Bluetooth HID endpoints only, opens with `FileAccessMode.Read`, coalesces unchanged reports, and emits semantic discovery/battery/freshness/offline/removal events. Output, Feature, vendor commands, and `DeviceClass.All` are not used.

## Update 28 — Gate 6 Production Pipeline Smoke Host

A time-bounded console host connects the Production Provider, coordinator, and reducer for real-device verification. It prints hashed keys and normalized state only, then cancels the Provider, drains the mailbox, and reports cleanup.

The first 15-second real-device run completed with `Unknown → Available 15% / NotCharging`, `Processed=2`, `Faulted=0`, and cleanup complete. The current integration verdict is `PASS WITH LIMITATION` pending Production OFF/ON, Dormant, and sleep/resume scenarios.

The 10-second Unknown and 30-second Dormant boundaries are also isolated in a monotonic `TimeProvider` freshness policy with deterministic boundary/recovery specs. Physical OFF/ON evidence remains a separate integration item.

## Update 29 — Gate 6 WPF Presentation Foundation

The WPF presentation project now contains a functional widget shell and revision-aware ViewModels for empty, waiting, available, estimated, charging, unknown, dormant, and removed states. Final styling, tray lifecycle, settings, and Production app composition remain subsequent increments.

## Update 30 — Gate 6 Executable App Shell

`DeviceBattery.App` now composes the Production DualSense Provider, coordinator, reducer, WPF widget, and WinForms tray icon. Widget X and tray Exit converge on one cleanup path; minimize hides the widget while the tray and Provider remain active.

## Update 31 — CHG-003

Requirements v1.3 adds DualSense USB, changes the widget to one compact indicator with USB priority, and moves Always On Top control exclusively to the tray menu. Read-only HID safety restrictions remain unchanged.

## Update 32 — CHG-004

Requirements v1.4 keeps the last validated charging state while the USB endpoint remains connected, retains freshness timeouts for Bluetooth, and removes the Windows title bar for a frameless indicator.

## Update 33 — CHG-005

Requirements v1.5 reduces indicator typography to avoid clipped labels and hides the widget window from the Windows taskbar. Runtime presence and controls remain available through the tray icon.

## Update 34 — DualSense Visual Candidate

The first visual candidate uses a DualSense-inspired white shell, dark center surfaces, PlayStation blue accents, and green charging feedback. It remains a review candidate until owner visual approval.

The second visual iteration removes the brand label, increases the gauge thickness, and overlays a centered lightning symbol only while charging.

The third visual iteration keeps the 360px shell width, places the device name to the left of the gauge with a 10px gap, and adds 5px vertical gauge spacing.

The fourth visual iteration shapes the gauge as a battery, centers the percentage inside it, and removes the lightning and separate charging label. Charging continues to use the established green gauge color.

The fifth visual iteration removes the in-window close control, routes user exit exclusively through the tray menu, and lets the shell height grow with the number of projected device items.

The sixth visual iteration adds a static green `● 충전` label beside the device name while charging. It uses no animation and retains the green charging gauge.

The seventh visual iteration shortens the battery gauge and gives the device name and charging label a flexible text area so the charging text remains visible.

The owner approved the seventh visual iteration as the Gate 6 visual baseline. The next increment persists widget position and Always On Top under the current user's local application data and clamps restored coordinates to a visible monitor working area.

## Update 35 — CHG-006 Standard BLE Battery

Requirements v1.6 adds a Production provider for the Bluetooth SIG Battery Service (`0x180F`) and Battery Level (`0x2A19`). It uses exact percent with unknown charging, prefers Notify/Indicate, falls back to a 30-second uncached read only when notifications are unavailable, and projects BLE devices beside the preferred DualSense endpoint.

## Update 36 — CHG-007 Xbox Battery

Requirements v1.7 adds a Production provider for Xbox controllers that expose a valid `Windows.Gaming.Input` BatteryReport. It uses Gamepad lifecycle events, polls only the battery report every 30 seconds, excludes DualSense to avoid duplicate ownership, and marks potentially granular percentages as estimated.

## Update 37 — Device Visibility Management

The tray `장치 표시` submenu now controls each known indicator without stopping provider monitoring. Hidden keys persist in the existing local settings file, BLE/WGI Xbox arbitration is shared by the widget and tray catalog, and the submenu remains open for consecutive multi-device changes.

## Update 38 — Windows Login Auto-start

The tray now controls an unpackaged current-user auto-start registration. It is OFF by default, uses only the app-owned `DeviceBatteryWidget` value under HKCU Run, quotes the executable path, and re-reads external state when the tray opens. Settings-style menu actions remain open for consecutive changes.
