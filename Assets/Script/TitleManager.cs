using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleManager : MonoBehaviour
{
    public void ClickNewGame()
    {
        // 1. 대사 진행 순서를 0번(맨 처음)으로 초기화
        // "SavedLineIndex"는 CSV 리스트의 몇 번째 대사를 보여줄지 결정하는 키입니다.
        PlayerPrefs.SetInt("SavedLineIndex", 0);
        // (선택) 호감도 같은 다른 스탯도 초기화
        PlayerPrefs.SetInt("SavedLoveScore", 0);
        PlayerPrefs.Save(); // 저장 확정

        // 2. 게임 씬으로 이동
        SceneManager.LoadScene("PlayScene");
    }
}