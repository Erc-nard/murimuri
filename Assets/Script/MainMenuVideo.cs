using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class MainMenuVideo : MonoBehaviour
{
    [Header("--- 설정 ---")]
    public string backgroundVideoName = "Kana_Idle"; // 메인에서 보여줄 영상 이름 (확장자 X)
    public string nextSceneName = "GameScene"; // 시작 버튼 누르면 넘어갈 씬 이름

    [Header("--- 연결 ---")]
    public VideoPlayer videoPlayer;
    public RawImage displayImage;

    void Start()
    {
        // 1. RawImage 텍스처 자동 연결 (안전장치)
        if (displayImage.texture == null && videoPlayer.targetTexture != null)
        {
            displayImage.texture = videoPlayer.targetTexture;
        }

        // 2. 배경 영상 재생
        PlayBackgroundVideo();
    }

    void PlayBackgroundVideo()
    {
        VideoClip clip = Resources.Load<VideoClip>("Videos/" + backgroundVideoName);

        if (clip != null)
        {
            videoPlayer.clip = clip;
            videoPlayer.isLooping = true; // 메인화면이니까 무한반복
            videoPlayer.Play();
            displayImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError($"🚨 메인 영상을 찾을 수 없습니다: Videos/{backgroundVideoName}");
        }
    }

    // ★ 시작 버튼에 연결할 함수
    public void OnStartButtonClicked()
    {
        // 씬 넘어가기
        SceneManager.LoadScene(nextSceneName);
    }
}