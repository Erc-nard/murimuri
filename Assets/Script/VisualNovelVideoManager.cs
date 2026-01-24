using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;


// 대사 한 줄에 어떤 정보가 들어가는지 정의
[System.Serializable] // 이 줄이 있어야 유니티 인스펙터 창에서 내용을 입력할 수 있음
public class DialogueLine
{
    public string speakerName;    // 캐릭터 이름 (예: 카나)
    [TextArea(3, 5)]              // 인스펙터에서 줄바꿈 가능하게 하기
    public string dialogueText;   // 대사 내용
    public string videoName;      // 재생할 영상 파일 이름 (확장자 제외, 예: Scene1)
    public bool isMonologue;      // 체크하면 독백 모드(UI 색 바뀜)
}


public class VisualNovelVideoManager : MonoBehaviour
{
    [Header("UI 연결 (드래그해서 넣으세요)")]
    public VideoPlayer videoPlayer;
    public RawImage backgroundDisplay;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    public Image dialogueBoxImage;

    [Header("설정")]
    public Color normalColor = new Color(1f, 1f, 1f, 0.8f);
    public Color monologueColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);

    // ▼ 인스펙터에서 대사를 직접 입력할 수 있게 리스트를 공개합니다.
    [Header("대사 데이터 입력")]
    public List<DialogueLine> scenarioData = new List<DialogueLine>();

    private Queue<DialogueLine> sentences = new Queue<DialogueLine>();
    private string currentVideoName = "";

    void Start()
    {
        // 게임 시작하자마자 인스펙터에 적어둔 대사들을 로드합니다.
        StartDialogue(scenarioData);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DisplayNextSentence();
        }
    }

    public void StartDialogue(List<DialogueLine> dialogueData)
    {
        sentences.Clear();
        foreach (DialogueLine line in dialogueData)
        {
            sentences.Enqueue(line);
        }
        DisplayNextSentence();
    }

    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = sentences.Dequeue();

        // 1. 텍스트 & UI 색상 처리
        nameText.text = line.speakerName;
        dialogueText.text = line.dialogueText;

        if (line.isMonologue)
        {
            dialogueBoxImage.color = monologueColor;
            nameText.gameObject.SetActive(false); // 독백일 땐 이름 숨김
        }
        else
        {
            dialogueBoxImage.color = normalColor;
            nameText.gameObject.SetActive(true);
        }

        // 2. 비디오 재생 로직
        // 영상 이름이 비어있지 않고, 이전과 다를 때만 재생
        if (!string.IsNullOrEmpty(line.videoName) && currentVideoName != line.videoName)
        {
            PlayVideo(line.videoName);
        }
    }

    void PlayVideo(string videoName)
    {
        // Resources/Videos 폴더 안에 있는 영상을 찾습니다.
        VideoClip clip = Resources.Load<VideoClip>("Videos/" + videoName);

        if (clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
            currentVideoName = videoName;
        }
        else
        {
            Debug.LogError($"'Resources/Videos' 폴더에 '{videoName}' 라는 영상이 없습니다!");
        }
    }

    void EndDialogue()
    {
        Debug.Log("모든 대사가 끝났습니다.");
    }
}