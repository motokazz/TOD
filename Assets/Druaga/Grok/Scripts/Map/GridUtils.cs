using UnityEngine;

public static class GridUtils
{
    // ==========================================
    // 基本設定（MapBuilder3Dと一致させる）
    // ==========================================
    public const int GRID_WIDTH = 20;
    public const int GRID_HEIGHT = 15;
    public const float TILE_SIZE = 1f;          // 1マス = 1 Unity単位
    public static readonly Vector3 MAP_OFFSET = Vector3.zero; // マップ全体のオフセット（必要に応じて調整）

    // ==========================================
    // 座標変換
    // ==========================================

    /// <summary>
    /// グリッド座標 → ワールド座標（中心位置）
    /// </summary>
    public static Vector3 GridToWorld(Vector2Int gridPos)
    {
        return new Vector3(gridPos.x, 0, gridPos.y);
    }

    /// <summary>
    /// グリッド座標 → ワールド座標（マスの中心ではなく左下角）
    /// </summary>
    public static Vector3 GridToWorldCorner(Vector2Int gridPos)
    {
        return new Vector3(
            gridPos.x * TILE_SIZE + MAP_OFFSET.x,
            0f,
            gridPos.y * TILE_SIZE + MAP_OFFSET.z
        );
    }

    /// <summary>
    /// ワールド座標 → 最も近いグリッド座標（四捨五入）
    /// </summary>
    public static Vector2Int WorldToGrid(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt((worldPos.x - MAP_OFFSET.x) / TILE_SIZE);
        int y = Mathf.RoundToInt((worldPos.z - MAP_OFFSET.z) / TILE_SIZE);
        return new Vector2Int(x, y);
    }

    /// <summary>
    /// ワールド座標 → グリッド座標（床にスナップ、下駄履き防止用）
    /// </summary>
    public static Vector2Int WorldToGridFloor(Vector3 worldPos)
    {
        int x = Mathf.FloorToInt((worldPos.x - MAP_OFFSET.x) / TILE_SIZE);
        int y = Mathf.FloorToInt((worldPos.z - MAP_OFFSET.z) / TILE_SIZE);
        return new Vector2Int(x, y);
    }

    // ==========================================
    // 範囲チェック
    // ==========================================

    /// <summary>
    /// 指定位置がマップ範囲内かどうか
    /// </summary>
    public static bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < GRID_WIDTH &&
               pos.y >= 0 && pos.y < GRID_HEIGHT;
    }

    /// <summary>
    /// 指定位置がマップ範囲外（外周）かどうか
    /// </summary>
    public static bool IsOutOfBounds(Vector2Int pos)
    {
        return !IsInBounds(pos);
    }

    // ==========================================
    // 方向関連
    // ==========================================

    public static readonly Vector2Int[] FourDirections = new Vector2Int[]
    {
        new Vector2Int( 0,  1), // 上
        new Vector2Int( 1,  0), // 右
        new Vector2Int( 0, -1), // 下
        new Vector2Int(-1,  0)  // 左
    };

    public static readonly Vector2Int[] EightDirections = new Vector2Int[]
    {
        new Vector2Int( 0,  1), // 上
        new Vector2Int( 1,  1), // 右上
        new Vector2Int( 1,  0), // 右
        new Vector2Int( 1, -1), // 右下
        new Vector2Int( 0, -1), // 下
        new Vector2Int(-1, -1), // 左下
        new Vector2Int(-1,  0), // 左
        new Vector2Int(-1,  1)  // 左上
    };

    /// <summary>
    /// 方向ベクトルからインデックスを取得（0=上,1=右,2=下,3=左）
    /// </summary>
    public static int DirectionToIndex(Vector2Int dir)
    {
        if (dir == Vector2Int.up) return 0;
        if (dir == Vector2Int.right) return 1;
        if (dir == Vector2Int.down) return 2;
        if (dir == Vector2Int.left) return 3;
        return -1;
    }

    /// <summary>
    /// 反対方向を返す
    /// </summary>
    public static Vector2Int GetOppositeDirection(Vector2Int dir)
    {
        return new Vector2Int(-dir.x, -dir.y);
    }

    // ==========================================
    // 距離・近さ判定
    // ==========================================

    /// <summary>
    /// マンハッタン距離（グリッド移動距離）
    /// </summary>
    public static int ManhattanDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    /// <summary>
    /// チェビシェフ距離（斜め含む最短距離）
    /// </summary>
    public static int ChebyshevDistance(Vector2Int a, Vector2Int b)
    {
        return Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));
    }

    // ==========================================
    // デバッグ用
    // ==========================================

    /// <summary>
    /// グリッド位置をデバッグ表示（Gizmos）
    /// </summary>
    public static void DrawGridGizmos(Vector2Int pos, Color color, float size = 0.9f)
    {
        Vector3 center = GridToWorld(pos);
        Gizmos.color = color;
        Gizmos.DrawWireCube(center, new Vector3(size, 0.1f, size));
    }
}