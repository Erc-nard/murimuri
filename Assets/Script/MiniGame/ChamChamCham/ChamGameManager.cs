using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ChamGameManager : MonoBehaviour
{
    [Header("Components")]
    public HeadDirectionDetector headDetector;
    
    // [추가] 배경 컨트롤러(AI)와 연결할 변수
    public BackgroundVideoController backgroundController; 

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
        
        // -----------------------------------------------------------
        // [동기화 핵심 로직] 
        // 1. 배경 스크립트가 이미 결정해놓은 값(currentDecision)을 가져옵니다.
        // -----------------------------------------------------------
        var visualDecision = backgroundController.currentDecision;
        HeadDir aiDir = HeadDir.Center; // 기본값 초기화

        // 2. 배경의 결정(AiChoice)을 게임의 방향(HeadDir)으로 변환
        if (visualDecision == BackgroundVideoController.AiChoice.Left)
        {
            aiDir = HeadDir.Left;
            aiDirectionText.text = "AI: 👈 왼쪽 공격!";
        }
        else if (visualDecision == BackgroundVideoController.AiChoice.Right)
        {
            aiDir = HeadDir.Right;
            aiDirectionText.text = "AI: 👉 오른쪽 공격!";
        }
        else 
        {
            // [추가된 부분] 중앙 공격 처리
            aiDir = HeadDir.Center;
            aiDirectionText.text = "AI: 👇 중앙 공격!";
        }

        yield return new WaitForSeconds(2.0f); 

        // -----------------------------------------------------------
        // 승패 판정 (중앙 포함)
        // -----------------------------------------------------------
        HeadDir playerDir = headDetector.currentDirection;
        bool isWin = false; 

        // 플레이어와 AI의 방향이 같으면 -> 패배 (공격 맞음)
        if (playerDir == aiDir)
        {
            gameText.text = "패배... 꽝! 💥\n(같은 방향)";
            gameText.color = Color.red;
            isWin = false;
        }
        else
        {
            // 방향이 다르면 -> 승리 (회피 성공)
            gameText.text = "승리!! 회피 성공! 🎉\n(다른 방향)";
            gameText.color = Color.green;
            isWin = true;
        }

        yield return new WaitForSeconds(2.0f); 
        EndGame(isWin); 
    }

    public void EndGame(bool isWin)
    {
        if (isWin) PlayerPrefs.SetString("GameResult", "WIN");
        else PlayerPrefs.SetString("GameResult", "LOSE");
        
        PlayerPrefs.Save();
        SceneManager.LoadScene("PlayScene");
    }
}