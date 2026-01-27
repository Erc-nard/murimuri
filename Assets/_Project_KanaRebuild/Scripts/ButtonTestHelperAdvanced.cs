using UnityEngine;
using UnityEngine.UI;

namespace KanaRebuild
{
    /// <summary>
    /// GameManager를 직접 호출하는 버튼 테스트 헬퍼
    /// ChoiceButton에 붙이고 GameManager를 연결하세요.
    /// </summary>
    public class ButtonTestHelperAdvanced : MonoBehaviour
    {
        public GameManager gameManager; // Inspector에서 GameManager 드래그
        public int buttonIndex = 0; // 이 버튼의 인덱스 (0, 1, 2, 3)
        
        void Start()
        {
            Button button = GetComponent<Button>();
            if (button == null)
            {
                Debug.LogError($"❌ '{gameObject.name}'에 Button 컴포넌트가 없습니다!");
                return;
            }
            
            if (gameManager == null)
            {
                Debug.LogError($"❌ GameManager가 연결되지 않았습니다!");
                return;
            }
            
            button.onClick.AddListener(() => {
                Debug.Log($"🔥🔥🔥 ButtonTestHelperAdvanced: 버튼 {buttonIndex} 클릭됨! 🔥🔥🔥");
                Debug.Log($"GameManager 있음: {gameManager != null}");
                Debug.Log($"Stages 있음: {gameManager.stages != null}");
                Debug.Log($"Stages 길이: {gameManager.stages?.Length}");
                
                // 간단하게 정답으로 처리 (테스트용)
                // OnChoiceSelected는 private이므로 public으로 바꾸거나 다른 방법 필요
                Debug.Log("⚠️ OnChoiceSelected는 private이므로 직접 호출 불가");
                Debug.Log("GameManager 코드를 확인해야 합니다.");
            });
            
            Debug.Log($"✅ ButtonTestHelperAdvanced가 '{gameObject.name}'에 연결됨 (Index: {buttonIndex})");
        }
    }
}
