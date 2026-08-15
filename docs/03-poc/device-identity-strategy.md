# POC-A05/A06 Device Identity 전략 초안

## 1. 문제

`DeviceInformation.Id` 하나를 Application의 영구적인 장치 ID로 즉시 사용하면 안 된다.

Windows 장치 열거에서는 하나의 물리 장치가 여러 Device Interface 또는 devnode로 표현될 수 있다.

## 2. 1차 Identity 후보

### Level 1 — Device Interface

- DeviceInformation.Id
- 특정 기능 Interface를 활성화하기에는 유용
- 물리 장치 하나당 여러 값이 존재할 수 있음

### Level 2 — Device Instance

- System.Devices.DeviceInstanceId
- Windows PnP Device Instance 식별 후보
- 연결 방식 및 재설치/재페어링 상황을 실제 검증해야 함

### Level 3 — Device Container

- System.Devices.ContainerId
- 여러 devnode를 하나의 물리 Device Container로 그룹화하는 용도
- 현재 **FR-014 동일 모델 장치 개별 식별**의 가장 중요한 1차 후보

## 3. 현재 가설

```text
Display Name
→ 사용자 표시용

Device Interface ID
→ 기능 접근용

ContainerId
→ 물리 장치 그룹 후보

Application Stable ID
→ POC 결과 후 결정
```

## 4. Stable ID 결정 조건

최종 ID는 최소 다음 조건을 만족해야 한다.

1. 동일 모델 두 장치를 서로 구분
2. Application 재실행 후 동일 장치를 다시 찾을 수 있음
3. 단순 연결 해제/재연결 후 가급적 동일
4. 숨김 설정을 다른 동일 모델 장치에 잘못 적용하지 않음
5. 사용자가 장치를 교체했을 때 기존 장치와 혼동하지 않음

## 5. 아직 확정하지 않는 항목

- ContainerId 단독 사용
- DeviceInstanceId 단독 사용
- Friendly Name 기반 ID
- 제조사/모델명 기반 ID
- Hash 조합 방식

실장비 결과 이후 결정한다.
