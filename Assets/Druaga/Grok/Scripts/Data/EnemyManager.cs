using UnityEngine;
using System.Collections.Generic;
// EnemyManager.cs などから使う例
public class EnemyManager : MonoBehaviour
{
    [SerializeField] private EnemyDatabase enemyDatabase;
    private List<EnemyBase> activeEnemies = new List<EnemyBase>();

    public void SpawnEnemies(List<EnemySpawnData> spawns)
    {
        foreach (var spawn in spawns)
        {
            var data = enemyDatabase.GetEnemy(spawn.enemyId);
            if (data == null) continue;

            var enemyObj = Instantiate(data.modelPrefab, GridUtils.GridToWorld(spawn.pos), Quaternion.identity);
            var enemy = enemyObj.GetComponent<EnemyBase>();
            enemy.Initialize(data); // HP, speed, aiTypeセット
            activeEnemies.Add(enemy);
        }
    }

    public void KillCount(string enemyType)
    {
        // FloorManagerの条件用: killCounts[enemyType]++
    }
}