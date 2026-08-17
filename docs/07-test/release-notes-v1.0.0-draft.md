# Device Battery Widget v1.0.0 — Release Notes (Draft)

Status: **PRE-RELEASE / DO NOT DISTRIBUTE**

## Supported battery sources

- Sony DualSense over Bluetooth and USB
- Standard Bluetooth LE GATT Battery Service devices
- Xbox controllers exposed through Windows.Gaming.Input

The widget automatically adds and removes detected devices and resolves duplicate Xbox BLE/WGI
rows. Individual device rows can be hidden or restored from the tray menu.

## Widget behavior

- Compact frameless battery indicators
- Dynamic height for multiple devices
- Charging indication and battery percentage/gauge
- Tray-only presence; no taskbar button
- Position and Always On Top settings persist across restarts
- Application exit is available only from the tray menu
- Optional per-user Windows logon startup, disabled by default

## Known limitations

- DualSense battery over HID is an estimated 10% bucket rather than exact 1% precision.
- Windows Topmost does not guarantee visibility over exclusive fullscreen applications.
- Standard BLE Battery Service reports battery percentage but usually does not expose charging state.
- Two physical devices of the exact same model have not completed a dedicated simultaneous identity test.
- The 24-hour and 72-hour Production soak requirements are Deferred with owner-accepted residual risk;
  the longest preserved partial run is 7.07 hours.
- Windows 11, signed installer, and clean install/upgrade/uninstall validation remain release blockers.

## Privacy and safety

- HID discovery and battery reads are read-only.
- No vendor-specific output or feature command is sent.
- Local diagnostics are bounded and omit raw HID reports, MAC addresses, and full device IDs.
