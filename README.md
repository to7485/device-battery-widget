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
