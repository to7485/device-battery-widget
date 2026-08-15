# Architecture Decision Draft

상태: APPROVED WITH CONDITIONS

| ADR | 결정 후보 | 상태 |
|---|---|---|
| ADR-001 | C#/.NET 10 layered Windows Desktop architecture | Accepted |
| ADR-002 | v1 DualSense targeted read-only HID provider (CHG-003: Bluetooth+USB) | Accepted |
| ADR-003 | Provider event를 single-reader coordinator로 직렬화 | Accepted |
| ADR-004 | Raw HID report coalescing; battery change/recovery만 publish | Accepted |
| ADR-005 | 10초 Unknown, 30초 Dormant, report 복구 시 재추가 | Accepted |
| ADR-006 | WPF window + WinForms NotifyIcon tray | Accepted |
| ADR-007 | provider-owned DeviceKey; ContainerId 단독 사용 금지 | Accepted |
| ADR-008 | self-contained win-x64 기본, FDD 보조 profile | Accepted |
| ADR-009 | autostart adapter; unpackaged v1은 사용자 HKCU Run | Accepted |
| ADR-010 | 최소 local log, 7일/10 MiB, raw HID/전체 ID 금지 | Accepted |
| ADR-011 | CHG-003: endpoint별 상태 유지 + single indicator USB 우선 projection | Accepted |
| ADR-012 | CHG-004: USB는 Removed 기반 lifecycle, Bluetooth만 report freshness timeout 적용 | Accepted |

2026-08-15 Gate 5 조건부 승인으로 ADR-001~010을 Accepted로 확정했고, CHG-003 승인으로 ADR-011을 추가했다.
