# DeviceBattery.Poc.DualSenseEventContinuityProbe

Gate 4 POC-C03 passive input-event continuity probe다.

Upstream Linux `hid-playstation`의 `dualsense_input_report`에는 `seq_number`가 정의돼
있다. Windows WinRT 78-byte buffer는 report ID 노출이 변형되므로 offset을 선결정하지
않고 후보 7/8의 연속 증가율을 동시에 측정한다. 1차 실측에서 offset 7이 대부분
`+4` stride를 보였다. 2차 실측에서 `(offset7 >> 2)`가 정확히 15/16만 sequential이고
1/16은 wrap 형태여서 bits 2~5의 modulo-16 후보도 함께 측정한다.

Primary reference:
https://github.com/torvalds/linux/blob/master/drivers/hid/hid-playstation.c

안전성:

- targeted DualSense selector
- Bluetooth HID session만 선택
- `FileAccessMode.Read`
- Output/Feature/vendor command 없음

```powershell
dotnet run -c Release --project .\DeviceBattery.Poc.DualSenseEventContinuityProbe.csproj
```

DualSense를 60초 동안 계속 조작한 후 `S`, `Q`를 입력한다. 실제 counter 후보는
Sequential 비율이 지배적이어야 하며, 그 후보에 대해서만 MissingEstimate를 해석한다.
