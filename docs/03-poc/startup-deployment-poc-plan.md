# POC-E04/E06 — Startup / Runtime Deployment

작성일: 2026-08-15
상태: COMPLETE — PASS WITH LIMITATION

## E04 Startup

SystemTrayLifecycle Release executable을 10회 시작하고 대상 PID의 첫 visible top-level
window 확보 시점을 ready로 측정한다. 각 실행은 Widget X와 동일한 정상 close 경로로
종료해 tray cleanup을 수행한다.

## E06 Deployment

Windows x64 기준으로 다음 publish 특성을 비교한다.

- Framework-dependent: 대상 PC에 .NET Desktop Runtime 필요
- Self-contained: Runtime 포함, 산출물 증가, 대상 Runtime 사전 설치 불필요

설치 크기 자체는 Gate 성능 판정 기준이 아니며 운영/배포 선택 자료로만 사용한다.

## 판정

- PASS: 10/10 startup ready, 정상 cleanup, 두 publish mode 생성 가능
- PASS WITH LIMITATION: 한 mode만 가능하거나 단일 OS에서만 검증
- NEED ALTERNATIVE: startup 불안정 또는 배포 산출물 실행 불가

## 실행 환경

- Windows 10 Home 22H2, build 19045.6466
- win-x64
- .NET SDK 10.0.400 / Host 10.0.11

## E04 결과

| Mode | Ready | Average | Min | Max/P95 |
|---|---:|---:|---:|---:|
| Framework-dependent | 10/10 | 68.8 ms | 63.5 ms | 82.2 ms |
| Self-contained | 10/10 | 67.5 ms | 64.2 ms | 81.2 ms |

두 mode 모두 정상 `WM_CLOSE` cleanup 경로로 종료됐다. E04 판정: **PASS**.

## E06 결과

| Mode | Files | Size | Runtime requirement |
|---|---:|---:|---|
| Framework-dependent | 5 | 0.18 MiB | .NET 10 Desktop Runtime 필요 |
| Self-contained | 271 | 117.08 MiB | Runtime 포함 |

두 win-x64 publish 및 실행이 성공했다. E06 판정은 **PASS WITH LIMITATION**이다.
Windows 10 한 환경만 검증됐고 installer/portable packaging 및 Windows 11은 미검증이다.
