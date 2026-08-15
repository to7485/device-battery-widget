# POC-E01~E03/E08 — Resource / Performance

작성일: 2026-08-15
상태: 5-MINUTE BASELINES COMPLETE — PASS WITH LIMITATION
기준: Requirements Baseline v1.2

## 1차 목표

- 동일 PC에서 Release process를 5분간 1초 간격 측정
- CPU 평균/최대, Working Set, Private Memory, Handle, Thread 확보
- 시작/종료 일시 변동과 지속 증가를 구분
- Tray와 DualSense battery monitoring의 실제 baseline 비교

## 1차 판정

- PASS: 5분 평균 CPU 1% 이하, memory 100 MiB 이하, handle/thread 지속 증가 없음
- PASS WITH LIMITATION: 목표 근접 또는 짧은 측정만 완료되어 soak 필요
- NEED ALTERNATIVE: idle CPU/Memory 목표 초과 또는 resource 지속 증가

5분 결과만으로 24시간 leak이나 72시간 soak를 PASS 처리하지 않는다.

## 1차 실측 결과 — PERF-TRAY

- Samples: 299 / 약 5분
- CPU Avg 0.001%, Max 0.131%
- Working Set Last 42.16 MiB, Delta +0.24 MiB
- Private Memory Last 10.57 MiB, Delta -0.15 MiB
- Handles Last 296, Delta -8
- Threads Last 8, Delta -5

판정: **PASS** — 5분 idle 목표를 만족하고 resource 지속 증가 징후가 없다.

## 1차 실측 결과 — PERF-BATTERY-BLUETOOTH

- Samples: 300 / 5분
- CPU Avg 0.144%, Max 0.904%
- Working Set Last 46.43 MiB, Delta +0.68 MiB
- Private Memory Last 12.74 MiB, Delta -0.75 MiB
- Handles Last 293, Delta -17
- Threads Last 10, Delta -10

판정: **PASS** — Bluetooth-only DualSense monitor가 5분 idle CPU/Memory 목표를
만족하고 Handle/Thread 지속 증가 징후가 없다.

## 비교와 최종 판정

Battery POC는 Tray POC보다 CPU 평균 +0.143%p, Working Set 약 +4.27 MiB,
Private Memory 약 +2.17 MiB였다. 서로 다른 POC executable 비교이므로 E08의 정밀한
동일-host incremental 결과가 아니라 참고치다.

1차 E01~E03 결과는 **PASS**다. 전체 POC-E는 **PASS WITH LIMITATION**으로 판정한다.
5분 측정은 24시간 leak/72시간 soak, startup, OS 교차검증, 배포 특성을 대체하지 않는다.
