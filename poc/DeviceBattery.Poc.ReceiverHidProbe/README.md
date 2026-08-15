# DeviceBattery.Poc.ReceiverHidProbe

Gate 4 POC-B04-1용 read-only HID discovery probe입니다.

목적은 G703/Corsair 같은 2.4 GHz receiver-backed 장치에서 곧바로 제조사 프로토콜을 하드코딩하기 전에 Windows가 노출하는 HID top-level collection을 확인하는 것입니다.

## 대상

- Logitech G703 테스트 receiver: VID `046D`, PID `C539`
- Corsair VOID WIRELESS V2 receiver: VID `1B1C`, PID `2A08`

## 하는 일

1. Windows HID device interface class를 열거합니다.
2. 대상 VID/PID만 필터링합니다.
3. 각 HID collection의 UsagePage/Usage 및 Input/Output/Feature report 최대 길이를 출력합니다.
4. vendor-defined UsagePage(`0xFF00` 이상)를 후보로 표시합니다.
5. WinRT `HidDevice`로 열 수 있는 collection은 read-only로 엽니다.
6. 최초 input report를 Device ID와 UsagePage/Usage별로 구분하여 출력합니다.
7. Receiver별 native collection, vendor-defined collection, selector/open 결과를 요약합니다.

Output report, Feature Set, vendor command는 보내지 않습니다.

## 실행

```powershell
cd D:\github\device-battery-widget\poc\DeviceBattery.Poc.ReceiverHidProbe
dotnet clean
dotnet run --project .\DeviceBattery.Poc.ReceiverHidProbe.csproj
```

## 보내야 할 결과

첫 실행에서는 별도 OFF/ON 테스트보다 **전체 초기 출력**을 먼저 저장합니다.

특히 다음 항목이 중요합니다.

- Target HID collection count
- ProductString
- VID/PID
- UsagePage / Usage
- InputReportLength
- OutputReportLength
- FeatureReportLength
- CollectionClass
- WINRT SELECTOR별 OPEN 결과
- Input report의 DeviceId / UsagePage / Usage / ReportId / 길이

## 실장비 실행 순서

1. 두 receiver와 본체를 정상 연결한 상태에서 실행합니다.
2. 초기 collection 및 open 결과를 저장합니다.
3. G703을 끄기 직전에 `1`(`G703_OFF`), 켜기 직전에 `2`(`G703_ON`)를 입력합니다.
4. Corsair headset을 끄기 직전에 `3`(`CORSAIR_OFF`), 켜기 직전에 `4`(`CORSAIR_ON`)를 입력합니다.
5. `S`로 collection별 input report count를 출력합니다.
6. `Q`로 정상 종료합니다.

각 marker 이후에는 report shape별 변경 report를 최대 20개 출력하며, 동일 report는 억제합니다. 출력의 `ChangedBytes`는 0-based byte offset과 이전/현재 값을 표시합니다. OFF/ON 왕복 재현성을 보기 위해 각 동작을 3회 반복합니다.

B04-2 passive battery correlation에서는 제조사 앱 구간을 다음 marker로 구분합니다.

```text
5 = APPS_CLOSED_BASELINE
6 = GHUB_BATTERY_SCREEN
7 = ICUE_BATTERY_SCREEN
```

Probe는 read-only를 유지한다. G HUB/iCUE가 공식 앱 동작으로 생성하는 traffic과 앱이 닫힌 baseline을 비교하기 위한 marker다.

이 단계에서는 Feature report를 읽거나 쓰지 않으며 Output report와 vendor command도 전송하지 않습니다.

이 결과를 보고 B04-2에서 어떤 collection/프로토콜을 조사할지 결정합니다.
