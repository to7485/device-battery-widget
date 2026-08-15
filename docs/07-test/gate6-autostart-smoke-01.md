# Gate 6 Windows Login Auto-start Smoke 01

- Date: 2026-08-16
- Scope: FR-015 Windows login auto-start, default OFF
- Environment: Windows real-device integration

## Observed result

- The tray `Windows 로그인 시 실행` item was enabled and reflected the current registration state.
- Clicking the item toggled the current-user auto-start registration ON/OFF and the checked state remained correct when the tray menu was reopened.
- The command used a quoted executable path and the app-owned `DeviceBatteryWidget` value only.
- Settings-style tray actions (`항상 위`, device visibility, and login auto-start) could be changed consecutively without closing the menu.
- `위젯 표시` and `종료` retained normal command-menu closing behavior.

## Result

**PASS**
