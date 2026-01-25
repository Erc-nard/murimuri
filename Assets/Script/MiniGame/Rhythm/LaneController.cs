using UnityEngine;
using System.Collections.Generic;

public class LaneController : MonoBehaviour
{
    [Header("Settings")]
    public GameObject notePrefab;    // 생성할 노트 프리팹
    public Transform spawnPoint;     // 노트가 생성될 위치 (화면 위쪽)
    public Transform hitPoint;       // 판정 위치 (버튼 위치)
    
    [Header("Balancing")]
    public float spawnInterval = 1.0f; // 노트 생성 간격
    public KeyCode pcKey;            // PC 테스트용 키 (A, S, D 등)

    private float timer;
    // 현재 이 레인에 살아있는 노트들을 관리하는 리스트
    private List<GameObject> activeNotes = new List<GameObject>(); 

    void Update()
    {
        // 1. 노트 자동 생성 (나중엔 음악 비트에 맞출 수 있음)
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnNote();
            timer = 0;
            // 랜덤한 박자감을 위해 간격을 약간 변동
            spawnInterval = Random.Range(0.5f, 1.5f);
        }

        // 2. 입력 감지 (PC 키보드 테스트용)
        if (Input.GetKeyDown(pcKey))
        {
            HandleInput();
        }

        // 3. 놓친 노트 리스트에서 정리 (null 체크)
        activeNotes.RemoveAll(item => item == null);
    }

    void SpawnNote()
    {
        GameObject newNote = Instantiate(notePrefab, spawnPoint);
        // 부모를 설정해서 UI 캔버스 안으로 들어오게 함
        newNote.transform.SetParent(spawnPoint.parent, false); 
        // 위치 초기화
        newNote.transform.position = spawnPoint.position;
        
        activeNotes.Add(newNote);
    }

    // 모바일 버튼이나 키보드가 눌렸을 때 실행
    public void HandleInput()
    {
        if (activeNotes.Count == 0) return;

        // 가장 아래에 있는(먼저 생성된) 노트를 가져옴
        GameObject targetNote = activeNotes[0];

        // 판정 라인과 노트 사이의 거리 계산 (Y축 거리)
        float distance = Mathf.Abs(targetNote.transform.position.y - hitPoint.position.y);

        if (distance < 100f) // 100f는 판정 범위 (테스트하며 조절)
        {
            // 성공! (Perfect/Good)
            Debug.Log("Hit!");
            RhythmGameManager.instance.AddScore(100);
            
            // 노트 삭제 및 리스트에서 제거
            Destroy(targetNote);
            activeNotes.RemoveAt(0);
        }
        else
        {
            Debug.Log("Miss - Too far");
        }
    }
}