# Domain / Provider Contracts 초안

상태: Gate 5 REVIEW DRAFT
관련 설계: ARC-001~005, ARC-009

## 1. Domain Model

```csharp
public sealed record DeviceKey(string ProviderId, string StableId);

public enum BatteryAvailability { Available, Unsupported, Unknown }
public enum ChargingState { Charging, NotCharging, Unknown }
public enum BatteryPrecision { ExactPercent, TenPercentBucket, Unknown }

public sealed record BatteryState(
    BatteryAvailability Availability,
    int? Percent,
    ChargingState Charging,
    BatteryPrecision Precision,
    bool IsEstimated,
    DateTimeOffset ObservedAt,
    string SourceProvider,
    string? Reason);

public sealed record DeviceSnapshot(
    DeviceKey Key,
    string DisplayName,
    BatteryState Battery,
    bool IsVisible,
    long Revision);
```

Invariant:

- Available만 Percent를 가질 수 있다.
- Percent 범위는 0~100이다.
- Unknown/Unsupported는 Percent=null이다.
- TenPercentBucket의 100% 미만 값은 IsEstimated=true다.
- UI는 raw HID byte를 알지 못한다.

## 2. Parser Contract

Parser는 Windows handle, timer, UI를 참조하지 않는 순수 함수다.

```csharp
public interface IHidBatteryParser
{
    bool TryParse(
        ushort reportId,
        ReadOnlySpan<byte> report,
        DateTimeOffset observedAt,
        out BatteryObservation observation);
}
```

`BatteryObservation`은 raw status와 normalized state를 포함할 수 있지만 raw data 전체를
장기 보관하지 않는다. invalid/unsupported shape는 `false`이며 이전 percent를 덮어쓰지 않는다.

## 3. Provider Contract

```csharp
public interface IBatteryProvider : IAsyncDisposable
{
    string ProviderId { get; }
    Task RunAsync(
        ChannelWriter<ProviderEvent> events,
        CancellationToken cancellationToken);
}
```

Provider는 하나의 `RunAsync` lifecycle을 소유한다. App은 같은 instance에 Start/Stop을
중복 호출하지 않는다. 종료는 cancellation 후 `DisposeAsync` 한 경로로 수렴한다.

## 4. Provider Event

```text
DeviceDiscovered
BatteryChanged
ReportRecovered
FreshnessExpired
DeviceOffline
DeviceRemoved
ProviderFaulted
```

모든 event 공통 필드:

- DeviceKey
- SessionGeneration
- ProviderSequence
- OccurredAt

`SessionGeneration`보다 오래된 callback은 coordinator가 무시한다. `ProviderSequence`는
디버깅/순서 검증용 단조 증가값이며 HID packet counter와 별개다.

## 5. DualSense Provider Boundary

- selector: UsagePage 0x0001 / Usage 0x0005 / VID 0x054C / PID 0x0CE6
- Bluetooth interface만 허용하고 USB interface는 v1 provider에서 제외
- `HidDevice.FromIdAsync(..., FileAccessMode.Read)`만 사용
- Output/Feature/vendor command 금지
- raw report마다 UI event를 발행하지 않음
- valid report마다 monotonic `LastValidReportTimestamp`만 갱신
- status byte 변경 또는 Unknown 복구일 때만 semantic event 발행
- invalid report는 metric만 증가시키고 freshness timestamp를 갱신하지 않음

## 6. Event Mailbox

`Channel<ProviderEvent>`는 single-reader coordinator가 소비한다. raw report가 coalescing된
뒤 semantic event만 들어오므로 unbounded channel을 사용하되 다음 제한을 둔다.

- ProviderFaulted는 동일 error fingerprint별 rate limit
- 동일 BatteryState 연속 발행 금지
- provider callback에서 channel 소비를 기다리지 않음
- coordinator 종료 후 event write 실패는 정상 shutdown으로 처리

## 7. Time Contract

- 사용자 표시/로그: `TimeProvider.GetUtcNow()`
- timeout duration: `TimeProvider.GetTimestamp()`와 `GetElapsedTime()`
- test는 `FakeTimeProvider` 사용
- wall-clock 변경으로 freshness가 만료되거나 연장되지 않음

## 8. Exception Contract

- Parser exception: 해당 report 격리, metric/log, session 유지
- HID open 실패: Unknown/Fault event 후 bounded retry 후보
- callback exception: provider 내부 격리, process crash 금지
- coordinator exception: 해당 command 격리, revision 순서 유지
- fatal startup/config corruption: 사용자에게 오류 상태를 표시하고 안전 종료
