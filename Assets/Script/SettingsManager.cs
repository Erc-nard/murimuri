using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using TMPro; // TextMeshPro 사용

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer audioMixer; // 아까 만든 믹서
    public Slider bgmSlider;
    public Slider sfxSlider;

    [Header("Code Popup Settings")]
    public GameObject codePopupPanel;  // 팝업창 전체
    public InputField codeInput;   // 코드 입력칸
    public TextMeshProUGUI errorText;  // 빨간색 에러 메시지

    private const string SECRET_CODE = "aaaa"; // 정답 코드

    void Start()
    {
        // 시작할 때 팝업 숨기기
        codePopupPanel.SetActive(false);
        errorText.text = "";

        // 슬라이더 초기값 설정 (저장된 값이 있다면 불러오는 로직이 여기에 들어감)
        bgmSlider.value = 0.5f;
        sfxSlider.value = 0.5f;
    }

    // === 1. 오디오 조절 함수 ===
    public void SetBGMVolume(float volume)
    {
        // 소리는 로그 스케일로 조절해야 자연스러움 (0.0001 ~ 1 -> -80dB ~ 0dB)
        audioMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
    }

    public void SetSFXVolume(float volume)
    {
        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
    }

    // === 2. 코드 팝업 관련 함수 ===

    // 코드 버튼 눌렀을 때
    public void OpenPopup()
    {
        codePopupPanel.SetActive(true);
        codeInput.text = ""; // 입력창 초기화
        errorText.text = ""; // 에러 메시지 초기화
    }

    // 팝업 닫기 버튼 눌렀을 때
    public void ClosePopup()
    {
        codePopupPanel.SetActive(false);
    }

    // 확인 버튼 눌렀을 때 (검사 로직)
    public void CheckCode()
    {
        if (codeInput.text == SECRET_CODE)
        {
            Debug.Log("Correct Code");
            // 여기에 성공 시 실행할 코드 작성 (예: 아이템 지급, 씬 이동 등)
            ClosePopup(); // 성공하면 창 닫기
        }
        else
        {
            errorText.text = "Invalid Code";
            // 흔들리는 애니메이션을 추가하면 더 좋음
        }
    }
}