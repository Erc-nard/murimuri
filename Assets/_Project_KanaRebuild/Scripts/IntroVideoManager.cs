using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

namespace KanaRebuild
{
    /// <summary>
    /// 인트로 영상을 재생하고 끝나면 게임을 시작하는 매니저
    /// </summary>
    public class IntroVideoManager : MonoBehaviour
    {
        [Header("Video Settings")]
        public VideoPlayer videoPlayer;
        public RawImage rawImage; // 영상이 표시될 RawImage
        public GameManager gameManager; // 영상 끝나면 게임 시작
        
        [Header("Skip Settings")]
        public bool allowSkip = true; // 스페이스바로 스킵 가능
        
        private bool videoEnded = false;

        void Start()
        {
            if (videoPlayer == null)
            {
                Debug.LogError("VideoPlayer가 연결되지 않았습니다!");
                StartGame(); // 영상 없으면 바로 게임 시작
                return;
            }

            // 비디오 종료 이벤트 등록
            videoPlayer.loopPointReached += OnVideoEnd;
            
            // 영상 준비 완료 시 자동 재생
            videoPlayer.prepareCompleted += OnVideoPrepared;
            
            // 영상 준비 시작
            videoPlayer.Prepare();
            
            Debug.Log("🎬 인트로 영상 준비 중...");
        }

        void Update()
        {
            // 새로운 Input System 사용 시 Input.GetKeyDown 사용 불가
            // 스킵 기능이 필요하면 새로운 Input System으로 구현 필요
            // 현재는 자동 재생 후 게임 시작
        }

        void OnVideoPrepared(VideoPlayer vp)
        {
            Debug.Log("✅ 영상 준비 완료 - 재생 시작");
            videoPlayer.Play();
        }

        void OnVideoEnd(VideoPlayer vp)
        {
            if (!videoEnded)
            {
                videoEnded = true;
                Debug.Log("🎬 인트로 영상 종료 - 게임 시작");
                StartGame();
            }
        }

        void SkipVideo()
        {
            videoPlayer.Stop();
            OnVideoEnd(videoPlayer);
        }

        void StartGame()
        {
            // 인트로 UI 숨기기
            gameObject.SetActive(false);
            
            // 게임 시작
            if (gameManager != null)
            {
                gameManager.StartGame();
            }
        }

        void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.loopPointReached -= OnVideoEnd;
                videoPlayer.prepareCompleted -= OnVideoPrepared;
            }
        }
    }
}
