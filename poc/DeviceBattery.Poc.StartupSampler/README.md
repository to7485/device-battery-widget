# DeviceBattery.Poc.StartupSampler

Gate 4 POC-E04 GUI startup sampler다. 실행 시작부터 대상 PID가 소유한 첫 visible
top-level window까지 측정한다. 각 iteration에서 sampler가 시작한 POC window에
`WM_CLOSE`를 보내 정상 cleanup 경로로 종료한다.

```powershell
dotnet run -c Release --project .\DeviceBattery.Poc.StartupSampler.csproj -- `
  --exe <SystemTrayLifecycle.exe> --stage TRAY-FDD --iterations 10
```
