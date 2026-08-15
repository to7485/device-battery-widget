# CHG-007 — Xbox Windows.Gaming.Input Battery Production Support

| 항목 | 내용 |
|---|---|
| 변경요청 ID | CHG-007 |
| 요청일 | 2026-08-16 |
| 상태 | **Approved** |
| 승인권자 | 발주자 |
| 기준 Baseline | Requirements v1.6 |
| 변경 Baseline | Requirements v1.7 |

## 변경 내용

1. 유효한 `Windows.Gaming.Input.Gamepad.TryGetBatteryReport()`를 제공하는 Xbox 컨트롤러를 Production 범위에 추가한다.
2. Gamepad Added/Removed 이벤트로 연결 수명주기를 관리하고 BatteryReport만 30초 주기로 조회한다.
3. Remaining/Full capacity로 퍼센트를 계산하되 단계형 값일 가능성을 반영해 estimated granular precision으로 표시한다.
4. DualSense는 기존 HID Provider가 담당하므로 WGI Provider에서 제외한다.

## 실장비 근거

```text
VID/PID = 0x045E/0x0B13
Status = Discharging
Remaining/Full = 100/1000
CalculatedPercent = 10%
```

## 안전

- 공개 read-only BatteryReport API만 사용한다.
- 입력 polling, vibration, output command는 사용하지 않는다.

```text
CHG-007 = APPROVED
Requirements Baseline = v1.7
```
