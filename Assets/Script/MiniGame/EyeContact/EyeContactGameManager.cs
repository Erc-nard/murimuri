using UnityEngine;
using UnityEngine.UI;

public class EyeContactGameManager : MonoBehaviour
{
    [Header("Targets")]
    public RectTransform targetUI; // 움직이는 타겟 (TargetMover가 붙은 객체)
    public float hitRadius = 100f; // 판정 범위 (반지름)

    [Header("UI")]
    public Slider loveGauge; // 호감도 게이지
    public float fillSpeed = 0.5f; // 차오르는 속도
    public float drainSpeed = 0.3f; // 떨어지는 속도

    private bool isTouching = false;
    private bool isHit = false;

    void Update()
    {
        HandleInput();
        UpdateGameLogic();
    }

    void HandleInput()
    {
        isTouching = false;

        // 모바일 터치 또는 PC 마우스 클릭 통합 처리
#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetMouseButton(0))
        {
            CheckHit(Input.mousePosition);
            isTouching = true;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            CheckHit(touch.position);
            isTouching = true;
        }
#endif
    }

    void CheckHit(Vector2 screenInputPosition)
    {
        // 타겟의 월드 좌표를 스크린 좌표로 변환하여 거리 계산이 필요할 수 있으나,
        // UI 모드(Screen Space Overlay)라면 transform.position이 스크린 좌표와 유사하게 작동합니다.
        // 가장 확실한 방법은 RectTransformUtility를 사용하는 것입니다.
        
        // 간단한 거리 계산법 (타겟이 Canvas - Screen Space Overlay일 때)
        float distance = Vector2.Distance(targetUI.position, screenInputPosition);

        // 거리가 반지름보다 작으면 '시선 고정 성공'
        isHit = (distance < hitRadius);
    }

    void UpdateGameLogic()
    {
        if (isTouching && isHit)
        {
            // 성공 중: 게이지 상승
            loveGauge.value += fillSpeed * Time.deltaTime;
            // TODO: 여기에 하트 파티클 효과 등을 재생하면 좋음
        }
        else
        {
            // 실패 중: 게이지 하락
            loveGauge.value -= drainSpeed * Time.deltaTime;
        }

        // 승리/패배 조건 체크
        if (loveGauge.value >= 1.0f)
        {
            GameOver(true);
        }
        else if (loveGauge.value <= 0.0f)
        {
            // 필요하다면 0이 되었을 때 바로 게임오버 시킬 수도 있음
            // GameOver(false); 
        }
    }

    void GameOver(bool isSuccess)
    {
        Debug.Log(isSuccess ? "심쿵 성공!" : "실패...");
        // 여기서 결과 팝업을 띄우거나 씬을 전환
        this.enabled = false; // 게임 로직 정지
    }
}