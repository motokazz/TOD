using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FloorData", menuName = "Druaga/FloorData")]
public class FloorData : ScriptableObject
{
    public int floorNumber = 1;

    // プロシージャル生成用のパラメータ
    [Header("Procedural Settings")]
    public bool useProcedural = false;
    public int proceduralMapWidth = 15;
    public int proceduralMapHeight = 15;

    // 生成結果：壁の座標リスト
    [System.NonSerialized]
    public List<Vector2Int> walls = new List<Vector2Int>();  // ← 名前をwallsg → walls に戻しました

    public Vector2Int startPos;
    public Vector2Int keyPos;
    public Vector2Int doorPos;
    public Vector2Int treasurePos;

    public List<EnemySpawnData> enemies = new List<EnemySpawnData>();

    [Header("宝箱の中身")]
    public string treasureItemId = "WhiteSword";
    public List<ConditionData> conditionDatas = new List<ConditionData>();
    public int nextTableFloor = 0;
    public int nextBackFloor = 0;
    public List<int> zapTargetFloors = new List<int>();

    [System.NonSerialized]
    public List<Vector2Int> indestructibleWalls = new List<Vector2Int>();  // ★ 新規追加

    // プロシージャル生成を実行
    public void GenerateProceduralMap()
    {
        if (!useProcedural) return;

        ProceduralFloorGenerator generator = FindObjectOfType<ProceduralFloorGenerator>();
        if (generator == null)
        {
            GameObject go = new GameObject("ProceduralGenerator");
            generator = go.AddComponent<ProceduralFloorGenerator>();
        }

        generator.GenerateProcedural(this);
    }

    // 壁判定ヘルパー（今後使う場合）
    public bool GetWall(Vector2Int pos)
    {
        if (useProcedural)
        {
            if (pos.x < 0 || pos.x >= proceduralMapWidth ||
                pos.y < 0 || pos.y >= proceduralMapHeight)
                return true; // 範囲外は壁

            return walls.Contains(pos) || indestructibleWalls.Contains(pos); ;
        }
        else
        {
            // 手動データが不要になった場合のフォールバック
            // → 必要に応じて true/false に固定、または警告
            Debug.LogWarning($"GetWall({pos}) が呼ばれたが、手動モードで壁データがありません");
            return true; // 安全のため壁扱い（または false に変更可能）
        }
    }
}

[System.Serializable]
public class EnemySpawnData
{
    public Vector2Int pos;
    public string enemyId;
}