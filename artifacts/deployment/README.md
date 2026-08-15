# Gate 4 Deployment Evidence

The generated publish binaries are intentionally not committed. Recreate them with:

```powershell
dotnet publish .\poc\DeviceBattery.Poc.SystemTrayLifecycle\DeviceBattery.Poc.SystemTrayLifecycle.csproj -c Release -r win-x64 --self-contained false -o .\artifacts\deployment\tray-fdd
dotnet publish .\poc\DeviceBattery.Poc.SystemTrayLifecycle\DeviceBattery.Poc.SystemTrayLifecycle.csproj -c Release -r win-x64 --self-contained true -o .\artifacts\deployment\tray-self-contained
```

2026-08-15 results:

- Framework-dependent: 5 files / 189,638 bytes / 0.18 MiB
- Self-contained: 271 files / 122,766,085 bytes / 117.08 MiB
- Both win-x64 executables passed 10/10 visible-window startup tests.
