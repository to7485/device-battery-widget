# Resource Measurement Plan

## 목적
항상 실행되는 경량 Utility 특성을 고려하여 최종 기능별 자원 증가량을 추적한다.

## 측정값
- CPU: 5분 평균 사용률
- Working Set: 프로세스의 물리 메모리 점유 관찰
- Private Memory: 프로세스 전용 메모리 관찰
- Handle Count: Native/Device/Event 자원 누수 보조 지표
- Thread Count: 불필요한 Thread 생성 또는 종료 실패 확인

## Stage
1. PERF-BASE
2. PERF-DEVICE
3. PERF-BATTERY
4. PERF-EVENT
5. PERF-TRAY
6. PERF-IDLE

가능하면 동일 PC, 동일 OS, 유사한 Background 상태에서 측정한다.

## 측정 도구

`poc/DeviceBattery.Poc.ResourceSampler`를 사용해 대상 PID를 read-only로 관찰한다.
기본값은 5분 / 1초 간격이며 CSV와 요약을 함께 남긴다. CPU는 logical processor
수로 정규화한다.

1차 측정 대상:

1. `PERF-TRAY` — SystemTrayLifecycle Release executable
2. `PERF-BATTERY` — DualSenseLifecycleProbe Release executable

`PERF-BASE`, 기능별 세분화 및 장시간 leak/soak는 후속 측정으로 남긴다.

2026-08-15 `PERF-TRAY` 5분 측정은 평균 CPU 0.001%, Working Set 42.16 MiB,
Private Memory 10.57 MiB로 PASS했다. Handle/Thread 지속 증가 징후는 없었다.

2026-08-15 `PERF-BATTERY-BLUETOOTH` 5분 측정은 평균 CPU 0.144%, 최대 0.904%,
Working Set 46.43 MiB, Private Memory 12.74 MiB로 PASS했다. Handle/Thread는 각각
17/10 감소하여 지속 증가 징후가 없었다.

## Leak 판단
시작 직후 초기화/JIT 또는 GC 전후의 일시적인 변동은 곧바로 Leak으로 판단하지 않는다.
반면 작업 반복 횟수나 시간 경과에 비례해 Memory, Handle, Thread가 계속 증가하면 추가 분석한다.
