using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EnemyDatabase", menuName = "Druaga/EnemyDatabase", order = 2)]
public class EnemyDatabase : ScriptableObject
{
    // Singleton + Resources.Loadで遅延ロード（シーン配置不要）
    private static EnemyDatabase _instance;
    public static EnemyDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<EnemyDatabase>("EnemyDatabase");
                if (_instance == null)
                {
                    Debug.LogError("Resources/EnemyDatabase.asset が見つかりません。ファイル名と場所を確認してください");
                }
            }
            return _instance;
        }
    }

    [SerializeField]
    private List<EnemyData> enemies = new List<EnemyData>();

    // IDで高速検索用の辞書
    private Dictionary<string, EnemyData> enemyLookup = new Dictionary<string, EnemyData>();


    private void OnEnable()
    {
        BuildLookupTable();
    }

    private void OnValidate()
    {
        BuildLookupTable();
    }

    private void BuildLookupTable()
    {
        enemyLookup.Clear();
        foreach (var enemy in enemies)
        {
            if (string.IsNullOrEmpty(enemy.enemyId))
            {
                Debug.LogWarning($"EnemyにIDが設定されていません: {enemy.enemyName}");
                continue;
            }

            if (enemyLookup.ContainsKey(enemy.enemyId))
            {
                Debug.LogWarning($"重複ID検出: {enemy.enemyId} ({enemy.enemyName})");
                continue;
            }

            enemyLookup.Add(enemy.enemyId, enemy);
        }
    }

    /// <summary>
    /// IDから敵データを取得（EnemyManagerでInstantiate用）
    /// </summary>
    public EnemyData GetEnemy(string enemyId)
    {
        foreach (var data in enemies)
        {
            if (data.enemyId == enemyId)
            {
                return data;
            }
        }
        Debug.LogWarning($"敵が見つかりません: ID = {enemyId}");
        return null;
    }

    /// <summary>
    /// すべての敵データを取得
    /// </summary>
    public List<EnemyData> GetAllEnemies()
    {
        return enemies;
    }

    /// <summary>
    /// 敵が存在するかどうか
    /// </summary>
    public bool Exists(string enemyId)
    {
        return enemyLookup.ContainsKey(enemyId);
    }

    public int Count => enemies.Count;
}