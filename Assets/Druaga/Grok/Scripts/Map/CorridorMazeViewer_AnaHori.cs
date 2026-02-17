using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CorridorMazeViewer_AnaHori : MonoBehaviour
{
    [Header("=== マップ設定 ===")]
    [SerializeField] private int mapWidth = 51;     // 奇数推奨（5以上）
    [SerializeField] private int mapHeight = 51;    // 奇数推奨

    [Header("=== プレハブ（基本） ===")]
    [SerializeField] private GameObject wallPrefab;     // 壁（Cubeなど、高さ2くらいが良い）
    [SerializeField] private GameObject floorPrefab;    // 床（任意・なくてもOK）

    [Header("=== 鍵・扉・プレイヤー・敵プレハブ（任意） ===")]
    [SerializeField] private GameObject keyPrefab;      // 鍵プレハブ（なければ黄色球体）
    [SerializeField] private GameObject doorPrefab;     // 扉プレハブ（なければ赤壁）
    [SerializeField] private GameObject playerPrefab;   // プレイヤープレハブ（なければ青立方体）
    [SerializeField] private EnemyDatabase enemyDatabase; // EnemyDatabase（敵プレハブ取得用）

    [Header("=== 敵設定 ===")]
    [SerializeField] private List<string> availableEnemyIds = new List<string> { "slime", "goblin" }; // FloorData登録のenemyIdをここにリスト（Inspectorで追加）
    [SerializeField][Range(5, 20)] private int enemyCount = 12; // 敵の数

    [Header("=== 表示調整 ===")]
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private Vector3 origin = Vector3.zero;

    // 自動配置結果
    [Header("=== 生成結果（自動更新）===")]
    [SerializeField] private Vector2Int autoStartPos;
    [SerializeField] private Vector2Int autoKeyPos;
    [SerializeField] private Vector2Int autoDoorPos;

    private int[,] map; // 0=壁, 1=通路
    private List<GameObject> spawned = new List<GameObject>();
    private List<GameObject> enemyObjects = new List<GameObject>(); // 敵専用リスト（後でクリア用）
    private GameObject keyObject, doorObject, playerObject;

    private void Start()
    {
        GenerateAndBuildMaze();
    }

    private void GenerateAndBuildMaze()
    {
        ClearPrevious();

        map = new int[mapWidth, mapHeight];

        // 1. 全部壁で初期化
        for (int x = 0; x < mapWidth; x++)
            for (int y = 0; y < mapHeight; y++)
                map[x, y] = 0; // 壁

        // 2. 外周を通路にする（閉領域防止）
        for (int x = 0; x < mapWidth; x++)
        {
            map[x, 0] = 1;
            map[x, mapHeight - 1] = 1;
        }
        for (int y = 0; y < mapHeight; y++)
        {
            map[0, y] = 1;
            map[mapWidth - 1, y] = 1;
        }

        // 3. 穴掘り開始
        DigMaze();

        // 4. ★ スタート・鍵・扉の自動配置（通路リスト作成）
        List<Vector2Int> availableFloors = PlaceStartKeyDoor();

        // 5. ★ 敵の自動配置（残った通路からランダム配置）
        PlaceEnemies(availableFloors);

        // 6. 3Dオブジェクトとして配置
        BuildMazeVisual();

        Debug.Log($"穴掘り法迷路生成完了！\nプレイヤー:{autoStartPos}, 鍵:{autoKeyPos}, 扉:{autoDoorPos}, 敵:{enemyCount}体");
    }

    private void DigMaze()
    {
        // 掘り可能な起点候補（奇数座標）
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 1; x < mapWidth - 1; x += 2)
            for (int y = 1; y < mapHeight - 1; y += 2)
            {
                candidates.Add(new Vector2Int(x, y));
            }

        // ランダムにシャッフル
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int rnd = Random.Range(0, i + 1);
            var temp = candidates[i];
            candidates[i] = candidates[rnd];
            candidates[rnd] = temp;
        }

        // 順番に掘る
        foreach (var start in candidates)
        {
            if (map[start.x, start.y] == 0)
            {
                DigFrom(start);
            }
        }
    }

    private void DigFrom(Vector2Int pos)
    {
        map[pos.x, pos.y] = 1;

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        Shuffle(directions);

        foreach (var dir in directions)
        {
            Vector2Int next = pos + dir * 2;
            Vector2Int mid = pos + dir;

            if (IsInBounds(next) && map[next.x, next.y] == 0)
            {
                map[mid.x, mid.y] = 1;
                map[next.x, next.y] = 1;
                DigFrom(next);
            }
        }
    }

    // ★ スタート・鍵・扉の自動配置（戻り値：残った通路リスト）
    private List<Vector2Int> PlaceStartKeyDoor()
    {
        // 全通路位置をリスト化
        List<Vector2Int> floors = new List<Vector2Int>();
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                if (map[x, y] == 1)
                {
                    floors.Add(new Vector2Int(x, y));
                }
            }
        }

        if (floors.Count == 0) return floors;

        // スタート：通路の中央寄り（外周除く）
        var innerFloors = floors.Where(p => p.x >= 3 && p.x <= mapWidth - 4 && p.y >= 3 && p.y <= mapHeight - 4).ToList();
        autoStartPos = innerFloors.Count > 0 ? innerFloors[Random.Range(0, innerFloors.Count)] : floors[Random.Range(0, floors.Count / 4)];
        floors.Remove(autoStartPos);

        // 鍵：スタートからマンハッタン距離で遠い通路（内側）
        var keyCandidates = floors.Where(p => Mathf.Abs(p.x - autoStartPos.x) + Mathf.Abs(p.y - autoStartPos.y) > 10 &&
                                             p.x >= 5 && p.x <= mapWidth - 6 && p.y >= 5 && p.y <= mapHeight - 6).ToList();
        autoKeyPos = keyCandidates.Count > 0 ? keyCandidates[Random.Range(0, keyCandidates.Count)] : floors[Random.Range(floors.Count / 2, floors.Count)];
        floors.Remove(autoKeyPos);

        // 扉：外周の端っこ（角付近優先）
        var doorCandidates = floors.Where(p => (p.x <= 2 || p.x >= mapWidth - 3 || p.y <= 2 || p.y >= mapHeight - 3)).ToList();
        autoDoorPos = doorCandidates.Count > 0 ? doorCandidates[Random.Range(0, doorCandidates.Count)] : floors[^1];
        floors.Remove(autoDoorPos);

        // Inspectorに反映
        autoStartPos = autoStartPos;
        autoKeyPos = autoKeyPos;
        autoDoorPos = autoDoorPos;

        return floors; // 敵配置用に残り通路を返す
    }

    // ★ 新規：敵の自動配置（FloorData登録のenemyIdからランダム選択）
    private void PlaceEnemies(List<Vector2Int> availableFloors)
    {
        if (availableFloors.Count == 0 || availableEnemyIds.Count == 0 || enemyDatabase == null) return;

        enemyObjects.Clear();

        for (int i = 0; i < enemyCount; i++)
        {
            if (availableFloors.Count == 0) break;

            // ランダム通路選択
            int floorIdx = Random.Range(0, availableFloors.Count);
            Vector2Int enemyPos = availableFloors[floorIdx];
            availableFloors.RemoveAt(floorIdx);

            // FloorData登録のenemyIdからランダム選択
            string enemyId = availableEnemyIds[Random.Range(0, availableEnemyIds.Count)];

            // EnemyDatabaseからプレハブ取得
            var enemyData = enemyDatabase.GetEnemy(enemyId);
            if (enemyData == null || enemyData.modelPrefab == null) continue;

            // 配置
            Vector3 enemyWorld = origin + new Vector3(enemyPos.x * cellSize, 0.5f, enemyPos.y * cellSize);
            var enemyObj = Instantiate(enemyData.modelPrefab, enemyWorld, Quaternion.identity, transform);
            enemyObjects.Add(enemyObj);
            spawned.Add(enemyObj);

            // ラベル（デバッグ用）
            CreateLabel(enemyObj, "ENEMY", Color.green);
        }
    }

    private bool IsInBounds(Vector2Int p)
    {
        return p.x >= 0 && p.x < mapWidth && p.y >= 0 && p.y < mapHeight;
    }

    private void Shuffle<T>(T[] array)
    {
        for (int i = array.Length - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    private void BuildMazeVisual()
    {
        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                Vector3 worldPos = origin + new Vector3(x * cellSize, 0, y * cellSize);

                if (map[x, y] == 1)
                {
                    if (floorPrefab != null)
                    {
                        var floor = Instantiate(floorPrefab, worldPos + Vector3.down * 0.05f, Quaternion.identity, transform);
                        spawned.Add(floor);
                    }
                }
                else
                {
                    if (wallPrefab != null)
                    {
                        var wall = Instantiate(wallPrefab, worldPos, Quaternion.identity, transform);
                        spawned.Add(wall);
                    }
                }
            }
        }

        // 鍵・扉・プレイヤーを配置
        PlaceKeyAndDoor();
        PlacePlayer();
    }

    // 鍵・扉のGameObject配置
    private void PlaceKeyAndDoor()
    {
        // 鍵（黄色球体 or プレハブ）
        Vector3 keyWorld = origin + new Vector3(autoKeyPos.x * cellSize, 0.5f, autoKeyPos.y * cellSize);
        if (keyPrefab != null)
        {
            keyObject = Instantiate(keyPrefab, keyWorld, Quaternion.identity, transform);
        }
        else
        {
            keyObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            keyObject.transform.position = keyWorld;
            keyObject.transform.localScale = Vector3.one * 0.6f;
            keyObject.GetComponent<Renderer>().material.color = Color.yellow;
            keyObject.transform.parent = transform;
        }
        spawned.Add(keyObject);
        CreateLabel(keyObject, "KEY", Color.yellow);

        // 扉（赤壁 or プレハブ）
        Vector3 doorWorld = origin + new Vector3(autoDoorPos.x * cellSize, 0, autoDoorPos.y * cellSize);
        if (doorPrefab != null)
        {
            doorObject = Instantiate(doorPrefab, doorWorld, Quaternion.identity, transform);
        }
        else
        {
            doorObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            doorObject.transform.position = doorWorld;
            doorObject.transform.localScale = new Vector3(1.2f, 2f, 0.3f);
            doorObject.GetComponent<Renderer>().material.color = Color.red;
            doorObject.transform.parent = transform;
        }
        spawned.Add(doorObject);
        CreateLabel(doorObject, "DOOR", Color.red);
    }

    // プレイヤーのGameObject配置
    private void PlacePlayer()
    {
        Vector3 playerWorld = origin + new Vector3(autoStartPos.x * cellSize, 0.5f, autoStartPos.y * cellSize);
        if (playerPrefab != null)
        {
            playerObject = Instantiate(playerPrefab, playerWorld, Quaternion.identity, transform);
        }
        else
        {
            playerObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            playerObject.transform.position = playerWorld;
            playerObject.transform.localScale = Vector3.one * 0.8f;
            playerObject.GetComponent<Renderer>().material.color = Color.cyan;
            playerObject.transform.parent = transform;
        }
        spawned.Add(playerObject);
        CreateLabel(playerObject, "PLAYER", Color.cyan);
    }

    // ラベル表示
    private void CreateLabel(GameObject parent, string text, Color color)
    {
        GameObject label = new GameObject("Label_" + text);
        label.transform.parent = parent.transform;
        label.transform.localPosition = Vector3.up * 1.5f;
        var renderer = label.AddComponent<TextMesh>();
        renderer.text = text;
        renderer.fontSize = 48; // 敵は少し小さく
        renderer.color = color;
        renderer.alignment = TextAlignment.Center;
        renderer.anchor = TextAnchor.MiddleCenter;
        spawned.Add(label);
    }

    private void ClearPrevious()
    {
        foreach (var obj in spawned)
            if (obj != null) DestroyImmediate(obj);
        foreach (var enemy in enemyObjects)
            if (enemy != null) DestroyImmediate(enemy);
        spawned.Clear();
        enemyObjects.Clear();
        keyObject = null;
        doorObject = null;
        playerObject = null;
    }

    [ContextMenu("再生成")]
    private void Regenerate()
    {
        GenerateAndBuildMaze();
    }
}