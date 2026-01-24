using UnityEngine;
using System.IO;
using System.Collections;

public class SaveLoadManager : MonoBehaviour
{
    public GameObject slotPrefab; // 만들어둔 슬롯 프리팹
    public Transform contentTransform; // ScrollView의 Content 객체

    void Start()
    {
        InitializeSlots();
    }

    void InitializeSlots()
    {
        // 1. 기존에 생성된 슬롯이 있다면 초기화(삭제)
        foreach (Transform child in contentTransform)
        {
            Destroy(child.gameObject);
        }

        // 2. 10개의 슬롯 생성 (0번 ~ 9번)
        for (int i = 0; i < 10; i++)
        {
            GameObject newSlot = Instantiate(slotPrefab, contentTransform);
            SaveSlotUI uiScript = newSlot.GetComponent<SaveSlotUI>();

            // 3. 파일이 존재하는지 확인하고 데이터 로드
            string filePath = Application.persistentDataPath + $"/savefile_{i}.json";

            if (File.Exists(filePath))
            {
                // 파일이 있으면 읽어서 UI에 표시 (JSON 파싱 로직 필요)
                // 예시: string json = File.ReadAllText(filePath);
                // 예시: SaveData data = JsonUtility.FromJson<SaveData>(json);

                // 썸네일 로드 로직 (별도 구현 필요)
                // uiScript.SetSlotInfo(i, data.chapterName, data.saveTime, loadedTexture);
            }
            else
            {
                // 파일이 없으면 빈 슬롯으로 표시
                uiScript.SetSlotInfo(i, "", "", null);
            }
        }
    }

    public IEnumerator CaptureAndSaveThumbnail(int slotIndex, System.Action onComplete)
    {
        // UI가 찍히면 안 되니까 잠시 캔버스를 숨길 수도 있지만, 일단은 그냥 찍습니다.
        yield return new WaitForEndOfFrame(); // 프레임이 다 그려질 때까지 대기

        // 1. 화면 캡처
        Texture2D screenTexture = ScreenCapture.CaptureScreenshotAsTexture();

        // 2. 썸네일용으로 크기 줄이기 (용량 최적화, 예: 320x180) - 선택사항이나 권장
        // (복잡하면 일단 원본 저장)

        // 3. 파일로 저장 (PNG)
        byte[] bytes = screenTexture.EncodeToPNG();
        string path = Application.persistentDataPath + $"/thumb_{slotIndex}.png";
        File.WriteAllBytes(path, bytes);

        // 메모리 정리
        Destroy(screenTexture);

        // 저장이 끝나면 다음 할 일(데이터 저장)을 진행
        onComplete?.Invoke();
    }

    public void SaveGame(int slotIndex)
    {
        // 썸네일 먼저 찍고 -> 다 찍으면 데이터 저장 (순서 중요!)
        StartCoroutine(CaptureAndSaveThumbnail(slotIndex, () =>
        {
            // === 여기부터 진짜 데이터 저장 로직 ===

            // 1. 저장할 데이터 뭉치 만들기
            SaveData newData = new SaveData();
            newData.slotIndex = slotIndex;
            newData.saveTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            // [중요] 실제 게임의 변수를 여기에 넣어야 합니다!
            // 예: newData.chapterName = GameManager.instance.currentChapterName;
            // 예: newData.playerScore = GameManager.instance.score;
            newData.chapterName = "제 1장: 개발의 시작"; // 일단 임시 텍스트

            // 2. JSON으로 변환
            string json = JsonUtility.ToJson(newData);
            string filePath = Application.persistentDataPath + $"/savefile_{slotIndex}.json";

            // 3. 파일 쓰기
            File.WriteAllText(filePath, json);

            Debug.Log(slotIndex + "번 슬롯 저장 완료!");

            // 4. 저장했으니 UI도 바로 갱신해줘야겠죠?
            InitializeSlots();
        }));
    }
}