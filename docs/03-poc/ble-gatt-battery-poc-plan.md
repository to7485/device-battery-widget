# POC-B02 — BLE GATT Battery Provider 검증 계획

작성일: 2026-08-15
상태: READY FOR EXECUTION

## 1. 목적

POC-B01에서 `Windows.Devices.Power.Battery`가 테스트 주변기기를 Battery Controller로 열거하지 못했으므로,
Bluetooth LE 장치에 대해서는 표준 GATT Battery Service를 직접 조회하는 경로를 검증한다.

이 검증은 특정 키보드 전용이 아니다. 연결 기술별 Provider 구조 중 `BLE GATT Provider`의 기술 타당성을 검증한다.

## 2. 표준 UUID

- Battery Service: `0000180f-0000-1000-8000-00805f9b34fb` (`0x180F`)
- Battery Level Characteristic: `00002a19-0000-1000-8000-00805f9b34fb` (`0x2A19`)

## 3. 구현 경로

1. `GattDeviceService.GetDeviceSelectorFromUuid(GattServiceUuids.Battery)`
2. `DeviceInformation.FindAllAsync(selector)`
3. `GattDeviceService.FromIdAsync(deviceInfo.Id)`
4. `GetCharacteristicsForUuidAsync(GattCharacteristicUuids.BatteryLevel, BluetoothCacheMode.Uncached)`
5. `ReadValueAsync(BluetoothCacheMode.Uncached)`
6. 첫 바이트를 0~100 Battery Level로 해석
7. Notify/Indicate 지원 시 CCCD 설정 후 `ValueChanged` 구독
8. 종료 시 CCCD=None 및 Service Dispose

## 4. 판정 기준

### PASS

- Battery Service가 열거됨
- Battery Level 0x2A19가 발견됨
- Read 결과가 Success이고 0~100% 값이 확보됨

### PASS WITH LIMITATION

- Read는 성공하지만 Notify/Indicate가 없어 Event-driven 갱신이 불가능함
- 이 경우 저주기 polling 후보로 남김

### NEED ALTERNATIVE

- Battery Service가 열거되지 않음
- Service는 있지만 0x2A19가 없음
- 접근/통신 오류로 Read 불가

## 5. 테스트 장치

대표 1차 검증 장치: AULA F87Pro Bluetooth

이유: POC-A 로그에서 표준 BLE Battery Service `0x180F` 인터페이스 노출 단서가 이미 확인됨.
이는 제품을 키보드에 고정한다는 의미가 아니라 BLE Provider 검증 성공 가능성이 높은 실장비를 선택한 것이다.

성공 후 다른 Bluetooth 장치에서도 동일 Provider를 재검증한다.

## 6. 수집할 증거

- Battery Service count
- DeviceInformation.Name / Id
- Service UUID / DeviceId / AttributeHandle
- Battery Level characteristic count
- CharacteristicProperties
- Initial Read Status / value
- Subscribe Status
- ValueChanged 발생 여부
- 종료 cleanup 결과
