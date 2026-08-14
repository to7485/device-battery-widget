# POC-A01~A04 실행 절차

## 1. 환경 확인
```powershell
dotnet --info
```
.NET 10 SDK 설치 여부를 확인한다.

## 2. 실행
Repository Root 기준:
```powershell
cd poc\DeviceBattery.Poc.DeviceEnumeration
dotnet restore
dotnet run
```

## 3. Evidence
프로그램은 실행 폴더 하위에 다음을 생성한다.
```text
artifacts/
├─ system-info.txt
├─ device-events.log
└─ initial-devices.csv
```

## 4. 테스트
- 초기 열거 후 `[ENUMERATION COMPLETED]` 확인
- 장치 하나 연결 후 `[ADDED]` 또는 관련 `[UPDATED]` 확인
- 같은 장치를 해제 후 `[REMOVED]` 확인

## 5. 관찰 포인트
하나의 물리 장치를 연결했는데 여러 Device Interface가 보일 수 있으므로 Name을 고유 ID로 판단하지 않는다.
Name, ID, Kind, 연결/해제 이벤트 수를 기록한다.

## 6. 다음 단계
초기 결과를 기반으로 POC-A05/A06에서 Stable Device Identity 후보를 검증한다.
