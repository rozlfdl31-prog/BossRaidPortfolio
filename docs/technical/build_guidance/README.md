# 🏗️ Build Guidance

이 폴더는 `Boss Raid Portfolio`의 빌드 가이드를 정리하는 기술 문서 묶음이다.  
현재 기준으로는 **멀티플레이 체크용 빠른 빌드**를 가장 중요한 사용 시나리오로 둔다.

---

## 1. 문서 목적

이 폴더의 문서는 아래 목표를 가진다.

* 멀티플레이 smoke check를 위한 빠른 빌드 절차를 고정한다.
* 매번 scene / prop / quality / build settings를 감으로 만지지 않게 한다.
* build time이 오래 걸릴 때, 무엇을 줄여도 되고 무엇은 절대 줄이면 안 되는지 기록한다.
* Host / Client / Lobby / Result check를 위한 최소 검증 흐름을 남긴다.

---

## 2. 문서 구성

| 문서 | 역할 |
| --- | --- |
| `Fast_Multiplayer_Check_Build_Guide.md` | 멀티플레이 체크 중심의 메인 빌드 가이드 |
| `Build_Time_Log_Template.md` | 빌드 시간 / 조건 / 결과를 기록하는 템플릿 |

---

## 3. 기본 운영 메모

* daily check는 `Fast Multiplayer Check Build`를 우선 사용한다.
* current one-click entry는 `Tools/Build/Fast Multiplayer Verify Build` 메뉴다.
* one-click fast build는 Beautify runtime `Resources`를 build 동안만 임시 제외하고, 끝나면 자동 복구한다.
* fast build runner는 temporary asset exclusion만 담당한다. multiplayer default gameplay target을 full-art scene으로 되돌리는 일은 별도 scene/profile switch로 관리한다.
* full art / full prop / release-like build는 꼭 필요할 때만 만든다.
* fast build를 위해 prop을 줄일 때도 **게임 로직 검증에 필요한 충돌, 시야, UI, spawn, result 경로**는 유지해야 한다.
* build time 단축용 scene 경량화는 random manual edit보다 **문서화된 고정 프로필**로 관리하는 편이 안전하다.

---

## 4. 현재 기준 경로

| 항목 | 현재 기준 |
| --- | --- |
| multiplayer title scene | `Assets/Scenes/mutiplayer/TitleScene.unity` |
| multiplayer fast-check gameplay scene | `Assets/Scenes/mutiplayer/GamePlayScene_Verify.unity` |
| multiplayer full-art gameplay scene | `Assets/Scenes/mutiplayer/GamePlayScene.unity` |
| fast build runner | `Assets/Editor/FastMultiplayerVerifyBuildRunner.cs` |
| compile quick check | `dotnet build BossRaidPortfolio.sln -v:minimal` |

---

## 5. 사용 순서

1. `Fast_Multiplayer_Check_Build_Guide.md`를 먼저 읽는다.
2. 빌드 전/후 체크를 수행한다.
3. 빌드 시간과 조건은 `Build_Time_Log_Template.md` 형식으로 남긴다.
