# Architecture Test Strategy 초안

상태: APPROVED WITH CONDITIONS
관련 설계: ARC-001~010

## Unit

- DualSense 64/78-byte layout과 status offset
- battery bucket/charging/invalid code
- BatteryState invariant
- state reducer의 모든 전이
- older session generation 폐기
- monotonic manual TimeProvider 기반 10초/30초 경계 (`SPEC-DS-FRESHNESS`)
- settings schema migration/atomic save

## Concurrency

- timeout과 report recovery 동시 command
- shutdown 중 late HID callback
- duplicate semantic event coalescing
- UI revision 역전 방지
- provider fault burst rate limiting

## Windows Integration

- Bluetooth DualSense targeted discovery/read-only open
- OFF/ON 3회
- sleep/resume
- USB interface가 v1 provider에 포함되지 않음
- Output/Feature command 호출 경로 없음

## Application Lifecycle

- minimize/hide/restore
- Widget X / Tray Exit 동일 cleanup
- ghost tray icon 없음
- settings restore와 화면 범위 보정

## Release Validation

- Windows 10 22H2 / Windows 11
- 24시간 stability
- 72시간 release-candidate soak
- FDD/SCD 및 선택된 packaging
- CPU/Memory/Handle/Thread regression

POC 프로젝트를 Production test 대상처럼 재사용하지 않는다. 검증된 raw samples와 상태값은
새 Production parser/reducer test fixture로 선별 이관한다.
