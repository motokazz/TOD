using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AchievementData", menuName = "Druaga/AchievementData")]
public class AchievementData : ScriptableObject
{
    [System.Serializable]
    public class Reward
    {
        public string rewardType;  // "Treasure", "StatUp", "Title", "Item"
        public int value;          // 宝箱ID, ステータス値, アイテムID
        public string description; // "攻撃力+10", "隠しタイトル解放"
    }

    public string id;              // "ach_kill_goblin_10"
    public string title;           // "ゴブリン狩りの達人"
    public string description;     // "ゴブリンを10体倒せ"
    public List<ConditionData> requiredConditions;  // 複数条件AND（全クリアで達成）
    public List<Reward> rewards;   // 達成時の報酬リスト
    public bool isSecret;          // シークレットアチーブメント（未達成時は非表示）
}