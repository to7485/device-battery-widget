# Architecture Decision Draft

상태: Gate 5 REVIEW DRAFT

| ADR | 결정 후보 | 상태 |
|---|---|---|
| ADR-001 | C#/.NET 10 layered Windows Desktop architecture | Recommended |
| ADR-002 | v1 DualSense Bluetooth targeted read-only HID provider | Recommended |
| ADR-003 | Provider event를 single-reader coordinator로 직렬화 | Recommended |
| ADR-004 | Raw HID report coalescing; battery change/recovery만 publish | Recommended |
| ADR-005 | 10초 Unknown, 30초 Dormant, report 복구 시 재추가 | Recommended |
| ADR-006 | WPF window + WinForms NotifyIcon tray | Recommended |
| ADR-007 | provider-owned DeviceKey; ContainerId 단독 사용 금지 | Recommended |
| ADR-008 | self-contained win-x64 기본, FDD 보조 profile | Recommended |
| ADR-009 | autostart adapter; unpackaged v1은 사용자 HKCU Run | Recommended |
| ADR-010 | 최소 local log, 7일/10 MiB, raw HID/전체 ID 금지 | Recommended |

Gate 5 승인 시 Recommended 항목을 Accepted/Rejected로 확정한다.
