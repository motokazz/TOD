using System.Collections.Generic;
using UnityEngine;

public class KillCountCondition : ICondition
{
    private readonly ConditionData data;
    private int currentKillCount = 0;
    private readonly HashSet<string> targetIds;

    public KillCountCondition(ConditionData data)
    {
        this.data = data;
        targetIds = new HashSet<string>(data.targetEnemyIds);
        Reset();
    }

    public bool IsMet(FloorManager manager)
    {
        return currentKillCount >= data.requiredCount;
    }

    public void Reset()
    {
        currentKillCount = 0;
    }

    // 敵死亡時に呼ぶ（後で追加）
    public void OnEnemyKilled(string killedEnemyId)
    {
        Debug.Log("kill : "+ killedEnemyId + targetIds);
        if (targetIds.Contains(killedEnemyId))
        {
            currentKillCount++;
            Debug.Log($"KillCount進捗: {currentKillCount}/{data.requiredCount} ({killedEnemyId})");
        }
    }
}