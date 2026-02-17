using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CustomSequenceCondition", menuName = "Druaga/Conditions/CustomSequence")]
public class CustomSequenceCondition : ScriptableObject, ICondition
{
    [Header("基本設定")]
    [SerializeField] private string conditionName = "カスタムシーケンス";
    [SerializeField] private string description = "特定の行動を順番通りに行う";

    [Header("シーケンス定義")]
    [SerializeField] private List<SequenceStep> sequenceSteps = new List<SequenceStep>();

    [Header("設定")]
    [SerializeField] private bool resetOnWrongStep = true;        // 間違えたら最初から
    [SerializeField] private bool allowExtraActions = false;      // 余分な行動を許容するか

    // 現在の進捗
    private int currentStepIndex = 0;
    private bool isCompleted = false;

    // ==============================================
    // ICondition 実装
    // ==============================================
    public bool IsMet(FloorManager floorManager)
    {
        return isCompleted;
    }

    public void Reset()
    {
        currentStepIndex = 0;
        isCompleted = false;
    }

    public string GetDescription()
    {
        return description;
    }

    public float GetProgress()
    {
        if (sequenceSteps.Count == 0) return 0f;
        return (float)currentStepIndex / sequenceSteps.Count;
    }

    // ==============================================
    // シーケンス進行管理
    // ==============================================

    /// <summary>
    /// プレイヤーが行動したときに呼ばれる
    /// </summary>
    public bool OnActionPerformed(string actionId, Vector2Int position = default)
    {
        if (isCompleted) return true;

        // 現在の期待ステップを取得
        if (currentStepIndex >= sequenceSteps.Count)
        {
            isCompleted = true;
            return true;
        }

        var currentStep = sequenceSteps[currentStepIndex];

        // アクションが一致するかチェック
        bool matched = currentStep.Matches(actionId, position);

        if (matched)
        {
            currentStepIndex++;

            if (currentStepIndex >= sequenceSteps.Count)
            {
                isCompleted = true;
                Debug.Log($"[{conditionName}] シーケンス完了！");
            }

            return true;
        }
        else
        {
            // 間違えた場合
            if (resetOnWrongStep)
            {
                currentStepIndex = 0;
                Debug.Log($"[{conditionName}] シーケンス失敗 → リセット");
            }
            return false;
        }
    }

    // ==============================================
    // ステップ定義クラス
    // ==============================================
    [System.Serializable]
    public class SequenceStep
    {
        [SerializeField] public string actionType;      // "MoveTo", "FaceDirection", "KillEnemy", "WaitSeconds", "PressButton" など
        [SerializeField] public Vector2Int targetPos;   // 移動先 / 向き / 敵位置など
        [SerializeField] public string targetEnemyId;   // 特定の敵を倒す場合
        [SerializeField] public float waitDuration;     // 待機時間（秒）
        [SerializeField] public string description;     // デバッグ用

        public bool Matches(string performedAction, Vector2Int performedPos)
        {
            switch (actionType.ToLower())
            {
                case "moveto":
                    return performedAction == "Move" && performedPos == targetPos;

                case "facetoward":
                    return performedAction == "Face" && performedPos == targetPos;

                case "kill":
                    return performedAction == "Kill" && targetEnemyId == performedAction; // 簡易版

                case "wait":
                    // 時間待機は別途タイマー管理が必要（ここでは未実装例）
                    return false;

                case "touchwall":
                    return performedAction == "TouchWall" && performedPos == targetPos;

                default:
                    Debug.LogWarning($"未対応のアクションタイプ: {actionType}");
                    return false;
            }
        }

        public override string ToString()
        {
            return $"{actionType} → {targetPos} ({description})";
        }
    }
}