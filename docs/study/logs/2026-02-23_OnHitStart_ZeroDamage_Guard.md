# 2026-02-23 Study Log: OnHitStart와 TakeDamage(0) 연관 이해

## 0) 왜 이 문서를 썼는가

증상: 플레이어가 공격하지 않았는데 보스가 피격 반응을 보이는 상황이 발생할 수 있었다.
핵심 질문: `OnHitStart`는 어디서 오고, 왜 `TakeDamage(0)`까지 이어질 수 있는가?

---

## 1) OnHitStart는 어디에 있나?

`OnHitStart`는 **2군데**에 있다.

1. `Assets/Scripts/Player/PlayerVisual.cs:36`
- 애니메이션 이벤트를 받는 진입점.
- Animator 클립 타임라인에서 `OnHitStart` 이벤트가 실행되면 여기 메서드가 호출된다.

2. `Assets/Scripts/Player/PlayerController.cs:228`
- 실제 전투 로직 진입점.
- `PlayerVisual.OnHitStart()`가 내부에서 `_controller.OnHitStart()`를 호출해 여기로 전달된다.

즉 호출 체인은 아래와 같다.

`Animation Clip Event -> PlayerVisual.OnHitStart() -> PlayerController.OnHitStart() -> DamageCaster.EnableHitbox(...)`

---

## 2) CurrentAttackDamage는 언제 채워지나?

`CurrentAttackDamage`는 공격 상태에서 세팅된다.

- `Assets/Scripts/Player/States/AttackState.cs:53`
- `StartComboStep()`에서 `Controller.CurrentAttackDamage = _currentAttackData.damage;`

의미:
- 정상 흐름이면 `AttackState` 진입 후 데미지 값(예: 10, 15, 30)이 먼저 세팅되고,
- 그 다음 공격 애니메이션 이벤트(`OnHitStart`)가 들어와야 한다.

---

## 3) TakeDamage(0)는 어떻게 연결되나? (문제 시나리오)

문제는 `OnHitStart`가 **비정상 타이밍**에 들어올 때였다.

예시 흐름(과거 위험 경로):

1. `OnHitStart`가 공격 상태 외 타이밍에 호출됨
2. `CurrentAttackDamage`가 아직 0
3. `DamageCaster.EnableHitbox(0)` 실행
4. `DamageCaster.FixedUpdate()`에서 겹친 대상을 찾음
5. `target.TakeDamage(_damagePayload)` 호출 (`_damagePayload == 0`)
6. `Health.TakeDamage(0)`에서 OnDamageTaken 이벤트가 발행되면, 피격 반응 로직이 실행될 수 있음

관련 코드 위치:
- `EnableHitbox`: `Assets/Scripts/Common/Combat/DamageCaster.cs:66`
- 실제 타격 호출: `Assets/Scripts/Common/Combat/DamageCaster.cs:159`
- Health 처리: `Assets/Scripts/Common/Combat/Health.cs:31`

---

## 4) 이번에 넣은 안전 가드(현재 기준)

### A. PlayerController 쪽 가드

`Assets/Scripts/Player/PlayerController.cs:228`
- `OnHitStart`에서 아래를 확인 후 통과 시에만 히트박스 오픈:
  - `_damageCaster != null`
  - 현재 FSM 상태가 `AttackState`
  - `Mathf.RoundToInt(CurrentAttackDamage) > 0`

### B. DamageCaster 쪽 가드

`Assets/Scripts/Common/Combat/DamageCaster.cs:66`
- `EnableHitbox(int damage)`에서 `damage <= 0`이면 즉시 리턴(캐스팅 상태 리셋).

`Assets/Scripts/Common/Combat/DamageCaster.cs:102`
- `ForceDisableHitbox()` 추가: 상태 전환/초기화 시 남은 판정을 강제 정리.

`Assets/Scripts/Common/Combat/DamageCaster.cs:179`
- `OnDrawGizmosSelected()` 추가: 선택 시 기즈모 가시성 강화.

### C. AttackState 쪽 가드

`Assets/Scripts/Player/States/AttackState.cs:158`
- `Exit()`에서 `Controller.OnHitEnd()`를 호출해 상태 이탈 시 히트박스 잔존 제거.

### D. Health 쪽 가드

`Assets/Scripts/Common/Combat/Health.cs:34`
- `damage <= 0`이면 무시.
- 무데미지로 피격 이벤트가 오염되는 것을 차단.

---

## 5) 디버깅할 때 보는 체크리스트

1. `PlayerVisual`의 `OnHitStart/OnHitEnd`가 의도한 공격 클립 프레임에만 있는지 확인.
2. Play 중 `PlayerController` 현재 상태가 `AttackState`가 아닐 때 `OnHitStart` 로그가 찍히는지 확인.
3. `DamageCaster`의 `_damagePayload`가 0일 때 `EnableHitbox`가 열리지 않는지 확인.
4. `Health.TakeDamage`에 0 데미지가 들어와도 반응이 없는지 확인.
5. SceneView의 Gizmos ON + 오브젝트 선택 상태에서 반경이 보이는지 확인.

---

## 6) 한 줄 요약

`OnHitStart`는 애니메이션 이벤트 진입점이고, 이 이벤트가 잘못된 시점에 들어오면 과거에는 `TakeDamage(0)` 경로로 피격 반응 오작동이 가능했다. 현재는 Controller/DamageCaster/AttackState/Health 4중 가드로 차단했다.
