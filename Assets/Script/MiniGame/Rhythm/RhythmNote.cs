using UnityEngine;

public class RhythmNote : MonoBehaviour
{
    public float noteSpeed = 500f; // 떨어지는 속도 (Canvas 크기에 따라 조절 필요)

    void Update()
    {
        // 아래로 이동 (RectTransform 기준)
        transform.Translate(Vector3.down * noteSpeed * Time.deltaTime);

        // 화면 밖으로 나가면 삭제 (메모리 관리)
        if (transform.localPosition.y < -1000) // 수치는 화면 크기에 맞춰 조절
        {
            Destroy(gameObject);
            // 여기에 "Miss" 처리를 추가할 수 있음
        }
    }
}