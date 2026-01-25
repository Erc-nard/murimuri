using UnityEngine;
using UnityEngine.UI;

public class TargetMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public RectTransform moveArea; // 타겟이 움직일 수 있는 범위 (예: 얼굴 영역 패널)
    public float moveSpeed = 2.0f; // 이동 속도
    public float changeTargetTime = 1.0f; // 목표 지점을 바꾸는 간격

    private RectTransform targetRect;
    private Vector2 targetPosition;
    private float timer;

    void Start()
    {
        targetRect = GetComponent<RectTransform>();
        SetNewTargetPosition();
    }

    void Update()
    {
        // 일정 시간마다 새로운 목표 위치 설정
        timer += Time.deltaTime;
        if (timer >= changeTargetTime)
        {
            SetNewTargetPosition();
            timer = 0;
        }

        // 현재 위치에서 목표 위치로 부드럽게 이동 (Lerp 사용)
        targetRect.anchoredPosition = Vector2.Lerp(targetRect.anchoredPosition, targetPosition, Time.deltaTime * moveSpeed);
    }

    void SetNewTargetPosition()
    {
        // moveArea의 크기 내에서 랜덤한 좌표 생성
        float x = Random.Range(-moveArea.rect.width / 2, moveArea.rect.width / 2);
        float y = Random.Range(-moveArea.rect.height / 2, moveArea.rect.height / 2);
        targetPosition = new Vector2(x, y);
    }
}