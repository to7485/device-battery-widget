# POC-B01~B04 — Battery / Charging Probe

## 목적

Windows.Devices.Power API가 실제 PC에서 노출하는 Battery Controller를 조사한다.

## 실행

```powershell
dotnet run
```

## 출력

- Aggregate Battery
- 개별 Battery Controller 목록
- Battery Status
- Charge Rate
- Full Charge Capacity
- Remaining Capacity
- 계산 Battery %
- 제품 요구사항용 Charging 상태 후보
- ReportUpdated Event

## 매우 중요한 해석 원칙

`Windows.Devices.Power.Battery`의 Battery 객체는 **Battery Controller**를 나타낸다.

따라서 출력되는 Battery Controller가 곧바로
마우스/키보드/게임패드/헤드셋 Battery를 의미한다고 가정하지 않는다.

반대로 특정 주변장치가 이 목록에 나오지 않는다고 해서
그 장치의 Battery를 Windows에서 절대로 읽을 수 없다고 즉시 결론내리지 않는다.

그 경우 다음 대체 경로를 후속 POC에서 검토한다.

- Device property
- Bluetooth GATT
- HID
- Win32 / SetupAPI
- Vendor-specific interface

## Battery %

Microsoft 문서 기준으로:

RemainingCapacityInMilliwattHours / FullChargeCapacityInMilliwattHours

비율을 사용한다.

두 값 중 하나가 없으면 `N/A`로 기록한다.

## Charging

현재 POC의 제품 상태 매핑 후보:

- BatteryStatus.Charging → Charging
- BatteryStatus.Discharging → Not Charging
- BatteryStatus.Idle → Not Charging
- BatteryStatus.NotPresent → Unknown

최종 매핑은 실제 장치 결과를 본 후 확정한다.
