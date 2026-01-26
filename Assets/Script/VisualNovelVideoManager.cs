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
    public string csvFileName = "Chapter1";

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
    private string currentVideoName = ""; // 현재 재생 중인 비디오 이름 기억
    private string playerName = "주인공";

    void Start()
    {
        // 안전 장치: UI 연결 확인
        if (characterVideoPlayer == null || characterDisplay == null)
        {
            Debug.LogError("🚨 VideoPlayer나 RawImage가 연결되지 않았습니다!");
            return;
        }

        // RawImage에 텍스처 자동 연결
        if (characterDisplay.texture == null && characterVideoPlayer.targetTexture != null)
        {
            characterDisplay.texture = characterVideoPlayer.targetTexture;
        }

        // 초기화
        if (dialogueBoxImage != null) dialogueBoxImage.gameObject.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);

        // 처음엔 영상 화면 끄기
        characterDisplay.gameObject.SetActive(false);

        playerName = PlayerPrefs.GetString("PlayerName", "주인공");

        LoadDialogueFromCSV(csvFileName);

        int targetID = PlayerPrefs.GetInt("JumpTargetID", -1);
        if (targetID != -1)
        {
            PlayerPrefs.DeleteKey("JumpTargetID");
            JumpToID(targetID);
        }
        else
        {
            StartDialogue();
        }
    }

    void Update()
    {
        if ((choicePanel != null && choicePanel.activeSelf) ||
            (nameInputPanel != null && nameInputPanel.activeSelf)) return;

        bool isClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
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

        if (data == null) { Debug.LogError($"Resources 폴더에 파일 없음: {filename}"); return; }

        string[] lines = data.text.Split('\n');
        for (int i = 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (string.IsNullOrWhiteSpace(line)) continue;

            string[] row = line.Split(',');
            DialogueLine dl = new DialogueLine();

            // 파싱 로직
            int.TryParse(row[0].Trim(), out dl.id);
            dl.speakerName = row[1].Trim();
            dl.dialogueText = row[2].Replace("<comma>", ",").Trim();
            dl.videoName = (row.Length > 3) ? row[3].Trim() : ""; // 비디오 이름
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

        if (line.isMonologue) { dialogueBoxImage.color = monologueColor; nameText.gameObject.SetActive(false); }
        else { dialogueBoxImage.color = normalColor; nameText.gameObject.SetActive(true); }

        // ==========================================
        // ★ 비디오 재생 핵심 로직 (수정됨) ★
        // ==========================================

        // 1. CSV 비디오 칸이 비어있지 않은 경우에만 비디오 변경 시도
        if (!string.IsNullOrEmpty(line.videoName))
        {
            // 2. "현재 재생 중인 비디오"와 "새로운 비디오"가 다를 때만 교체
            // (같으면 PlayVideo를 호출하지 않으므로 기존 영상이 계속 재생됨)
            if (currentVideoName != line.videoName)
            {
                if (line.videoName == "HIDE")
                {
                    characterDisplay.gameObject.SetActive(false); // 화면 끄기
                    characterVideoPlayer.Stop();
                    currentVideoName = "HIDE";
                }
                else
                {
                    PlayVideo(line.videoName);
                }
            }
        }
        // 만약 line.videoName이 비어있다면("") 아무것도 안 함 -> 이전 영상 유지됨!

        // ==========================================

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

        if (!string.IsNullOrEmpty(line.nextScene) && !line.nextScene.StartsWith("JUMP_"))
        {
            SceneManager.LoadScene(line.nextScene);
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

    // ★ 비디오 파일 로드 함수 ★
    void PlayVideo(string videoName)
    {
        // 1. Assets/Resources/Videos/ 폴더 안의 파일을 찾음
        string path = "Videos/" + videoName;

        // 2. 파일 로드
        VideoClip clip = Resources.Load<VideoClip>(path);

        if (clip != null)
        {
            characterVideoPlayer.clip = clip;
            characterVideoPlayer.isLooping = true; // ★ 무한 반복 설정
            characterVideoPlayer.Play();
            characterDisplay.gameObject.SetActive(true);
            currentVideoName = videoName; // 현재 비디오 이름 갱신
        }
        else
        {
            Debug.LogError($"🚨 영상을 찾을 수 없습니다! 경로 확인: Assets/Resources/Videos/{videoName}");
        }
    }
}