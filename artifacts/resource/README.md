# Gate 4 Resource Evidence

2026-08-15 POC-E read-only process measurements.

- `resource-PERF-TRAY-11524-20260815-180743.csv`: 5-minute System Tray idle baseline
- `resource-PERF-BATTERY-BLUETOOTH-8800-20260815-183356.csv`: 5-minute v1.0 DualSense Bluetooth-only baseline
- `resource-PERF-BATTERY-28464-20260815-181537.csv`: Bluetooth + USB dual-transport reference; not the v1.0 baseline

CSV files contain timestamp, elapsed seconds, normalized process CPU, Working Set,
Private Memory, Handle Count, and Thread Count. PID values are run-local and not device identifiers.
