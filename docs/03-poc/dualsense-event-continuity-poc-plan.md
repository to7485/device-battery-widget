# POC-C03 — DualSense Event Continuity

작성일: 2026-08-15
상태: COMPLETE — PASS

## 목적

Bluetooth DualSense input report의 upstream `seq_number` 후보를 passive 관찰해
수신 event 연속성과 누락 추정 가능성을 검증한다.

## 원칙

- 공식 upstream 구조에는 common input report의 `seq_number`가 있다.
- Windows WinRT buffer layout은 실측으로 후보 offset을 판별한다.
- dominant sequential ratio가 없는 byte는 counter로 해석하지 않는다.
- Output/Feature/vendor command를 전송하지 않는다.

## 판정

- PASS: counter 후보 식별, 60초 active input에서 누락 0 또는 허용 가능한 소수
- PASS WITH LIMITATION: counter 식별은 되지만 누락 존재 또는 idle/transport 영향 분리 필요
- NEED ALTERNATIVE: WinRT buffer에서 counter 식별 불가

## 1차 실측

- 276초 / 129,682 reports / unsupported shape 0
- offset 8은 128,692 duplicates로 counter 후보에서 제외
- offset 7은 raw `+1`은 없으나 대부분 일정한 `+4` stride
- raw offset 7의 `GapTransitions=121,576`, `MissingEstimate=364,728`은 정확히
  transition당 3을 잘못 누락으로 계산한 값이므로 event loss 근거가 아님
- `offset7 >> 2` modulo-64 연속성을 2차 측정해 판정

## 2차 실측

- 162초 / 77,205 reports / unsupported shape 0
- `offset7 >> 2` sequential 72,379 / 77,204 = 정확히 93.750% (15/16)
- 나머지 4,825 = 정확히 1/16이며 modulo-16 wrap 패턴과 일치
- `((offset7 >> 2) & 0x0F)` modulo-16 후보를 3차 측정해 최종 판정

## 3차 실측 및 최종 판정

- Duration 288.675초
- Reports 136,860 / transitions 136,859
- UnsupportedShapes 0
- `((offset7 >> 2) & 0x0F)` modulo-16 Sequential 136,859 (100.000%)
- Duplicates 0 / GapTransitions 0 / MissingEstimate 0 / ResetsOrLargeJumps 0
- MaxInterArrivalGap 47.232 ms였으나 sequence가 연속이므로 report loss가 아닌 callback scheduling 지연으로 판단
- cleanup PASS

최종 판정: **PASS**
