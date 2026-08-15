# Gate 6 CHG-003 USB Smoke 01

- 일시: 2026-08-15
- 대상: DualSense USB, VID `0x054C`, PID `0x0CE6`
- 안전: targeted selector, `FileAccessMode.Read`, input report only

## 실장비 결과

```text
DeviceDiscovered -> DualSense Controller (USB), Unknown, Revision=1
ReportRecovered  -> Available, Percent=5%, Charging, Revision=2
Processed=2
Faulted=0
CLEANUP=COMPLETE
```

원본 HID ID는 출력하지 않았고 `USB-` transport prefix와 축약 hash key만 사용했다.
판정은 `PASS`다.

## CHG-004 35초 유지 회귀

USB 전용 freshness timeout 제외 후 35초 동안 관찰했다. 30초 Dormant 경계를 지난 최종
summary에서도 `Visible=True`, `Available`, `5%`, `Charging`, `Faulted=0`이 유지됐고
cleanup이 완료됐다. CHG-004 USB 충전 표시 유지 판정은 `PASS`다.
