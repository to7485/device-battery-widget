# POC-A02/A03 Fix 02

## 수정 목적

직전 버전에서 다음 호출 중 Native Access Violation이 발생했다.

```text
DeviceInformation.GetAqsFilterFromDeviceClass(DeviceClass.All)
0xC0000005
```

따라서 해당 API를 제거한다.

## 변경 구조

```text
DeviceInformation.CreateWatcher()
        ↓
ADDED / UPDATED / REMOVED
        ↓
Device ID 확보
        ↓
DeviceInformation.CreateFromIdAsync()
        ↓
상세 Property 재조회
```

## 조회 Property

- System.Devices.Connected
- System.Devices.ContainerId
- System.Devices.DeviceInstanceId
- System.Devices.FriendlyName
- System.Devices.DeviceManufacturer
- System.Devices.ModelName

## 기존 파일 교체 위치

```text
poc/
└─ DeviceBattery.Poc.DeviceEnumeration/
   └─ Program.cs
```

## 빌드

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.DeviceEnumeration

dotnet clean
dotnet run --project .\DeviceBattery.Poc.DeviceEnumeration.csproj
```

## DualSense 재시험

### Bluetooth

```text
1. DualSense Bluetooth 연결
2. POC 실행
3. 전원 OFF
4. 로그 확인
5. 전원 ON
6. 로그 확인
```

### USB

```text
1. DualSense USB 연결
2. USB 케이블 제거
3. 로그 확인
4. 다시 연결
5. 로그 확인
```

## 확인할 로그

다음이 가장 중요하다.

```text
[UPDATED]
...
[UPDATED REFRESH]
Connected=False
```

또는

```text
[UPDATED REFRESH]
Connected=True
```

그리고 Interface 자체가 사라지는 경우:

```text
[REMOVED]
...
[REMOVED REFRESH] Result=null
```

또는 조회 Error가 기록될 수 있다.

테스트 후 `artifacts/device-events.log`의 DualSense 구간을 분석한다.
