# Gate 6 CHG-006/CHG-007 Multi-provider Smoke 01

- Date: 2026-08-16
- Environment: Windows real-device integration
- Scope: DualSense HID + Standard BLE GATT Battery + Windows.Gaming.Input fallback
- Safety: read-only HID, standard BLE Battery Service read/subscribe, and `TryGetBatteryReport()` only

## Observed result

| Indicator | Observed state |
|---|---|
| DualSense Controller (USB) | Charging, approximately 15% |
| AULA-F87Pro 5.0 | 97% through BLE Battery Level |
| Xbox Wireless Controller | 69% through BLE Battery Level |

- The three devices were projected as three rows and widget height expanded with the device count.
- Xbox was discovered through both BLE and Windows.Gaming.Input, but only one row was shown. BLE is preferred when both sources have the same display name; Windows.Gaming.Input remains the fallback.
- The earlier Windows.Gaming.Input real-device probe returned Discharging and approximately 10%, demonstrating that the fallback path is readable but may be substantially more granular than BLE Battery Level.
- The approved normal gauge color `#4F71E0` and black charging gauge text were visible.
- After the Xbox controller was powered off, its indicator row was automatically removed while the DualSense and AULA rows remained visible.

## Result

**PASS**

- Initial discovery, battery projection, provider arbitration, and dynamic widget sizing passed on real devices.
- Xbox and AULA both passed automatic row removal on power OFF and automatic re-registration with battery display on power ON.
- Windows.Gaming.Input battery precision is device/transport dependent. When a valid BLE Battery Level is available for the same displayed device, BLE is the authoritative UI source.
