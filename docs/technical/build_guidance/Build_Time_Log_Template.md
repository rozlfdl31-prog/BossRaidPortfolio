# 📝 Build Time Log Template

이 템플릿은 fast multiplayer check build의 시간을 기록하기 위한 문서다.

---

## 1. Build Record

| 항목 | 값 |
| --- | --- |
| 날짜 | |
| 작업자 | |
| branch / commit | |
| variant | `Fast Multiplayer Check Build` / `Feature QA Build` / `Release-like Build` |
| platform | |
| output folder | |
| build start | |
| build end | |
| total time | |

---

## 2. Fast Build Profile

| 항목 | 값 |
| --- | --- |
| 줄인 prop 범위 | |
| 유지한 핵심 object | |
| disabled VFX / sky / decoration | |
| build settings scene list 확인 | Yes / No |
| compile check 실행 | Yes / No |
| compile check command | `dotnet build Assembly-CSharp.csproj -v:minimal` |

---

## 3. Smoke Test Result

| 체크 | 결과 |
| --- | --- |
| Host create | |
| Client join | |
| `1/2 -> 2/2` 표시 | |
| Host `Start` unlock after `2s` | |
| cleanup (`Back` / `Cancel`) | |
| gameplay scene enter | |
| result UI (`Victory` / `Game Over`) | |

---

## 4. Notes

### 무엇을 줄였는가

* 

### 무엇을 유지했는가

* 

### 무엇이 느렸는가

* 

### 다음 빌드에서 바꿀 것

* 
