# Gate 6 Production Startup 10x Run 01

- Date: 2026-08-16
- Build: Release
- Configuration: all Production providers
- Iterations: 10
- Normal shutdown: `--smoke-seconds 3`, up to 20-second provider cleanup allowance
- External evidence: `artifacts/startup/startup-G6-PRODUCTION-ALL-20260816-025413.csv`

## Widget visibility (external process observation)

| Metric | Result | Target | Judgment |
|---|---:|---:|---|
| Ready | 10/10 | 10/10 | PASS |
| Average | 846.7 ms | <= 2,000 ms | PASS |
| Minimum | 816.5 ms | observation | PASS |
| Maximum / P95 | 939.1 ms | <= 2,000 ms | PASS |

## First device battery availability (process-relative marker)

The bounded local diagnostic log recorded `FIRST_DEVICE_AVAILABLE` with PID and process-relative elapsed time only. No device ID was added to the marker.

| Metric | Result | Target | Judgment |
|---|---:|---:|---|
| Available markers | 10/10 | 10/10 | PASS |
| Average | 781.0 ms | <= 5,000 ms | PASS |
| Minimum | 753.1 ms | observation | PASS |
| Maximum / P95 | 849.6 ms | <= 5,000 ms | PASS |

The external window-ready clock includes process launch observation; the first-device marker starts at WPF `OnStartup`. They are separate clocks and are not subtracted from one another. Both independently satisfy their acceptance limits with substantial margin.

## Result

**PASS**
