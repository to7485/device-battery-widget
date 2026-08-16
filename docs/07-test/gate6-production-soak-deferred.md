# Gate 6 Production Soak — Deferred

- Decision date: 2026-08-17
- Owner decision: stop long-duration testing and continue the project
- Final status: **INCOMPLETE / DEFERRED — residual risk accepted by owner**

## Preserved attempts

Four partial CSV files are preserved under `artifacts/resource`:

- Codex-hosted 24-hour attempt: 191 samples, interrupted before 24 hours
- Codex-hosted 72-hour attempt: 165 samples, interrupted before 72 hours
- First user-owned 72-hour attempt: 158 samples / 2.63 hours
- Final user-owned 72-hour attempt: 424 samples / 7.07 hours

## Final partial-run observation

| Metric | Result |
|---|---:|
| CPU average / maximum | 0.270% / 0.684% |
| Working Set last / delta | 127.26 MiB / -16.79 MiB |
| Private Memory last / delta | 73.05 MiB / +0.26 MiB |
| Handles last / delta | 684 / -127 |
| Threads last / delta | 18 / -11 |

The 7.07-hour partial run stayed within Requirements v1.8 CPU and memory limits and showed no short-run handle/thread growth. It does not satisfy or replace the approved 24-hour and 72-hour durations.

## Gate treatment

- NFR-STAB-001, NFR-STAB-002, and NFR-STAB-003 are not marked PASS.
- They are deferred by explicit owner decision and remain release residual risks.
- No later document may reinterpret these partial runs as completed soak evidence.
