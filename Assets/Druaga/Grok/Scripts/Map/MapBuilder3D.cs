using UnityEngine;
using System.Collections.Generic;

public class MapBuilder3D : MonoBehaviour
{
    // Singleton（どこからでもアクセス可能）
    public static MapBuilder3D Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab;      // 壁のプレハブ
    [SerializeField] private GameObject floorPrefab;     // 床のプレハブ
    [SerializeField] private GameObject indestructibleWallPrefab; // ★ 新規：不壊壁

    [Header("Map Settings")]
    [SerializeField] private Vector2Int mapSize = new Vector2Int(20, 15); // 20×15
    [SerializeField] private float tileSize = 1f;        // 1マス = 1ユニット

    // 生成したオブジェクトを保持（クリア用）
    private List<GameObject> activeWalls = new List<GameObject>();
    private List<GameObject> activeFloors = new List<GameObject>();
    private List<GameObject> activeIndestructibleWalls = new List<GameObject>();

    private void Awake()
    {
        // Singleton初期化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    /// <summary>
    /// 指定されたFloorDataからマップを構築
    /// </summary>
    public void BuildMap(FloorData data)
    {
        ClearMap();

        if (data.useProcedural)
        {
            // ★ 壊せる壁
            if (data.walls != null && data.walls.Count > 0)
            {
                foreach (var pos in data.walls)
                {
                    Vector3 worldPos = GridUtils.GridToWorld(pos);
                    var wallObj = Instantiate(wallPrefab, worldPos, Quaternion.identity, transform);
                    activeWalls.Add(wallObj);  // 破壊可能リスト
                }
            }

            // ★ 不壊壁
            if (data.indestructibleWalls != null && data.indestructibleWalls.Count > 0)
            {
                foreach (var pos in data.indestructibleWalls)
                {
                    Vector3 worldPos = GridUtils.GridToWorld(pos);
                    var wallObj = Instantiate(indestructibleWallPrefab, worldPos, Quaternion.identity, transform);
                    activeIndestructibleWalls.Add(wallObj);  // ★ 新規リスト（破壊不可）
                }
            }
        }
    }

    /// <summary>
    /// 現在のマップをクリア（再利用時）
    /// </summary>
    public void ClearMap()
    {
        // 壊せる壁クリア
        foreach (var wall in activeWalls) if (wall != null) Destroy(wall);
        activeWalls.Clear();

        // 不壊壁クリア
        foreach (var wall in activeIndestructibleWalls) if (wall != null) Destroy(wall);
        activeIndestructibleWalls.Clear();

        // 床クリア（必要なら）
        foreach (var floor in activeFloors) if (floor != null) Destroy(floor);
        activeFloors.Clear();
    }

    public void DestroyWallAt(Vector2Int gridPos)
    {
        var currentData = FloorManager.Instance?.CurrentData;
        if (currentData == null || !currentData.GetWall(gridPos))
        {
            // 壁では無いので破壊不可
            return;
        }

        // ★ 不壊壁リストに含まれるかチェック（含まれればスキップ）
        if (currentData != null && currentData.indestructibleWalls != null &&
            currentData.indestructibleWalls.Contains(gridPos))
        {
            Debug.Log($"不壊壁のため破壊不可: {gridPos}");
            return;
        }

        // 壁を破棄（activeWallsから探してDestroy）
        Vector3 worldPos = GridUtils.GridToWorld(gridPos);
        for (int i = activeWalls.Count - 1; i >= 0; i--)
        {
            var wall = activeWalls[i];
            if (wall != null && Vector3.Distance(wall.transform.position, worldPos) < 0.1f)
            {
                Destroy(wall);
                activeWalls.RemoveAt(i);
                Debug.Log($"壁破壊成功: {gridPos}");

                // ★ 重要：マップの壁データを更新（これで通路として通れるようになる）
                if (currentData != null && currentData.useProcedural)
                {
                    currentData.walls.Remove(gridPos);
                }
                return;
            }
        }

        Debug.LogWarning($"壁が見つかりませんでした: {gridPos}");
    }
}