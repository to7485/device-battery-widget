# DeviceBattery.Poc.NormalizedBatteryState

Gate 4 POC-B05: 검증된 DualSense HID status byte를 애플리케이션 공통 `BatteryState` 후보로 정규화한다.

## 범위

- CHG-002에 따른 Sony DualSense Bluetooth-only v1.0
- B03-2의 실장비 검증 결과를 입력 evidence로 사용
- Production 프로젝트가 아닌 독립 POC
- 장치 열거/접근 및 HID command 없음

## 검증 항목

- `Available / Unsupported / Unknown` 구분
- `Charging / NotCharging / Unknown` 구분
- 10% bucket의 midpoint 추정값과 `IsEstimated`
- Full 상태의 100% 처리
- invalid bucket/charging error를 `Unknown` 처리
- 일시 읽기 실패 시 이전 percent를 폐기하고 `Unknown` 처리
- 정상 report 복구 시 `Available`로 복귀

## 실행

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.NormalizedBatteryState
dotnet run --project .\DeviceBattery.Poc.NormalizedBatteryState.csproj
```

모든 deterministic case가 PASS해야 POC-B05 모델 변환을 PASS로 판정한다. 실장비 lifecycle과 timeout 정책은 별도 Gate 4 항목에서 확인한다.
