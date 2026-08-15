# DeviceBattery.Poc.SystemTrayLifecycle

Gate 4 POC-D01~D05 System Tray/Lifecycle feasibility probe다.

## 검증

- tray icon 표시
- context menu와 double-click으로 widget 표시
- minimize 시 widget만 숨기고 application/tray 유지
- Widget X에서 application 전체 종료
- tray `종료`에서 application 전체 종료
- 종료 시 tray icon/menu/handler/widget dispose 및 ghost icon 여부
- Always On Top 동작

`숨긴 장치 관리`는 안내창만 표시한다. `Windows 로그인 자동 실행`은 in-memory check만
바꾸며 registry나 OS 설정을 변경하지 않는다. Device/HID 접근도 없다.

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.SystemTrayLifecycle
dotnet run -c Release --project .\DeviceBattery.Poc.SystemTrayLifecycle.csproj
```

## 실장비 순서

1. widget과 tray icon 표시를 확인한다.
2. 최소화 후 widget이 숨고 tray가 유지되는지 확인한다.
3. tray double-click과 `Widget 표시`로 각각 복원한다.
4. Always On Top을 ON/OFF한다.
5. 자동 실행 항목을 눌러도 registry가 변경되지 않는다는 로그를 확인한다.
6. tray `종료` 후 `[CLEANUP]`과 ghost icon 부재를 확인한다.
7. 다시 실행하여 Widget X 종료 후 같은 cleanup을 확인한다.
