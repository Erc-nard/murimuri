using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [Header("설정창")]
    public GameObject settingPanel;

    [Header("이어하기창")]
    public GameObject continuePanel;

    void Start()
    {
        settingPanel.SetActive(false);
        continuePanel.SetActive(false);
    }

    public void OpenSettings()
    {
        settingPanel.SetActive(true);
    }
    public void CloseSettings()
    {
        settingPanel.SetActive(false);
    }



    public void OpenContinue()
    {
        continuePanel.SetActive(true);
    }
    public void CloseContinue()
    {
        continuePanel.SetActive(false);
    }
}
