using UnityEngine;

public class PassPositionCondition : ICondition
{
    private readonly ConditionData data;
    private bool hasPassed = false;
    private FloorData floorData;  // 扉位置などを参照するため

    public PassPositionCondition(ConditionData data)
    {
        this.data = data;
    }
    
    // ★ ここがエラーになっているメソッドを実装
    public bool IsMet(FloorManager fm)
    {
        // すでに通過済みなら true を返す
        return hasPassed;
    }

    public bool IsSatisfied => hasPassed;

    public void UpdatePlayerPosition(Vector2Int playerPos)
    {
        if (hasPassed) return;

        if (data.requireKeyAndOpenDoor && floorData != null && playerPos == floorData.doorPos)
        {
            Debug.Log($"扉通過");
            var fm = FloorManager.Instance;
            if (fm != null && !fm.HasKey && !fm.IsDoorOpen)
            {
                hasPassed = true;
                Debug.Log($"扉通過条件クリア！ (鍵持ち＆開放状態)");
            }
        }
        // 基本：指定位置に到達したら通過
        else if (playerPos == data.targetPosition)
        {
                // 通常の位置通過 or 扉チェック不要の場合
                hasPassed = true;
                Debug.Log($"位置通過条件クリア！ {playerPos}");
        }
    }

    // FloorManager からフロアデータを注入（Init時に呼ぶ）
    public void SetFloorData(FloorData data)
    {
        this.floorData = data;
    }

    // ★ ここを追加：インターフェースの実装
    public void Reset()
    {
        hasPassed = false;
        // floorData は必要に応じて null にしてもOK（フロアロード時に再設定されるはず）
        // floorData = null;
        Debug.Log("PassPositionCondition リセット");
    }
}