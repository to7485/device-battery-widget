# DeviceBattery.Poc.BleBatteryProbe

Gate 4 POC-B02용 BLE GATT Battery Service 검증 프로젝트입니다.

목적:

- Windows에서 Bluetooth SIG Battery Service `0x180F` 인스턴스가 열거되는지 확인
- Battery Level Characteristic `0x2A19`를 uncached read로 직접 읽기
- 값이 0~100%로 해석되는지 확인
- Notify/Indicate 지원 시 `ValueChanged` 이벤트 구독 가능 여부 확인
- 종료 시 CCCD 구독 해제 및 `GattDeviceService.Dispose()` 수행

## 실행

Bluetooth 테스트 장치를 연결한 상태에서 PowerShell:

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.BleBatteryProbe
dotnet clean
dotnet run
```

## 기대 관찰값

성공적인 예:

```text
Battery Service DeviceInformation count: 1

[BATTERY SERVICE #1]
DeviceInformation.Name = <device/service name>
...
GetCharacteristics Status = Success
Battery Level count        = 1

  Properties = Read, Notify
  INITIAL READ Status = Success
  INITIAL READ Value  = 73%
  Subscribe(Notify) Status = Success
```

## 판정

- `Battery Service count > 0` + `INITIAL READ Value = n%`: BLE Battery Level 경로 PASS
- Read 성공 + Notify/Indicate 미지원: PASS WITH LIMITATION, 저주기 polling 후보
- Battery Service 존재 + Read 실패: NEED ALTERNATIVE/추가 원인 분석
- Battery Service count = 0: 해당 환경/장치에서는 이 selector 경로 NEED ALTERNATIVE

## 주의

이 POC는 AULA 전용 구현이 아닙니다. 표준 BLE Battery Service를 노출하는 모든 테스트 장치를 대상으로 하는 기술 Provider 검증입니다.
AULA F87Pro는 이전 POC에서 `0x180F` 노출 단서가 확인되어 대표 테스트 장치로 사용하는 것입니다.
