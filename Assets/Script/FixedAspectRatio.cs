using UnityEngine;

public class FixedAspectRatio : MonoBehaviour
{
    // 목표로 하는 해상도 비율 (여기서 360, 620 설정)
    public float targetWidth = 360f;
    public float targetHeight = 620f;

    void Start()
    {
        Camera camera = GetComponent<Camera>();
        
        // 1. 목표 비율 계산
        float targetAspect = targetWidth / targetHeight;

        // 2. 현재 기기 화면 비율 계산
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // 3. 비율 비교 및 보정 비율 산출
        float scaleHeight = windowAspect / targetAspect;

        // 4. 카메라 뷰포트 조절 (Rect)
        if (scaleHeight < 1.0f)
        {
            // [상황 A] 기기 화면이 더 길쭉함 (위아래에 검은 여백 필요 - 레터박스)
            Rect rect = camera.rect;
            rect.width = 1.0f;
            rect.height = scaleHeight;
            rect.x = 0;
            rect.y = (1.0f - scaleHeight) / 2.0f; // 중앙 정렬
            camera.rect = rect;
        }
        else
        {
            // [상황 B] 기기 화면이 더 넓음 (양옆에 검은 여백 필요 - 필러박스)
            float scaleWidth = 1.0f / scaleHeight;
            Rect rect = camera.rect;
            rect.width = scaleWidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scaleWidth) / 2.0f; // 중앙 정렬
            rect.y = 0;
            camera.rect = rect;
        }
    }
}