# CHG-008 — Production Memory Acceptance Target

| 항목 | 내용 |
|---|---|
| 변경요청 ID | CHG-008 |
| 요청일 | 2026-08-16 |
| 상태 | **Approved** |
| 승인권자 | 발주자 |
| 기준 Baseline | Requirements v1.7 |
| 변경 Baseline | Requirements v1.8 |

## 변경 내용

1. 정상 상태 메모리 수용 기준을 Working Set 150 MiB 이하로 변경한다.
2. 프로세스 전용 메모리 관찰 기준으로 Private Memory 100 MiB 이하를 함께 적용한다.
3. CPU, Handle/Thread 증가 추세, 24시간 및 72시간 안정성 요구사항은 완화하지 않는다.
4. 강제 Working Set trim이나 주기적 강제 GC를 수치 충족 수단으로 사용하지 않는다.

## 실측 근거

- 5분 전체 Production: Working Set 138.16 MiB, Private Memory 71.43 MiB
- 60초 전체 재현: Working Set 136.04 MiB, Private Memory 70.45 MiB
- Shell 기준선: Working Set 108.78 MiB
- DualSense HID 추가: 약 20.54 MiB
- BLE/WGI 추가: 약 6.72 MiB
- 모든 단기 측정에서 Working Set, Private Memory, Handle, Thread 증가 추세 없음

기존 v1.7의 100 MiB Working Set 실패 결과와 원본 CSV는 변경하지 않고 보존한다.

```text
CHG-008 = APPROVED
Requirements Baseline = v1.8
Working Set <= 150 MiB
Private Memory <= 100 MiB
```
