# Device Provider 설계 입력 — POC-A 결과

## Connection State 계층

```text
Physical Device
  ├─ Transport / Receiver State
  │    └─ Generic DeviceWatcher
  └─ Peripheral Online State
       ├─ Bluetooth / BLE state
       ├─ HID Feature / Input state
       └─ Vendor-specific state
```

## 현재 설계 원칙 후보

- `DeviceInformation.Id`는 Interface 접근용 ID로 취급한다.
- 물리 장치 그룹핑은 Provider가 책임진다.
- ContainerId는 값이 의미 있을 때만 1차 그룹핑 후보로 사용한다.
- sentinel/default/empty ContainerId는 식별 키로 사용하지 않는다.
- Display Name도 Provider 우선 + Generic fallback 구조가 필요하다.
- Receiver 기반 2.4GHz 장치는 Receiver 연결과 실제 Peripheral Online을 분리한다.
- 하나의 child interface가 내려갔다고 즉시 전체 물리 장치를 disconnected 처리하지 않는다.
