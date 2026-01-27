using UnityEngine;
using UnityEditor;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;

namespace Project.KanaRebuild.Editor
{
    /// <summary>
    /// 씬의 모든 TextMeshPro 컴포넌트의 폰트를 한번에 변경하는 에디터 유틸리티
    /// 사용법: Tools → Change All TMP Fonts
    /// </summary>
    public class FontChanger : EditorWindow
{
    private TMP_FontAsset newFont;
    private bool includeInactive = true;
    
    [MenuItem("Tools/Change All TMP Fonts")]
    static void ShowWindow()
    {
        var window = GetWindow<FontChanger>("TMP Font Changer");
        window.minSize = new Vector2(400, 200);
        window.Show();
    }
    
    void OnGUI()
    {
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "현재 씬의 모든 TextMeshPro 폰트를 한번에 변경합니다.\n" +
            "⚠️ 변경 전에 씬을 저장하는 것을 권장합니다!",
            MessageType.Info
        );
        
        GUILayout.Space(10);
        
        // 현재 씬 정보
        Scene currentScene = SceneManager.GetActiveScene();
        EditorGUILayout.LabelField("현재 씬:", currentScene.name, EditorStyles.boldLabel);
        
        GUILayout.Space(10);
        
        // 폰트 선택
        EditorGUILayout.LabelField("새로운 폰트 선택:", EditorStyles.boldLabel);
        newFont = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "Font Asset", 
            newFont, 
            typeof(TMP_FontAsset), 
            false
        );
        
        GUILayout.Space(5);
        
        // 비활성 오브젝트 포함 여부
        includeInactive = EditorGUILayout.Toggle("비활성 오브젝트 포함", includeInactive);
        
        GUILayout.Space(10);
        
        // 미리보기 버튼
        if (GUILayout.Button("폰트 사용 중인 오브젝트 수 확인", GUILayout.Height(30)))
        {
            PreviewChanges();
        }
        
        GUILayout.Space(10);
        
        // 변경 버튼
        GUI.enabled = newFont != null;
        GUI.backgroundColor = new Color(0.5f, 1f, 0.5f);
        
        if (GUILayout.Button("✓ 모든 폰트 변경하기", GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "폰트 변경 확인",
                $"현재 씬의 모든 TextMeshPro 폰트를 '{newFont.name}'으로 변경하시겠습니까?",
                "변경",
                "취소"
            ))
            {
                ChangeAllFonts();
            }
        }
        
        GUI.backgroundColor = Color.white;
        GUI.enabled = true;
    }
    
    void PreviewChanges()
    {
        TextMeshProUGUI[] tmpUGUIComponents = FindObjectsOfType<TextMeshProUGUI>(includeInactive);
        TextMeshPro[] tmpComponents = FindObjectsOfType<TextMeshPro>(includeInactive);
        
        int totalCount = tmpUGUIComponents.Length + tmpComponents.Length;
        
        string message = $"총 {totalCount}개의 TextMeshPro 컴포넌트가 발견되었습니다.\n\n";
        message += $"- UI (UGUI): {tmpUGUIComponents.Length}개\n";
        message += $"- 3D (World Space): {tmpComponents.Length}개";
        
        EditorUtility.DisplayDialog("폰트 변경 미리보기", message, "확인");
        
        Debug.Log($"[Font Changer] 발견된 TextMeshPro 컴포넌트: {totalCount}개");
    }
    
    void ChangeAllFonts()
    {
        if (newFont == null)
        {
            EditorUtility.DisplayDialog("오류", "폰트를 선택해주세요!", "확인");
            return;
        }
        
        int changedCount = 0;
        
        // UI용 TextMeshPro (UGUI) 변경
        TextMeshProUGUI[] tmpUGUIComponents = FindObjectsOfType<TextMeshProUGUI>(includeInactive);
        foreach (var tmp in tmpUGUIComponents)
        {
            Undo.RecordObject(tmp, "Change TMP Font");
            tmp.font = newFont;
            EditorUtility.SetDirty(tmp);
            changedCount++;
        }
        
        // 3D용 TextMeshPro 변경
        TextMeshPro[] tmpComponents = FindObjectsOfType<TextMeshPro>(includeInactive);
        foreach (var tmp in tmpComponents)
        {
            Undo.RecordObject(tmp, "Change TMP Font");
            tmp.font = newFont;
            EditorUtility.SetDirty(tmp);
            changedCount++;
        }
        
        // 씬 저장
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        
        EditorUtility.DisplayDialog(
            "완료",
            $"총 {changedCount}개의 폰트가 '{newFont.name}'으로 변경되었습니다!",
            "확인"
        );
        
        Debug.Log($"[Font Changer] {changedCount}개의 TextMeshPro 폰트가 변경되었습니다.");
    }
    }
}
