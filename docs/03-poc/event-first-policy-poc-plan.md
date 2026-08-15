# POC-B06 — Event-first / Poll Fallback 정책 검증

작성일: 2026-08-15
상태: COMPLETE — PASS WITH LIMITATION
기준: Requirements Baseline v1.2 / CHG-002

## 목적

Provider capability에 따라 event-first를 우선하고, 신뢰 가능한 battery read endpoint가
있는 경우에만 polling fallback을 허용하는지 deterministic test로 검증한다.

## DualSense v1.0 정책

- `InputReportReceived` event-first
- 별도 battery read endpoint가 없으므로 device polling 없음
- timer는 마지막 valid event의 freshness만 판단
- timeout 시 stale percentage를 제거하고 Unknown
- 정상 input event 재수신 시 Available 복구
- 10초 값은 lifecycle POC용이며 Production 주기가 아님

## 안전성

이 POC는 장치 API를 호출하지 않는다. Output/Feature/vendor command가 없다.

## 판정

- PASS: 정책 matrix 전체 통과 및 DualSense timer device-read 0회
- PASS WITH LIMITATION: 정책은 통과하지만 Production timeout/직렬화 정책 미확정
- NEED ALTERNATIVE: event-only provider에서 polling 또는 stale 값 유지 발생

## 실행 결과

2026-08-15 deterministic policy matrix `8/8 PASS`.

- DualSense event-only 선택: PASS
- DualSense timer device-read 0회: PASS
- timeout stale percentage 제거: PASS
- event recovery: PASS
- readable non-event provider만 poll fallback 허용: PASS

최종 판정은 **PASS WITH LIMITATION**이다. 정책 경계는 검증됐지만 Production timeout과
timer/input callback 직렬화는 Gate 4 이후 설계 승인을 받아 확정한다.
