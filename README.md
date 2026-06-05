# Portfolio 01 — 3D Platformer

Unity 5.1.4f1에서 시작하여 Unity 6000.4.8f1까지 직접 마이그레이션·유지보수한 3D 플랫포머 프로젝트입니다.  
단순히 동작하는 게임을 넘어, 레거시 코드베이스를 현대 Unity 클라이언트 엔지니어링 관점에서 개선한 과정을 담고 있습니다.

---

## Migration History

| 버전 | 비고 |
|---|---|
| Unity 5.1.4f1 | 최초 개발 |
| Unity 5.5.0f3 | 1차 업그레이드 |
| Unity 2020.3.37f1 | LTS 마이그레이션 |
| Unity 6000.4.8f1 | 현재 — Unity 6 포팅 |

---

## Key Features

- **CharacterController 기반 플레이어 이동** — 점프, 더블점프, 넉백, 가속패드, 안티그래비티, 이동 플랫폼 탑승
- **FMOD 오디오 미들웨어 통합** — SoundBoard 추상화 레이어를 통한 BGM·SFX·환경음 제어
- **오브젝트 풀링 시스템** — 런타임 오브젝트 생성 비용 절감
- **AssetBundle 런타임 로딩** — 온라인/로컬/스트리밍 모드 분기 지원
- **모바일·PC 입력 분기** — CNJoystick과 레거시 Input System을 단일 인터페이스로 추상화
- **복수 씬 플로우** — 프리로딩, 씬 전환, 게임 레디 카운트다운, 스테이지 클리어·게임오버 흐름
- **인피니트 모드** — 절차적 맵 확장 및 DeathBall 추격 메커니즘
- **리플레이 시스템** — 플레이 데이터 레코딩 및 재생

---

## Modernization

Unity 5 시대 코드베이스에서 발견한 주요 문제점과 개선 내용입니다.

### 아키텍처 개선

| 항목 | 변경 전 | 변경 후 |
|---|---|---|
| 입력 처리 | `PlayerMoveCC` 내부에서 조이스틱·키보드 직접 폴링 | `PlayerInputAdapter`로 분리 — 플랫폼·입력 소스 추상화 |
| 입력 모드 설정 | `PlayerMoveCC` 내부에서 PlayerPrefs 읽기·CNJoystick 설정 혼재 | `PlayerInputAdapter`의 책임으로 통합 |
| `FindObjectOfType` | Unity 6 deprecated API 사용 | `FindAnyObjectByType`으로 전환 |

### 런타임 버그 수정

| 항목 | 문제 | 수정 |
|---|---|---|
| `OnDestroy` 콜백 | `OnDestoy()` 오타로 콜백 미실행, 싱글턴 참조 미해제 | `OnDestroy()` 수정 |
| 씬 종료 NullReferenceException | `PlayerMoveCC` / `ReplayGameplay`의 `OnDestroy`에서 파괴 순서 미보장으로 NRE 발생 | `GameRuleManager.instance` null 가드 추가 |

### 코드 품질 개선

| 항목 | 변경 전 | 변경 후 |
|---|---|---|
| 애니메이터 파라미터 | `"Speed"`, `"JumpTgr"` 등 하드코딩 문자열 | `AnimatorParams` 정적 상수 클래스 |
| 사운드 ID | `"SND_PC_JUMP"` 등 하드코딩 문자열 | `SoundID` 정적 상수 클래스 |
| PlayerPrefs 키 | `"Speed"`, `"InputMode"` 등 하드코딩 문자열 | `PrefKeys` 정적 상수 클래스 |
| 서버 URL | 로컬 네트워크 IP 주소 코드 내 하드코딩 | `[SerializeField]`로 Inspector 설정으로 이동 |
| 런타임 Debug.Log | 개발용 로그 18개 파일에 산재 | 제거 또는 `LogError`/`LogWarning` 등급 상향 |

### 환경 설정

| 항목 | 문제 | 수정 |
|---|---|---|
| `.gitignore` | `*.dll`, `*.so` 등 전체 제외로 FMOD 등 플러그인 바이너리 미추적 | `Assets/Plugins/**` 예외 처리 추가 |
| `com.unity.ugui` 패키지 | `manifest.json` 누락으로 `UnityEngine.UI` 컴파일 에러 | `manifest.json`에 명시적 추가 |

---

## Architecture Overview

```
Assets/Scripts/
├── Player/
│   ├── PlayerMoveCC.cs          # CharacterController 기반 이동 (점프·중력·넉백 포함)
│   ├── PlayerHealth.cs          # 체력·무적·데미지 처리
│   ├── PlayerFX.cs              # 이동·착지·자석 이펙트
│   ├── PlayerSpawn.cs           # 스폰 시퀀스
│   └── Input/
│       ├── PlayerInputAdapter.cs    # 입력 읽기 + 입력 모드 설정
│       └── PlayerInputState.cs      # 입력 값 전달 struct
├── System/
│   ├── GameRuleManager.cs       # 게임 상태·타이머·점수·씬 흐름
│   ├── DataLoadingSystem/
│   │   ├── BundleManager/       # AssetBundle 로딩
│   │   ├── ObjectPooler/        # 오브젝트 풀링
│   │   └── ResourcesManager.cs  # 리소스 로드 중앙화
│   └── Sound/
│       ├── FMODSoundManager.cs  # FMOD 래퍼
│       └── SoundBoard.cs        # 사운드 ID 기반 재생 인터페이스
├── Util/
│   ├── AnimatorParams.cs        # 애니메이터 파라미터 상수
│   ├── PrefKeys.cs              # PlayerPrefs 키 상수
│   └── SoundID.cs               # 사운드 ID 상수
└── UI/
    ├── UIManager.cs             # UI 전반 관리
    └── OptionPanel.cs           # 입력 모드·튜닝값 설정 UI
```

---

## Legacy & Known Issues

- `Scripts/Deprecated/` — 이전 버전 스크립트 보존 (일부 활성 코드에서 참조 중)
- `ExternalAssets/CNControls/` — 서드파티 모바일 조이스틱 (현재 사용 중)
- `GameRuleManager`가 여러 책임을 보유 — 포트폴리오 범위 내에서 ScoreController 분리는 의존 관계로 인해 보류
