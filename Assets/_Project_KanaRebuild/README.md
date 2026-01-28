# 🎮 Kana Rebuild - 코딩 퍼즐 게임

> AI 캐릭터 '카나'를 복원하는 코딩 교육용 비주얼 노벨 게임

---

## 📋 프로젝트 개요

플레이어는 손상된 AI 시스템의 코드를 수정하며 '카나'를 복원합니다.  
터미널 인터페이스를 통해 문제를 파악하고, 올바른 코드 수정안을 선택하여 단계별로 동기화율을 높여나갑니다.

### 🎯 핵심 메커니즘
- **터미널 기반 스토리텔링**: 타이핑 효과로 몰입감 있는 시스템 로그 연출
- **코드 수정 퍼즐**: 실제 프로그래밍 오류를 찾아 고치는 선택형 퀴즈
- **동기화 시스템**: 정답률에 따라 카나의 글리치 이펙트가 점진적으로 복원
- **교육적 요소**: 초보자도 이해할 수 있는 기초 프로그래밍 개념

---

## 📁 프로젝트 구조

```
_Project_KanaRebuild/
├── README.md                 # 이 파일
│
├── Scripts/                  # C# 스크립트
│   ├── StageData.cs         # 스테이지 데이터 구조 (ScriptableObject)
│   ├── GameManager.cs       # 게임 루프 제어 (문제 제시 → 선택 → 결과)
│   ├── SyncManager.cs       # 동기화 게이지 및 셰이더 관리
│   └── TerminalManager.cs   # 터미널 텍스트 타이핑 효과
│
├── Data/                     # (예정) StageData ScriptableObject 에셋
│   ├── Stage_01.asset
│   ├── Stage_02.asset
│   ├── Stage_03.asset
│   └── Stage_04.asset
│
├── Prefabs/                  # (예정) UI 프리팹
│   ├── TerminalUI.prefab
│   └── ChoiceButton.prefab
│
├── Materials/                # (예정) 카나 글리치 셰이더 매터리얼
│   └── KanaGlitchMaterial.mat
│
└── Scenes/                   # (예정) 게임 씬
    └── KanaRebuildScene.unity
```

---

## 🛠️ 기술 스택

- **Engine**: Unity 2021.3+ (URP)
- **UI**: TextMesh Pro, Unity UI
- **VFX**: Shader Graph (글리치 효과)
- **Architecture**: ScriptableObject 기반 데이터 관리

---

## 🔧 스크립트 설명

### 1️⃣ **StageData.cs** (ScriptableObject)
각 스테이지의 문제, 선택지, 정답을 저장하는 데이터 컨테이너

**주요 필드:**
- `stageIndex`: 스테이지 번호
- `targetSyncRate`: 이 스테이지 완료 시 목표 동기화율
- `backgroundLogs[]`: 시작 시 흘러나올 터미널 로그
- `problemCode`: 수정해야 할 코드 (TextArea)
- `choices[]`: 선택지 배열
- `correctChoiceIndex`: 정답 인덱스 (0부터 시작)
- `successMessage` / `failMessage`: 결과 메시지

**생성 방법:**
```
Assets 우클릭 → Create → KanaRebuild → StageData
```

---

### 2️⃣ **GameManager.cs**
게임의 전체 흐름을 제어하는 메인 컨트롤러

**주요 기능:**
- `PlayStage()`: 배경 로그 → 문제 제시 → 선택지 표시
- `ShowChoices()`: 선택지 버튼 동적 생성 및 이벤트 바인딩
- `OnChoiceSelected()`: 정답/오답 판정 후 분기
- `SuccessRoutine()`: 성공 시 동기화율 증가 및 다음 스테이지
- `FailRoutine()`: 실패 시 동기화율 감소 및 재시도

**Inspector 설정:**
- `stages[]`: StageData 배열 (순서대로 플레이)
- `terminal`: TerminalManager 참조
- `sync`: SyncManager 참조
- `choicePanel`: 선택지 UI 패널
- `choiceButtons[]`: 선택지 버튼 배열 (최소 4개)

---

### 3️⃣ **SyncManager.cs**
동기화 게이지와 비주얼 이펙트를 관리

**주요 기능:**
- `UpdateSync(float amount)`: 동기화율 증감 (0~100 Clamp)
- Slider UI 실시간 업데이트
- 셰이더 파라미터 자동 조절 (`_GlitchAmount`)

**Inspector 설정:**
- `syncGauge`: Slider UI 참조
- `kanaMaterial`: 글리치 셰이더가 적용된 매터리얼

**연동 로직:**
```csharp
동기화율 ↑ → 글리치 강도 ↓ (선형 보간)
glitchIntensity = 1.0 - (currentSync / 100)
```

---

### 4️⃣ **TerminalManager.cs**
터미널 인터페이스의 텍스트 출력 담당

**주요 기능:**
- `TypeLine()`: 타이핑 효과로 한 줄씩 출력 (Coroutine)
- `AddInstantLine()`: 즉시 한 줄 출력 (스킵 기능용)
- `ClearTerminal()`: 터미널 초기화
- 자동 스크롤 처리 (ScrollRect 연동)

**Inspector 설정:**
- `terminalText`: TextMeshProUGUI 참조
- `scrollRect`: ScrollRect 참조
- `typingSpeed`: 글자당 대기 시간 (기본 0.02초)

**색상 시스템:**
- 초록색: 일반 로그
- 빨간색: 에러 메시지
- 노란색: 문제 코드
- 청록색: 성공 메시지
- 흰색: 시스템 메시지

---

## 🎨 Unity 설정 가이드

### **1단계: Scene 구성**
1. Canvas 생성 (Canvas Scaler: Scale With Screen Size, 1920x1080)
2. Empty GameObject "GameManager" 생성
3. GameManager, SyncManager, TerminalManager 컴포넌트 추가

### **2단계: UI 구성**
```
Canvas/
├── TerminalScrollView (Scroll View)
│   └── Viewport/Content/TerminalText (TextMeshPro)
│
├── SyncGauge (Slider)
│
└── ChoicePanel (Panel) [시작 시 비활성화]
    ├── ChoiceButton1 (Button - TextMeshPro)
    ├── ChoiceButton2
    ├── ChoiceButton3
    └── ChoiceButton4
```

### **3단계: Inspector 연결**
- TerminalManager: `terminalText`, `scrollRect` 드래그
- SyncManager: `syncGauge`, `kanaMaterial` 드래그
- GameManager: 모든 참조 드래그 (terminal, sync, choicePanel, choiceButtons[])

### **4단계: StageData 생성**
- 우클릭 → Create → KanaRebuild → StageData
- Stage_01 ~ Stage_04 생성 및 데이터 입력
- GameManager의 `stages[]` 배열에 드래그

---

## 📝 StageData 샘플 예시

### **Stage_01 - 연산자 오류**
```
Stage Index: 0
Target Sync Rate: 25

Background Logs:
- "Initializing core modules..."
- "Loading player data..."

Problem Code:
"if (player.health = 0) { Die(); }"

Choices:
- "== 대신 = 사용 (비교 연산자)"
- "!= 대신 = 사용"
- "< 대신 = 사용"

Correct Choice Index: 0
Success Message: "비교 연산자 수정 완료!"
Fail Message: "대입과 비교를 구분하세요."
```

### **Stage_02 - 세미콜론 누락**
```
Problem Code:
"int score = 100
 print(score);"

Choices:
- "첫 줄 끝에 ; 추가"
- "둘째 줄 끝에 ; 제거"
- "괄호 추가"

Correct Choice Index: 0
```

---

## 🚀 빌드 및 실행

1. Unity에서 Play 버튼 클릭
2. 터미널에 로그가 타이핑되는지 확인
3. 선택지 클릭 시 게이지 변화 확인
4. 4개 스테이지 모두 클리어 시 "KANA FULLY RESTORED" 메시지

---

## 📌 다음 작업 예정

### Phase 1: 기본 게임플레이 (현재 완료)
- [x] 스크립트 구조 설계
- [x] 터미널 시스템
- [x] 선택지 시스템
- [x] 동기화 게이지

### Phase 2: 비주얼 향상
- [ ] 카나 캐릭터 이미지 + 글리치 셰이더
- [ ] 터미널 UI 디자인 개선 (CRT 효과)
- [ ] 배경 애니메이션
- [ ] 사운드 이펙트 (타이핑, 성공/실패)

### Phase 3: 컨텐츠 확장
- [ ] 10개 이상의 스테이지 제작
- [ ] 난이도별 분류 (초급/중급/고급)
- [ ] 힌트 시스템
- [ ] 스테이지 해금 시스템

### Phase 4: 스토리 추가
- [ ] 프롤로그/에필로그 씬
- [ ] 스테이지별 대화 이벤트
- [ ] 엔딩 분기

---

## 📖 참고 자료

- Unity TextMesh Pro 문서: https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0
- Unity UI 이벤트 시스템: https://docs.unity3d.com/Manual/EventSystem.html
- ScriptableObject 패턴: https://unity.com/how-to/architect-game-code-scriptable-objects

---

## 🔄 버전 히스토리

### v0.1.0 (2026-01-25)
- 초기 프로젝트 구조 생성
- 4대 핵심 스크립트 구현
- 기본 게임플레이 루프 완성

---

## 📧 문의

프로젝트 관련 문의사항이 있으시면 이슈를 등록해주세요.

---

**Made with ❤️ using Unity & Cursor AI**
