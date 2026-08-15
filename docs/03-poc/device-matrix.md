# POC Device Matrix

| Device ID | Category | Manufacturer / Model | Connection | OS | Friendly Name | Enumeration | Battery % | Charging | Battery Event | Polling | Stable Identity | Reconnect Identity | Result | Notes |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| DEV-001 | Mouse | Logitech G703 | 2.4GHz Receiver | Current POC OS | USB Receiver | PASS WITH LIMITATION | DEFERRED | DEFERRED | Passive online candidate | DEFERRED | Limitation | NOT TESTED | DEFERRED | vNext; B04 evidence preserved |
| DEV-002 | Keyboard | AULA F87Pro | Bluetooth LE | Current POC OS | AULA-F87Pro 5.0 | PASS | PASS (100%) | NOT TESTED | Notify subscribed | Fallback TBD | Candidate | PASS | DEFERRED | vNext; BLE POC evidence preserved |
| DEV-003 | Game Controller | Sony DualSense | Bluetooth HID | Windows 10 22H2 | DualSense Wireless Controller | PASS | PASS WITH LIMITATION | PASS | PASS | NOT REQUIRED (event-only) | PASS WITH LIMITATION | PASS WITH LIMITATION | PASS WITH LIMITATION | **v1.0 scope**; 10% battery bucket; sleep/resume 자동 복구 |
| DEV-004 | Headset | Corsair VOID WIRELESS V2 | 2.4GHz Receiver | Current POC OS | CORSAIR VOID WIRELESS V2 Gaming Receiver | PASS WITH LIMITATION | DEFERRED | DEFERRED | Passive online candidate | DEFERRED | Candidate | NOT TESTED | DEFERRED | vNext; B04 evidence preserved |

## 판정
PASS / PASS WITH LIMITATION / FAIL / NEED ALTERNATIVE / NOT TESTED

## 주의
Friendly Name은 표시용이다. Stable Identity와 동일하게 취급하지 않는다.
