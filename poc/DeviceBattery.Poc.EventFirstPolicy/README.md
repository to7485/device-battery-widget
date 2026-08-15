# DeviceBattery.Poc.EventFirstPolicy

Gate 4 POC-B06의 deterministic 정책 검증이다.

- battery event가 있으면 event-first
- event가 없고 신뢰 가능한 read endpoint가 있을 때만 poll fallback
- DualSense HID는 event-only이며 timer가 device read를 호출하지 않음
- freshness timeout은 stale percentage를 제거할 뿐 HID command를 전송하지 않음
- 정상 event가 다시 들어오면 Available로 복구

이 프로젝트는 장치를 열지 않으며 실장비 명령을 전혀 전송하지 않는다.

```powershell
dotnet run --project .\DeviceBattery.Poc.EventFirstPolicy.csproj
```
