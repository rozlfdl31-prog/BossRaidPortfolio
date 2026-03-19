# 🚀 Progress Log: Boss Raid Portfolio

## 🧭 운영 방식
- Progress Log는 docs/Progress_Log/ 폴더 단위로 관리한다.
- 날짜별 기록은 YYYY-MM-DD.md 파일로 분리한다.
- 같은 날짜의 추가 작업은 기존 날짜 파일에 병합하고, 새 날짜 헤더를 만들지 않는다.
- 신규 로그 작성 시 TEMPLATE.md를 복사해 사용한다.

## 🧩 로그 작성 규칙 (신규 엔트리부터 적용)
- 신규 엔트리는 체크리스트 업데이트와 맥락노트를 분리해서 작성한다.
- 같은 날짜 로그에 topic이 2개 이상이면 한 블록에 섞어 쓰지 않는다.
- multiple topic 로그는 `Topic 1`, `Topic 2`처럼 주제를 먼저 나누고, 각 topic 아래에 `오늘 반영한 작업`, `체크리스트 업데이트`, `맥락노트`, `기술적 고려`를 별도로 작성한다.
- 서로 다른 topic의 발견/수정/판단 근거는 같은 bullet 묶음에 합치지 않는다.
- 기술적 고려에는 아래 3항목을 고정으로 포함한다.
  - **무엇을 발견했는가**
  - **무엇을 수정했는가**
  - **왜 그렇게 판단했는가**
- 코드 변경이 포함된 로그는 코드 검사 결과 블록(명령/결과/미실행 사유)을 반드시 포함한다.
- 장기 작업 목록(마일스톤/버그/폴리싱)은 docs/roadmap/Milestone_Backlog.md에서 관리한다.

## 🔎 문서 동기화 참조 규칙
- `System_Blueprint`/`Technical_Glossary`를 최신화할 때는 기준 로그 파일(`docs/Progress_Log/YYYY-MM-DD.md`)을 먼저 지정한다.
- 완료 보고에는 `참조 로그: docs/Progress_Log/YYYY-MM-DD.md` 형식을 사용해 근거를 남긴다.
- 여러 날짜를 근거로 썼다면 `참조 로그`를 여러 줄로 기록해 추적 가능성을 유지한다.

## 📄 기록 템플릿
- [TEMPLATE.md](./TEMPLATE.md)

## 📅 날짜별 로그
- [2026-03-19.md](./2026-03-19.md) - Session 6 gameplay start 세션 구현 + multiplayer verify scene + fast verify build runner + manual smoke validation + Session 7 strict cleanup hardening
- [2026-03-18.md](./2026-03-18.md) - multiplayer test gameplay scene artist map/UI merge + partner HUD multiplayer-only gate + combo HUD hit-confirm gate + multiplayer lobby active session + host 2초 stable Start unlock gate
- [2026-03-17.md](./2026-03-17.md) - Client join 런타임 + Lobby Events compile 안정화
- [2026-03-16.md](./2026-03-16.md)
- [2026-03-13.md](./2026-03-13.md)
- [2026-03-12.md](./2026-03-12.md)
- [2026-03-11.md](./2026-03-11.md)
- [2026-03-06.md](./2026-03-06.md)
- [2026-03-05.md](./2026-03-05.md)
- [2026-03-04.md](./2026-03-04.md)
- [2026-03-03.md](./2026-03-03.md)
- [2026-02-28.md](./2026-02-28.md)
- [2026-02-27.md](./2026-02-27.md)
- [2026-02-26.md](./2026-02-26.md)
- [2026-02-24.md](./2026-02-24.md)
- [2026-02-23.md](./2026-02-23.md)
- [2026-02-21.md](./2026-02-21.md)
- [2026-02-20.md](./2026-02-20.md)
- [2026-02-12.md](./2026-02-12.md)
- [2026-02-11.md](./2026-02-11.md)
- [2026-02-10.md](./2026-02-10.md)
- [2026-02-09.md](./2026-02-09.md)
- [2026-02-06.md](./2026-02-06.md)
- [2026-02-05.md](./2026-02-05.md)
- [2026-02-04.md](./2026-02-04.md)
- [2026-02-03.md](./2026-02-03.md)
- [2026-02-02.md](./2026-02-02.md)

## 🗂️ Milestone Backlog
- [Milestone_Backlog.md](../roadmap/Milestone_Backlog.md)

## 📦 Legacy
- [LEGACY_MONOLITH.md](./LEGACY_MONOLITH.md) (분할 전 단일 파일 원본 보관)
