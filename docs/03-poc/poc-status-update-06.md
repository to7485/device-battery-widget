# Gate 4 POC 상태 업데이트 — POC-B02 완료 / POC-B03 시작

작성일: 2026-08-15
상태: Gate 4 진행 중

## 1. POC-B02 BLE GATT Battery 결과

AULA F87Pro Bluetooth 실장비에서 표준 BLE Battery Service 검증을 수행했다.

관찰 결과:

```text
DeviceInformation.Name = AULA-F87Pro 5.0
Service.Uuid            = 0000180f-0000-1000-8000-00805f9b34fb
GetCharacteristics Status = Success
Battery Level count        = 1
Properties                 = Read, Notify
INITIAL READ Status        = Success
INITIAL READ Value         = 100%
Subscribe(Notify) Status   = Success
Active subscriptions       = 1
```

전원 OFF/ON 및 Bluetooth 재연결 후 재실행에서도 동일하게 100% Read와 Notify 구독 성공이 재현되었다.

판정:

- Battery Service 발견: PASS
- Battery Level 발견: PASS
- Battery % Read: PASS
- Notify 지원/구독: PASS
- 재연결 후 재조회: PASS
- 실제 ValueChanged 발생 관찰: POC-C로 이관

**POC-B02 최종: PASS**

추가 관찰:

- Xbox Wireless Controller의 `0x180F` 서비스 항목은 발견되었으나 `GetCharacteristics Status=Unreachable`이었다. 별도 재검증 대상으로 유지한다.
- DualSense는 Bluetooth 연결 상태였지만 `0x180F` 서비스 검색 결과에 나타나지 않았다. 따라서 BLE 표준 Battery Service Provider 대상에서는 제외하고 다음 Provider 검증으로 이관한다.

## 2. POC-B03 검증 전략 수정

바로 raw/undocumented HID report 해석으로 들어가기 전에 Microsoft 공개 API인 `Windows.Gaming.Input`을 먼저 검증한다.

`RawGameController` 및 `Gamepad`는 `TryGetBatteryReport()`를 제공하므로, DualSense가 이 경로에서 BatteryReport를 노출하는지 확인한다.

새 프로젝트:

```text
poc/DeviceBattery.Poc.GameControllerBatteryProbe
```

B03-1 결과가 불충분할 경우에만 B03-2 Raw HID fallback으로 진행한다.

## 3. Gate 상태

- Gate 1: APPROVED
- Gate 2: APPROVED
- Gate 3: APPROVED / requirements baseline v1.1
- Gate 4: IN PROGRESS
  - POC-A: 결과 동결
  - POC-B01 Generic Battery: FAIL / NEED ALTERNATIVE
  - POC-B02 BLE GATT Battery: PASS
  - POC-B03 Game Controller Battery: READY FOR EXECUTION
  - POC-B04 Receiver/Vendor: NOT TESTED
  - POC-B05 Normalized BatteryState: NOT TESTED


## Build fix

- Added `using Windows.System.Power;` to `GameControllerBatteryProbe/Program.cs` because `BatteryStatus` is defined in the `Windows.System.Power` namespace.
