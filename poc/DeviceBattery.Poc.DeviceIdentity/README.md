# POC-A05/A06 — Device Identity

## 목적

동일 모델 장치를 서로 구분하고 재연결 후에도 같은 물리 장치를 추적할 수 있는지 검증한다.

Microsoft의 Container ID는 하나의 물리 장치에 속한 여러 PnP devnode를 묶기 위한 값이므로,
현재 POC에서는 `System.Devices.ContainerId`를 **물리 장치 그룹 후보**로 본다.

단, 최종 Application의 Stable Device ID로 아직 확정하지 않는다.

## 실행

```powershell
dotnet run -- --label before
```

장치 재연결 후:

```powershell
dotnet run -- --label after
```

생성 파일:

```text
artifacts/
├─ identity-before.csv
└─ identity-after.csv
```

## 비교 항목

- ContainerId
- DeviceInstanceId
- DeviceInformation.Id (InterfaceId)
- Friendly Name
- Manufacturer / Model
- DiscoveryMethod

## 핵심 판정

1. 같은 물리 장치의 여러 Interface가 같은 ContainerId로 묶이는가?
2. 동일 모델 장치 2개가 서로 다른 ContainerId를 갖는가?
3. 단순 재연결 후 ContainerId가 유지되는가?
4. USB ↔ Bluetooth 변경 시 Identity가 어떻게 달라지는가?
5. Unpair/Pair 후 Identity가 유지되는가?

3~5는 실제 장치가 있을 때 검증한다.
