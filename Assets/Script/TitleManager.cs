using UnityEngine;
using UnityEngine.SceneManagement; // 씬 이동을 위해 필요

public class TitleManager : MonoBehaviour
{
    public void ClickNewGame()
    {
        // 1. 게임 데이터를 1챕터로 초기화해서 저장
        // "SavedChapter"라는 이름의 사물함에 숫자 1을 넣음
        PlayerPrefs.SetInt("SavedChapter", 1);
        PlayerPrefs.SetInt("SavedLoveScore", 0);

        // 저장을 확실하게 함
        PlayerPrefs.Save();

        // 2. 게임 씬(PlayScene)으로 이동
        // 주의: Build Settings에 등록된 씬 이름과 똑같아야 합니다!
        SceneManager.LoadScene("PlayScene");
    }
}