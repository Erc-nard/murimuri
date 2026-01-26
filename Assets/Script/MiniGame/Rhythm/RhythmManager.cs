using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용을 위해 필수

public class RhythmManager : MonoBehaviour
{
    [Header("---- [설정: 프리팹 & 위치] ----")]
    public GameObject notePrefab;       // 노트 프리팹
    public Transform noteContainer;     // 노트가 담길 부모 객체
    public RectTransform[] lanes;       // 3개의 레인 (Lane0, Lane1, Lane2)
    public RectTransform judgmentLine;  // 판정선 (Y축 기준)

    [Header("---- [설정: 게임 플레이] ----")]
    public float spawnInterval = 1.0f;  // 노트 생성 간격 (초)
    public float hitRange = 100f;       // 판정 범위 (이 거리 안이면 Hit)
    
    [Header("---- [설정: UI] ----")]
    public TextMeshProUGUI scoreText;   // 점수 표시 텍스트

    // 내부 변수
    private float timer = 0f;
    private int currentScore = 0;

    void Start()
    {
        // 게임 시작 시 점수 초기화
        currentScore = 0;
        UpdateScoreUI();
    }

    void Update()
    {
        // 1. 노트 자동 생성 타이머
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnNote();
            timer = 0f;
        }

        // 2. 입력 처리 (PC 마우스 & 모바일 터치 통합)
        HandleInput();
    }

    void SpawnNote()
    {
        // 1. 랜덤 레인 선택
        int laneIndex = Random.Range(0, 3);

        // 2. 노트 생성
        GameObject newNote = Instantiate(notePrefab, noteContainer);

        // 3. X축 위치 맞추기 (레인의 월드 좌표 기준)
        Vector3 lanePos = lanes[laneIndex].transform.position;
        newNote.transform.position = new Vector3(lanePos.x, 0, 0); // Y는 일단 0으로 둠

        // 4. [핵심] Y축 위치를 화면 위쪽 바깥으로 강제 이동
        RectTransform noteRect = newNote.GetComponent<RectTransform>();
        
        // 화면 높이가 620이므로, 절반은 310입니다.
        // 310보다 커야 화면 밖입니다. 넉넉하게 450으로 설정합니다.
        // x값은 위에서 맞춘 값을 그대로 유지(anchoredPosition.x)
        noteRect.anchoredPosition = new Vector2(noteRect.anchoredPosition.x, 450f);
    }

    // 입력 처리 함수
    void HandleInput()
    {
        // A. 모바일 터치 지원 (멀티 터치 가능)
        if (Input.touchCount > 0)
        {
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began) // 막 눌렀을 때만
                {
                    CheckHit(touch.position);
                }
            }
        }
        
        // B. PC 마우스 클릭 지원 (테스트용)
        if (Input.GetMouseButtonDown(0))
        {
            CheckHit(Input.mousePosition);
        }
    }

    // 터치한 좌표가 어느 레인인지 계산
    void CheckHit(Vector2 inputPos)
    {
        float screenWidth = Screen.width;
        float oneLaneWidth = screenWidth / 3f; // 화면을 3등분

        int laneIndex = -1;

        if (inputPos.x < oneLaneWidth)
        {
            laneIndex = 0; // 왼쪽
        }
        else if (inputPos.x < oneLaneWidth * 2)
        {
            laneIndex = 1; // 가운데
        }
        else
        {
            laneIndex = 2; // 오른쪽
        }

        // 유효한 레인을 눌렀다면 판정 검사
        if (laneIndex != -1)
        {
            CheckTiming(laneIndex);
        }
    }

    // 실제 노트와 판정선 거리 계산
    void CheckTiming(int laneIndex)
    {
        // NoteContainer 안의 모든 노트를 검사
        foreach (Transform note in noteContainer)
        {
            RectTransform noteRect = note.GetComponent<RectTransform>();
            
            // 1. 노트의 X위치가 내가 누른 레인의 X위치와 비슷한가? (오차 범위 10)
            // (월드 좌표가 아니라 anchoredPosition으로 비교하여 같은 레인인지 확인)
            float laneX = lanes[laneIndex].anchoredPosition.x;
            // *주의: 레인 배치가 LayoutGroup이라 anchoredPosition이 0일 수도 있음. 
            // 가장 확실한 건 World Position X 차이 비교
            
            if (Mathf.Abs(note.position.x - lanes[laneIndex].position.x) < 1.0f) 
            {
                // 2. Y축 거리가 판정 범위 내인가?
                float distance = Mathf.Abs(note.position.y - judgmentLine.position.y);
                
                if (distance < hitRange)
                {
                    OnHit(note.gameObject);
                    break; // 한 번 터치에 노트 하나만 처리하고 종료
                }
            }
        }
    }

    // 판정 성공 시 처리
    void OnHit(GameObject note)
    {
        Debug.Log("Hit Success!");

        // 1. 점수 증가
        currentScore += 10;
        
        // 2. UI 갱신
        UpdateScoreUI();

        // 3. 이펙트 생성 (나중에 추가 가능)
        // Instantiate(hitEffectPrefab, note.transform.position, Quaternion.identity);

        // 4. 노트 제거
        Destroy(note);
    }

    // UI 텍스트 갱신
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"내 점수: {currentScore}점";
        }
    }
}