using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Settings")]
    public AudioMixer audioMixer; // 'MasterMixer' 연결
    public Slider bgmSlider;
    public Slider sfxSlider;

    // (참고) 다른 스크립트에서 선언된 UI 패널이라고 가정
    // public GameObject codePopupPanel; 
    // public TextMeshProUGUI errorText; 

    void Start()
    {
        // codePopupPanel.SetActive(false); // UI 관련 코드는 상황에 맞게 사용
        // errorText.text = "";

        // 저장된 볼륨값 로드 (없으면 기본값 0.5)
        float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        float savedSfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        bgmSlider.value = savedBgm;
        sfxSlider.value = savedSfx;

        // 초기 볼륨 적용
        SetBGMVolume(savedBgm);
        SetSFXVolume(savedSfx);
    }

    public void SetBGMVolume(float volume)
    {
        // 0.0001f는 로그 계산시 -Infinity 방지용 최소값
        if (volume <= 0) volume = 0.0001f;

        audioMixer.SetFloat("BGM", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("BGMVolume", volume); // 값 저장
    }

    public void SetSFXVolume(float volume)
    {
        if (volume <= 0) volume = 0.0001f;

        audioMixer.SetFloat("SFX", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("SFXVolume", volume); // 값 저장
    }
}