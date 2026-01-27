using UnityEngine;

namespace KanaRebuild
{
    [CreateAssetMenu(fileName = "NewStageData", menuName = "KanaRebuild/StageData")]
    public class StageData : ScriptableObject
    {
        public int stageIndex;
        [Range(0, 100)] public float targetSyncRate; // 이 단계 완료 시 목표 게이지
        
        [Header("Terminal Logs")]
        [TextArea(3, 5)] public string[] backgroundLogs; // 시작 시 흘러나올 코드들
        
        [Header("Problem")]
        [TextArea(3, 5)] public string problemCode; // 수정해야 할 문제 코드
        public string[] choices; // 선택지
        public int correctChoiceIndex; // 정답 번호 (0, 1...)

        [Header("Reaction")]
        public string successMessage;
        public string failMessage;
    }
}
