# Gate 6 Local Diagnostics Smoke 01

- Date: 2026-08-16
- Scope: ADR-010 bounded minimal local diagnostics
- Environment: Windows real-device integration

## Observed result

- A UTC daily log was created under `%LocalAppData%\DeviceBatteryWidget\logs`.
- The real log contained application lifecycle and normalized state records with provider-owned short-hash DeviceKeys, availability, percent, and charging state.
- No raw HID report, MAC address, or full `DeviceInformation.Id` was recorded.
- Automated verification confirmed that files older than seven days are pruned; the listener also enforces the approved 10 MiB total budget during pruning.
- Logging initialization failure is isolated and does not prevent application startup.

## Result

**PASS**
