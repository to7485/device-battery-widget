# DeviceBattery.ProductionSmoke

Gate 6에서 Production `DualSenseHidProvider → DeviceStateCoordinator → DeviceStateReducer`
경로를 실장비로 확인하는 제한 시간 console host다. 제품 UI가 아니며 Gate 4 POC 코드를
변경하거나 복사하지 않는다.

## 안전 범위

- targeted DualSense gamepad selector
- Bluetooth HID service endpoint만 허용
- `FileAccessMode.Read`만 사용
- 원본 HID Device ID를 출력하지 않음
- Output/Feature/vendor command 없음
- `DeviceClass.All` 없음

## 실행

```powershell
dotnet run -c Release --project .\tools\DeviceBattery.ProductionSmoke -- 30
```

인수는 5~600초이며 기본값은 15초다. 시간이 끝나거나 `Ctrl+C`를 누르면 Provider를
취소하고 channel drain 및 HID/watcher cleanup 후 종료한다.
