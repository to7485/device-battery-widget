# DeviceBattery.Poc.DeviceEnumeration

## 대상
- POC-A01 초기 Enumeration
- POC-A02 Device Added
- POC-A03 Device Removed
- POC-A04 Name/ID 기초 확인

## 실행
Windows에서 .NET 10 SDK 설치 후:

```powershell
dotnet restore
dotnet run
```

초기 열거 후 장치를 연결/해제하여 ADDED/REMOVED 로그를 확인한다.
종료는 Enter 키.

결과는 `docs/03-poc/device-matrix.md`와 `poc-test-cases.md`에 기록한다.
