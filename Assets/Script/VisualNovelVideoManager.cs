using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

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
}

public class VisualNovelVideoManager : MonoBehaviour
{
    [Header("--- 파일 설정 ---")]
    public string csvFileName = "Chapter1_new";

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

    [Header("--- 디자인 설정 ---")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.8f);
    public Color monologueColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

    private List<DialogueLine> scenarioData = new List<DialogueLine>();
    private int currentLineIndex = 0;
    private string currentVideoName = ""; 
    private string playerName = "주인공";

    [Header("Face Detection")]
    public HeadDirectionDetector headDetector; 
    private bool isWaitingForFace = false;    
    public GameObject guideTextObject;

    void Start()
    {
        // 1. 초기화 및 안전 장치
        if (characterVideoPlayer == null || characterDisplay == null) return;

        if (characterDisplay.texture == null && characterVideoPlayer.targetTexture != null)
        {
            characterDisplay.texture = characterVideoPlayer.targetTexture;
        }

        if (dialogueBoxImage != null) dialogueBoxImage.gameObject.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);
        characterDisplay.gameObject.SetActive(false);

        playerName = PlayerPrefs.GetString("PlayerName", "주인공");
        LoadDialogueFromCSV(csvFileName);

        // =========================================================
        // ★ [핵심] 게임에서 돌아왔는지 확인하는 로직
        // =========================================================
        if (PlayerPrefs.GetString("IsReturningFromGame") == "TRUE")
        {
            // A. 참참참 게임처럼 승패 결과가 있는 경우
            if (PlayerPrefs.HasKey("GameResult"))
            {
                string result = PlayerPrefs.GetString("GameResult");
                if (result == "WIN")
                {
                    int winID = PlayerPrefs.GetInt("WinTargetID", 0);
                    JumpToID(winID);
                }
                else // LOSE
                {
                    int loseID = PlayerPrefs.GetInt("LoseTargetID", 0);
                    JumpToID(loseID);
                }
                PlayerPrefs.DeleteKey("GameResult"); // 결과 사용 후 삭제
            }
            // B. 리듬게임/눈싸움처럼 그냥 다음 줄로 넘어가는 경우
            else
            {
                int lastIndex = PlayerPrefs.GetInt("SavedLineIndex", 0);
                currentLineIndex = lastIndex + 1; // 저장된 위치의 다음 줄
                DisplayCurrentLine();
            }

            // 복귀 처리 완료했으므로 플래그 삭제
            PlayerPrefs.DeleteKey("IsReturningFromGame");
        }
        else
        {
            // 처음 시작
            StartDialogue();
        }
    }

    void Update()
    {
        // 1. 얼굴 감지 대기
        if (isWaitingForFace)
        {
            if (headDetector != null && headDetector.currentDistance <= 0.3f) 
            {
                Debug.Log("얼굴 인식됨! 다음으로 넘어갑니다.");
                isWaitingForFace = false; 
                DisplayNextSentence();    
            }
            return; 
        }

        // 2. UI 터치 막기
        if ((choicePanel != null && choicePanel.activeSelf) ||
            (nameInputPanel != null && nameInputPanel.activeSelf)) return;

        // 3. 입력 감지
        bool isClick = Pointer.current != null && Pointer.current.press.wasPressedThisFrame;
        bool isSpace = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (isClick || isSpace)
        {
            DisplayNextSentence();
        }
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

            scenarioData.Add(dl);
        }
    }

    public void StartDialogue()
    {
        currentLineIndex = 0;
        DisplayCurrentLine();
    }

    public void DisplayNextSentence()
    {
        if (currentLineIndex < scenarioData.Count)
        {
            DialogueLine currentLine = scenarioData[currentLineIndex];
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
                if (choiceButton1 != null) { choiceButton1.onClick.RemoveAllListeners(); choiceButton1.onClick.AddListener(() => { choicePanel.SetActive(false); JumpToID(line.opt1TargetID); }); }
                if (choiceButton2 != null) { choiceButton2.onClick.RemoveAllListeners(); choiceButton2.onClick.AddListener(() => { choicePanel.SetActive(false); JumpToID(line.opt2TargetID); }); }
            }
            return;
        }

        // ==========================================
        // ★ [핵심] 씬 이동 (게임 실행) 로직 수정
        // ==========================================
        if (!string.IsNullOrEmpty(line.nextScene) 
            && !line.nextScene.StartsWith("JUMP_")
            && line.nextScene != "WAIT_FACE")
        {
            // 1. 현재 몇 번째 줄인지 기억 (책갈피)
            PlayerPrefs.SetInt("SavedLineIndex", currentLineIndex);
            
            // 2. 돌아올 때를 위해 승/패 분기 ID 미리 저장 (필요한 경우)
            PlayerPrefs.SetInt("WinTargetID", line.opt1TargetID); 
            PlayerPrefs.SetInt("LoseTargetID", line.opt2TargetID);
            
            // 3. "나 게임하러 간다" 표시
            PlayerPrefs.SetString("IsReturningFromGame", "TRUE");

            PlayerPrefs.Save();

            // 4. 씬 이동
            SceneManager.LoadScene(line.nextScene);
            return; 
        }

        if (line.nextScene == "WAIT_FACE")
        {
            isWaitingForFace = true;
            if(guideTextObject != null) guideTextObject.SetActive(true);
        }
        else
        {
            isWaitingForFace = false;
            if(guideTextObject != null) guideTextObject.SetActive(false);
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
        else
        {
            Debug.LogError($"🚨 영상을 찾을 수 없습니다: {path}");
        }
    }
}