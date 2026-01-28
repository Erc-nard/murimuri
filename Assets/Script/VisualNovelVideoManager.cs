using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

[System.Serializable]
public struct BGMData
{
    public string name;
    public AudioClip clip;
}

[System.Serializable]
public class DialogueLine
{
    public int id;
    public string speakerName;
    public string dialogueText;
    public string videoName;
    public string nextScene;
    public bool isMonologue;
    public string opt1Text;
    public int opt1TargetID;
    public string opt2Text;
    public int opt2TargetID;
    public string bgmName;

    // [추가] 호감도 점수 (CSV 12, 13번째 칸)
    public int opt1Score;
    public int opt2Score;
}

public class VisualNovelVideoManager : MonoBehaviour
{
    [Header("--- 파일 설정 ---")]
    public string csvFileName = "Chapter1_5";

    [Header("--- UI 연결 ---")]
    public VideoPlayer characterVideoPlayer;
    public RawImage characterDisplay;
    public Text nameText;
    public Text dialogueText;
    public Image dialogueBoxImage;

    [Header("--- 이름 입력 UI ---")]
    public GameObject nameInputPanel;
    public InputField nameInputField;

    [Header("--- 선택지 UI ---")]
    public GameObject choicePanel;
    public Button choiceButton1;
    public Text choiceButton1Text;
    public Button choiceButton2;
    public Text choiceButton2Text;

    [Header("--- 오디오 설정 ---")]
    public AudioSource bgmAudioSource;
    public AudioMixerGroup bgmOutput;
    public List<BGMData> bgmList;
    private Dictionary<string, AudioClip> bgmDict = new Dictionary<string, AudioClip>();

    [Header("--- 디자인 설정 ---")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.8f);
    public Color monologueColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

    private List<DialogueLine> scenarioData = new List<DialogueLine>();
    private int currentLineIndex = 0;
    private string currentVideoName = "";
    private string playerName = "주인공";

    // [추가] 호감도 변수
    private int affectionScore = 0;
    private const int MAX_SCORE = 2; // 총 선택지가 2개라고 가정했을 때 만점

    [Header("Face Detection")]
    public HeadDirectionDetector headDetector;
    private bool isWaitingForFace = false;
    public GameObject guideTextObject;

    void Start()
    {
        // 1. 오디오 초기화
        if (bgmAudioSource == null)
        {
            bgmAudioSource = GetComponent<AudioSource>();
            if (bgmAudioSource == null) bgmAudioSource = gameObject.AddComponent<AudioSource>();
        }
        bgmAudioSource.loop = true;
        if (bgmOutput != null) bgmAudioSource.outputAudioMixerGroup = bgmOutput;

        bgmDict.Clear();
        foreach (var data in bgmList)
        {
            if (!bgmDict.ContainsKey(data.name)) bgmDict.Add(data.name, data.clip);
        }

        // 2. UI 및 플레이어 초기화
        if (characterVideoPlayer != null && characterDisplay != null)
        {
            if (characterDisplay.texture == null && characterVideoPlayer.targetTexture != null)
                characterDisplay.texture = characterVideoPlayer.targetTexture;
        }

        if (dialogueBoxImage != null) dialogueBoxImage.gameObject.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        if (characterDisplay != null) characterDisplay.gameObject.SetActive(false);

        playerName = PlayerPrefs.GetString("PlayerName", "주인공");

        // [추가] 저장된 호감도 불러오기 (미니게임 다녀와도 유지되도록)
        affectionScore = PlayerPrefs.GetInt("AffectionScore", 0);

        LoadDialogueFromCSV(csvFileName);

        // 3. 저장된 위치 로드 로직
        if (PlayerPrefs.GetString("IsReturningFromGame") == "TRUE")
        {
            if (PlayerPrefs.HasKey("GameResult"))
            {
                string result = PlayerPrefs.GetString("GameResult");
                if (result == "WIN") JumpToID(PlayerPrefs.GetInt("WinTargetID", 0));
                else JumpToID(PlayerPrefs.GetInt("LoseTargetID", 0));
                PlayerPrefs.DeleteKey("GameResult");
            }
            else
            {
                currentLineIndex = PlayerPrefs.GetInt("SavedLineIndex", 0) + 1;
                DisplayCurrentLine();
            }
            PlayerPrefs.DeleteKey("IsReturningFromGame");
        }
        else
        {
            currentLineIndex = PlayerPrefs.GetInt("SavedLineIndex", 0);
            DisplayCurrentLine();
        }
    }

    void Update()
    {
        if (isWaitingForFace)
        {
            bool isFaceClose = false;

            // [추가된 로직] HeadDetector에게 "지금 얼굴 가깝니?"라고 물어봄
            if (headDetector != null)
            {
                isFaceClose = headDetector.isFaceClose; 
            }

            // 키보드 엔터키 입력 확인 (테스트용 비상키)
            bool isEnterPressed = (Keyboard.current != null && Keyboard.current.enterKey.wasPressedThisFrame);

            // [조건] 얼굴이 가까워졌거나(True) OR 엔터키를 눌렀다면 진행
            if (isFaceClose || isEnterPressed)
            {
                Debug.Log("얼굴 인식(또는 키입력) 성공! 다음 대사로 넘어갑니다.");
                isWaitingForFace = false;
                if (guideTextObject != null) guideTextObject.SetActive(false);
                DisplayNextSentence();
            }
            return; // 여기서 리턴해서 아래 클릭 로직이 실행 안 되게 막음
        }

        // 2. 평소 상태 (선택지나 이름 입력 중이면 클릭 방지)
        if ((choicePanel != null && choicePanel.activeSelf) ||
            (nameInputPanel != null && nameInputPanel.activeSelf)) return;

        // 3. 일반 대사 넘기기 (마우스 클릭 or 스페이스바)
        bool isClick = Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        bool isSpace = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (isClick || isSpace) DisplayNextSentence();
    }

    void LoadDialogueFromCSV(string filename)
    {
        scenarioData.Clear();
        TextAsset data = Resources.Load<TextAsset>(filename);

        if (data == null) { Debug.LogError($"파일 없음: {filename}"); return; }

        string[] lines = data.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] row = line.Split(',');
            DialogueLine dl = new DialogueLine();

            int.TryParse(row[0].Trim(), out dl.id);
            dl.speakerName = row[1].Trim();
            dl.dialogueText = row[2].Replace("<comma>", ",").Trim();
            dl.videoName = (row.Length > 3) ? row[3].Trim() : "";
            dl.nextScene = (row.Length > 4) ? row[4].Trim() : "";
            if (row.Length > 5 && row[5].Trim().ToUpper() == "TRUE") dl.isMonologue = true;

            if (row.Length > 7) { dl.opt1Text = row[6].Trim(); int.TryParse(row[7].Trim(), out dl.opt1TargetID); }
            if (row.Length > 9) { dl.opt2Text = row[8].Trim(); int.TryParse(row[9].Trim(), out dl.opt2TargetID); }
            if (row.Length > 10) { dl.bgmName = row[10].Trim(); }

            // [추가] 점수 파싱 (11번, 12번 인덱스)
            if (row.Length > 11) int.TryParse(row[11].Trim(), out dl.opt1Score);
            if (row.Length > 12) int.TryParse(row[12].Trim(), out dl.opt2Score);

            scenarioData.Add(dl);
        }
    }

    public void DisplayNextSentence()
    {
        // 현재 줄의 NextScene이 CHECK_ENDING이면 엔딩 분기 검사
        if (currentLineIndex < scenarioData.Count)
        {
            DialogueLine currentLine = scenarioData[currentLineIndex];

            // [핵심] 엔딩 분기점 처리
            if (currentLine.nextScene == "CHECK_ENDING")
            {
                CheckEndingRoute();
                return;
            }

            if (!string.IsNullOrEmpty(currentLine.nextScene) && currentLine.nextScene.StartsWith("JUMP_"))
            {
                int targetID = int.Parse(currentLine.nextScene.Replace("JUMP_", ""));
                JumpToID(targetID);
                return;
            }
        }
        currentLineIndex++;
        DisplayCurrentLine();
    }

    void DisplayCurrentLine()
    {
        if (currentLineIndex >= scenarioData.Count) return;

        DialogueLine line = scenarioData[currentLineIndex];

        string finalName = line.speakerName.Replace("{PlayerName}", playerName);
        string finalDialogue = line.dialogueText.Replace("{PlayerName}", playerName);

        nameText.text = finalName;
        dialogueText.text = finalDialogue;
        if (dialogueBoxImage != null) dialogueBoxImage.gameObject.SetActive(true);

        if (line.isMonologue)
        {
            dialogueBoxImage.color = monologueColor;
            nameText.gameObject.SetActive(false);
        }
        else
        {
            dialogueBoxImage.color = normalColor;
            nameText.gameObject.SetActive(true);
        }

        PlayBGM(line.bgmName);

        if (!string.IsNullOrEmpty(line.videoName))
        {
            if (currentVideoName != line.videoName)
            {
                if (line.videoName == "HIDE")
                {
                    characterDisplay.gameObject.SetActive(false);
                    characterVideoPlayer.Stop();
                    currentVideoName = "HIDE";
                }
                else
                {
                    PlayVideo(line.videoName);
                }
            }
        }

        if (line.nextScene == "INPUT_NAME")
        {
            if (nameInputPanel != null) { nameInputPanel.SetActive(true); nameInputField.text = ""; }
            return;
        }

        if (line.nextScene == "CHOICE")
        {
            if (choicePanel != null)
            {
                choicePanel.SetActive(true);
                if (choiceButton1Text != null) choiceButton1Text.text = line.opt1Text;
                if (choiceButton2Text != null) choiceButton2Text.text = line.opt2Text;

                // [수정] 버튼 클릭 시 점수 추가 로직 적용
                if (choiceButton1 != null)
                {
                    choiceButton1.onClick.RemoveAllListeners();
                    choiceButton1.onClick.AddListener(() =>
                    {
                        AddScore(line.opt1Score); // 점수 추가
                        choicePanel.SetActive(false);
                        JumpToID(line.opt1TargetID);
                    });
                }
                if (choiceButton2 != null)
                {
                    choiceButton2.onClick.RemoveAllListeners();
                    choiceButton2.onClick.AddListener(() =>
                    {
                        AddScore(line.opt2Score); // 점수 추가
                        choicePanel.SetActive(false);
                        JumpToID(line.opt2TargetID);
                    });
                }
            }
            return;
        }

        if (!string.IsNullOrEmpty(line.nextScene)
            && !line.nextScene.StartsWith("JUMP_")
            && line.nextScene != "WAIT_FACE"
            && line.nextScene != "CHECK_ENDING") // CHECK_ENDING은 위에서 처리함
        {
            PlayerPrefs.SetInt("SavedLineIndex", currentLineIndex);
            PlayerPrefs.SetInt("AffectionScore", affectionScore); // 씬 이동 시 점수 저장
            PlayerPrefs.SetInt("WinTargetID", line.opt1TargetID);
            PlayerPrefs.SetInt("LoseTargetID", line.opt2TargetID);
            PlayerPrefs.SetString("IsReturningFromGame", "TRUE");
            PlayerPrefs.Save();
            SceneManager.LoadScene(line.nextScene);
            return;
        }

        if (line.nextScene == "WAIT_FACE")
        {
            isWaitingForFace = true;
            if (guideTextObject != null) guideTextObject.SetActive(true);
        }
        else
        {
            isWaitingForFace = false;
            if (guideTextObject != null) guideTextObject.SetActive(false);
        }
    }

    // [추가] 점수 추가 함수
    void AddScore(int score)
    {
        affectionScore += score;
        Debug.Log($"현재 호감도: {affectionScore}");
        PlayerPrefs.SetInt("AffectionScore", affectionScore); // 즉시 저장
    }

    // [핵심] 엔딩 분기 처리 함수
    void CheckEndingRoute()
    {
        Debug.Log($"엔딩 분기점 도착! 최종 점수: {affectionScore}");

        // 점수가 2점 이상 (True End) -> 314번으로 이동
        if (affectionScore >= 2)
        {
            Debug.Log(">>> 트루 엔딩 루트 진입");
            JumpToID(314);
        }
        // 점수가 1점 이상 (Normal End) -> 279번으로 이동
        else if (affectionScore >= 1)
        {
            Debug.Log(">>> 노말 엔딩 루트 진입");
            JumpToID(279);
        }
        // 점수가 0점 (Bad End) -> 248번(공포 루트)으로 이동
        else
        {
            Debug.Log(">>> 배드 엔딩 루트 진입");
            JumpToID(248);
        }
    }

    void JumpToID(int targetID)
    {
        for (int i = 0; i < scenarioData.Count; i++)
        {
            if (scenarioData[i].id == targetID)
            {
                currentLineIndex = i;
                DisplayCurrentLine();
                return;
            }
        }
    }

    public void OnNameSubmitted()
    {
        if (nameInputField.text.Length > 0)
        {
            playerName = nameInputField.text;
            PlayerPrefs.SetString("PlayerName", playerName);
            PlayerPrefs.Save();
        }
        nameInputPanel.SetActive(false);
        DisplayNextSentence();
    }

    void PlayVideo(string videoName)
    {
        string path = "Videos/" + videoName;
        VideoClip clip = Resources.Load<VideoClip>(path);
        if (clip != null)
        {
            characterVideoPlayer.clip = clip;
            characterVideoPlayer.isLooping = true;
            characterVideoPlayer.Play();
            characterDisplay.gameObject.SetActive(true);
            currentVideoName = videoName;
        }
    }

    void PlayBGM(string bgmName)
    {
        if (string.IsNullOrEmpty(bgmName)) return;
        if (bgmName.ToUpper() == "STOP" || bgmName.ToUpper() == "NONE")
        {
            bgmAudioSource.Stop();
            return;
        }

        if (bgmDict.ContainsKey(bgmName))
        {
            AudioClip nextClip = bgmDict[bgmName];
            if (bgmAudioSource.clip == nextClip && bgmAudioSource.isPlaying) return;

            bgmAudioSource.clip = nextClip;
            bgmAudioSource.Play();
        }
    }
}