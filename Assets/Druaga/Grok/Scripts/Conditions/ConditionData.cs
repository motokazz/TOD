using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ConditionData", menuName = "Druaga/ConditionData")]
public class ConditionData : ScriptableObject
{
    public ConditionType conditionType = ConditionType.KillCount;

    [Header("KillCount 用")]
    [SerializeField] public List<string> targetEnemyIds = new List<string>(); // ★ 複数ID対応
    public int requiredCount = 3;          // 必要な倒した数

    [Header("PassPosition 用")]
    [SerializeField] public Vector2Int targetPosition = Vector2Int.zero;  // 通過すべき位置（扉位置を入れることが多い）
    [SerializeField] public bool requireKeyAndOpenDoor = true;           // trueなら「鍵持ち＆扉開放時のみ通過とみなす」

    [Header("BlockSpellCount 用")]
    [SerializeField] public int requiredBlockCount = 3;     // 必要な防御回数（デフォルト3）


    [Header("その他の条件用")]
    [SerializeField] public float timeLimit = 60f;  // Time条件の場合
    [SerializeField] public string description = "";

    public ICondition CreateInstance()
    {
        switch (conditionType)
        {
            case ConditionType.KillCount:
                return new KillCountCondition(this);
            case ConditionType.Time:
                return new TimeCondition(this);
            case ConditionType.PassPosition:
                return new PassPositionCondition(this);
            case ConditionType.BlockSpellCount:
                return new BlockSpellCountCondition(this);
            // 他の条件を追加可能
            default:
                Debug.LogError("未対応の条件タイプ");
                return null;
        }
    }
}

public enum ConditionType
{
    KillCount,
    Time,
    PassPosition,
    BlockSpellCount,   // ← 追加
    // 他
}
