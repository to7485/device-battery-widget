# POC-A02/A03 재시험 업데이트

기존 파일:

```text
poc/DeviceBattery.Poc.DeviceEnumeration/Program.cs
```

을 이 패키지의 `Program.cs`로 교체합니다.

## 주요 변경

- `System.Devices.Connected` 요청 및 출력
- `System.Devices.ContainerId` 요청 및 출력
- `System.Devices.DeviceInstanceId` 요청 및 출력
- `UPDATED` 이벤트에서 실제 변경된 Property 이름/값 출력
- `DeviceInformation.Update()` 적용 후 현재 상태 출력
- `REMOVED` 시 마지막 Connected/Container/Instance 정보 출력

## DualSense 재시험

Bluetooth:
1. 연결된 상태에서 프로그램 실행
2. DualSense 전원 OFF
3. 로그 확인
4. 전원 ON
5. 로그 확인

USB:
1. USB 연결
2. 케이블 제거
3. 로그 확인
4. 다시 연결
5. 로그 확인

확인할 핵심 로그:

```text
CHANGED System.Devices.Connected = False
CHANGED System.Devices.Connected = True
```

또는 실제 `[REMOVED]`, `[ADDED]` 발생 여부입니다.
