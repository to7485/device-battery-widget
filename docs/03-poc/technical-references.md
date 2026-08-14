# POC 기술 참고자료

POC 구현 근거는 Microsoft 공식 문서를 우선한다.

## 핵심 API
- Windows.Devices.Enumeration.DeviceInformation
- Windows.Devices.Enumeration.DeviceWatcher
- Windows.Devices.Power.Battery
- Windows.Devices.Power.BatteryReport
- Battery.ReportUpdated
- .NET Windows OS-specific TFM
- Desktop App에서 Windows Runtime API 호출
- System.Windows.Forms.NotifyIcon

## 핵심 주의사항
- 기본 DeviceInformation 열거의 Id는 Device Interface Identifier일 수 있으므로 물리 장치 Identity와 동일하다고 가정하지 않는다.
- DeviceInformation Name은 표시용으로 보고 고유 식별자로 사용하지 않는다.
- Windows.Devices.Power.Battery는 Battery Controller 개념이므로 모든 주변장치 배터리가 같은 방식으로 노출된다고 가정하지 않는다.
- Tray Resource는 종료 시 명시적으로 정리한다.
