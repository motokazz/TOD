using UnityEngine;

[CreateAssetMenu(fileName = "BlockSpellCountCondition", menuName = "Druaga/Conditions/BlockSpellCount")]
public class BlockSpellCountCondition : ScriptableObject, ICondition
{
    [SerializeField] private ConditionData data;  // Inspectorで関連づけ（またはコンストラクタで注入）

    private int blockCount = 0;
    private bool isCompleted = false;

    public BlockSpellCountCondition(ConditionData sourceData)
    {
        this.data = sourceData;
    }

    public bool IsMet(FloorManager floorManager)
    {
        return blockCount >= data.requiredBlockCount;
    }

    public void Reset()
    {
        blockCount = 0;
        isCompleted = false;
    }

    public string GetDescription() => data?.description ?? "呪文を盾で防ぐ";

    public float GetProgress()
    {
        return Mathf.Clamp01((float)blockCount / data.requiredBlockCount);
    }

    // ★ ここが重要：魔法弾が盾に当たったときに呼ばれる
    public void OnSpellBlocked()
    {
        if (isCompleted) return;

        blockCount++;
        Debug.Log($"呪文防御！ 現在: {blockCount}/{data.requiredBlockCount}");

        if (blockCount >= data.requiredBlockCount)
        {
            isCompleted = true;
            Debug.Log("条件達成！ 盾で呪文を3回防御しました");
            // ここで即座に宝箱出現をトリガーしてもOK（後述）
        }
    }
}