using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

// 방향 정의
public enum HeadDir { Center, Left, Right }

public class HeadDirectionDetector : MonoBehaviour
{
    [Header("AR Settings")]
    public ARFaceManager faceManager;
    public Camera arCamera;

    [Header("Detection Settings")]
    public float threshold = 0.15f; // 고개를 얼마나 돌려야 인식할지 (민감도)
    public HeadDir currentDirection = HeadDir.Center; // 현재 내 머리 방향

    [Header("Debug UI")]
    public Text statusText; // 화면에 현재 방향 표시용

    void Update()
    {
        if (faceManager.trackables.count == 0)
        {
            currentDirection = HeadDir.Center;
            if(statusText) statusText.text = "얼굴 찾는 중...";
            return;
        }

        ARFace face = null;
        foreach (var f in faceManager.trackables) { face = f; break; }

        // 카메라 기준 얼굴 방향 계산
        Vector3 faceDir = arCamera.transform.InverseTransformDirection(face.transform.forward);

        // 방향 판별 (X값 기준)
        // 거울 모드 고려: 내 고개가 왼쪽이면 화면상 X는 -일 수도 +일 수도 있음. 
        // 직접 해보고 반대면 이 부분 부호를 바꾸세요.
        if (faceDir.x < -threshold) 
        {
            currentDirection = HeadDir.Left;
            if(statusText) statusText.text = "⬅️ 왼쪽";
        }
        else if (faceDir.x > threshold)
        {
            currentDirection = HeadDir.Right;
            if(statusText) statusText.text = "➡️ 오른쪽";
        }
        else
        {
            currentDirection = HeadDir.Center;
            if(statusText) statusText.text = "⏺️ 정면";
        }
    }
}