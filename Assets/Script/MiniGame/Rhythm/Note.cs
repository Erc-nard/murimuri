using UnityEngine;

public class Note : MonoBehaviour
{
    public float noteSpeed = 800f; // 떨어지는 속도

    void Update()
    {
        // 1. 아래로 이동 (Y축 감소)
        transform.Translate(Vector3.down * noteSpeed * Time.deltaTime);

        // 2. 화면 밖으로 나가면 삭제 (메모리 관리)
        // NoteContainer의 Bottom이 0일 때, y가 -1000 정도면 확실히 화면 밖
        if (transform.localPosition.y < -1000f)
        {
            Debug.Log("Miss!"); // 나중에 미스 처리 로직 추가
            Destroy(gameObject);
        }
    }
}