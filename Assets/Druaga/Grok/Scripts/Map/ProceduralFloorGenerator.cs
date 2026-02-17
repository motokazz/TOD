using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ProceduralFloorGenerator : MonoBehaviour
{
    [SerializeField] private int mapWidth = 51;
    [SerializeField] private int mapHeight = 51;

    // ProceduralFloorGenerator.cs の GenerateProcedural メソッドを以下のように修正/追加
    public void GenerateProcedural(FloorData data)
    {
        if (data == null) return;

        int width = data.proceduralMapWidth;
        int height = data.proceduralMapHeight;

        int[,] map = GenerateAnaHoriMaze(width, height);

        // 壁リスト生成
        data.walls.Clear();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 0) // 壁
                {
                    data.walls.Add(new Vector2Int(x, y));
                }
            }
        }

        // ★ 新規：通路リストを作成（敵配置用）
        List<Vector2Int> availableFloors = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    availableFloors.Add(new Vector2Int(x, y));
                }
            }
        }

        // ★ スタート位置を先に決める（内側から選ぶ）
        data.startPos = FindSuitableStartPosition(map, width, height);
        
        // ★ 宝箱位置をスタート位置に固定
        data.treasurePos = data.startPos;

        // ★ スタート位置を除外
        availableFloors.Remove(data.startPos);

        // ★ 鍵位置（スタートから遠い場所）
        data.keyPos = FindFarPosition(map, data.startPos, 12, width, height);
        availableFloors.Remove(data.keyPos);  // 鍵位置も除外

        // ★ 扉位置（鍵からさらに遠い場所、または外周寄り）
        // 重複防止のため、鍵位置とスタート位置を避けて選ぶ
        data.doorPos = FindFarPositionWithExclusions(map, data.startPos, data.keyPos, 15, width, height);

        // 敵配置（厳密にFloorData.enemiesを使う）
        PlaceEnemiesStrictly(data, availableFloors);
        Debug.Log($"プロシージャル生成完了 | 壁数: {data.walls.Count} | 敵数: {data.enemies.Count}");

        // ... 既存の壁リスト生成 ...

        // ★ 新規：不壊壁リストをクリア・生成
        data.indestructibleWalls.Clear();

        // 外周を不壊壁にする（例: 外周1マス分）
        for (int x = 0; x < width; x++)
        {
            // 下辺
            data.indestructibleWalls.Add(new Vector2Int(x, 0));
            // 上辺
            data.indestructibleWalls.Add(new Vector2Int(x, height - 1));
        }
        for (int y = 0; y < height; y++)
        {
            // 左辺
            data.indestructibleWalls.Add(new Vector2Int(0, y));
            // 右辺
            data.indestructibleWalls.Add(new Vector2Int(width - 1, y));
        }

        // 重複除去（角が2重になるので）
        HashSet<Vector2Int> uniqueIndestructible = new HashSet<Vector2Int>(data.indestructibleWalls);
        data.indestructibleWalls = new List<Vector2Int>(uniqueIndestructible);

        // 内部の壁リストから不壊壁を除外（普通の壊せる壁だけ残す）
        data.walls.RemoveAll(p => data.indestructibleWalls.Contains(p));

        Debug.Log($"不壊壁生成完了 | 数: {data.indestructibleWalls.Count}");
    }
    private Vector2Int FindFarPositionWithExclusions(int[,] map, Vector2Int exclude1, Vector2Int exclude2, int minDistance, int width, int height)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1) // 通路のみ
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (pos == exclude1 || pos == exclude2) continue; // 除外

                    int distFromStart = Mathf.Abs(x - exclude1.x) + Mathf.Abs(y - exclude1.y);
                    int distFromKey = Mathf.Abs(x - exclude2.x) + Mathf.Abs(y - exclude2.y);

                    // スタートから十分遠く、かつ鍵からも離れている
                    if (distFromStart >= minDistance && distFromKey >= 5)
                        candidates.Add(pos);
                }
            }
        }

        if (candidates.Count == 0)
        {
            // 候補がなければ鍵位置の近くから選ぶ（最悪ケース）
            Debug.LogWarning("扉位置の候補が見つかりませんでした。鍵近くから選択");
            return FindFarPosition(map, exclude1, minDistance / 2, width, height);
        }

        return candidates[Random.Range(0, candidates.Count)];
    }
    private void PlaceEnemiesStrictly(FloorData data, List<Vector2Int> availableFloors)
    {
        // data.enemies が空なら何もしない（敵なし）
        if (data.enemies == null || data.enemies.Count == 0)
        {
            Debug.Log("FloorData に敵が設定されていません → 敵なし");
            return;
        }

        // 通路リストからプレイヤー近くを除外
        availableFloors.RemoveAll(p => Vector2Int.Distance(p, data.startPos) < 5);

        // 設定済みの敵リストを直接使う（上書きを最小限に）
        // 位置だけ割り当て直す
        for (int i = 0; i < data.enemies.Count; i++)
        {
            if (availableFloors.Count == 0) break;

            int randIdx = Random.Range(0, availableFloors.Count);
            Vector2Int newPos = availableFloors[randIdx];
            availableFloors.RemoveAt(randIdx);

            // ★ 位置だけ上書き（種類・その他のデータはそのまま保持）
            data.enemies[i] = new EnemySpawnData
            {
                pos = newPos,
                enemyId = data.enemies[i].enemyId  // 元の種類をそのまま使う
                                                   // もし他のフィールド（例: level, hp など）がある場合もここで保持
            };
        }

        Debug.Log($"敵位置割り当て完了 | 設定数: {data.enemies.Count} | 配置数: {data.enemies.Count}");
    }

    private int[,] GenerateAnaHoriMaze(int width, int height)
    {
        int[,] map = new int[width, height];

        // 初期化：全部壁
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                map[x, y] = 0;

        // 外周は常に壁（useOuterPath 削除）
        // → 何もせず初期化のまま（壁のまま）

        // 穴掘り（内部を掘る）
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 1; x < width - 1; x += 2)
            for (int y = 1; y < height - 1; y += 2)
                candidates.Add(new Vector2Int(x, y));

        Shuffle(candidates);

        foreach (var start in candidates)
        {
            if (map[start.x, start.y] == 0)
                DigFrom(map, start, width, height);
        }

        return map;
    }

    private void DigFrom(int[,] map, Vector2Int pos, int width, int height)
    {
        map[pos.x, pos.y] = 1;

        List<Vector2Int> dirs = new List<Vector2Int>
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        Shuffle(dirs);

        foreach (var dir in dirs)
        {
            Vector2Int next = pos + dir * 2;
            Vector2Int mid = pos + dir;

            if (next.x >= 0 && next.x < width && next.y >= 0 && next.y < height && map[next.x, next.y] == 0)
            {
                map[mid.x, mid.y] = 1;
                map[next.x, next.y] = 1;
                DigFrom(map, next, width, height);
            }
        }
    }

    private void Shuffle(List<Vector2Int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            Vector2Int temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }

    private Vector2Int FindSuitableStartPosition(int[,] map, int width, int height)
    {
        List<Vector2Int> inner = new List<Vector2Int>();
        for (int x = 3; x < width - 3; x++)
            for (int y = 3; y < height - 3; y++)
                if (map[x, y] == 1) inner.Add(new Vector2Int(x, y));

        return inner.Count > 0 ? inner[Random.Range(0, inner.Count)] : new Vector2Int(width / 2, height / 2);
    }

    private Vector2Int FindFarPosition(int[,] map, Vector2Int from, int minDistance, int width, int height)
    {
        List<Vector2Int> candidates = new List<Vector2Int>();
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                if (map[x, y] == 1)
                {
                    int dist = Mathf.Abs(x - from.x) + Mathf.Abs(y - from.y);
                    if (dist >= minDistance)
                        candidates.Add(new Vector2Int(x, y));
                }
            }
        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : from;
    }
}