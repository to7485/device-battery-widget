# DeviceBattery.Poc.ResourceSampler

Gate 4 POC-E01~E03/E08를 위한 read-only process resource sampler다.

대상 PID의 다음 값을 기본 5분 동안 1초 간격으로 CSV에 기록한다.

- logical CPU count로 정규화한 process CPU percentage
- Working Set
- Private Memory
- Handle Count
- Thread Count

대상 프로세스를 실행하거나 종료하지 않으며 state를 변경하지 않는다.

```powershell
dotnet run -c Release --project .\DeviceBattery.Poc.ResourceSampler.csproj -- `
  --pid 1234 --stage PERF-TRAY --duration 300 --interval 1
```

CSV 기본 위치는 현재 경로의 `artifacts/resource`다.
