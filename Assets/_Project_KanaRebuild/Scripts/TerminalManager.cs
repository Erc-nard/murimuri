using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace KanaRebuild
{
    public class TerminalManager : MonoBehaviour
    {
        public TextMeshProUGUI terminalText;
        public ScrollRect scrollRect;
        public float typingSpeed = 0.00025f; // 더 빠른 타이핑 속도

        public void ClearTerminal() => terminalText.text = "";

        public IEnumerator TypeLine(string line, Color color)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            terminalText.text += $"<color=#{colorHex}>></color> ";
            
            foreach (char c in line)
            {
                terminalText.text += c;
                yield return new WaitForSeconds(typingSpeed);
            }
            terminalText.text += "\n";
            
            // 한 줄 완성 후 자동 스크롤
            ScrollToBottom();
        }

        public void AddInstantLine(string line, Color color)
        {
            string colorHex = ColorUtility.ToHtmlStringRGB(color);
            terminalText.text += $"<color=#{colorHex}>{line}</color>\n";
            ScrollToBottom();
        }

        // 맨 아래로 스크롤 (약간 여유 있게)
        public void ScrollToBottom()
        {
            if (scrollRect == null || scrollRect.content == null)
            {
                return;
            }

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);
            
            // 0.05 = 바닥에서 약간 위에서 멈춤 (0 = 완전 바닥, 1 = 맨 위)
            scrollRect.verticalNormalizedPosition = 0.19f;
        }
    }
}
