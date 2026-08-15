# Gate 6 Executable App Shell Smoke 01

- 일시: 2026-08-15
- 대상: `DeviceBattery.App` Release build
- 실행: `--smoke-seconds 5`
- 구성: DualSense Provider → Coordinator → WPF Dispatcher → WidgetViewModel, NotifyIcon tray

## 결과

- 최초 실행에서 WPF `ProgressBar.Value` 기본 TwoWay binding과 읽기 전용 ViewModel 속성의
  충돌을 발견했다.
- binding을 명시적 `Mode=OneWay`로 수정했다.
- 재실행은 WPF 창과 tray를 생성하고 5초 후 exit code 0으로 종료됐다.
- shutdown은 Provider cancel → coordinator complete/drain → Provider dispose → tray dispose →
  WPF shutdown 순서로 수렴한다.

## 판정

`PASS WITH LIMITATION`

- 실행/자동 종료 smoke는 PASS
- Tray 메뉴 Show/Topmost/Exit 및 minimize/hide 수동 동작 검증은 다음 실사용 확인 대상
- 최종 visual style은 아직 승인 전 baseline
