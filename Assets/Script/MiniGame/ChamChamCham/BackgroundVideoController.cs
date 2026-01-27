using UnityEngine;
using UnityEngine.Video;

public class BackgroundVideoController : MonoBehaviour
{
    public enum AiChoice {Left, Center, Right}

    [Header("Components")]
    public VideoPlayer videoPlayer; // VideoPlayer 컴포넌트 연결

    [Header("Video Clips (Existing Files)")]
    public VideoClip leftVideo, centerVideo, rightVideo;

    [Header("AI Decision Info")] 
    public AiChoice currentDecision;

    void Awake()
    {
        // 1. 화면이 켜지기 전(게임 로직 초기화 단계)에 AI의 결정을 받습니다.
        currentDecision = GetAiDecision(); 

        // 2. 결정에 따라 비디오 클립을 교체합니다.
        SetBackgroundVideo(currentDecision);
    }

    // AI 로직이 들어갈 자리 (여기서는 임의로 랜덤 결정하도록 구현)
    AiChoice GetAiDecision()
    {
        // *실제 개발하신 AI 로직*을 여기에 연결하세요.
        // 예: return AiSystem.GetNextDirection();
        
        // 테스트를 위해 랜덤으로 하나를 뽑습니다.
        int randomVal = Random.Range(0, 3);
        if (randomVal == 0) return AiChoice.Left;
        else if (randomVal == 1) return AiChoice.Center;
        else return AiChoice.Right;
    }

    void SetBackgroundVideo(AiChoice choice)
    {
        switch (choice)
        {
            case AiChoice.Left:
                videoPlayer.clip = leftVideo;
                break;
            case AiChoice.Center:
                videoPlayer.clip = centerVideo;
                break;
            case AiChoice.Right:
                videoPlayer.clip = rightVideo;
                break;
        }

        // 비디오 루프 설정 (배경이므로 보통 반복 재생)
        videoPlayer.isLooping = true;
        
        // 즉시 재생
        videoPlayer.Play();
    }
}