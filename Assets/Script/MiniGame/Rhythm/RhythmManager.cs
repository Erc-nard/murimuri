using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using UnityEngine.SceneManagement; 
using System.Collections; 

public class RhythmManager : MonoBehaviour
{
    [Header("---- [설정: 프리팹 & 위치] ----")]
    public GameObject notePrefab;       
    public Transform noteContainer;     
    public RectTransform[] lanes;       
    public RectTransform judgmentLine;  

    [Header("---- [설정: 게임 플레이] ----")]
    public float spawnInterval = 1.0f;  
    public float hitRange = 100f;       
    
    [Header("---- [설정: UI] ----")]
    public TextMeshProUGUI scoreText;   
    public TextMeshProUGUI gameText;    // "준비...", "Miss!" 등 표시

    // 내부 변수
    private float timer = 0f;
    private int currentScore = 0;
    private bool isGameEnded = false;
    private bool isPlaying = false; 

    void Start()
    {
        currentScore = 0;
        isGameEnded = false;
        isPlaying = false; 
        UpdateScoreUI();

        StartCoroutine(GameStartRoutine());
    }

    IEnumerator GameStartRoutine()
    {
        if (gameText != null) 
        {
            gameText.gameObject.SetActive(true);
            gameText.text = "준비...";
        }

        yield return new WaitForSeconds(2.0f); 

        if (gameText != null) 
        {
            gameText.text = "Start!";
            yield return new WaitForSeconds(1.0f);
            gameText.text = ""; 
        }

        isPlaying = true; 
    }

    void Update()
    {
        if (!isPlaying || isGameEnded) return;

        // 1. 노트 자동 생성
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnNote();
            timer = 0f;
        }

        // 2. 입력 처리 (터치/클릭)
        HandleInput();

        // 3. ★ [추가] 놓친 노트(Miss) 체크
        CheckMisses();
    }

    // ★ [핵심 1] 화면 밖으로 나간 노트 처리 (Miss 판정)
    void CheckMisses()
    {
        // 리스트를 거꾸로 돌면서 삭제해야 에러가 안 납니다.
        for (int i = noteContainer.childCount - 1; i >= 0; i--)
        {
            Transform note = noteContainer.GetChild(i);
            RectTransform noteRect = note.GetComponent<RectTransform>();

            // 노트의 바닥 위치 계산
            float halfHeight = noteRect.rect.height * 0.5f; 
            float noteBottomY = note.position.y - halfHeight;

            // 판정선보다 훨씬 아래로 내려갔다면? (Miss!)
            // hitRange만큼의 여유를 주고 그보다 더 내려갔을 때 처리
            if (noteBottomY < judgmentLine.position.y - hitRange)
            {
                OnMiss(note.gameObject);
            }
        }
    }

    void SpawnNote()
    {
        int laneIndex = Random.Range(0, 3);
        GameObject newNote = Instantiate(notePrefab, noteContainer);

        Vector3 lanePos = lanes[laneIndex].transform.position;
        newNote.transform.position = new Vector3(lanePos.x, 0, 0); 
        
        RectTransform noteRect = newNote.GetComponent<RectTransform>();
        // 화면 위쪽에서 시작
        noteRect.anchoredPosition = new Vector2(noteRect.anchoredPosition.x, 450f);
    }

    void HandleInput()
    {
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began) CheckHit(touch.position);
            }
        }
        if (Input.GetMouseButtonDown(0)) CheckHit(Input.mousePosition);
    }

    void CheckHit(Vector2 inputPos)
    {
        float screenWidth = Screen.width;
        float oneLaneWidth = screenWidth / 3f; 
        int laneIndex = -1;

        if (inputPos.x < oneLaneWidth) laneIndex = 0; 
        else if (inputPos.x < oneLaneWidth * 2) laneIndex = 1; 
        else laneIndex = 2; 

        if (laneIndex != -1) CheckTiming(laneIndex);
    }

    // ★ [핵심 2] 판정 로직 수정 (노트 최하단 기준)
    void CheckTiming(int laneIndex)
    {
        foreach (Transform note in noteContainer)
        {
            if (note == null) continue;

            // X축 체크 (같은 레인인지)
            if (Mathf.Abs(note.position.x - lanes[laneIndex].position.x) < 1.0f) 
            {
                RectTransform noteRect = note.GetComponent<RectTransform>();

                // 노트의 높이 절반 구하기
                // (Pivot이 중앙(0.5, 0.5)이라고 가정)
                float halfHeight = noteRect.rect.height * 0.5f; 

                // 노트의 바닥 Y좌표 = 현재 중심 Y좌표 - 높이 절반
                float noteBottomY = note.position.y - halfHeight;

                // 판정선과 '노트 바닥' 사이의 거리 계산
                float distance = Mathf.Abs(noteBottomY - judgmentLine.position.y);
                
                if (distance < hitRange)
                {
                    OnHit(note.gameObject);
                    break; // 한 번 터치에 하나만 처리
                }
            }
        }
    }

    void OnHit(GameObject note)
    {
        // 점수 획득
        currentScore += 10;
        UpdateScoreUI();
        
        // 노트 삭제
        Destroy(note);

        // 목표 달성 체크
        if (currentScore >= 400)
        {
            EndGame();
        }
    }

    // ★ [추가] 미스 처리 함수
    void OnMiss(GameObject note)
    {
        // 1. 점수 깎기 (50점)
        currentScore -= 50;

        // 2. 0점 미만 방지
        if (currentScore < 0) currentScore = 0;

        // 3. UI 갱신
        UpdateScoreUI();

        // 4. "Miss" 표시 (잠깐 띄우기)
        if (gameText != null)
        {
            StartCoroutine(ShowMissText());
        }

        // 5. 노트 삭제
        Destroy(note);
    }

    IEnumerator ShowMissText()
    {
        gameText.text = "Miss!";
        gameText.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        gameText.text = ""; // 다시 끄기
        gameText.color = Color.white;
    }

    void UpdateScoreUI()
    {
        if (scoreText != null) scoreText.text = $"내 점수: {currentScore} / 400";
    }

    void EndGame()
    {
        if (isGameEnded) return; 
        isGameEnded = true;

        Debug.Log("목표 달성! 메인으로 돌아갑니다.");
        SceneManager.LoadScene("PlayScene");
    }
}