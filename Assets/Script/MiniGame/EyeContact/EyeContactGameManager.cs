using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // 씬 이동 필수

public class EyeContactGameManager : MonoBehaviour
{
    [Header("Objects")]
    public RectTransform targetUI;  // 도망 다니는 타겟
    public RectTransform eyeCursor; // 내 눈 (시선)

    [Header("Game Settings")]
    public float hitRadius = 80f; // 판정 범위
    
    [Header("UI")]
    public Slider loveGauge;      // 호감도 게이지
    public float fillSpeed = 0.5f;   // 차오르는 속도
    public float drainSpeed = 0.3f;  // 떨어지는 속도

    // 내부 변수
    private bool isHit = false;
    private bool isGameEnded = false; 

    void Update()
    {
        // 게임 종료되었거나 오브젝트 없으면 실행 안 함
        if (isGameEnded || targetUI == null || eyeCursor == null) return;

        CheckHit();
        UpdateGameLogic();
    }

    void CheckHit()
    {
        // 타겟과 내 눈 사이 거리 계산
        float distance = Vector2.Distance(targetUI.position, eyeCursor.position);

        // 거리가 반지름보다 작으면 명중
        isHit = (distance < hitRadius);
    }

    void UpdateGameLogic()
    {
        if (isHit)
        {
            // 👀 시선 고정 성공: 게이지 상승
            loveGauge.value += fillSpeed * Time.deltaTime;
        }
        else
        {
            // 딴청: 게이지 하락
            loveGauge.value -= drainSpeed * Time.deltaTime;
        }

        // 승리 조건: 게이지 가득 참
        if (loveGauge.value >= 1.0f)
        {
            GameOver(true);
        }
    }

    void GameOver(bool isSuccess)
    {
        if (isGameEnded) return; 
        isGameEnded = true;     

        if (isSuccess)
        {
            Debug.Log("💖 심쿵 성공! 메인 스토리로 복귀합니다.");

            // ★ 번호 지정 없이 메인 씬만 로드하면 됩니다.
            // (메인 매니저가 알아서 다음 줄을 재생합니다)
            SceneManager.LoadScene("PlayScene");
        }
        else
        {
            // 실패 시 로직 (필요하면 구현)
            Debug.Log("💔 실패...");
        }
    }

    // 개발자 편의 기능: 판정 범위 그리기
    void OnDrawGizmos()
    {
        if (targetUI != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(targetUI.position, hitRadius);
        }
    }
}