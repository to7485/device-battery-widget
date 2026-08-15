# CHG-004 — USB Charging Persistence / Frameless Indicator

| 항목 | 내용 |
|---|---|
| 변경요청 ID | CHG-004 |
| 요청일 | 2026-08-15 |
| 상태 | **Approved** |
| 승인권자 | 발주자 |
| 기준 Baseline | Requirements v1.3 |
| 변경 Baseline | Requirements v1.4 |

## 변경 내용

1. DualSense USB endpoint가 연결된 동안 마지막 valid battery/charging 표시를 유지한다.
2. Bluetooth에는 기존 10초 Unknown / 30초 Dormant 정책을 유지한다.
3. 위젯의 Windows title bar를 제거하고 frameless indicator로 표시한다.
4. 카드 내부 최소 `×` 버튼은 전체 Application cleanup 종료 경로를 유지한다.

## 제한

USB에서는 endpoint `Removed`가 연결 해제의 권위 있는 신호다. 연결 중 report가 정지해도
마지막 valid 값을 유지하므로 순간적인 실제 값 변화 반영이 늦을 수 있다. Output/Feature
report를 사용하지 않는 read-only 안전 정책을 우선한다.

```text
CHG-004 = APPROVED
Requirements Baseline = v1.4
```
