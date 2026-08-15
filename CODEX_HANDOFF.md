# Device Battery Widget — Codex Handoff

## 1. 프로젝트 개요

프로젝트명: **Device Battery Widget**

Windows PC에 현재 연결된 주변기기의 이름과 배터리 상태를 표시하는 가벼운 데스크톱 위젯을 개발한다. 단순 데모가 아니라 실제 배포 가능한 Windows 응용프로그램을 목표로 하며, 기획 → 요구사항 → 기술검증 → 설계 → 구현 → 테스트 → 배포까지 SI 프로젝트 방식으로 진행한다.

CHG-002 승인에 따른 v1.0 지원 장치:
- Sony DualSense Bluetooth (`VID 0x054C / PID 0x0CE6`) only

후속 릴리스 확장 대상:
- Mouse
- Keyboard
- Headset
- 기타 Game Controller

현재 실장비 POC 대상:
- Logitech G703
- AULA F87Pro
- Sony DualSense
- Corsair VOID WIRELESS V2

특정 제품 전용으로 설계하지 않는다. 장치별 배터리 획득 방식 차이를 고려하여 확장 가능한 Provider/Parser 구조를 목표로 한다.

---

## 2. 승인된 핵심 요구사항

### 배터리 표시
장치명 + 막대 게이지 + 퍼센트로 표시한다.

```text
Device Name
████████░░ 80%
```

### 충전 상태
충전 중:
- 번개 아이콘 표시
- 배터리 게이지 연두색

충전 상태를 알 수 없으면 임의로 NotCharging으로 판단하지 않는다.

### Battery 상태
- Available
- Unsupported
- Unknown

원래 배터리를 읽을 수 있는 장치에서 일시 조회 실패가 발생하면 이전 값을 계속 보여주지 않고 `Unknown`으로 변경한다. 조회가 복구되면 정상 숫자로 돌아간다.

### Charging 상태
- Charging
- NotCharging
- Unknown

Unknown과 NotCharging은 반드시 구분한다.

---

## 3. 위젯 요구사항

- 드래그로 위치 이동
- 마지막 위치 저장
- 모니터 구성 변경으로 화면 밖에 나가면 위치 보정
- Always On Top 옵션 제공
- 최초 Always On Top 기본값 OFF
- Always On Top 상태 저장
- 특정 장치 숨기기
- 숨긴 장치 관리 및 다시 표시
- 연결 해제된 장치는 UI에서 제거
- 동일 모델 여러 개를 각각 독립 장치로 관리
- X 버튼은 전체 앱 종료 및 리소스 정리

---

## 4. System Tray — CHG-001 승인

최소 메뉴:
- 위젯 표시
- Always On Top ON/OFF
- 숨긴 장치 관리
- Windows 로그인 자동 실행 ON/OFF
- 종료

Widget 숨김/최소화 시 앱과 Tray는 유지한다. Tray에서 Widget 복원이 가능해야 한다. X 및 Tray 종료는 watcher/event/timer/tray icon을 포함하여 전체 정리한다.

---

## 5. Windows 로그인 자동 실행

- Should 요구사항
- 기본 OFF
- 사용자 Opt-in
- OFF로 변경 시 자동 실행 등록 제거

---

## 6. 성능 및 안정성 초기 목표

“가벼움”은 설치파일 크기보다 실행 중 프로세스 자원 사용을 의미한다.

- Idle CPU 5분 평균 ≤ 1%
- 순간 작업 CPU ≤ 5% 목표
- 일반 Memory ≤ 100 MB 목표
- 24시간 Memory 증가 ≤ 10 MB 또는 10%
- 24시간 비정상 종료 0회
- Widget 표시 ≤ 2초
- 초기 장치 정보 표시 ≤ 5초

장시간 검증:
- 개발 1h
- 통합 8h
- 시스템 24h
- RC 72h

측정:
CPU, Working Set, Private Memory, Handles, Threads, Exceptions, 재연결 안정성

---

## 7. Polling 정책

원칙: **Event-driven first**

- 이벤트를 제공하는 장치는 Event 기반
- 이벤트가 없는 장치/경로만 Polling
- 기본 Polling 후보 30초
- App start / 새 장치 / Resume 시 즉시 조회
- 근거 없는 100ms/1초 Polling 금지

---

## 8. 지원 OS

- Windows 10 22H2+
- Windows 11

Windows 10 22H2 지원 종료는 Risk로 기록되어 있다.

---

## 9. 현재 기술 후보

POC 기준:
- C#
- .NET 10
- Windows API / WinRT
- WPF 후보
- 필요 시 Win32/HID interop

Production UI 기술은 아직 최종 승인되지 않았다.

---

## 10. Architecture 방향

모든 장치를 하나의 API로 처리할 수 있다고 가정하지 않는다.

현재 유력 구조:

```text
                 DeviceBattery Core
                        │
                  IBatteryProvider
                        │
          ┌─────────────┼─────────────┐
          ▼             ▼             ▼
      BLE GATT         HID       Receiver/Vendor
      Provider       Provider       Provider
          │             │             │
          └─────────────┴─────────────┘
                        │
                  BatteryState
                        │
                       UI
```

장치별 해석은 Parser로 분리한다.

예:
```text
HidBatteryProvider
  └─ DualSenseBatteryParser
```

DualSense 전용 실행 프로그램을 Production 구조로 유지하지 않는다.

---

## 11. POC 프로젝트 정책

`poc/` 아래 프로젝트들은 실제 제품 프로젝트가 아니라 독립 기술 검증용이다.

```text
POC → 기술 검증 증거 → Architecture 결정 → Production 재설계
```

POC 코드는 기술 선택 근거로 보존한다.

---

## 12. Gate 상태

| Gate | 상태 |
|---|---|
| Gate 1 Initiation | APPROVED |
| Gate 2 Execution Plan | APPROVED |
| Gate 3 Requirements | APPROVED |
| CHG-001 System Tray | APPROVED |
| Gate 4 Technical Feasibility POC | APPROVED WITH CONDITIONS |
| Gate 5 Architecture Design | APPROVED WITH CONDITIONS |
| Gate 6 Production Implementation | IN PROGRESS |
| Gate 7 이후 | NOT STARTED |

Gate 6 구현은 승인된 Architecture/RTM 범위 안에서 진행한다.

---

## 13. 주요 Requirement ID

- FR-001 장치 탐색
- FR-002 사용자 친화적 장치명
- FR-003 Battery Available/Unsupported/Unknown
- FR-004 Charging 상태
- FR-005 Event-driven battery
- FR-006 Poll fallback
- FR-007 시작 시 즉시 조회
- FR-008 새 장치 즉시 조회
- FR-009 연결 해제 장치 제거
- FR-010 Resume 후 재조회
- FR-011 장치 숨김/저장
- FR-012 Widget 위치 저장
- FR-013 Always On Top
- FR-014 동일 모델 개별 식별
- FR-015 Windows Login Auto Start
- FR-016 숨긴 장치 관리
- FR-017 System Tray

---

## 14. Device Enumeration / Identity POC 결과

### DualSense USB
한 물리 장치가 Windows에서 USB/HID/Audio/MMDEVAPI 등 여러 Device Interface로 노출됐다.

따라서 `DeviceInterface 1개 = Physical Device 1개`로 보면 안 된다.

DualSense에서는 유효한 ContainerId가 여러 Interface를 그룹핑하는 데 유용했다.

### ContainerId
유용하지만 범용 Identity 단독 키로 사용하지 않는다.

- DualSense: 유효
- Corsair: 유효
- AULA: 유효 사례
- Logitech Receiver: sentinel 형태 발견

예:
```text
00000000-0000-0000-ffff-ffffffffffff
```

따라서 ContainerId를 universal physical identity로 가정하지 않는다.

---

## 15. DeviceWatcher 중요 이슈

다음 경로는 이 POC 환경에서 native AccessViolation 0xC0000005를 재현했다.

```csharp
DeviceInformation.GetAqsFilterFromDeviceClass(DeviceClass.All)
```

Microsoft API 전체 문제라고 일반화하지 않는다.

현재 안정적으로 쓰는 경로:
```csharp
DeviceInformation.CreateWatcher();
```

문제 코드를 근거 없이 다시 도입하지 않는다.

---

## 16. 2.4GHz Receiver 연결 상태 POC

### Logitech G703
- Receiver 자체 연결/해제: DeviceWatcher로 감지
- Receiver 연결 유지 + G703 본체 OFF/ON: Generic DeviceWatcher 이벤트 없음

### Corsair VOID WIRELESS V2
- 동글 제거: InterfaceEnabled=False
- 동글 삽입: InterfaceEnabled=True
- 동글 유지 + Headset 본체 OFF/ON: Generic DeviceWatcher 이벤트 없음

중요 결론:
```text
Receiver Connected ≠ Actual Peripheral Online
```

FR-009를 만족하려면 Receiver 뒤 실제 장치 상태를 판단할 별도 Provider가 필요할 수 있다.

---

## 17. AULA F87Pro Bluetooth 결과

연결 방식: Bluetooth

장치명:
```text
AULA-F87Pro 5.0
```

BLE/HID 상태 변화가 Windows에 노출되며, 표준 BLE Battery Service도 발견됐다.

```text
Battery Service UUID = 0000180f-0000-1000-8000-00805f9b34fb
```

---

## 18. POC-B01 — Windows.Devices.Power

결과:
```text
AggregateBattery
Status=NotPresent

Battery controller DeviceInformation count = 0
```

판정:
```text
POC-B01 = NEED ALTERNATIVE
```

`Windows.Devices.Power.Battery`는 이 환경에서 범용 주변기기 Battery Provider로 부적합하다.

---

## 19. POC-B02 — BLE GATT Battery

AULA F87Pro로 검증.

표준 UUID:
```text
Battery Service = 0x180F
Battery Level   = 0x2A19
```

실측:
```text
DeviceInformation.Name = AULA-F87Pro 5.0
GetCharacteristics Status = Success
Battery Level count = 1
Properties = Read, Notify
INITIAL READ Status = Success
INITIAL READ Value = 100%
INITIAL READ Bytes = 1
Subscribe(Notify) Status = Success
Active subscriptions = 1
```

AULA 전원 OFF/ON 및 Bluetooth 재연결 후 재실행해도 동일하게 성공.

판정:
```text
POC-B02 = PASS
```

실제 Battery 값 변화에 따른 ValueChanged 관찰은 POC-C로 이관.

---

## 20. Xbox Wireless Controller GATT 관찰

Battery Service metadata는 발견됐으나:
```text
GetCharacteristics Status = Unreachable
Battery Level count = 0
```

현재 blocker 아님. 필요 시 재검증.

---

## 21. DualSense Bluetooth — BLE 결과

DualSense는 Bluetooth 연결 상태였지만 BLE Battery Service `0x180F` 검색 결과에 나타나지 않았다.

현재 POC 환경 기준:
```text
DualSense Bluetooth → standard BLE 0x180F battery path 아님
```

---

## 22. POC-B03-1 — Windows.Gaming.Input

Bluetooth DualSense:

```text
RawGameController count = 1
Gamepad count = 1
DisplayName = HID 규격 게임 컨트롤러
VID = 0x054C
PID = 0x0CE6
IsWireless = True
```

그러나:
```text
RawGameController.TryGetBatteryReport() = null
Gamepad.TryGetBatteryReport() = null
```

판정:
```text
POC-B03-1 = NEED ALTERNATIVE
```

---

## 23. POC-B03-2 — DualSense HID Battery

Bluetooth DualSense:
```text
Device Name = DualSense Wireless Controller
VID = 0x054C
PID = 0x0CE6
OPEN = Success
ReportId = 0x31
BufferLength = 78
StatusOffset = 54
```

충전 케이블 연결 전:
```text
StatusByte       = 0x00
BatteryBucketRaw = 0
ChargingCodeRaw  = 0x0
EstimatedPercent = 5%
ChargingState    = Not Charging / Discharging
```

실제 USB 충전 케이블 연결 후:
```text
StatusByte       = 0x10
BatteryBucketRaw = 0
ChargingCodeRaw  = 0x1
EstimatedPercent = 5%
ChargingState    = Charging
```

사용자 실제 행동과 상태 변화가 일치했다.

판정:
```text
POC-B03-2 = PASS WITH LIMITATION
```

제한: 배터리 값이 exact 1% 단위가 아니라 coarse 10% bucket.

대략:
```text
0  → 0~9%
1  → 10~19%
...
9  → 90~99%
10 → 100%
```

POC는 중간 대표값 5/15/.../95/100을 사용한다.

향후 BatteryState에서 정밀도 메타데이터를 고려:
```text
Percent = 5
IsEstimated = true
Precision = TenPercentBucket
```

UI 표시 방식(`5%`, `약 5%`, `0~9%`)은 UI/UX 단계에서 결정한다.

---

## 24. Production 코드 구조 후보

```text
src/
├─ DeviceBattery.Core/
│  ├─ Models/
│  │  ├─ DeviceInfo.cs
│  │  ├─ BatteryState.cs
│  │  ├─ BatteryPrecision.cs
│  │  └─ ChargingState.cs
│  └─ Abstractions/
│     ├─ IBatteryProvider.cs
│     └─ IHidBatteryParser.cs
├─ DeviceBattery.Windows/
│  ├─ Battery/
│  │  ├─ BleGatt/
│  │  │  └─ BleGattBatteryProvider.cs
│  │  ├─ Hid/
│  │  │  ├─ HidBatteryProvider.cs
│  │  │  └─ Parsers/
│  │  │     └─ DualSenseBatteryParser.cs
│  │  └─ Receiver/
│  └─ Devices/
│     └─ WindowsDeviceWatcher.cs
└─ DeviceBattery.App/
```

아직 Architecture Gate에서 최종 승인된 구조는 아니지만 POC 결과가 강하게 지지한다.

---

## 25. Battery POC 진행 상태

```text
POC-B01 Windows Generic Battery
→ NEED ALTERNATIVE

POC-B02 BLE GATT
→ PASS
→ AULA F87Pro

POC-B03-1 Windows.Gaming.Input
→ NEED ALTERNATIVE
→ DualSense

POC-B03-2 Raw HID
→ PASS WITH LIMITATION
→ DualSense

POC-B04-1 Receiver HID Discovery
→ PASS WITH LIMITATION

POC-B04-2 Passive Battery Correlation
→ DEFERRED TO VNEXT / CHG-002

POC-B05 Normalized BatteryState
→ PASS WITH LIMITATION (8/8 deterministic cases; lifecycle/timeout pending)

POC-B05-1 DualSense Lifecycle / Timeout
→ PASS WITH LIMITATION (USB charging PASS; Bluetooth timeout/recovery 3회 PASS; cleanup PASS; callback 직렬화 필요)

POC-B06 Event-first / Poll Fallback Policy
→ PASS WITH LIMITATION (8/8 deterministic; DualSense device polling 없음; Production timeout/직렬화 pending)

POC-C06 DualSense Sleep / Resume
→ PASS (기존 HID session 자동 복구 약 20초; R/restart 불필요; cleanup PASS)

POC-C03 DualSense Event Continuity
→ PASS (288.675초, 136,859 transitions, modulo-16 sequence 100%, missing/duplicate 0)

POC-D01~D05 System Tray / Lifecycle
→ PASS (tray/menu/hide/restore/X exit/tray exit/cleanup; ghost icon 없음)

POC-E01~E03/E08 Resource / Performance
→ PASS WITH LIMITATION (Tray CPU 0.001%/WS 42.16 MiB; BT Battery CPU 0.144%/WS 46.43 MiB; 5분 baseline만 완료)

POC-E04/E06 Startup / Runtime Deployment
→ PASS WITH LIMITATION (FDD 68.8 ms, SCD 67.5 ms; 각 10/10; Win10만 검증, packaging pending)
```

---

## 26. 보존된 POC-B04 결과 — vNext 이관

대상:

### Logitech G703 Receiver
```text
VID = 0x046D
PID = 0xC539
```

### Corsair VOID WIRELESS V2 Receiver
```text
VID = 0x1B1C
PID = 0x2A08
```

현재 구현 POC:
```text
poc/DeviceBattery.Poc.ReceiverHidProbe/
```

목표: Receiver의 HID Top-Level Collection 구조 조사.

실장비 B04-1 결과:

```text
Logitech: 7 TLC / 4 vendor-defined / WinRT read open 6
  FF00/0001 Report 0x10 offset 4: OFF 0x62 <-> ON 0xA2 (2회)

Corsair: 5 TLC / 3 vendor-defined / WinRT read open 5
  FF42/0002 Report 0x03:
  OFF 030001360000 -> transition 030001360002 -> ON 030101D00001 (2회)
```

B04-1은 `PASS WITH LIMITATION`으로 동결됐다. CHG-002 승인으로 v1.0은 DualSense Bluetooth만 지원하며 B04-2는 vNext로 이관됐다. Output/Feature/vendor command는 전송하지 않았다.

첫 battery correlation 기준값:

```text
G HUB G703 = 91%
iCUE Corsair VOID WIRELESS V2 = 33%
```

기존 passive report에서 raw `0x5B`/`0x21` 단일 byte 대응은 발견되지 않았다.

확인:
- VID/PID
- Product String
- Manufacturer
- Serial
- UsagePage
- Usage
- InputReportLength
- OutputReportLength
- FeatureReportLength
- Open 가능 여부
- 최초 Input Report

특히:
```text
UsagePage >= 0xFF00
```
인 Vendor-defined HID Collection을 주요 후보로 본다.

---

## 27. POC-B04 안전 규칙

현재 단계는 **READ ONLY**.

아직 금지:
- Vendor command 전송
- Output Report 전송
- 임의 Feature command 전송
- 장치 상태 변경 명령 전송
- 알려지지 않은 byte sequence 전송

먼저 HID 구조를 파악하고 실측 결과를 바탕으로 다음 POC를 설계한다.

---

## 28. Codex가 지금 바로 해야 할 작업

1. repository 루트의 이 `CODEX_HANDOFF.md`를 끝까지 읽는다.
2. `README.md`, `docs/`, `poc/`를 읽어 handoff와 실제 저장소 상태를 비교한다.
3. 현재 Gate 4 / POC-B04 위치를 확인한다.
4. `poc/DeviceBattery.Poc.ReceiverHidProbe`를 분석한다.
5. G703/Corsair Receiver의 read-only HID discovery가 충분히 가능한지 확인한다.
6. 수정 필요 시 무엇을 왜 수정할지 먼저 설명한다.
7. 사용자 실장비에서 실행할 테스트 절차와 판정 기준을 제시한다.
8. 실장비 결과를 받기 전 다음 POC/Production 구현으로 넘어가지 않는다.

---

## 29. 개발 원칙

1. 실측 POC 결과보다 추측을 우선하지 않는다.
2. 특정 장치 성공 방식을 모든 장치에 일반화하지 않는다.
3. Generic Windows API → 표준 Protocol → HID → Vendor-specific 순으로 검증한다.
4. Event-driven 우선.
5. Polling은 필요한 Provider에만 적용.
6. Timer/Event/Watcher 생성 시 반드시 정리 경로를 설계.
7. Memory/Handle/Thread leak을 허용하지 않는다.
8. DeviceInterface와 Physical Device를 혼동하지 않는다.
9. ContainerId 하나에 의존하지 않는다.
10. Gate 승인 없이 다음 프로젝트 단계로 넘어가지 않는다.
11. POC와 Production 코드를 구분한다.
12. 미확인 Protocol을 성공했다고 가정하지 않는다.
13. 상태는 PASS / PASS WITH LIMITATION / FAIL / NEED ALTERNATIVE / NOT TESTED로 명확히 기록한다.
14. 사용자 승인 없이 Requirements baseline을 변경하지 않는다.

---

## 30. Git convention

Commit prefix:
```text
docs:
poc:
feat:
fix:
test:
chore:
```

예:
```text
poc: add receiver HID discovery probe
fix: improve DeviceWatcher logging
docs: update Gate 4 POC findings
```

Branch 후보:
- main
- develop
- poc/*
- feature/*

기존 POC 증거를 삭제하거나 Production 코드로 덮어쓰지 않는다.

---

## 31. RTM / 추적성

Requirement → Design → Implementation → Test Case → Result 관계를 유지한다.

현재 Gate 4와 관련성이 높은 Requirement:
- FR-001
- FR-002
- FR-003
- FR-004
- FR-005
- FR-006
- FR-008
- FR-009
- FR-014
- NFR-MAINT-001
- NFR-MAINT-002

---

## 32. 주요 Risk

- Windows의 배터리 노출 방식이 장치별로 다름
- Vendor-specific protocol 필요 가능성
- Bluetooth/USB/2.4GHz 차이
- Charging 상태 미제공 장치
- 잦은 Polling으로 CPU 증가
- Timer/Event listener 해제 누락
- Native interop 리소스 누수
- Connect/Disconnect 중 예외
- Event 미지원/유실
- Windows 10 지원 종료

---

# Codex 과거 첫 프롬프트 — CHG-002로 대체됨

> 아래 프롬프트는 Receiver POC 시작 당시의 기록이며 현재 작업 지시가 아니다. 최신 범위는 DualSense Bluetooth-only v1.0이다.

```text
이 저장소는 Windows용 Device Battery Widget 프로젝트다.

먼저 repository 루트의 CODEX_HANDOFF.md를 처음부터 끝까지 읽어라.

그 다음 README.md, docs/, poc/를 확인해서
CODEX_HANDOFF.md에 기록된 프로젝트 상태와 실제 저장소 상태를 비교하라.

이 프로젝트는 SI 방식으로 단계별 Gate 승인을 받으며 진행하고 있으며
현재 Gate 4 Technical Feasibility POC 단계다.

현재 진행 위치는 POC-B04:
2.4GHz Receiver / Vendor Battery 검증이다.

직전까지의 POC 결과를 변경하거나 다시 구현하지 마라.

현재 우선 작업은:
poc/DeviceBattery.Poc.ReceiverHidProbe

목표는 다음 두 Receiver의 HID Top-Level Collection을
read-only 방식으로 조사하는 것이다.

- Logitech G703 Receiver
  VID 0x046D
  PID 0xC539

- Corsair VOID WIRELESS V2 Receiver
  VID 0x1B1C
  PID 0x2A08

먼저 현재 repository와 코드를 분석하고 다음을 나에게 보고해라.

1. 현재 프로젝트/Gate 상태를 어떻게 이해했는지
2. ReceiverHidProbe의 현재 구현 상태
3. 발견한 문제점 또는 개선할 부분
4. 수정해야 할 코드
5. 실장비에서 확인해야 할 테스트 항목
6. 예상되는 결과를 PASS / PASS WITH LIMITATION / NEED ALTERNATIVE 중 어떤 기준으로 판정할지

내 확인 없이 다음 POC 또는 Production 구현으로 넘어가지 마라.

아직 vendor-specific output command나 장치 상태를 변경하는 명령은 전송하지 마라.
현재 단계는 read-only HID discovery다.

기존 DeviceWatcher POC에서
DeviceInformation.GetAqsFilterFromDeviceClass(DeviceClass.All)
경로가 이 환경에서 native AccessViolation을 발생시킨 이력이 있으므로
해당 코드를 근거 없이 다시 도입하지 마라.

모든 판단은 repository 문서와 실제 실장비 POC 결과를 우선한다.
```

---

## 33. 현재 최우선 목표 — Gate 6 Production Foundation

```text
승인된 Domain/Provider contract와 state lifecycle을 Production project로 구현하는 것.
먼저 Domain/Application foundation과 deterministic specs를 만들고, Windows HID와 WPF는
해당 contract 위에 단계적으로 추가한다.
Mouse/Keyboard/Headset/Receiver 지원은 vNext로 유지한다.
```

Gate 5 Architecture는 `APPROVED WITH CONDITIONS`다. ADR-001~010은 `Accepted`다.

Gate 6 구현은 승인된 Architecture와 ADR 범위 안에서 단계별로 진행하며,
각 구현 increment는 검증 후 기록한다. Installer와 Gate 7 검증으로는 별도 승인 없이 넘어가지 않는다.
