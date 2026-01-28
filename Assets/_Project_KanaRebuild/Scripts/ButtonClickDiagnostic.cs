using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace KanaRebuild
{
    /// <summary>
    /// 버튼이 실제로 클릭 가능한지 종합 진단
    /// GameManager나 아무 GameObject에 붙이세요.
    /// </summary>
    public class ButtonClickDiagnostic : MonoBehaviour
    {
        public Button testButton; // Inspector에서 테스트할 버튼을 드래그

        void Start()
        {
            if (testButton != null)
            {
                RunDiagnostic();
            }
            else
            {
                Debug.LogError("❌ testButton이 설정되지 않았습니다! Inspector에서 버튼을 드래그하세요.");
            }
        }

        void RunDiagnostic()
        {
            Debug.Log("========== 버튼 클릭 진단 시작 ==========");
            
            // 1. EventSystem 체크
            EventSystem es = EventSystem.current;
            if (es == null)
            {
                Debug.LogError("❌ EventSystem이 없습니다!");
            }
            else
            {
                Debug.Log($"✅ EventSystem 존재: {es.gameObject.name}");
            }
            
            // 2. 버튼 자체 상태
            Debug.Log($"\n[버튼 기본 정보]");
            Debug.Log($"버튼 이름: {testButton.gameObject.name}");
            Debug.Log($"GameObject 활성화: {testButton.gameObject.activeInHierarchy}");
            Debug.Log($"컴포넌트 활성화: {testButton.enabled}");
            Debug.Log($"Interactable: {testButton.interactable}");
            
            // 3. Image 컴포넌트
            Image img = testButton.GetComponent<Image>();
            if (img != null)
            {
                Debug.Log($"\n[Image 컴포넌트]");
                Debug.Log($"Raycast Target: {img.raycastTarget}");
                Debug.Log($"Color Alpha: {img.color.a}");
            }
            else
            {
                Debug.LogWarning("⚠️ Image 컴포넌트가 없습니다!");
            }
            
            // 4. Canvas 체크
            Canvas canvas = testButton.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"\n[Canvas 정보]");
                Debug.Log($"Render Mode: {canvas.renderMode}");
                
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster != null)
                {
                    Debug.Log($"GraphicRaycaster 존재: ✅");
                    Debug.Log($"Blocking Objects: {raycaster.blockingObjects}");
                }
                else
                {
                    Debug.LogError("❌ GraphicRaycaster가 없습니다!");
                }
            }
            else
            {
                Debug.LogError("❌ 버튼이 Canvas의 자식이 아닙니다!");
            }
            
            // 5. Canvas Group 체크
            CanvasGroup cg = testButton.GetComponentInParent<CanvasGroup>();
            if (cg != null)
            {
                Debug.Log($"\n[Canvas Group 발견]");
                Debug.Log($"Interactable: {cg.interactable}");
                Debug.Log($"Block Raycasts: {cg.blocksRaycasts}");
                Debug.Log($"Alpha: {cg.alpha}");
                
                if (!cg.interactable || !cg.blocksRaycasts)
                {
                    Debug.LogError("❌ Canvas Group이 클릭을 차단하고 있습니다!");
                }
            }
            
            // 6. onClick 리스너 수
            int persistentCount = testButton.onClick.GetPersistentEventCount();
            Debug.Log($"\n[onClick 리스너]");
            Debug.Log($"Inspector 리스너 수: {persistentCount}");
            
            // 7. 테스트 리스너 추가
            testButton.onClick.AddListener(() => {
                Debug.Log("🎉🎉🎉 진단 스크립트의 테스트 리스너 호출됨! 🎉🎉🎉");
            });
            Debug.Log("✅ 테스트 리스너 추가 완료 - 버튼을 클릭해보세요!");
            
            Debug.Log("========== 진단 완료 ==========");
        }
    }
}
