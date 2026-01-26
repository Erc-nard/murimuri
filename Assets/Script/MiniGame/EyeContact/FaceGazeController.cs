using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

public class FaceGazeController : MonoBehaviour
{
    [Header("AR Settings")]
    public ARFaceManager faceManager; 
    public Camera arCamera;           

    [Header("UI Settings")]
    public RectTransform eyeCursor;   
    public float sensitivity = 2.0f; // 🔥 감도를 확 줄이세요 (폰 해상도는 크니까요)
    public float smoothing = 10.0f;   

    [Header("Debug")]
    public Text debugText; 

    private Vector2 targetPos;

    void Update()
    {
        if (faceManager.trackables.count == 0) return;

        ARFace face = null;
        foreach (var f in faceManager.trackables)
        {
            face = f;
            break; 
        }

        if (face == null || arCamera == null) return;

        // 🚀 [핵심 1] "지구 기준" 방향을 "카메라 기준"으로 변환!
        // 이제 폰을 돌려도 폰 화면을 기준으로 계산합니다.
        Vector3 faceDir = arCamera.transform.InverseTransformDirection(face.transform.forward);

        // 디버깅 텍스트
        if(debugText) debugText.text = $"X: {faceDir.x:F2}, Y: {faceDir.y:F2}";

        // 좌표 계산 (Screen.width 대신 좀 더 안전한 고정값 추천)
        // 일단 화면 크기 비례로 하되, 감도를 낮춰서 계산
        float xMove = faceDir.x * sensitivity * (Screen.width / 2); // 0.1 * 2 * 500 = 100
        float yMove = -faceDir.y * sensitivity * (Screen.height / 2);

        // 🚀 [핵심 2] 화면 밖으로 못 나가게 가두기 (Clamp)
        // 캔버스 크기의 절반 정도(예: -450 ~ 450)로 제한
        float limitX = Screen.width / 2.5f; 
        float limitY = Screen.height / 2.5f;

        xMove = Mathf.Clamp(xMove, -limitX, limitX);
        yMove = Mathf.Clamp(yMove, -limitY, limitY);

        targetPos = new Vector2(xMove, yMove);

        // 부드럽게 이동
        eyeCursor.anchoredPosition = Vector2.Lerp(eyeCursor.anchoredPosition, targetPos, Time.deltaTime * smoothing);
    }
}