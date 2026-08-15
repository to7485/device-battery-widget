# DeviceBattery.Poc.DualSenseLifecycleProbe

Gate 4 DualSense-only 실장비 lifecycle/timeout POC다.

## 검증 대상

- 시작 시 targeted HID enumeration
- DualSense Bluetooth OFF/ON 시 Added/Removed 또는 reopen 동작
- valid report 수신 시 `Available`
- 10초 동안 valid report가 없으면 stale percent를 제거하고 `Unknown`
- report 복구 시 `Available` 복귀
- 종료 시 watcher/timer/event/HID handle 정리

## 안전성

- `HidDevice.GetDeviceSelector(0001/0005, 054C/0CE6)`만 사용
- `DeviceInformation.CreateWatcher(selector)` 사용
- `DeviceInformation.GetAqsFilterFromDeviceClass(DeviceClass.All)` 사용 금지
- `FileAccessMode.Read`만 사용
- Output/Feature/vendor command 없음

실측 Windows Bluetooth HID 경로에서는 78-byte full packet이 WinRT
`Report.Id=0x01`, `Data[0]=0x01`로 노출된다. 따라서 report ID보다 packet
length를 우선하여 Bluetooth layout을 판별하고 검증된 status offset 54를 사용한다.

## 실행

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.DualSenseLifecycleProbe
dotnet run --project .\DeviceBattery.Poc.DualSenseLifecycleProbe.csproj
```

## 실장비 순서

1. Bluetooth DualSense ON 상태에서 실행한다.
2. `REPORT_RECOVERED`와 `Available`을 확인한다.
3. DualSense를 OFF하고 watcher Removed 또는 10초 timeout을 확인한다.
4. DualSense를 ON하고 Added/open/report recovery를 확인한다.
5. 위 OFF/ON을 3회 반복한다.
6. 필요하면 `R`로 read-only reopen을 실행한다.
7. `S` summary 후 `Q`로 종료하고 cleanup 로그를 확인한다.

10초는 빠른 실장비 검증을 위한 POC 후보이며 Production 정책이 아니다.
