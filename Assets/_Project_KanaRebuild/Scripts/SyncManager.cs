using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KanaRebuild
{
    public class SyncManager : MonoBehaviour
    {
        public Slider syncGauge;
        public Image fillImage; // 게이지 Fill 이미지 (색상 변경용)
        public Material kanaMaterial;
        
        private float _currentSync = 0f; // 시작을 0%로 설정
        private bool _isBlinking = false;
        
        // 색상 설정
        private Color normalColor = new Color(0.5f, 1f, 0.5f); // 연두색
        private Color warningColor = Color.red; // 빨간색
        private Color negativeColor = Color.red; // 마이너스 게이지 색상

        void Start()
        {
            UpdateGaugeDisplay();
        }

        // 게이지 업데이트 (정답/오답/타임아웃)
        public void UpdateSync(float amount, bool showEffect = false)
        {
            float previousSync = _currentSync;
            _currentSync += amount;
            
            Debug.Log($"게이지 변경: {previousSync}% → {_currentSync}% (변화량: {amount}%)");
            
            // 양수 범위에서 감소 시에만 깜박임 효과
            if (amount < 0 && previousSync >= 0 && !_isBlinking && showEffect)
            {
                StartCoroutine(BlinkEffect());
            }
            
            UpdateGaugeDisplay();
            
            // 셰이더 값 조절
            if (kanaMaterial != null)
            {
                float glitchIntensity = 1.0f - (Mathf.Max(_currentSync, 0) / 100f);
                kanaMaterial.SetFloat("_GlitchAmount", glitchIntensity);
            }
        }

        // 게이지 표시 업데이트
        private void UpdateGaugeDisplay()
        {
            if (syncGauge == null)
            {
                Debug.LogWarning("SyncGauge가 연결되지 않았습니다!");
                return;
            }

            if (_currentSync >= 0)
            {
                // 양수: 0~100% 표시
                syncGauge.value = _currentSync / 100f;
                
                if (fillImage != null && !_isBlinking)
                {
                    fillImage.color = normalColor; // 연두색
                }
            }
            else
            {
                // 음수: 빨간색으로 역으로 채우기 (깜박임 없음)
                syncGauge.value = Mathf.Abs(_currentSync) / 100f;
                
                if (fillImage != null)
                {
                    fillImage.color = negativeColor; // 빨간색 (깜박임 없이)
                }
            }
        }

        // 깜박임 효과 (게이지 감소 시)
        private IEnumerator BlinkEffect()
        {
            _isBlinking = true;
            
            if (fillImage != null)
            {
                // 3번 깜박임
                for (int i = 0; i < 3; i++)
                {
                    fillImage.color = warningColor; // 빨간색
                    yield return new WaitForSeconds(0.1f);
                    
                    fillImage.color = normalColor; // 연두색
                    yield return new WaitForSeconds(0.1f);
                }
            }
            
            _isBlinking = false;
            UpdateGaugeDisplay(); // 최종 색상 업데이트
        }

        // 정답 시 (25% 증가, 효과 없음)
        public void OnCorrectAnswer()
        {
            UpdateSync(25f, false);
        }

        // 오답 시 (50% 감소, 깜박임 효과)
        public void OnWrongAnswer()
        {
            UpdateSync(-50f, true);
        }

        // 타임 페널티 (점진적 감소, 깜박임 효과)
        public void ApplyTimePenalty(float amount)
        {
            UpdateSync(-amount, true);
        }

        public float GetCurrentSync() => _currentSync;
        
        public bool IsGameOver() => _currentSync <= -100f; // 마이너스 100% 도달 시 게임 오버
    }
}
