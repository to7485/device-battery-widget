# CHG-006 — Standard BLE GATT Battery Production Support

| 항목 | 내용 |
|---|---|
| 변경요청 ID | CHG-006 |
| 요청일 | 2026-08-16 |
| 상태 | **Approved** |
| 승인권자 | 발주자 |
| 기준 Baseline | Requirements v1.5 |
| 변경 Baseline | Requirements v1.6 |

## 변경 내용

1. Bluetooth SIG Battery Service `0x180F`와 Battery Level `0x2A19`를 노출하는 BLE 장치를 Production 지원 범위에 추가한다.
2. AULA F87Pro는 Gate 4 POC-B02 실장비 대표 검증 장치이며 Provider는 제품 전용이 아닌 표준 서비스 기반으로 구현한다.
3. Battery Level은 exact percent로 표시하고, 표준 Battery Level만으로 알 수 없는 charging 상태는 `Unknown`으로 유지한다.
4. Notify/Indicate를 우선 사용하고 미지원 장치만 30초 uncached read fallback을 사용한다.
5. DualSense USB/Bluetooth 우선 선택은 유지하며 BLE 장치는 별도 indicator row로 자동 추가/제거한다.

## 안전 및 제한

- 표준 GATT service/characteristic만 탐색한다.
- Battery Level read와 CCCD Notify/Indicate subscribe/unsubscribe만 사용한다.
- vendor-specific characteristic write와 임의 command는 사용하지 않는다.
- BLE 서비스 노출 여부가 실제 물리 연결 상태와 항상 동일하다는 가정은 하지 않는다.

```text
CHG-006 = APPROVED
Requirements Baseline = v1.6
Production Providers = DualSense HID + Standard BLE GATT Battery
```
