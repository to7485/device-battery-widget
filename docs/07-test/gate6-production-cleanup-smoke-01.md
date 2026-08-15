# Gate 6 Production Cleanup Smoke 01

- Date: 2026-08-16
- Scope: NFR-STAB-004 resource cleanup
- Build: Release
- Mode: `DeviceBatteryWidget.exe --smoke-seconds 8`

## Observed result

- The production app initialized its tray, providers, coordinator, settings, and bounded log listener.
- The timed shutdown completed with exit code `0` within the 15-second observation limit.
- The process no longer existed after shutdown.
- The local diagnostic log ended with `APP_STOP`, confirming the common graceful shutdown path reached log disposal.
- Provider cancellation, resume-task cancellation, coordinator drain, provider disposal, tray disposal, settings save, and listener disposal are sequenced by `ShutdownAsync`.

## Result

**PASS**

## Separate performance observation

At four seconds the single startup sample reported 143.34 MiB Working Set, 77.53 MiB Private Memory, 827 handles, and 35 threads. This does not invalidate cleanup, but it exceeds the 100 MiB steady-state Working Set target and requires a separate five-minute Production performance measurement before NFR-PERF-003 can be judged.
