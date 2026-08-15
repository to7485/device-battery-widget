# POC-B05 — DualSense Normalized BatteryState

작성일: 2026-08-15
상태: PASS WITH LIMITATION
기준: Requirements Baseline v1.2 / CHG-002

## 목적

B03-2에서 검증된 DualSense HID battery bucket/charging code를 UI와 독립적인 공통 상태로 변환할 수 있는지 검증한다.

## 모델 후보

```text
BatteryState
  Availability: Available / Unsupported / Unknown
  Percent: int?
  Charging: Charging / NotCharging / Unknown
  Precision: Unknown / TenPercentBucket / Full
  IsEstimated: bool
  SourceProvider
  IsEventDriven
  LastUpdatedAt
  Reason
```

## 핵심 규칙

- 정상 DualSense status: `Available`
- bucket `0..9`: midpoint `5..95`, `TenPercentBucket`, estimated
- bucket `10` 또는 Full code: `100`, `Full`, not estimated
- 일시 읽기 실패: 이전 percent를 유지하지 않고 즉시 `Unknown`
- 오류/미확인 charging code 또는 invalid bucket: `Unknown`
- 지원 프로파일이 없는 장치의 `Unsupported`는 알려진 DualSense의 일시 실패와 구분
- 정상 report 재수신 시 `Available`로 복귀

## 판정

- PASS: 모든 deterministic normalization/state-transition case 통과
- PASS WITH LIMITATION: 모델 변환은 통과하지만 lifecycle/timeout 실장비 검증이 남음
- NEED ALTERNATIVE: Unsupported/Unknown 구분 또는 stale value 제거를 일관되게 표현할 수 없음

## 실행 결과

```text
Discharging bucket 0                 PASS
Charging bucket 0                    PASS
Full                                 PASS
Controller error code -> Unknown     PASS
Invalid bucket -> Unknown            PASS
Read failure clears stale percent    PASS
Unsupported != Unknown               PASS
Valid report recovers from Unknown   PASS

RESULT = PASS (8/8)
```

모델 정규화는 PASS다. 다만 실제 disconnect/reconnect, timeout 기준과 lifecycle 연계는 실장비 통합 검증 전이므로 B05 최종 판정은 `PASS WITH LIMITATION`이다.
