[System.Serializable]
public class SaveData
{
    public int slotIndex;       // 파일 순서 (0~9)
    public string chapterName;  // 챕터명 (예: "제 3장: 엇갈린 운명")
    public string saveTime;     // 저장 시간 (String으로 저장 추천)
    public string thumbPath;    // 썸네일 이미지 파일 경로

    // 여기에 게임 진행 변수, 호감도 등 추가
}