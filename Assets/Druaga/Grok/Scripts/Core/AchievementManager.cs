using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    [SerializeField] private List<AchievementData> allAchievements;  // Inspectorで割り当て or CSVインポート
    private HashSet<string> unlockedAchievements = new HashSet<string>();  // 達成済みID
    private List<AchievementData> activeAchievements = new List<AchievementData>();  // 進行中

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }

        LoadAchievements();
        InitActiveAchievements();
    }

    // フロア開始時：該当アチーブメントをアクティブ化
    /*
    public void ActivateAchievementsForFloor(FloorManager fm)
    {
        activeAchievements.Clear();
        activeAchievements = allAchievements.Where(a =>
            a.requiredConditions.Any(c => c.id.ToString() == fm.CurrentFloorNum)  // FloorDataにfloorId追加前提
        ).ToList();
    }
    */
    // 初期化
    public void InitActiveAchievements()
    {
        allAchievements.Clear();
    }

    // FloorManagerから呼ばれる：条件クリア通知
    public void NotifyConditionCleared(List<ICondition> clearedConditions, FloorManager fm)
    {
        foreach (var ach in activeAchievements)
        {
            if (unlockedAchievements.Contains(ach.id)) continue;

            bool allMet = ach.requiredConditions.All(cd =>
            {
                ICondition cond = cd.CreateInstance();  // ConditionDataの拡張メソッド前提
                // 簡易チェック（本番は状態保持版）
                return cond.IsMet(fm);
            });

            if (allMet)
            {
                UnlockAchievement(ach);
            }
        }
    }

    private void UnlockAchievement(AchievementData ach)
    {
        unlockedAchievements.Add(ach.id);
        SaveAchievements();

        // 報酬即時適用
        foreach (var reward in ach.rewards)
        {
            ApplyReward(reward);
        }

        // UI通知（Steam風ポップアップ）
        UIManager.Instance?.ShowAchievementPopup();
       
    }

    private void ApplyReward(AchievementData.Reward r) { /* 実装: 宝箱生成、ステータス加算など */ }

    // 永続化
    private void LoadAchievements()
    {
        int count = PlayerPrefs.GetInt("AchCount", 0);
        for (int i = 0; i < count; i++)
        {
            string id = PlayerPrefs.GetString($"Ach_{i}");
            unlockedAchievements.Add(id);
        }
    }

    private void SaveAchievements()
    {
        PlayerPrefs.SetInt("AchCount", unlockedAchievements.Count);
        int idx = 0;
        foreach (var id in unlockedAchievements)
        {
            PlayerPrefs.SetString($"Ach_{idx++}", id);
        }
        PlayerPrefs.Save();
    }

    // AchievementManager.cs に追加

    public void CheckAchievements()
    {
        if (activeAchievements == null || activeAchievements.Count == 0) return;

        foreach (var ach in activeAchievements.ToList()) // ToList() でコピー作成（中での削除対策）
        {
            bool allMet = true;

            foreach (var condData in ach.requiredConditions)
            {
                // ここで実際の条件インスタンスを探すか、FloorManager経由で状態を確認
                // （あなたの設計によって変わる）
                var condition = FindConditionInstance(condData); // ← 後述

                if (condition == null || !condition.IsMet(FloorManager.Instance))
                {
                    allMet = false;
                    break;
                }
            }

            if (allMet)
            {
                UnlockAchievement(ach);
                activeAchievements.Remove(ach); // 一度達成したらリストから外す（任意）
                Debug.Log($"アチーブメント達成: {ach.title} ({ach.id})");
            }
        }
    }

    // ヘルパー例（実装はあなたの条件管理に合わせて）
    private ICondition FindConditionInstance(ConditionData data)
    {
        var conditions = FloorManager.Instance?.activeConditions;

        if (conditions == null) return null;

        return conditions.FirstOrDefault(c =>
        {
            // ここに実際のマッチングを書く（例）
            // 仮に ConditionData が ICondition に紐づくフィールドを持っていると仮定
            return c.GetType().GetProperty("data")?.GetValue(c) == data;
            // またはもっと単純に：
            // return true;  // とりあえず全部通す（テスト用）
        });
    }
    // UI用
    public bool IsUnlocked(string achId) => unlockedAchievements.Contains(achId);
    public int UnlockedCount => unlockedAchievements.Count;
}