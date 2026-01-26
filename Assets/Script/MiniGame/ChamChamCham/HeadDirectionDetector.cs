using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.UI;

// 방향 정의
public enum HeadDir { Center, Left, Right }

public class HeadDirectionDetector : MonoBehaviour
{
    [Header("디버깅용 UI")]
    public Text debugText;
    public Text statusText; // 방향 표시용

    [Header("거리 감지 설정")]
    // ARFoundation에서는 실제 미터(m) 단위입니다.
    // 0.3m (30cm) 정도면 꽤 가까운 것입니다. 테스트하며 조절하세요.
    public float closeDistanceThreshold = 0.2f; 
    public bool isFaceClose = false; 

    [Header("AR Settings")]
    public ARFaceManager faceManager;
    public Camera arCamera;

    [Header("Detection Settings")]
    public float threshold = 0.15f; 
    public HeadDir currentDirection = HeadDir.Center;

    public float currentDistance = 100f;

    void Update()
    {
        // 1. 얼굴이 감지되지 않았을 때
        if (faceManager.trackables.count == 0)
        {
            currentDirection = HeadDir.Center;
            isFaceClose = false;

            if(statusText) statusText.text = "얼굴 찾는 중...";
            if(debugText) debugText.text = "거리: 감지 안됨";
            return;
        }

        // 2. 얼굴 하나 가져오기
        ARFace face = null;
        foreach (var f in faceManager.trackables) { face = f; break; }

        if (face == null) return;

        float dist = Vector3.Distance(arCamera.transform.position, face.transform.position);
        currentDistance = dist; // 여기에 저장해둬야 매니저가 가져감!\

        if (dist < closeDistanceThreshold) isFaceClose = true;
        else isFaceClose = false;

        // 거리 판정
        if (dist < closeDistanceThreshold) // 거리가 기준보다 작으면(가까우면)
        {
            isFaceClose = true;
        }
        else
        {
            isFaceClose = false;
        }

        // 디버그 텍스트 갱신 (직접 호출!)
        if (debugText != null)
        {
            debugText.text = $"거리: {dist:F2}m\n상태: {(isFaceClose ? "가까움!😲" : "멀음")}";
            // 가까우면 빨간색, 멀면 흰색 등으로 색깔도 바꿔주면 더 잘 보임
            debugText.color = isFaceClose ? Color.red : Color.white;
        }


        // ==========================================
        // ★ [기존] 방향 계산 로직 ★
        // ==========================================
        Vector3 faceDir = arCamera.transform.InverseTransformDirection(face.transform.forward);

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