# Gate 4 Startup Evidence

Windows 10 Home 22H2 build 19045.6466 / win-x64 / .NET SDK 10.0.400.

- `startup-TRAY-FDD-20260815-184436.csv`: framework-dependent 10-iteration startup
- `startup-TRAY-SELF-CONTAINED-20260815-184443.csv`: self-contained 10-iteration startup

Readiness is the first visible top-level window owned by the target PID. Each iteration exits
through `WM_CLOSE`, exercising the normal Widget X cleanup path.
