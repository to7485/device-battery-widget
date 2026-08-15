# Gate 5 — Architecture Design 계획

작성일: 2026-08-15
상태: IN PROGRESS
입력: Requirements Baseline v1.2 / Gate 4 APPROVED WITH CONDITIONS

## 목표

POC 코드를 복사해 제품화하지 않고, 검증된 기술 사실을 Production 경계와 정책으로
재설계한다. Gate 5 승인 전 `src/` Production 구현을 시작하지 않는다.

## 산출물

1. Architecture Overview 및 구성요소 책임
2. Provider/Parser/State contract
3. 상태 전이 및 concurrency serialization 정책
4. Freshness/Offline 정책 결정
5. WPF/Tray application lifecycle
6. Settings/Identity/Deployment 정책
7. RTM Design ID 매핑
8. Architecture Decision Record

## Gate 4 이관 조건

- v1.0 DualSense Bluetooth-only
- 10% bucket estimated precision UI
- timer/input callback 직렬화
- polling 없는 event-first DualSense provider
- Release 전 Windows 11, 24시간/72시간 soak, packaging 검증

## 승인 기준

- Must requirement에 설계 책임과 Design ID가 매핑됨
- device callback에서 UI까지 단일 상태 순서가 정의됨
- dispose/exception/recovery 경로가 정의됨
- open decision과 Release 검증 항목이 명시됨
