using UnityEngine;
using UnityEngine.UI;

public class EyeContactGameManager : MonoBehaviour
{
    [Header("Objects")]
    public RectTransform targetUI;  // 도망 다니는 타겟 (TargetMover가 붙은 애)
    public RectTransform eyeCursor; // 🔥 내 눈 (EyeTrackingReceiver가 붙은 빨간 점)

    [Header("Game Settings")]
    public float hitRadius = 100f; // 판정 범위 (이 거리 안에 들어오면 맞은 것으로 침)
    
    [Header("UI")]
    public Slider loveGauge;      // 호감도 게이지
    public float fillSpeed = 0.5f;   // 차오르는 속도
    public float drainSpeed = 0.3f;  // 떨어지는 속도

    // 내부 변수
    private bool isHit = false;

    void Update()
    {
        // 안전 장치
        if (targetUI == null || eyeCursor == null) return;

        CheckHit();
        UpdateGameLogic();
    }

    void CheckHit()
    {
        // 1. 타겟과 내 눈(커서) 사이의 거리 계산
        // UI(Overlay) 환경에서는 transform.position이 화면 상의 픽셀 좌표와 비슷하게 동작합니다.
        float distance = Vector2.Distance(targetUI.position, eyeCursor.position);

        // 2. 거리가 반지름보다 작으면 '명중'
        isHit = (distance < hitRadius);
    }

    void UpdateGameLogic()
    {
        if (isHit)
        {
            // 👀 시선 고정 성공: 게이지 상승
            loveGauge.value += fillSpeed * Time.deltaTime;
            
            // (선택사항) 여기에 "하트 파티클" 재생 함수를 넣으면 좋습니다.
            // PlayHeartEffect();
        }
        else
        {
            // 딴청 피우는 중: 게이지 하락
            loveGauge.value -= drainSpeed * Time.deltaTime;
        }

        // 승리/패배 조건 체크
        if (loveGauge.value >= 1.0f)
        {
            GameOver(true);
        }
        else if (loveGauge.value <= 0.0f)
        {
            // 게이지가 0이 되면 게임오버 시킬지, 그냥 0에서 멈출지 결정
            // GameOver(false); 
        }
    }

    void GameOver(bool isSuccess)
    {
        Debug.Log(isSuccess ? "💖 심쿵 성공! (게임 클리어)" : "💔 실패...");
        this.enabled = false; // 게임 로직 정지
    }

    // 🛠️ 개발자 편의 기능: 판정 범위를 눈으로 보여줍니다.
    void OnDrawGizmos()
    {
        if (targetUI != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetUI.position, hitRadius);
        }
    }
}