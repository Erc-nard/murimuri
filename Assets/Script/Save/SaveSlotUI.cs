using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용 시

public class SaveSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public Text indexText;   // 파일 순서
    public Text chapterText; // 챕터명
    public Text timeText;    // 저장 시간
    public RawImage thumbnailImage;     // 썸네일
    public int mySlotIndex;
    
    // 데이터를 받아서 UI를 갱신하는 함수
    public void SetSlotInfo(int index, string chapter, string time, Texture2D screenshot)
    {

        mySlotIndex = index;

        indexText.text = $"FILE {index + 1}"; // 1번부터 표시
        chapterText.text = string.IsNullOrEmpty(chapter) ? "빈 슬롯" : chapter;
        timeText.text = string.IsNullOrEmpty(time) ? "- - - -" : time;
        
        if (screenshot != null)
        {
            thumbnailImage.texture = screenshot;
            thumbnailImage.color = Color.white;
        }
        else
        {
            // 빈 슬롯일 경우 기본 이미지 처리
            thumbnailImage.color = Color.gray; 
        }
    }

    public void OnClickSlot()
    {
        // 매니저를 찾아서 SaveGame 함수 호출!
        GameObject.Find("SaveSystem").GetComponent<SaveLoadManager>().SaveGame(mySlotIndex);
    }
}