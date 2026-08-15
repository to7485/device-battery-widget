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
