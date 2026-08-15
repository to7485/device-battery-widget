# CHG-002 — v1.0 DualSense 단일 장치 범위

## 문서 정보

| 항목 | 내용 |
|---|---|
| 변경요청 ID | CHG-002 |
| 요청일 | 2026-08-15 |
| 상태 | **Approved** |
| 승인권자 | 발주자 |
| 기준 Baseline | Requirements v1.1 |
| 변경 Baseline | Requirements v1.2 |

## 변경 내용

첫 정식 릴리스 v1.0의 지원 장치를 **Sony DualSense Bluetooth (`VID 054C / PID 0CE6`)** 하나로 제한한다.

다음 장치/경로는 v1.0 필수 범위에서 제외하고 후속 릴리스 확장 후보로 이관한다.

- Mouse / Logitech G703 receiver
- Keyboard / AULA F87Pro
- Headset / Corsair VOID WIRELESS V2 receiver
- 기타 game controller 및 범용 peripheral

## 변경 사유

- DualSense HID battery bucket과 charging state가 실장비에서 검증됐다.
- Receiver 장치는 online 후보까지 확인했으나 battery protocol 검증 비용과 불확실성이 남았다.
- v1.0을 검증된 단일 장치로 완성한 뒤 Provider/Parser 구조를 통해 후속 장치를 확장한다.

## 영향

- FR-001 v1.0 지원 대상 변경
- UIR-011 Battery 미지원 장치 표시를 vNext로 이관
- POC-B04 Receiver Battery 추가 조사를 중단/동결
- 기존 POC 결과와 코드는 기술 증거로 보존
- Gate 4 이후 Production 설계/구현은 별도 Gate 승인 필요

## 승인 결과

```text
CHG-002 = APPROVED
Requirements Baseline = v1.2
v1.0 Supported Device = Sony DualSense Bluetooth only
```
