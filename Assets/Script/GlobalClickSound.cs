using UnityEngine;

public class GlobalClickSound : MonoBehaviour
{
    public AudioSource audioSource; // 소리를 재생할 스피커
    public AudioClip clickClip;     // 딸깍거리는 효과음 파일

    void Update()
    {
        // 마우스 왼쪽 버튼을 클릭하거나, 화면을 터치했을 때
        if (Input.GetMouseButtonDown(0))
        {
            if (audioSource != null && clickClip != null)
            {
                // 효과음을 중첩해서 재생 (연타해도 소리가 끊기지 않음)
                audioSource.PlayOneShot(clickClip);
            }
        }
    }
}