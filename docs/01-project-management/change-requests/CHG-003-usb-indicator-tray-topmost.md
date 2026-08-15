# CHG-003 — DualSense USB / Simple Indicator / Tray-only Topmost

| 항목 | 내용 |
|---|---|
| 변경요청 ID | CHG-003 |
| 요청일 | 2026-08-15 |
| 상태 | **Approved** |
| 승인권자 | 발주자 |
| 기준 Baseline | Requirements v1.2 |
| 변경 Baseline | Requirements v1.3 |

## 변경 내용

1. v1.0 DualSense 지원 transport를 Bluetooth와 USB로 확대한다.
2. 위젯은 한 개의 간단한 배터리 인디케이터로 표시한다.
3. Bluetooth와 USB endpoint가 함께 있으면 USB 상태를 우선 표시한다.
4. Always On Top은 Tray 메뉴에서만 변경한다.

## 기술 제약

- 같은 컨트롤러의 Bluetooth와 USB ContainerId는 서로 다르게 실측됐다.
- Feature/vendor 명령 없이 두 transport의 물리 동일성을 보편적으로 가정하지 않는다.
- 내부 상태는 endpoint별로 유지하고 indicator projection에서 USB를 우선한다.
- targeted selector, `FileAccessMode.Read`, Output/Feature/vendor command 금지는 유지한다.

```text
CHG-003 = APPROVED
Requirements Baseline = v1.3
v1.0 Supported Device = Sony DualSense Bluetooth + USB
Widget = Single compact indicator
Always On Top control = Tray only
```
