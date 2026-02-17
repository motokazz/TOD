using UnityEngine;

public class TimeCondition : ICondition
{
    private readonly ConditionData data;
    private float elapsedTime = 0f;

    // ★ ここを追加：ConditionDataを受け取るコンストラクター
    public TimeCondition(ConditionData data)
    {
        this.data = data;
        Reset();
    }

    public bool IsMet(FloorManager manager)
    {
        elapsedTime += Time.deltaTime;
        bool cleared = elapsedTime >= data.timeLimit;

        if (cleared)
        {
            Debug.Log($"TimeConditionクリア！ 経過時間: {elapsedTime} / {data.timeLimit}");
        }

        return cleared;
    }

    public void Reset()
    {
        elapsedTime = 0f;
        Debug.Log($"TimeConditionリセット: 制限時間 {data.timeLimit}秒");
    }
}