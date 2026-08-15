# Gate 4 POC 상태 업데이트 — POC-A 실장비 결과 반영

작성일: 2026-08-15
상태: Gate 4 진행 중 / POC-A 결과 동결 후 POC-B로 이동

## 1. POC-A 결과 요약

| 장치 | 연결 방식 | 연결/해제 감지 | 본체 OFF/ON 감지 | 이름 | ContainerId | 판정 |
|---|---|---|---|---|---|---|
| DualSense | USB | UPDATED + InterfaceEnabled False/True | USB 연결 기준 감지 | 양호 | 유효 | PASS WITH LIMITATION |
| Logitech G703 | USB 직접 / 2.4GHz Receiver | USB/Receiver 경로 변화 감지 | Receiver 유지 상태의 마우스 OFF/ON 이벤트 없음 | Generic 이름 품질 낮음 | Receiver 경로에서 sentinel 값 관찰 | NEED ALTERNATIVE |
| CORSAIR VOID WIRELESS V2 | 2.4GHz Receiver | 동글 제거/재삽입 감지 | Receiver 유지 상태의 Headset OFF/ON 이벤트 없음 | 양호 | 유효 | PASS + NEED ALTERNATIVE |
| AULA F87Pro | Bluetooth LE | BLE/HID Interface 이벤트 감지 | OFF/ON 시 InterfaceEnabled 변화 관찰 | `AULA-F87Pro 5.0` | 유효 후보 | PASS WITH LIMITATION |

## 2. 확정 가능한 기술적 발견

1. Generic `DeviceInformation.CreateWatcher()`는 USB/Device Interface의 활성/비활성 변화를 감지할 수 있다.
2. 실제 실장비에서는 `ADDED/REMOVED`보다 `UPDATED + System.Devices.InterfaceEnabled=False/True`가 중요한 연결 상태 신호로 나타나는 경우가 많다.
3. `System.Devices.Connected`는 테스트 장치들에서 반복적으로 null이므로 주 연결 상태 신호로 사용하지 않는다.
4. 하나의 물리 장치는 USB/HID/Audio 등 여러 Device Interface로 노출될 수 있다. 따라서 Interface 1개를 곧바로 물리 장치 1개로 취급하면 안 된다.
5. ContainerId는 DualSense/Corsair/AULA에서 물리 장치 그룹 후보로 유효했으나 Logitech Receiver 사례에서는 sentinel/default 값이 관찰되어 범용 단일 키로 사용할 수 없다.
6. 2.4GHz Receiver가 계속 PC에 꽂혀 있는 동안 실제 무선 Peripheral의 전원을 OFF/ON해도 G703과 Corsair에서는 Generic DeviceWatcher 이벤트가 발생하지 않았다.
7. 따라서 `ReceiverConnected == PeripheralOnline`으로 간주하면 안 된다.
8. 2.4GHz 장치의 실제 본체 online/offline은 HID Feature Report, Vendor-specific Interface 또는 별도 상태 조회 Provider가 필요할 수 있다.
9. AULA F87Pro Bluetooth LE에서는 표준 BLE Battery Service UUID `0x180F` 노출 단서가 확인되어 Battery POC의 주요 대상이다.

## 3. POC-A 판정

- A01 Enumeration: PASS
- A02/A03 Generic USB Interface 변화 감지: PASS WITH LIMITATION
- A04 Friendly Name: 장치별 편차 존재 / PASS WITH LIMITATION
- A05 물리 장치 그룹핑: ContainerId 단독 범용화 불가 / NEED PROVIDER FALLBACK
- A06 재연결 Identity: 일부 장치에서 안정성 확인, 전 연결 방식 범용 보장 불가 / PASS WITH LIMITATION

POC-A는 '범용 단일 API로 모든 연결/해제와 Identity를 해결할 수 없다'는 결론까지 확보한 상태로 동결한다.
세부 Alternative Provider 검증은 POC-B/C에서 Battery/상태 신호와 함께 이어간다.

## 4. 다음 단계

POC-B Battery / Charging 검증으로 이동한다.
우선 `Windows.Devices.Power.Battery` 기반 BatteryProbe를 실행하여 실제 주변장치 Battery Controller 노출 여부를 확인한다.

우선 테스트 대상:
1. AULA F87Pro — Bluetooth
2. Logitech G703 — 2.4GHz
3. CORSAIR VOID WIRELESS V2 — 2.4GHz
4. DualSense — USB/Bluetooth 필요 시 추가

Generic Battery API에서 주변장치가 노출되지 않으면 장치별 Provider 검증으로 전환한다.
AULA Bluetooth의 경우 BLE GATT Battery Service `0x180F` 직접 조회를 1순위 Alternative로 둔다.
