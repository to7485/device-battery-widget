# Device State Machine 초안

상태: Gate 5 REVIEW DRAFT
관련 설계: ARC-003, ARC-004, ARC-007

## 1. 상태

| State | UI | Percent | Provider session |
|---|---|---|---|
| Absent | 목록 없음 | 없음 | 없음 또는 grace session |
| Waiting | 조회 중 | null | 열기/첫 report 대기 |
| Available | 표시 | 0~100 | active |
| Unknown | 표시 | null | active/recovery 대기 |
| Dormant | active 목록 없음 | null | recovery를 위해 제한적으로 유지 |
| Unsupported | v1에서는 사용하지 않음 | null | provider 정책 |

사용자 `Hidden`은 lifecycle state가 아니라 `IsHidden` 표시 설정이다. lifecycle 전이는 계속
진행하되 UI projection에서 제외한다.

## 2. 전이

| Current | Command | Next | Action |
|---|---|---|---|
| Absent | DeviceDiscovered | Waiting | snapshot 생성, session generation 등록 |
| Waiting | ValidBattery | Available | normalized battery 반영 |
| Available | BatteryChanged | Available | revision 증가, UI 갱신 |
| Available | FreshnessExpired | Unknown | stale percent 제거 |
| Unknown | ReportRecovered | Available | 즉시 복구 |
| Any | WindowsRemoved | Absent | callback detach/HID dispose/UI 제거 |
| Waiting/Available/Unknown | OfflineGraceExpired | Dormant | UI 제거, session 유지 |
| Dormant | ReportRecovered | Available | UI 재추가 |
| Any | HideRequested | same | IsHidden=true 저장, UI projection 제외 |
| Any | UnhideRequested | same | IsHidden=false 저장, 현재 state로 UI 복원 |
| Any | OlderGenerationEvent | same | 무시 + metric |

## 3. Freshness 후보

- 1초 coordinator tick
- 10초 valid report 없음: Unknown, Percent=null
- 30초 valid report 없음: active UI에서 제거
- report 복구: 즉시 Available 및 목록 재추가
- Windows Removed: 30초 grace 없이 제거/정리

10초/30초는 Architecture 승인 대상이다. POC에서는 active stream 최대 callback gap이
47.232ms였고 OFF/ON 및 sleep/resume recovery가 확인됐다.

## 4. Sleep / Resume

1. suspend 중 timer command가 지연돼도 resume 후 monotonic freshness를 재평가한다.
2. stale이면 Unknown/null로 전환한다.
3. 기존 session report를 최대 30초 기다린다.
4. 자동 복구가 없으면 targeted read-only reopen을 1회 수행한다.
5. reopen 실패 시 backoff 상태로 남고 UI에는 stale percent를 표시하지 않는다.

## 5. Serialization

Provider callback과 timer callback은 직접 snapshot을 수정하지 않는다. 둘 다 command를
mailbox에 넣고 single reader만 revision을 증가시킨다. 따라서 같은 시각의 timeout/recovery도
mailbox 순서대로 하나의 최종 상태를 만든다.

UI는 revision이 현재 값보다 큰 snapshot만 적용한다. WPF Dispatcher queue가 지연돼도
오래된 snapshot이 최신 상태를 덮어쓰지 않는다.

## 6. Shutdown

```text
ShutdownRequested
→ 추가 UI command 차단
→ provider cancellation
→ watcher stop
→ freshness timer stop
→ input handler detach
→ HID dispose / provider RunAsync 종료
→ channel writer complete
→ coordinator drain/stop
→ settings flush
→ tray Visible=false + Dispose
→ WPF Application shutdown
```

Widget X와 Tray Exit는 동일한 `ShutdownRequested` command를 사용한다.
