using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalAspectRatio : MonoBehaviour
{
    [Header("원하는 화면 비율 (가로 / 세로)")]
    public float targetAspect = 360.0f / 620.0f; 

    private static GlobalAspectRatio instance;

    void Awake()
    {
        // 1. 싱글톤 패턴: 이 오브젝트가 중복으로 생기는 것을 방지
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // ★ 씬이 넘어가도 파괴되지 않게 설정
        }
        else
        {
            Destroy(gameObject); // 이미 있으면 새로 생긴 건 삭제
            return;
        }
    }

    void OnEnable()
    {
        // 씬이 로드될 때마다 함수가 실행되도록 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // 스크립트가 꺼질 때 등록 해제 (에러 방지)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때마다 자동으로 호출되는 함수
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateCameraRect();
    }

    // ★ 실제 레터박스를 만드는 핵심 함수
    void UpdateCameraRect()
    {
        Camera cam = Camera.main;
        if (cam == null) return; // 메인 카메라가 없으면 패스

        // 1. 현재 화면 비율 계산
        float windowaspect = (float)Screen.width / (float)Screen.height;
        
        // 2. 원하는 비율과의 차이 계산
        float scaleheight = windowaspect / targetAspect;

        Rect rect = cam.rect;

        // A. 화면이 더 넓적할 때 (가로로 길 때) -> 좌우에 레터박스(Pillarbox) 필요
        if (scaleheight < 1.0f)
        {
            rect.width = 1.0f;
            rect.height = scaleheight;
            rect.x = 0;
            rect.y = (1.0f - scaleheight) / 2.0f;
        }
        // B. 화면이 더 길쭉할 때 (세로로 길 때) -> 위아래에 레터박스(Letterbox) 필요
        else
        {
            float scalewidth = 1.0f / scaleheight;
            
            rect.width = scalewidth;
            rect.height = 1.0f;
            rect.x = (1.0f - scalewidth) / 2.0f;
            rect.y = 0;
        }

        cam.rect = rect;
        
        // ★[중요] 레터박스 부분을 검은색으로 보이게 하려면
        // 카메라 뒤에 검은색 배경을 깔거나, GL.Clear 처리가 필요할 수 있습니다.
        // 가장 쉬운 방법은 카메라 배경색을 검정으로 하는 것입니다.
        // cam.backgroundColor = Color.black; 
    }
}