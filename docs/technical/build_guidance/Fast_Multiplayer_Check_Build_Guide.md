# 🚀 Fast Multiplayer Check Build Guide

이 문서는 **멀티플레이 기능 확인용 빠른 빌드**를 위한 기준 문서다.  
목표는 build time을 줄이면서도, Host / Client / Lobby / basic gameplay / result flow를 안전하게 확인하는 것이다.

---

## 1. Goal

이 가이드는 아래 상황을 위해 사용한다.

* `Session 3 ~ Session 6` 멀티플레이 흐름을 빠르게 체크할 때
* Editor 1개 + standalone build 1개로 Host / Client smoke test를 돌릴 때
* full art build 대신 logic-first build가 필요할 때
* build가 너무 오래 걸려서 daily iteration이 막힐 때

이 문서는 **release build 가이드가 아니다.**  
이 문서는 먼저 `multiplayer check build`를 빠르게 반복하기 위한 문서다.

---

## 2. Scope

현재 fast build의 주 목적은 아래 확인이다.

* Host create
* Client join
* lobby `1/2 -> 2/2`
* Host `Start` unlock after stable `2s`
* cleanup (`Back`, `Cancel`, disconnect fail path)
* gameplay scene enter
* basic fight start
* result UI path (`Victory` / `Game Over`) 확인

현재 범위 밖:

* release performance profiling
* final visual quality review
* full map decoration review
* long full-content soak test

---

## 3. Current Project Path

| 항목 | 경로 / 값 |
| --- | --- |
| multiplayer title scene | `Assets/Scenes/mutiplayer/TitleScene.unity` |
| multiplayer fast-check gameplay scene | `Assets/Scenes/mutiplayer/GamePlayScene_Verify.unity` |
| multiplayer full-art gameplay scene | `Assets/Scenes/mutiplayer/GamePlayScene.unity` |
| fast build runner | `Assets/Editor/FastMultiplayerVerifyBuildRunner.cs` |
| quick compile check | `dotnet build BossRaidPortfolio.sln -v:minimal` |
| current completed session | `Session 6 - gameplay start` |
| current next step | `2P spawn / gameplay ownership sync` |

---

## 4. Build Variant

| Variant | 목적 | 사용 시점 | 품질 기준 |
| --- | --- | --- | --- |
| `Fast Multiplayer Check Build` | daily smoke test | 가장 자주 사용 | logic-first |
| `Feature QA Build` | 기능 검증 + 더 넓은 playtest | 기능 완료 직후 | medium |
| `Release-like Build` | 최종 품질 확인 | milestone close 직전 | full |

### 4.1. 추천 기본값

기본 반복은 아래를 추천한다.

* Host: Unity Editor
* Client: `Fast Multiplayer Check Build`

이 조합이 가장 빠르다.

---

## 5. Fast Build Rule

### 5.1. Core Rule

Fast build는 **logic 검증에 필요 없는 무거운 시각 리소스**를 줄이는 방향으로 만든다.

중요한 원칙:

* gameplay logic를 깨는 삭제는 금지
* collision / spawn / UI / boss detection / result flow에 영향이 없는 것만 줄인다
* 줄인 내용은 반드시 문서에 남긴다
* release 직전에는 full restore check를 해야 한다

### 5.2. Keep List

아래는 fast build에서도 유지해야 한다.

* `TitleScene` 멀티플레이 UI
* `LobbyPanel`
* join code / room title / player count 표시
* `Start` button gate
* `Back` / `Cancel` / wrong key popup
* gameplay verify scene의 `Player`, `Boss`, `Systems`, `EventSystem`
* spawn anchor
* result UI (`GameOver_Panel`, `GameResult`)
* gameplay collision에 직접 영향을 주는 바닥 / 주요 벽 / 전투 구역
* boss attack telegraph / hit 판정 확인에 필요한 최소 VFX

### 5.3. Safe Reduce List

아래는 fast build에서 줄일 가능성이 높은 항목이다.

* 큰 수의 장식용 prop
* 멀리 있는 background decoration
* gameplay와 무관한 extra mesh
* 무거운 sky / atmosphere variation
* dense foliage
* high-cost particle decoration
* 고해상도 decorative texture set
* 테스트에 필요 없는 alternative environment set

### 5.4. Do Not Strip List

아래는 fast build에서도 함부로 제거하면 안 된다.

* player 이동 경로를 막는 collision object
* boss path / attack range / landing 위치에 영향을 주는 지형
* gameplay camera 기준점
* HUD / result UI / popup
* network bootstrap과 scene entry path
* spawn point
* 보스와 플레이어의 피격 확인에 필요한 최소 visual cue
* gameplay result trigger에 연결된 object

### 5.5. Better Strategy

prop을 줄일 때는 아래 순서를 추천한다.

1. `disable` first
2. `duplicate test scene` or fixed fast-build profile 사용
3. random delete는 마지막 수단

쉬운 설명:

Delete보다 disable이 더 안전하다.  
Random manual cleanup보다 **고정된 fast profile**이 더 좋다.

---

## 6. Props Reduction Guidance

### 6.1. When Props Can Be Reduced

아래 질문에 모두 `Yes`면 줄여도 된다.

1. 이 object가 collision에 영향을 주지 않는가?
2. 이 object가 boss movement / attack readability에 영향을 주지 않는가?
3. 이 object가 result UI visibility를 가리지 않는가?
4. 이 object가 build time만 늘리고 smoke test value는 거의 없는가?

### 6.2. When Props Must Stay

아래 중 하나라도 `Yes`면 유지하는 편이 좋다.

1. 플레이어가 여기에 걸리거나 올라설 수 있는가?
2. 보스가 여기 때문에 이동 / 회전 / 공격 연출이 달라지는가?
3. spectator / camera / result UI 시야에 영향을 주는가?
4. “빠른 빌드에서는 되는데 실제 빌드에서는 안 된다” 같은 괴리를 만들 가능성이 큰가?

### 6.3. Restore Rule

fast build를 위해 줄인 내용은 아래 release restore checklist로 다시 확인한다.

* disabled prop restore
* full environment restore
* quality restore
* result UI visibility final check
* boss readability final check
* build settings final check

---

## 7. Scene Rule

### 7.1. Entry Rule

멀티플레이 smoke check는 아래 scene 기준으로 시작한다.

* start scene: `Assets/Scenes/mutiplayer/TitleScene.unity`
* gameplay target: `Assets/Scenes/mutiplayer/GamePlayScene_Verify.unity`
* preferred build entry: `Tools/Build/Fast Multiplayer Verify Build`

### 7.2. Build Settings Rule

확인할 것:

* correct title scene included
* correct fast-check gameplay scene included
* `LoadingScene` / full-art `GamePlayScene`는 current fast profile에서 disabled 상태인지 확인
* old / duplicate / temporary test scene not accidentally enabled
* one-click build를 쓸 때는 build runner가 scene list를 직접 지정하므로, `Build Settings`는 fallback/manual path 확인용으로 본다

### 7.3. Scene Naming Rule

권장 규칙:

* fast-check scene는 이름만 보고 목적이 드러나야 한다
* temporary scene는 오래 남기지 않는다
* 실제 active scene path는 문서 기준과 맞아야 한다

### 7.4. Restore / Full Mode Rule

아래 2가지는 서로 다른 문제다.

1. fast build 중 임시 asset exclusion
2. multiplayer 기본 gameplay target을 verify scene으로 둘지, full-art scene으로 둘지

설명:

* `Tools/Build/Fast Multiplayer Verify Build`는 `Assets/Map/Beautify/URP/Runtime/Resources`를 build 동안만 잠시 제외하고, build 후 자동 복구한다.
* build가 중간에 끊기거나 Unity가 비정상 종료되면 `Tools/Build/Restore Fast Build Assets`로 수동 복구한다.
* 이 자동 복구는 **Beautify resource inclusion**만 되돌린다.
* 이 자동 복구가 multiplayer scene target까지 full-art mode로 되돌려 주지는 않는다.
* current project default가 `GamePlayScene_Verify`라면, full-art multiplayer mode로 돌아갈 때는 scene/profile switch를 별도로 수행해야 한다.

---

## 8. Pre-Build Checklist

빌드 전에 아래를 본다.

* 최신 branch / 작업 파일 반영 확인
* `dotnet build BossRaidPortfolio.sln -v:minimal` 통과 확인
* `Assets/Scenes/mutiplayer/TitleScene.unity` 경로 확인
* `Assets/Scenes/mutiplayer/GamePlayScene_Verify.unity` 경로 확인
* `Assets/Editor/FastMultiplayerVerifyBuildRunner.cs`가 최신인지 확인
* Cloud Project / UGS 연결 확인
* wrong key popup / lobby panel / result UI object가 scene에 남아 있는지 확인
* 오늘 fast build에서 줄인 prop 목록 확인
* release용 full art 상태가 아니라는 점 명시

---

## 9. Build Steps

### 9.1. Editor Side

1. Unity `2022.3.x` 프로젝트를 연다.
2. compile error가 없는지 확인한다.
3. fast build profile 기준으로 scene / prop 상태를 맞춘다.
4. `Tools/Build/Fast Multiplayer Verify Build`를 사용한다.
5. build runner가 `TitleScene + GamePlayScene_Verify`만 직접 지정하고, build 동안 `Assets/Map/Beautify/URP/Runtime/Resources`를 임시 제외/복구한다.
6. manual fallback이 필요할 때만 `Build Settings`에서 `TitleScene + GamePlayScene_Verify` 구성을 다시 확인한다.

### 9.2. Build Action

1. 메뉴 `Tools/Build/Fast Multiplayer Verify Build`를 실행한다.
2. output 이름은 기본적으로 `Builds/FastMultiplayerVerify/BossRaidPortfolio_MPVerify.exe`를 사용한다.
3. 다른 output 이름이 필요하면 build runner를 복제하거나 별도 variant runner를 만든다.
4. 정상 종료된 build는 asset restore를 따로 할 필요가 없다.
5. build가 중단됐을 때만 `Tools/Build/Restore Fast Build Assets`를 사용한다.

예시:

```text
Builds/2026-03-18_MP_Smoke/
Builds/2026-03-18_MP_FastCheck_Client/
```

### 9.3. Recommended Pattern

권장 패턴:

* Editor = Host
* Standalone build = Client

이렇게 하면 build 2개를 매번 만들 필요가 없다.

---

## 10. Post-Build Smoke Test

### 10.1. Minimum Test

아래는 fast build 후 최소 확인 항목이다.

* Host can create room
* Client can join with real join code
* Host sees `1/2 connected`
* both peers see `2/2 connected`
* Host `Start` unlock waits stable `2s`
* Client does not get Start authority
* `Back` / `Cancel` cleanup works
* gameplay scene opens
* result UI appears after fight

### 10.2. Result Check

최소 result check:

* boss dead -> `Victory`
* both players dead -> `Game Over`
* result UI is visible on screen
* no obvious missing reference in result path

### 10.3. Failure Note

아래 중 하나면 fast build profile을 다시 점검한다.

* build에서는 join 안 되는데 editor에서는 됨
* result UI가 안 뜸
* boss / player collision이 이상함
* prop을 줄인 뒤 gameplay path가 달라짐

---

## 11. Build Time Log

매번 아래를 남기면 좋다.

| 항목 | 기록 예시 |
| --- | --- |
| date | `2026-03-18` |
| branch / commit | `feature/...` |
| variant | `Fast Multiplayer Check Build` |
| platform | `Windows` |
| build start | `20:10` |
| build end | `21:18` |
| total time | `68 min` |
| stripped content | `background props, extra VFX` |
| smoke result | `Host/Client join ok` |
| known issue | `result UI visibility needs one more check` |

실제 기록은 `Build_Time_Log_Template.md`를 사용한다.

---

## 12. Troubleshooting

### 12.1. Build Takes Too Long

확인할 것:

* `Tools/Build/Fast Multiplayer Verify Build` 대신 plain `Build`를 눌러 full asset set로 빌드한 것은 아닌가
* full prop set가 들어가 있는가
* 불필요한 scene가 Build Settings에 들어가 있는가
* high-cost VFX / texture set가 켜져 있는가
* build 중 `Assets/Map/Beautify/URP/Runtime/Resources` exclusion이 실제로 적용됐는가
* fast build profile이 문서대로 적용됐는가

### 12.2. Lobby Works but Gameplay Check Fails

확인할 것:

* active gameplay scene path가 맞는가
* current fast profile이 `GamePlayScene_Verify`를 가리키는가
* result UI object가 scene에 남아 있는가
* gameplay collision object를 너무 많이 줄이지 않았는가

### 12.3. Host/Client Behavior Mismatch

확인할 것:

* Editor and build are same code revision
* build를 새로 만들었는가
* old build를 재사용하고 있지 않은가

### 12.4. Fast Build Is Too Different from Real Build

이 경우는 fast build를 너무 많이 줄인 것이다.

복구 방향:

1. gameplay collision 관련 prop restore
2. boss readability 관련 VFX 일부 restore
3. result UI visibility blocking object restore

### 12.5. Need Full-Art Multiplayer Mode Again

확인할 것:

* fast build asset restore와 full-art mode restore를 같은 것으로 생각하지 않았는가
* current multiplayer gameplay target이 아직 `GamePlayScene_Verify`를 가리키고 있지 않은가
* full-art multiplayer build가 필요하면 `GamePlayScene` target/profile로 별도 전환했는가

---

## 13. Team Rule Recommendation

### 13.1. Good Rule

권장 운영:

* `daily logic check`는 fast build 사용
* `milestone close` 전에는 full QA build 사용
* fast build에서 줄인 내용은 항상 문서와 log에 남김

### 13.2. Bad Rule

피해야 할 운영:

* random manual strip
* 무엇을 껐는지 기록 없음
* old build를 계속 재사용
* build가 느리다는 이유로 result / UI / collision까지 같이 제거

---

## 14. Quick Summary

fast multiplayer build의 핵심은 아래 4가지다.

1. **logic check에 필요 없는 prop만 줄인다**
2. **Host / Client / Lobby / Result path는 유지한다**
3. **Editor Host + standalone Client 조합을 기본으로 쓴다**
4. **fast build는 `FastMultiplayerVerifyBuildRunner`로 돌리고, Beautify exclusion을 자동화한다**
