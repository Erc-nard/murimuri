using UnityEngine;
using UnityEngine.EventSystems;

namespace KanaRebuild
{
    /// <summary>
    /// EventSystem이 올바르게 설정되어 있는지 체크하는 유틸리티
    /// 아무 GameObject에 붙여서 사용하세요.
    /// </summary>
    public class EventSystemChecker : MonoBehaviour
    {
        void Start()
        {
            CheckEventSystem();
        }

        [ContextMenu("Check EventSystem")]
        public void CheckEventSystem()
        {
            Debug.Log("========== EventSystem 진단 시작 ==========");
            
            // 1. EventSystem 존재 여부
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
            {
                Debug.LogError("❌ EventSystem이 씬에 없습니다!");
                Debug.LogError("   해결: Hierarchy > 우클릭 > UI > Event System");
                return;
            }
            
            Debug.Log($"✅ EventSystem 발견: {eventSystem.gameObject.name}");
            Debug.Log($"   - GameObject 활성화: {eventSystem.gameObject.activeSelf}");
            Debug.Log($"   - 컴포넌트 활성화: {eventSystem.enabled}");
            
            // 2. Input Module 확인
            BaseInputModule inputModule = eventSystem.currentInputModule;
            if (inputModule == null)
            {
                Debug.LogError("❌ Input Module이 없습니다!");
                Debug.LogError("   해결: EventSystem에 'Standalone Input Module' 추가");
                return;
            }
            
            Debug.Log($"✅ Input Module 발견: {inputModule.GetType().Name}");
            Debug.Log($"   - 활성화: {inputModule.enabled}");
            
            // 3. Input Module 타입 체크
            if (inputModule is StandaloneInputModule)
            {
                StandaloneInputModule standalone = inputModule as StandaloneInputModule;
                Debug.Log($"📱 Standalone Input Module 설정:");
                // forceModuleActive는 deprecated - 더 이상 필요 없음
                Debug.Log($"   - Input Actions Per Second: {standalone.inputActionsPerSecond}");
            }
            else
            {
                Debug.Log($"📱 Input System UI Input Module 사용 중");
            }
            
            // 4. 씬에 EventSystem이 여러 개 있는지 체크
            EventSystem[] allEventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
            if (allEventSystems.Length > 1)
            {
                Debug.LogWarning($"⚠️ EventSystem이 {allEventSystems.Length}개 발견됨!");
                Debug.LogWarning("   하나만 남기고 나머지를 삭제하세요:");
                foreach (var es in allEventSystems)
                {
                    Debug.LogWarning($"   - {es.gameObject.name}", es.gameObject);
                }
            }
            else
            {
                Debug.Log("✅ EventSystem 개수 정상 (1개)");
            }
            
            Debug.Log("========== EventSystem 진단 완료 ==========");
        }
    }
}
