# Gate 4 POC Update 04

POC-A 실장비 결과를 반영하고 POC-B Battery/Charging 검증으로 이동하기 위한 업데이트입니다.

## 다음 실행

AULA F87Pro를 Bluetooth로 연결한 상태에서:

```powershell
cd .\poc\DeviceBattery.Poc.BatteryProbe
dotnet clean
dotnet run
```

출력 전체를 저장해서 분석합니다.
주변장치 Battery Controller가 없더라도 정상적인 POC 결과입니다.

## Update 05 — POC-B02 BLE GATT Battery Probe

POC-B01에서 Windows Generic Battery Controller가 주변기기를 0개 반환하여 해당 경로를 `FAIL / NEED ALTERNATIVE`로 기록했습니다.

새 프로젝트:

```text
poc/DeviceBattery.Poc.BleBatteryProbe
```

실행:

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.BleBatteryProbe
dotnet clean
dotnet run
```

이 POC는 표준 BLE Battery Service `0x180F`와 Battery Level `0x2A19`를 읽고 Notify/Indicate 가능 여부를 확인합니다.
