# Gate 6 Production Soak Runbook

## Start the 72-hour run

Open a user-owned PowerShell window and run from the repository root:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\tools\Start-ProductionSoak.ps1
```

The script builds the current Release app, prevents a duplicate widget process, launches the app,
and runs the read-only ResourceSampler in the foreground for 259200 seconds at a 60-second interval.
Keep that PowerShell window open.

## During the run

- Normal PC and device use is allowed.
- Sleep/resume is allowed.
- Do not exit the widget from the tray.
- Do not log off or reboot Windows.
- Do not close the PowerShell window.

The first 24 hours provide the NFR-STAB-001/002 checkpoint. Continue the same process to 72 hours
for NFR-STAB-003 rather than restarting after the checkpoint.

## Completion

The sampler prints a summary and the CSV path under `artifacts/resource`. Preserve the CSV, then
exit the widget through the tray so the diagnostic log records `APP_STOP`. Do not classify a run
that stops early as PASS.
