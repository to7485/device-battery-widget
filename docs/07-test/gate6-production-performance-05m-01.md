# Gate 6 Production Performance 5-minute Run 01

- Date: 2026-08-16
- Build: Release
- Target: current multi-provider Production app
- Providers/features: DualSense HID, BLE GATT, Xbox WGI, WPF/tray, bounded local diagnostics
- Sampler: read-only, 300 seconds, 1-second interval
- Evidence: `artifacts/resource/resource-G6-PRODUCTION-MULTIPROVIDER-16524-20260816-021042.csv`

## Result

| Metric | Result | Target | Judgment |
|---|---:|---:|---|
| Samples | 300 | 300 | PASS |
| CPU average | 0.358% | <= 1% | PASS |
| CPU maximum | 1.679% | <= 5% | PASS |
| Working Set last | 138.16 MiB | <= 100 MiB | **FAIL** |
| Working Set delta | -0.98 MiB | no growth trend | PASS (short run) |
| Private Memory last | 71.43 MiB | observation | PASS |
| Private Memory delta | -2.16 MiB | no growth trend | PASS (short run) |
| Handles last/delta | 767 / -62 | no growth trend | PASS (short run) |
| Threads last/delta | 16 / -17 | no growth trend | PASS (short run) |

## Overall judgment

**PASS WITH LIMITATION**

CPU and five-minute stability passed, but NFR-PERF-003 does not pass because the Production Working Set remained above 100 MiB. This is an optimization input and must not be reclassified using Private Memory alone. The five-minute run also does not replace the required 24-hour and 72-hour stability tests.

## Requirements v1.8 re-evaluation

CHG-008 preserved the original v1.7 FAIL and changed the approved Production acceptance limits to Working Set 150 MiB and Private Memory 100 MiB. Against v1.8, the measured 138.16 MiB Working Set and 71.43 MiB Private Memory are **PASS**. Long-duration stability remains pending.
