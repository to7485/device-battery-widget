# Gate 6 Device Visibility Smoke 01

- Date: 2026-08-16
- Scope: FR-011 individual device visibility persistence, FR-016 hidden-device management
- Environment: Windows real-device integration

## Observed result

- The tray `장치 표시` submenu listed the currently known device indicators without the BLE/Windows.Gaming.Input Xbox duplicate.
- Clicking a checked device removed only that device row from the widget while monitoring continued.
- Clicking it again restored the row immediately.
- Multiple device visibility settings could be changed while the submenu remained open.
- The approved visibility settings were persisted in the existing local `settings.json` model.

## Result

**PASS**
