# Gate 6 Memory Attribution 01

- Date: 2026-08-16
- Build: Release
- Method: same Production executable, diagnostic-only `--providers` selection, 60 seconds per stage
- Safety: read-only process observation; default Production provider selection remains `all`

## Results

| Stage | Providers | Working Set last | Increment | Private last | CPU average |
|---|---|---:|---:|---:|---:|
| G6-MEM-SHELL | none | 108.78 MiB | baseline | 59.26 MiB | 0.000% |
| G6-MEM-DUALSENSE | DualSense HID | 129.32 MiB | +20.54 MiB | 66.44 MiB | 0.236% |
| G6-MEM-DUALSENSE-BLE | DualSense + BLE | 134.93 MiB | +5.61 MiB | 69.92 MiB | 0.283% |
| G6-MEM-ALL | DualSense + BLE + WGI | 136.04 MiB | +1.11 MiB | 70.45 MiB | 0.481% |

All four stages completed 59 samples, exited normally with code 0, and showed decreasing Working Set, Private Memory, handle, and thread counts over the short run.

## Interpretation

- The WPF/WinForms tray/.NET shell baseline already exceeds the 100 MiB Working Set target by 8.78 MiB.
- DualSense WinRT HID is the largest Provider increment at approximately 20.54 MiB.
- BLE and WGI together add approximately 6.72 MiB; removing Xbox support would not resolve NFR-PERF-003.
- Full Production Working Set reproduced near the earlier five-minute result (136.04 MiB versus 138.16 MiB).
- Private Memory remains below 100 MiB, but the approved target has been evaluated as Working Set and must not be silently redefined.

## Result

**NEED DECISION**

NFR-PERF-003 remains FAIL. Reaching 100 MiB requires a material shell/HID technology change or an approved requirement change based on measured full-feature behavior. No working-set trimming API or periodic forced collection should be introduced merely to make the metric appear lower.
