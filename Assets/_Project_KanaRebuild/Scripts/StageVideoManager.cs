using System.Collections;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

namespace KanaRebuild
{
    /// <summary>
    /// 각 스테이지의 4가지 영상을 관리하는 매니저
    /// </summary>
    [System.Serializable]
    public class StageVideos
    {
        public VideoClip normalVideo;   // 평소 영상 (루프)
        public VideoClip correctVideo;  // 정답 시 점프 영상
        public VideoClip wrongVideo;    // 오답 시 영상
        public VideoClip deathVideo;    // 게임오버 시 영상
    }

    public class StageVideoManager : MonoBehaviour
    {
        [Header("Video Settings")]
        public VideoPlayer videoPlayer;
        public RawImage videoDisplay;
        public RenderTexture renderTexture;
        
        [Header("Stage Videos (총 4개 스테이지)")]
        public StageVideos[] stageVideos = new StageVideos[4];
        
        [Header("Ending Video")]
        public VideoClip endingVideo; // 게임 클리어 후 엔딩 영상
        
        [Header("Fade Effect")]
        public Image whiteFadeImage; // 하얀 페이드용 Image
        public float fadeDuration = 1.2f; // 페이드 인/아웃 시간 (각각)
        
        private int currentStage = 0;
        private bool isPlayingVideo = false;

        void Start()
        {
            if (videoPlayer == null)
            {
                Debug.LogError("VideoPlayer가 연결되지 않았습니다!");
                return;
            }

            // Video Player 기본 설정
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            
            if (videoDisplay != null)
            {
                videoDisplay.texture = renderTexture;
            }

            // 하얀 페이드 초기화
            if (whiteFadeImage != null)
            {
                whiteFadeImage.color = new Color(1, 1, 1, 0); // 투명
                whiteFadeImage.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 스테이지 시작 - Normal 영상 루프 재생
        /// </summary>
        public void PlayNormalVideo(int stageIndex)
        {
            currentStage = stageIndex;
            
            if (stageIndex >= stageVideos.Length || stageVideos[stageIndex].normalVideo == null)
            {
                Debug.LogWarning($"Stage {stageIndex}의 Normal 영상이 없습니다!");
                return;
            }

            videoPlayer.clip = stageVideos[stageIndex].normalVideo;
            videoPlayer.isLooping = true;
            videoPlayer.Play();
            
            Debug.Log($"🎬 Stage {stageIndex + 1} Normal 영상 재생 (루프)");
        }

        /// <summary>
        /// 정답 시 - Correct 영상 재생 후 다음 스테이지로
        /// </summary>
        public IEnumerator PlayCorrectVideo(int stageIndex, System.Action onComplete)
        {
            if (stageIndex >= stageVideos.Length || stageVideos[stageIndex].correctVideo == null)
            {
                Debug.LogWarning($"Stage {stageIndex}의 Correct 영상이 없습니다!");
                onComplete?.Invoke();
                yield break;
            }

            isPlayingVideo = true;
            
            // Correct 영상 재생
            videoPlayer.clip = stageVideos[stageIndex].correctVideo;
            videoPlayer.isLooping = false;
            videoPlayer.Play();
            
            Debug.Log($"✅ Stage {stageIndex + 1} Correct 영상 재생 (점프)");
            
            float elapsedTime = 0f;
            
            // 영상 끝날 때까지 대기
            while (videoPlayer.isPlaying)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log($"Correct 영상 종료 (재생 시간: {elapsedTime:F2}초)");
            
            // 3초 대기
            Debug.Log("3초 대기 시작...");
            yield return new WaitForSeconds(3f);
            Debug.Log("3초 대기 종료");
            
            // 하얀 페이드 효과
            Debug.Log("하얀 페이드 시작");
            yield return StartCoroutine(FadeToWhite());
            Debug.Log("하얀 페이드 종료");
            
            isPlayingVideo = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 오답 시 - Wrong 영상 재생 후 Normal로 복귀 (최소 5초)
        /// </summary>
        public IEnumerator PlayWrongVideo(int stageIndex, System.Action onComplete)
        {
            if (stageIndex >= stageVideos.Length || stageVideos[stageIndex].wrongVideo == null)
            {
                Debug.LogWarning($"Stage {stageIndex}의 Wrong 영상이 없습니다!");
                onComplete?.Invoke();
                yield break;
            }

            isPlayingVideo = true;
            
            // Wrong 영상 재생
            videoPlayer.clip = stageVideos[stageIndex].wrongVideo;
            videoPlayer.isLooping = false;
            videoPlayer.Play();
            
            Debug.Log($"❌ Stage {stageIndex + 1} Wrong 영상 재생 (최소 5초)");
            
            float elapsedTime = 0f;
            
            // 영상이 끝날 때까지 또는 최소 5초 중 더 긴 시간만큼 대기
            while (videoPlayer.isPlaying || elapsedTime < 5f)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log($"Wrong 영상 종료 (재생 시간: {elapsedTime:F2}초)");
            
            // Normal 영상으로 복귀
            PlayNormalVideo(stageIndex);
            
            isPlayingVideo = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 게임오버 시 - Death 영상 재생
        /// </summary>
        public IEnumerator PlayDeathVideo(int stageIndex, System.Action onComplete)
        {
            if (stageIndex >= stageVideos.Length || stageVideos[stageIndex].deathVideo == null)
            {
                Debug.LogWarning($"Stage {stageIndex}의 Death 영상이 없습니다!");
                onComplete?.Invoke();
                yield break;
            }

            isPlayingVideo = true;
            
            // Death 영상 재생
            videoPlayer.clip = stageVideos[stageIndex].deathVideo;
            videoPlayer.isLooping = false;
            videoPlayer.Play();
            
            Debug.Log($"💀 Stage {stageIndex + 1} Death 영상 재생");
            
            float elapsedTime = 0f;
            
            // 영상 끝날 때까지 대기
            while (videoPlayer.isPlaying)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log($"Death 영상 종료 (재생 시간: {elapsedTime:F2}초)");
            
            isPlayingVideo = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 게임 클리어 - 엔딩 영상 재생
        /// </summary>
        public IEnumerator PlayEndingVideo(System.Action onComplete)
        {
            if (endingVideo == null)
            {
                Debug.LogWarning("엔딩 영상이 설정되지 않았습니다!");
                onComplete?.Invoke();
                yield break;
            }

            isPlayingVideo = true;
            
            // 엔딩 영상 재생
            videoPlayer.clip = endingVideo;
            videoPlayer.isLooping = false;
            videoPlayer.Play();
            
            Debug.Log("🎉 엔딩 영상 재생 시작");
            
            float elapsedTime = 0f;
            
            // 영상 끝날 때까지 대기
            while (videoPlayer.isPlaying)
            {
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            Debug.Log($"🎉 엔딩 영상 종료 (재생 시간: {elapsedTime:F2}초)");
            
            isPlayingVideo = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// 하얀색 페이드 효과
        /// </summary>
        IEnumerator FadeToWhite()
        {
            if (whiteFadeImage == null) yield break;
            
            whiteFadeImage.gameObject.SetActive(true);
            
            // 페이드 인 (투명 → 불투명)
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsed / fadeDuration);
                whiteFadeImage.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            
            // 잠깐 유지
            yield return new WaitForSeconds(0.3f);
            
            // 페이드 아웃 (불투명 → 투명)
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float alpha = 1f - Mathf.Clamp01(elapsed / fadeDuration);
                whiteFadeImage.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            
            whiteFadeImage.gameObject.SetActive(false);
        }

        /// <summary>
        /// 영상 일시정지 (선택지 나타날 때)
        /// </summary>
        public void PauseVideo()
        {
            if (videoPlayer != null && videoPlayer.isPlaying)
            {
                videoPlayer.Pause();
            }
        }

        /// <summary>
        /// 영상 재개
        /// </summary>
        public void ResumeVideo()
        {
            if (videoPlayer != null && !videoPlayer.isPlaying)
            {
                videoPlayer.Play();
            }
        }

        public bool IsPlayingVideo()
        {
            return isPlayingVideo;
        }
    }
}
