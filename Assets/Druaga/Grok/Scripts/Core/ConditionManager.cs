using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ConditionManager : MonoBehaviour
{
    public static ConditionManager Instance { get; private set; }

    private List<ICondition> activeConditions = new List<ICondition>();
    private FloorData currentFloorData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize(FloorData data)
    {
        currentFloorData = data;
        activeConditions.Clear();

        foreach (var condData in data.conditionDatas)
        {
            var cond = condData.CreateInstance();
            if (cond == null) continue;

            // PassPositionCondition など FloorData が必要なものはここで注入
            if (cond is PassPositionCondition pass)
                pass.SetFloorData(data);

            activeConditions.Add(cond);
            cond.Reset();
        }

        Debug.Log($"[ConditionManager] {activeConditions.Count} 個の条件を初期化しました");
    }

    public void ResetCurrentFloorConditions()
    {
        foreach (var c in activeConditions) c.Reset();
    }

    // ── イベント受信 ───────────────────────────────────────

    public void OnPlayerMoved(Vector2Int gridPos)
    {
        foreach (var cond in activeConditions.OfType<PassPositionCondition>())
        {
            cond.UpdatePlayerPosition(gridPos);
        }
        CheckTreasureCondition();
    }

    public void OnEnemyKilled(EnemyBase enemy)
    {
        if (enemy == null) return;
        string id = enemy.GetEnemyId();

        foreach (var cond in activeConditions.OfType<KillCountCondition>())
        {
            cond.OnEnemyKilled(id);
        }

        CheckTreasureCondition();
    }

    // MagicProjectile から呼び出したい場合（例：特定の敵を魔法で倒したとき）
    public void OnMagicProjectileHit()
    {
        foreach (var condition in activeConditions)
        {
            if (condition is BlockSpellCountCondition blockCond)
            {
                blockCond.OnSpellBlocked();
            }
        }

        // 必要ならここで即宝箱チェック
        CheckTreasureCondition();
    }

    private void CheckTreasureCondition()
    {
        if (FloorManager.Instance?.TreasureSpawned ?? false) return;

        bool allMet = activeConditions.All(c => c.IsMet(FloorManager.Instance));

        if (allMet)
        {
            FloorManager.Instance?.SpawnTreasure();
        }
    }

    // デバッグ用
    /*
    public string GetConditionStatus()
    {
        return string.Join("\n", activeConditions.Select(c => c.GetDescription() + " : " + c.GetProgress()));
    }
    */
}