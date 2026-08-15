# POC-B04 — 2.4 GHz Receiver / Vendor Battery 검증 계획

## 목적

2.4 GHz receiver-backed 장치에서 Windows가 receiver 존재 여부만 노출하는 한계를 보완하기 위해, 실제 peripheral online/battery 상태를 얻을 수 있는 HID/vendor 경로를 단계적으로 찾는다.

## 테스트 장비

- Logitech G703 + 2.4 GHz receiver (`VID 046D / PID C539`)
- Corsair VOID WIRELESS V2 + 2.4 GHz receiver (`VID 1B1C / PID 2A08`)

## B04-1 — HID Collection Discovery

우선 Windows가 각 receiver에 대해 노출하는 HID top-level collection을 조사한다.

수집:

- HID symbolic path
- VID/PID/version
- product/manufacturer/serial string
- UsagePage / Usage
- InputReportByteLength
- OutputReportByteLength
- FeatureReportByteLength
- WinRT HidDevice read-only open 가능 여부

vendor-defined UsagePage(`0xFF00` 이상)는 제조사 receiver protocol 후보로 표시한다.

## B04-2 — Battery/Online Signal

B04-1 결과에 따라 후보 collection만 대상으로 후속 POC를 만든다.

우선순위:

1. passive input/status report
2. read-only feature report
3. vendor request/response protocol

고주기 polling이나 무차별 vendor command 전송은 하지 않는다.

## B04-1 실장비 결과 — 2026-08-15

### Logitech G703 receiver

- Native HID top-level collection: 7개
- Vendor-defined collection: 4개
- WinRT read-only open: 6/7
- 보호된 keyboard collection 1개는 `OPEN=null`
- `FF00/0001`, Report ID `0x10`, 7-byte passive report에서 본체 전원과 연동되는 변화 확인

```text
OFF = 10 01 41 0C 62 86 40
ON  = 10 01 41 0C A2 86 40
                    ^ offset 4: 0x62 <-> 0xA2
```

두 번의 OFF/ON에서 동일하게 왕복했다.

### Corsair VOID WIRELESS V2 receiver

- Native HID top-level collection: 5개
- Vendor-defined collection: 3개
- WinRT read-only open: 5/5
- `FF42/0002`, Report ID `0x03`, 64-byte passive report에서 본체 전원/초기화와 연동되는 상태 순서 확인

```text
OFF           = 03 00 01 36 00 00 ...
ON transition = 03 00 01 36 00 02 ...
ON initialized= 03 01 01 D0 00 01 ...
```

두 번의 OFF/ON에서 동일한 순서가 반복됐다.

### B04-1 판정

- HID TLC discovery/read-only open: `PASS`
- Passive peripheral-online signal: `PASS WITH LIMITATION`
- 제한: 2회 반복 실측이며 앱 재시작/장시간/재연결 검증은 아직 수행하지 않음
- Battery byte 해석: `NOT TESTED`

승인에 따라 B04-1 결과를 동결하고 B04-2의 passive battery correlation 조사로 이동한다.

## 판정

- PASS: 실제 peripheral battery/online 신호를 안정적으로 획득
- PASS WITH LIMITATION: 특정 vendor/model에서만 가능하거나 polling/정밀도 제한
- NEED ALTERNATIVE: receiver HID는 보이지만 필요한 signal을 얻을 수 없음
- FAIL: 해당 방식이 제품 요구사항에 부적합
