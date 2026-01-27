# _Project_KanaRebuild 폴더 구조

```
_Project_KanaRebuild/
├── README.md                 # 프로젝트 전체 문서
│
├── Scripts/                  # C# 스크립트
│   ├── StageData.cs         # ✅ 완료
│   ├── GameManager.cs       # ✅ 완료
│   ├── SyncManager.cs       # ✅ 완료
│   └── TerminalManager.cs   # ✅ 완료
│
├── Data/                     # StageData ScriptableObject 에셋 저장
│   ├── Stage_01.asset       # (Unity에서 생성 예정)
│   ├── Stage_02.asset
│   ├── Stage_03.asset
│   └── Stage_04.asset
│
├── Prefabs/                  # UI 및 게임오브젝트 프리팹
│   ├── TerminalUI.prefab    # (Unity에서 생성 예정)
│   └── ChoiceButton.prefab
│
├── Materials/                # 셰이더 매터리얼
│   └── KanaGlitchMaterial.mat  # (Unity에서 생성 예정)
│
└── Scenes/                   # 게임 씬
    └── KanaRebuildScene.unity  # (Unity에서 생성 예정)
```

## 현재 상태

### ✅ 완료된 작업
- Scripts 폴더 및 4개 핵심 스크립트 생성
- Data, Prefabs, Materials, Scenes 폴더 구조 생성
- README.md 문서 작성

### 🔲 Unity 에디터에서 진행할 작업
1. Scene 구성 (Canvas, GameManager GameObject)
2. UI 생성 (TerminalScrollView, SyncGauge, ChoicePanel)
3. StageData 4개 생성 (Data 폴더에 저장)
4. Inspector에서 참조 연결
5. 테스트 플레이

## 빠른 참조

- **스크립트 위치**: `Scripts/`
- **데이터 저장 위치**: `Data/`
- **UI 프리팹 위치**: `Prefabs/`
- **씬 저장 위치**: `Scenes/`
