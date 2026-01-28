using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace KanaRebuild
{
    public class GameManager : MonoBehaviour
    {
        public StageData[] stages;
        public TerminalManager terminal;
        public SyncManager sync;
        public GameObject choicePanel;
        public Button[] choiceButtons;
        
        [Header("Intro Video")]
        public bool hasIntroVideo = false; // 인트로 영상 사용 여부
        
        [Header("Stage Videos")]
        public StageVideoManager stageVideoManager; // 스테이지별 영상 매니저

        private int _currentStageIndex = 0;
        private float _timer = 0f;
        private bool _isTimerRunning = false;
        private Coroutine _timerCoroutine;
        private bool _gameStarted = false;

        void Start()
        {
            // 인트로 영상이 있으면 게임 시작을 미룸
            if (!hasIntroVideo)
            {
                StartGame();
            }
        }

        // 외부(IntroVideoManager)에서 호출
        public void StartGame()
        {
            if (_gameStarted) return;
            _gameStarted = true;
            
            Debug.Log("🎮 게임 시작!");
            
            // 중복 EventSystem 체크 및 제거
            CheckEventSystem();
            
            // choiceButtons 배열 체크 및 자동 설정
            CheckChoiceButtons();
            
            // Stages 배열 체크
            if (stages == null || stages.Length == 0)
            {
                Debug.LogError("❌ Stages 배열이 비어있습니다! Inspector에서 StageData를 추가하세요!");
                return;
            }
            Debug.Log($"✅ 총 스테이지 수: {stages.Length}개");
            
            choicePanel.SetActive(false);
            StartCoroutine(PlayStage(_currentStageIndex));
        }
        
        void CheckChoiceButtons()
        {
            Debug.Log("=== Choice Buttons 체크 ===");
            
            if (choiceButtons == null || choiceButtons.Length == 0)
            {
                Debug.LogWarning("⚠️ choiceButtons 배열이 비어있습니다! 자동으로 찾는 중...");
                
                if (choicePanel != null)
                {
                    // ChoicePanel 아래의 모든 버튼을 찾기
                    choiceButtons = choicePanel.GetComponentsInChildren<Button>(true);
                    Debug.Log($"✅ {choiceButtons.Length}개의 버튼을 자동으로 찾았습니다.");
                    
                    for (int i = 0; i < choiceButtons.Length; i++)
                    {
                        Debug.Log($"  버튼 {i}: {choiceButtons[i].gameObject.name}");
                    }
                }
                else
                {
                    Debug.LogError("❌ ChoicePanel이 연결되지 않았습니다!");
                }
            }
            else
            {
                Debug.Log($"✅ choiceButtons 배열에 {choiceButtons.Length}개의 버튼이 설정되어 있습니다.");
                
                for (int i = 0; i < choiceButtons.Length; i++)
                {
                    if (choiceButtons[i] == null)
                    {
                        Debug.LogError($"❌ choiceButtons[{i}]이 null입니다! Inspector에서 연결하세요.");
                    }
                    else
                    {
                        Debug.Log($"  버튼 {i}: {choiceButtons[i].gameObject.name}");
                    }
                }
            }
        }

        void CheckEventSystem()
        {
            EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            
            if (eventSystems.Length > 1)
            {
                Debug.LogWarning($"⚠️ EventSystem이 {eventSystems.Length}개 발견됨! 하나만 남기고 삭제 중...");
                
                // 첫 번째 것만 유지하고 나머지 삭제
                for (int i = 1; i < eventSystems.Length; i++)
                {
                    Debug.Log($"삭제: {eventSystems[i].gameObject.name}");
                    Destroy(eventSystems[i].gameObject);
                }
                
                Debug.Log($"✅ EventSystem 정리 완료. 남은 개수: 1");
            }
            else if (eventSystems.Length == 0)
            {
                Debug.LogError("❌ EventSystem이 없습니다! GameObject > UI > Event System을 추가하세요!");
            }
            else
            {
                Debug.Log("✅ EventSystem 개수 정상 (1개)");
            }
        }

        IEnumerator PlayStage(int index)
        {
            StageData data = stages[index];
            
            // 0. 스테이지 Normal 영상 재생 (루프)
            if (stageVideoManager != null)
            {
                stageVideoManager.PlayNormalVideo(index);
            }
            
            // 1. 배경 로그 출력
            foreach (string log in data.backgroundLogs)
            {
                yield return terminal.TypeLine(log, Color.green);
                yield return new WaitForSeconds(0.2f);
            }

            // 2. 문제 제시
            yield return terminal.TypeLine("CRITICAL ERROR FOUND:", Color.red);
            yield return terminal.TypeLine(data.problemCode, Color.yellow);
            
            // 3. 1초 대기
            yield return new WaitForSeconds(1f);
            
            // 4. 선택지 활성화 및 타이머 시작
            ShowChoices(data);
            
            // 5. 버튼 표시 후 스크롤
            terminal.ScrollToBottom();
            
            // 6. 타이머 시작
            _timerCoroutine = StartCoroutine(TimerRoutine());
        }

        void ShowChoices(StageData data)
        {
            Debug.Log("=== 선택지 표시 시작 ===");
            Debug.Log($"choicePanel null? {choicePanel == null}");
            Debug.Log($"choiceButtons null? {choiceButtons == null}");
            Debug.Log($"choiceButtons 길이: {choiceButtons?.Length}");
            
            // Canvas 및 Graphic Raycaster 상세 체크
            Canvas canvas = choicePanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"📺 Canvas 렌더 모드: {canvas.renderMode}");
                if (canvas.renderMode != RenderMode.ScreenSpaceOverlay && canvas.worldCamera == null)
                {
                    Debug.LogError("❌ Canvas가 Camera 모드인데 Event Camera가 설정되지 않았습니다!");
                }
                
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null)
                {
                    Debug.Log($"✅ GraphicRaycaster 발견");
                    Debug.Log($"  - Ignore Reversed Graphics: {raycaster.ignoreReversedGraphics}");
                    Debug.Log($"  - Blocking Objects: {raycaster.blockingObjects}");
                    
                    if (raycaster.blockingObjects != GraphicRaycaster.BlockingObjects.None)
                    {
                        Debug.LogWarning($"⚠️ Blocking Objects가 '{raycaster.blockingObjects}'로 설정되어 있습니다. 'None'으로 변경하는 것을 권장합니다.");
                    }
                }
                else
                {
                    Debug.LogError("❌ Canvas에 GraphicRaycaster가 없습니다!");
                }
            }
            
            // ChoicePanel의 Canvas Group 체크
            CanvasGroup panelGroup = choicePanel.GetComponent<CanvasGroup>();
            if (panelGroup != null)
            {
                Debug.Log($"📦 ChoicePanel Canvas Group 발견:");
                Debug.Log($"  - Interactable: {panelGroup.interactable}");
                Debug.Log($"  - Block Raycasts: {panelGroup.blocksRaycasts}");
                
                if (!panelGroup.interactable || !panelGroup.blocksRaycasts)
                {
                    Debug.LogWarning("⚠️ Canvas Group 설정 문제 - 자동 수정 중...");
                    panelGroup.interactable = true;
                    panelGroup.blocksRaycasts = true;
                }
            }
            
            // EventSystem 체크
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogError("❌ EventSystem이 씬에 없습니다! GameObject > UI > Event System을 추가하세요!");
            }
            else
            {
                Debug.Log($"✅ EventSystem 존재: {eventSystem.gameObject.name}");
            }
            
            choicePanel.SetActive(true);
            
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                if (choiceButtons[i] == null)
                {
                    Debug.LogError($"버튼 {i}이(가) null입니다!");
                    continue;
                }
                
                if (i < data.choices.Length)
                {
                    choiceButtons[i].gameObject.SetActive(true);
                    int index = i; // 클로저 이슈 방지
                    
                    Debug.Log($"버튼 {index} 설정 중... 텍스트: {data.choices[i]}");
                    Debug.Log($"버튼 {index} Interactable: {choiceButtons[i].interactable}");
                    Debug.Log($"버튼 {index} RaycastTarget: {choiceButtons[i].GetComponent<Image>()?.raycastTarget}");
                    
                    // 버튼을 명시적으로 활성화
                    choiceButtons[i].interactable = true;
                    
                    // Image 컴포넌트의 Raycast Target 확인
                    Image buttonImage = choiceButtons[i].GetComponent<Image>();
                    if (buttonImage != null && !buttonImage.raycastTarget)
                    {
                        Debug.LogWarning($"⚠️ 버튼 {index}의 Raycast Target이 꺼져있습니다. 켜는 중...");
                        buttonImage.raycastTarget = true;
                    }
                    
                    // TextMeshPro 또는 Legacy Text 모두 지원
                    TextMeshProUGUI tmpText = choiceButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                    if (tmpText != null)
                    {
                        tmpText.text = data.choices[i];
                        Debug.Log($"버튼 {index} TextMeshPro 설정 완료");
                    }
                    else
                    {
                        UnityEngine.UI.Text legacyText = choiceButtons[i].GetComponentInChildren<UnityEngine.UI.Text>();
                        if (legacyText != null)
                        {
                            legacyText.text = data.choices[i];
                            Debug.Log($"버튼 {index} Legacy Text 설정 완료");
                        }
                        else
                        {
                            Debug.LogWarning($"버튼 {index}에 텍스트 컴포넌트가 없습니다!");
                        }
                    }
                    
                    // 기존 리스너 완전 제거
                    choiceButtons[i].onClick.RemoveAllListeners();
                    
                    // 새 리스너 추가 (클로저 주의!)
                    int capturedIndex = index; // 클로저를 위한 로컬 변수
                    bool isCorrect = (capturedIndex == data.correctChoiceIndex);
                    
                    // 버튼 이벤트 강제 재설정
                    Button btn = choiceButtons[i];
                    btn.onClick.AddListener(() => {
                        Debug.Log($"★★★★★★★★★★★★★★★★★★★★★★");
                        Debug.Log($"★★★ 버튼 {capturedIndex} 클릭됨! ★★★");
                        Debug.Log($"정답 인덱스: {data.correctChoiceIndex}");
                        Debug.Log($"클릭한 인덱스: {capturedIndex}");
                        Debug.Log($"정답 여부: {isCorrect}");
                        Debug.Log($"★★★★★★★★★★★★★★★★★★★★★★");
                        
                        OnChoiceSelected(isCorrect, data);
                    });
                    
                    // 리스너 등록 확인
                    Debug.Log($"버튼 {index} onClick 리스너 등록 완료");
                }
                else
                {
                    choiceButtons[i].gameObject.SetActive(false);
                }
            }
            
            Debug.Log("=== 선택지 표시 완료 ===");
        }

        // 타이머 시스템 (10초 후부터 게이지 감소)
        IEnumerator TimerRoutine()
        {
            _timer = 0f;
            _isTimerRunning = true;
            
            // 10초까지는 여유 시간
            while (_timer < 10f && _isTimerRunning)
            {
                _timer += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log("⏱️ 10초 경과! 게이지 감소 시작");
            
            // 15초 이후부터 1초마다 5% 감소 + 깜박임
            while (_isTimerRunning)
            {
                yield return new WaitForSeconds(1f);
                if (_isTimerRunning)
                {
                    Debug.Log("⏱️ 타임 페널티 -5%");
                    sync.ApplyTimePenalty(5f); // 1초마다 5% 감소 + 깜박임
                    
                    // 게임 오버 체크
                    if (sync.IsGameOver())
                    {
                        Debug.Log(">>> 타임 페널티로 게임 오버! Death 영상 재생");
                        _isTimerRunning = false;
                        choicePanel.SetActive(false);
                        
                        // Death 영상 재생
                        if (stageVideoManager != null)
                        {
                            bool videoComplete = false;
                            yield return stageVideoManager.PlayDeathVideo(_currentStageIndex, () => videoComplete = true);
                            
                            while (!videoComplete)
                            {
                                yield return null;
                            }
                        }
                        
                        yield return terminal.TypeLine("> SYSTEM FAILURE. KANA CORRUPTED.", Color.red);
                        yield return new WaitForSeconds(2f);
                        yield return terminal.TypeLine("> Restarting in 3...", Color.white);
                        yield return new WaitForSeconds(1f);
                        yield return terminal.TypeLine("> 2...", Color.white);
                        yield return new WaitForSeconds(1f);
                        yield return terminal.TypeLine("> 1...", Color.white);
                        yield return new WaitForSeconds(1f);
                        
                        // 씬 재시작
                        Debug.Log("🔄 게임 재시작!");
                        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                        yield break;
                    }
                }
            }
        }

        void OnChoiceSelected(bool isCorrect, StageData data)
        {
            Debug.Log($"==========================================");
            Debug.Log($"OnChoiceSelected 호출됨!");
            Debug.Log($"정답 여부: {isCorrect}");
            Debug.Log($"StageData null? {data == null}");
            Debug.Log($"==========================================");
            
            choicePanel.SetActive(false);
            Debug.Log("ChoicePanel 비활성화됨");
            
            if (isCorrect)
            {
                // 정답일 때만 타이머 중지
                _isTimerRunning = false;
                if (_timerCoroutine != null)
                {
                    StopCoroutine(_timerCoroutine);
                    Debug.Log("✅ 정답! 타이머 중지됨");
                }
                
                Debug.Log("✅ 정답! SuccessRoutine 시작!");
                StartCoroutine(SuccessRoutine(data));
            }
            else
            {
                // 오답일 때는 타이머 계속 진행 (중지하지 않음)
                Debug.Log("❌ 오답! 타이머는 계속 진행됨");
                Debug.Log("❌ 오답! FailRoutine 시작!");
                StartCoroutine(FailRoutine(data));
            }
        }

        IEnumerator SuccessRoutine(StageData data)
        {
            Debug.Log(">>> SuccessRoutine 코루틴 시작");
            
            // 정답 시 25% 증가
            Debug.Log(">>> 게이지 증가 시작");
            sync.OnCorrectAnswer();
            
            Debug.Log(">>> 성공 메시지 출력 시작");
            yield return terminal.TypeLine(data.successMessage, Color.cyan);
            
            int previousStageIndex = _currentStageIndex;
            _currentStageIndex++;
            Debug.Log($">>> 스테이지 증가: {previousStageIndex} → {_currentStageIndex}");
            Debug.Log($">>> 총 스테이지 수: {stages.Length}");
            Debug.Log($">>> 다음 스테이지 있음? {_currentStageIndex < stages.Length}");
            
            yield return new WaitForSeconds(2f);
            
            // 게임 오버 체크
            if (sync.IsGameOver())
            {
                Debug.Log(">>> 게임 오버!");
                yield return terminal.TypeLine("SYSTEM FAILURE. KANA CORRUPTED.", Color.red);
                yield break;
            }
            
            // 다음 스테이지가 있는지 확인
            if (_currentStageIndex < stages.Length)
            {
                Debug.Log(">>> 정답 점프 영상 재생 + 하얀 페이드");
                
                // 정답 영상 재생 (점프 + 3초 대기 + 하얀 페이드)
                if (stageVideoManager != null)
                {
                    bool videoComplete = false;
                    yield return stageVideoManager.PlayCorrectVideo(previousStageIndex, () => videoComplete = true);
                    
                    // 영상 완료 대기
                    while (!videoComplete)
                    {
                        yield return null;
                    }
                }
                
                Debug.Log(">>> 하얀 페이드 완료 - 터미널 클리어 후 다음 스테이지 시작");
                
                // 터미널 완전 초기화 (메시지 없이 바로 클리어)
                terminal.ClearTerminal();
                
                // 다음 스테이지 즉시 시작 (Normal 영상이 바로 재생됨)
                yield return PlayStage(_currentStageIndex);
            }
            else
            {
                Debug.Log(">>> 마지막 스테이지 정답! 점프 영상 재생");
                Debug.Log($"StageVideoManager 연결됨? {stageVideoManager != null}");
                
                // 마지막 스테이지 정답 영상 (점프 + 3초 대기 + 하얀 페이드)
                if (stageVideoManager != null)
                {
                    Debug.Log("Correct 영상 재생 시작...");
                    bool videoComplete = false;
                    yield return stageVideoManager.PlayCorrectVideo(previousStageIndex, () => videoComplete = true);
                    
                    while (!videoComplete)
                    {
                        yield return null;
                    }
                    Debug.Log("Correct 영상 완료!");
                }
                else
                {
                    Debug.LogError("❌ StageVideoManager가 연결되지 않았습니다! GameManager Inspector에서 연결하세요!");
                }
                
                Debug.Log(">>> 하얀 페이드 완료 - 엔딩 영상 재생");
                
                // 엔딩 영상 재생
                if (stageVideoManager != null)
                {
                    Debug.Log("엔딩 영상 재생 시작...");
                    bool endingComplete = false;
                    yield return stageVideoManager.PlayEndingVideo(() => endingComplete = true);
                    
                    while (!endingComplete)
                    {
                        yield return null;
                    }
                    Debug.Log("엔딩 영상 완료!");
                }
                else
                {
                    Debug.LogError("❌ StageVideoManager가 연결되지 않았습니다!");
                }
                
                // 마지막 스테이지 클리어 - 엔딩 연출
                Debug.Log(">>> 모든 스테이지 클리어! 엔딩 연출 시작");
                
                yield return new WaitForSeconds(1f);
                yield return terminal.TypeLine("", Color.white);
                yield return terminal.TypeLine("> ================================", Color.cyan);
                yield return terminal.TypeLine("> ALL MODULES RESTORED", Color.cyan);
                yield return terminal.TypeLine("> KANA SYNCHRONIZATION: 100%", Color.cyan);
                yield return terminal.TypeLine("> ================================", Color.cyan);
                yield return new WaitForSeconds(1f);
                
                yield return terminal.TypeLine("> System status: ONLINE", Color.green);
                yield return terminal.TypeLine("> Core integrity: STABLE", Color.green);
                yield return terminal.TypeLine("> Memory sectors: RECOVERED", Color.green);
                yield return new WaitForSeconds(1f);
                
                yield return terminal.TypeLine("", Color.white);
                yield return terminal.TypeLine("> KANA: \"Thank you... I'm back.\"", Color.white);
                yield return new WaitForSeconds(1f);
                
                yield return terminal.TypeLine("", Color.white);
                yield return terminal.TypeLine("> === RESTORATION COMPLETE ===", Color.cyan);
                
                Debug.Log(">>> 게임 종료!");
                
                // 엔딩 연출 후 원래 대화 씬으로 복귀
                yield return new WaitForSeconds(2f);
                Debug.Log(">>> PlayScene으로 복귀");
                SceneManager.LoadScene("PlayScene");
            }
        }

        IEnumerator FailRoutine(StageData data)
        {
            Debug.Log(">>> FailRoutine 코루틴 시작");
            
            // 오답 시 15% 감소 + 깜박임
            Debug.Log(">>> 게이지 감소 시작");
            sync.OnWrongAnswer();
            
            Debug.Log(">>> 실패 메시지 출력 시작");
            yield return terminal.TypeLine(data.failMessage, Color.red);
            
            yield return new WaitForSeconds(1f);
            
            // 게임 오버 체크
            if (sync.IsGameOver())
            {
                Debug.Log(">>> 게임 오버! 타이머 중지 및 Death 영상 재생");
                
                // 타이머 중지
                _isTimerRunning = false;
                if (_timerCoroutine != null)
                {
                    StopCoroutine(_timerCoroutine);
                }
                
                // Death 영상 재생
                if (stageVideoManager != null)
                {
                    bool videoComplete = false;
                    yield return stageVideoManager.PlayDeathVideo(_currentStageIndex, () => videoComplete = true);
                    
                    while (!videoComplete)
                    {
                        yield return null;
                    }
                }
                
                yield return terminal.TypeLine("SYSTEM FAILURE. KANA CORRUPTED.", Color.red);
                yield return new WaitForSeconds(2f);
                yield return terminal.TypeLine("> Restarting in 3...", Color.white);
                yield return new WaitForSeconds(1f);
                yield return terminal.TypeLine("> 2...", Color.white);
                yield return new WaitForSeconds(1f);
                yield return terminal.TypeLine("> 1...", Color.white);
                yield return new WaitForSeconds(1f);
                
                // 씬 재시작
                Debug.Log("🔄 게임 재시작!");
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                yield break;
            }
            
            Debug.Log(">>> 오답 영상 재생");
            
            // Wrong 영상 재생 후 Normal로 복귀
            if (stageVideoManager != null)
            {
                bool videoComplete = false;
                yield return stageVideoManager.PlayWrongVideo(_currentStageIndex, () => videoComplete = true);
                
                while (!videoComplete)
                {
                    yield return null;
                }
            }
            
            // 다시 선택지 표시 (타이머는 계속 진행 중)
            Debug.Log(">>> 선택지 다시 표시 (타이머는 계속 진행)");
            ShowChoices(data);
            terminal.ScrollToBottom();
            // 타이머는 이미 진행 중이므로 다시 시작하지 않음
        }
    }
}
