using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChamChamChamUI : MonoBehaviour
{
    [Header("Components")]
    public Image[] imageSlots; // 화면의 하얀 네모 3개
    public BackgroundVideoController aiScript; // [연결 필요] AI 스크립트가 붙은 오브젝트

    [Header("Common Photos (1, 2번째 박자)")]
    public Sprite photoStep1; // 첫 번째 사진 (공통)
    public Sprite photoStep2; // 두 번째 사진 (공통)

    [Header("Ending Photos (3번째 박자 - AI 결과)")]
    public Sprite lastPhotoLeft;   // AI가 왼쪽일 때 뜰 사진
    public Sprite lastPhotoCenter; // AI가 중앙일 때 뜰 사진
    public Sprite lastPhotoRight;  // AI가 오른쪽일 때 뜰 사진

    [Header("Settings")]
    public float interval = 0.5f;

    void Start()
    {
        // 시작 시 모두 투명하게
        foreach (Image img in imageSlots)
        {
            img.gameObject.SetActive(true);
            Color c = img.color;
            c.a = 0f;
            img.color = c;
        }

        StartCoroutine(ShowPhotosRoutine());
    }

    IEnumerator ShowPhotosRoutine()
    {
        // 총 3번 반복 (0, 1, 2)
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(interval);

            // 보여줄 사진을 담을 변수
            Sprite selectedSprite = null;

            if (i == 0)
            {
                selectedSprite = photoStep1; // 첫 번째 박자
            }
            else if (i == 1)
            {
                selectedSprite = photoStep2; // 두 번째 박자
            }
            else if (i == 2)
            {
                // [핵심] 마지막 박자에서는 AI의 선택을 확인해서 사진 결정
                var choice = aiScript.currentDecision;

                if (choice == BackgroundVideoController.AiChoice.Left)
                    selectedSprite = lastPhotoLeft;
                else if (choice == BackgroundVideoController.AiChoice.Center)
                    selectedSprite = lastPhotoCenter;
                else
                    selectedSprite = lastPhotoRight;
            }

            // 사진 적용 및 투명도 해제
            if (i < imageSlots.Length && selectedSprite != null)
            {
                imageSlots[i].sprite = selectedSprite;
                
                Color c = imageSlots[i].color;
                c.a = 1f; // 불투명하게
                imageSlots[i].color = c;
            }
        }
    }
}