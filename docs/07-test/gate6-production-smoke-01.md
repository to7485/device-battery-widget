# Gate 6 Production Pipeline Smoke 01

- 일시: 2026-08-15 20:21 KST
- 대상: Bluetooth Sony DualSense VID `0x054C`, PID `0x0CE6`
- 실행: `DeviceBattery.ProductionSmoke`, 15초
- 안전 범위: targeted selector, Bluetooth endpoint filter, `FileAccessMode.Read`
- 금지 항목: 원본 HID ID 출력, Output/Feature/vendor command, `DeviceClass.All`

## 결과

```text
DeviceDiscovered -> Unknown / Percent=null / Revision=1
ReportRecovered  -> Available / Percent=15% / NotCharging / Revision=2
Processed=2
Faulted=0
Snapshots=1
CLEANUP=COMPLETE
```

DeviceKey는 원본 Windows HID ID 대신 Provider 소유 SHA-256 축약 key로 출력됐다.
배터리 정밀도는 `TenPercentBucket`, `Estimated=true`였다.

## 환경 관찰

제한된 자동 실행 sandbox에서는 `DeviceInformation.CreateWatcher`가
`FileNotFoundException`으로 차단됐다. 동일 binary를 일반 사용자 Windows 실행 환경에서
재실행하자 watcher, read-only open, input report, cleanup이 정상 완료됐다. 이는 Provider
기능 실패가 아니라 실행 환경의 장치 열거 접근 차이로 판정한다.

## 판정

`PASS WITH LIMITATION`

- PASS 근거: Production Provider → Coordinator → Reducer 실제 장비 경로와 cleanup 성공
- 남은 제한: OFF/ON, 10초 Unknown, 30초 Dormant, sleep/resume는 Production host에서 추가 검증 필요
