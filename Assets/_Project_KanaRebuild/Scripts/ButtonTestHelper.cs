using UnityEngine;
using UnityEngine.UI;

namespace KanaRebuild
{
    /// <summary>
    /// 버튼이 정상 작동하는지 테스트하는 간단한 헬퍼 스크립트
    /// 버튼에 붙이고 클릭하면 Console에 메시지가 나타납니다.
    /// </summary>
    public class ButtonTestHelper : MonoBehaviour
    {
        void Start()
        {
            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => {
                    Debug.Log($"🎯 버튼 '{gameObject.name}' 클릭 감지됨!");
                });
                Debug.Log($"✅ ButtonTestHelper가 '{gameObject.name}'에 연결됨");
            }
            else
            {
                Debug.LogError($"❌ '{gameObject.name}'에 Button 컴포넌트가 없습니다!");
            }
        }
    }
}
