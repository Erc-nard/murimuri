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

    // 선택지 데이터
    public string opt1Text;
    public int opt1TargetID;
    public string opt2Text;
    public int opt2TargetID;
}

public class VisualNovelVideoManager : MonoBehaviour
{
    [Header("--- UI 연결 ---")]
    public VideoPlayer characterVideoPlayer;
    public RawImage characterDisplay;
    public Text nameText;
    public Text dialogueText;
    public Image dialogueBoxImage;

    [Header("--- 이름 입력 UI ---")]
    public GameObject nameInputPanel;
    public InputField nameInputField;

    [Header("--- 선택지 UI (범용) ---")]
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

    void Start()
    {
        // UI 초기화
        if (dialogueBoxImage != null) dialogueBoxImage.gameObject.SetActive(false);
        if (choicePanel != null) choicePanel.SetActive(false);
        if (nameInputPanel != null) nameInputPanel.SetActive(false);

        playerName = PlayerPrefs.GetString("PlayerName", "주인공");

        // 1. CSV 파일 로드
        string filename = "Chapter1";
        LoadDialogueFromCSV(filename);

        // 2. ★ 미니게임 복귀 체크 (핵심 기능)
        // 미니게임에서 이기거나 져서 돌아왔을 때, 지정된 ID로 점프합니다.
        int targetID = PlayerPrefs.GetInt("JumpTargetID", -1);

        if (targetID != -1)
        {
            Debug.Log($"미니게임 복귀! ID {targetID}번으로 이동합니다.");
            PlayerPrefs.DeleteKey("JumpTargetID"); // 사용 후 삭제 (필수)
            JumpToID(targetID);
        }
        else
        {
            StartDialogue();
        }
    }

    void Update()
    {
        // 입력창이나 선택지창이 떠있으면 대사 넘기기 금지
        if ((choicePanel != null && choicePanel.activeSelf) ||
            (nameInputPanel != null && nameInputPanel.activeSelf)) return;

        bool isClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool isSpace = Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;

        if (isClick || isSpace)
        {
            DisplayNextSentence();
        }
    }

    // ★ CSV 파싱 로직
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

            // 선택지 데이터 읽기
            if (row.Length > 7)
            {
                dl.opt1Text = row[6].Trim();
                int.TryParse(row[7].Trim(), out dl.opt1TargetID);
            }
            if (row.Length > 9)
            {
                dl.opt2Text = row[8].Trim();
                int.TryParse(row[9].Trim(), out dl.opt2TargetID);
            }

            scenarioData.Add(dl);
        }
    }

    public void StartDialogue()
    {
        currentLineIndex = 0;
        DisplayCurrentLine();
    }

    // ★ 다음 대사로 넘어가기 (클릭 시 호출)
    public void DisplayNextSentence()
    {
        // 현재 인덱스가 범위 내에 있는지 확인
        if (currentLineIndex < scenarioData.Count)
        {
            DialogueLine currentLine = scenarioData[currentLineIndex];

            // ★ 중요: 현재 줄의 NextScene에 "JUMP_"가 적혀있으면, 인덱스를 늘리지 말고 바로 점프!
            if (!string.IsNullOrEmpty(currentLine.nextScene) && currentLine.nextScene.StartsWith("JUMP_"))
            {
                string idStr = currentLine.nextScene.Replace("JUMP_", "");
                int targetID = int.Parse(idStr);

                JumpToID(targetID); // 여기서 DisplayCurrentLine이 호출되므로 바로 리턴
                return;
            }
        }

        // 별다른 점프가 없다면 다음 줄로 진행
        currentLineIndex++;
        DisplayCurrentLine();
    }

    // ★ 디버깅용 로그가 추가된 DisplayCurrentLine
    void DisplayCurrentLine()
    {
        if (currentLineIndex >= scenarioData.Count) { return; }

        DialogueLine line = scenarioData[currentLineIndex];

        // --- 디버깅 로그 (콘솔창 확인용) ---
        Debug.Log($"[현재 진행] ID: {line.id} / NextScene 값: '{line.nextScene}'");
        // -------------------------------

        // [1] 대사 표시
        string finalName = line.speakerName.Replace("{PlayerName}", playerName).Replace("{playername}", playerName);
        string finalDialogue = line.dialogueText.Replace("{PlayerName}", playerName).Replace("{playername}", playerName);

        nameText.text = finalName;
        dialogueText.text = finalDialogue;

        if (dialogueBoxImage != null) dialogueBoxImage.gameObject.SetActive(true);

        if (line.isMonologue) { dialogueBoxImage.color = monologueColor; nameText.gameObject.SetActive(false); }
        else { dialogueBoxImage.color = normalColor; nameText.gameObject.SetActive(true); }

        if (!string.IsNullOrEmpty(line.videoName) && currentVideoName != line.videoName)
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

        // [2] 특수 기능 체크
        if (line.nextScene == "INPUT_NAME")
        {
            if (nameInputPanel != null) { nameInputPanel.SetActive(true); nameInputField.text = ""; }
            return;
        }

        // ★★★ 여기가 문제의 구간 ★★★
        if (line.nextScene == "CHOICE")
        {
            Debug.Log(">>> [성공] CHOICE 조건문 진입함!");

            if (choicePanel == null)
            {
                Debug.LogError(">>> [비상!] Inspector에서 'Choice Panel' 연결이 비어있습니다(None)!");
            }
            else
            {
                Debug.Log($">>> [확인] ChoicePanel을 켭니다. 현재 상태: {choicePanel.activeSelf}");
                choicePanel.SetActive(true);

                // 버튼 텍스트 설정
                if (choiceButton1Text != null) choiceButton1Text.text = line.opt1Text;
                if (choiceButton2Text != null) choiceButton2Text.text = line.opt2Text;

                // 버튼 이벤트 연결
                if (choiceButton1 != null)
                {
                    choiceButton1.onClick.RemoveAllListeners();
                    choiceButton1.onClick.AddListener(() =>
                    {
                        choicePanel.SetActive(false);
                        JumpToID(line.opt1TargetID);
                    });
                }
                if (choiceButton2 != null)
                {
                    choiceButton2.onClick.RemoveAllListeners();
                    choiceButton2.onClick.AddListener(() =>
                    {
                        choicePanel.SetActive(false);
                        JumpToID(line.opt2TargetID);
                    });
                }
                Debug.Log(">>> [완료] 버튼 설정 끝. 패널이 켜져야 정상.");
            }
            return;
        }
        // ★★★★★★★★★★★★★★★★★★★

        if (!string.IsNullOrEmpty(line.nextScene) && !line.nextScene.StartsWith("JUMP_"))
        {
            SceneManager.LoadScene(line.nextScene);
            return;
        }
    }
    
    // ★ ID 검색해서 점프하는 함수
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
        Debug.LogError($"CSV에서 ID {targetID}를 찾을 수 없습니다!");
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
        VideoClip clip = Resources.Load<VideoClip>("Videos/" + videoName);
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
            Debug.LogWarning($"비디오를 찾을 수 없음: Videos/{videoName}");
        }
    }
}