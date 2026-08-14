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

## Leak 판단
시작 직후 초기화/JIT 또는 GC 전후의 일시적인 변동은 곧바로 Leak으로 판단하지 않는다.
반면 작업 반복 횟수나 시간 경과에 비례해 Memory, Handle, Thread가 계속 증가하면 추가 분석한다.
