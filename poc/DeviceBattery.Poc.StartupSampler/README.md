# DeviceBattery.Poc.StartupSampler

Gate 4 POC-E04 GUI startup sampler다. 실행 시작부터 대상 PID가 소유한 첫 visible
top-level window까지 측정한다. 각 iteration에서 sampler가 시작한 POC window에
`WM_CLOSE`를 보내 정상 cleanup 경로로 종료한다.

```powershell
dotnet run -c Release --project .\DeviceBattery.Poc.StartupSampler.csproj -- `
  --exe <SystemTrayLifecycle.exe> --stage TRAY-FDD --iterations 10
```

현재 Production tray 앱은 `--arguments "--smoke-seconds 3 --providers all"`을 전달해
각 iteration을 공통 `ShutdownAsync` 경로로 종료할 수 있다. 앱의 제한형 local log에 기록되는
`WIDGET_VISIBLE`과 `FIRST_DEVICE_AVAILABLE` marker는 PID와 process-relative elapsed time만
포함하며 장치 ID를 포함하지 않는다.
다중 Provider 정리가 필요한 Production 측정은 `--shutdown-timeout-ms 15000` 이상을 사용한다.
