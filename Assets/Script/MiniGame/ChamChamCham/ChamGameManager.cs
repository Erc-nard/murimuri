using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChamGameManager : MonoBehaviour
{
    [Header("Components")]
    public HeadDirectionDetector headDetector;

    [Header("UI")]
    public Text gameText;       
    public Text aiDirectionText;

    private bool isPlaying = false;

    void Start()
    {
        aiDirectionText.text = "";
        StartCoroutine(GameRoutine());
    }

    IEnumerator GameRoutine()
    {
        isPlaying = true;

        gameText.text = "준비..."; 
        yield return new WaitForSeconds(0.5f); 
        
        gameText.text = "참!";
        yield return new WaitForSeconds(0.5f);
        
        gameText.text = "참!!";
        yield return new WaitForSeconds(0.5f);

        gameText.text = "참!!!";
        
        int aiChoice = Random.Range(0, 2); 
        HeadDir aiDir = (aiChoice == 0) ? HeadDir.Left : HeadDir.Right;

        aiDirectionText.text = (aiDir == HeadDir.Left) ? "AI: 👈 왼쪽 공격!" : "AI: 👉 오른쪽 공격!";

        yield return new WaitForSeconds(0.2f); 

        // 승패 판정
        HeadDir playerDir = headDetector.currentDirection;
        bool isWin = false; 

        if (playerDir == HeadDir.Center)
        {
            gameText.text = "늦었어요! 😰\n(패배)";
            gameText.color = Color.yellow;
            isWin = false; 
        }
        else if (playerDir == aiDir)
        {
            gameText.text = "패배... 꽝! 💥\n(같은 방향)";
            gameText.color = Color.red;
            isWin = false;
        }
        else
        {
            gameText.text = "승리!! 회피 성공! 🎉\n(다른 방향)";
            gameText.color = Color.green;
            isWin = true;
        }

        yield return new WaitForSeconds(2.0f); 

        EndGame(isWin); 
    }

    public void EndGame(bool isWin)
    {
        // ★ [핵심] 결과만 저장하고 메인으로 복귀
        if (isWin)
        {
            PlayerPrefs.SetString("GameResult", "WIN");
        }
        else
        {
            PlayerPrefs.SetString("GameResult", "LOSE");
        }
        
        PlayerPrefs.Save();
        SceneManager.LoadScene("PlayScene");
    }
}