using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ChamGameManager : MonoBehaviour
{
    [Header("Components")]
    public HeadDirectionDetector headDetector;

    [Header("UI")]
    public Text gameText;       // "준비", "참!", "승리!" 텍스트
    public Text aiDirectionText;// AI가 선택한 방향 표시 (나중에 손가락 이미지로 교체 가능)
    public Button startButton;  // 시작 버튼

    private bool isPlaying = false;

    void Start()
    {
        aiDirectionText.text = "";
        gameText.text = "참참참 게임\n(버튼을 눌러 시작)";
        startButton.onClick.AddListener(StartGame);
    }

    void StartGame()
    {
        if (isPlaying) return;
        StartCoroutine(GameRoutine());
    }

    IEnumerator GameRoutine()
    {
        isPlaying = true;
        startButton.gameObject.SetActive(false); // 버튼 숨김
        aiDirectionText.text = "🤖"; // AI 대기 표정

        // 1. 카운트다운
        gameText.text = "참!";
        yield return new WaitForSeconds(1.0f);
        
        gameText.text = "참!!";
        yield return new WaitForSeconds(1.0f);

        // 2. 판정 순간 (플레이어는 이 타이밍에 고개를 돌려야 함)
        gameText.text = "참!!!";
        
        // AI가 방향 결정 (왼쪽 or 오른쪽)
        // 0: 왼쪽, 1: 오른쪽
        int aiChoice = Random.Range(0, 2); 
        HeadDir aiDir = (aiChoice == 0) ? HeadDir.Left : HeadDir.Right;

        // AI 방향 표시
        aiDirectionText.text = (aiDir == HeadDir.Left) ? "AI: 👈 왼쪽 공격!" : "AI: 👉 오른쪽 공격!";

        yield return new WaitForSeconds(0.2f); // 찰나의 순간 대기 (인식 오차 보정)

        // 3. 승패 판정
        // 플레이어의 현재 머리 방향 가져오기
        HeadDir playerDir = headDetector.currentDirection;

        // 중앙을 보고 있으면 패배 (안 피했으니까)
        if (playerDir == HeadDir.Center)
        {
            gameText.text = "늦었어요! 😰\n(정면을 보고 있었음)";
            gameText.color = Color.yellow;
        }
        // 방향이 같으면 패배 (공격수 방향으로 고개를 돌림)
        else if (playerDir == aiDir)
        {
            gameText.text = "패배... 꽝! 💥\n(같은 방향)";
            gameText.color = Color.red;
        }
        // 방향이 다르면 승리 (회피 성공)
        else
        {
            gameText.text = "승리!! 회피 성공! 🎉\n(다른 방향)";
            gameText.color = Color.green;
        }

        // 4. 재시작 대기
        yield return new WaitForSeconds(2.0f);
        gameText.color = Color.white;
        gameText.text = "다시 하기?";
        startButton.gameObject.SetActive(true);
        isPlaying = false;
    }
}