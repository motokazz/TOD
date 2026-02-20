using UnityEngine;
using System;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // イベント定義（プレファブから発火される）
    public event Action<EnemyBase> OnEnemyDied;  // 敵死亡
    public event Action<string> OnEnemyKilled;   // 敵殺害（ID通知）
    public event Action<string> OnItemPicked;    // アイテム取得（ID通知）
    public event Action<Vector2Int> OnPlayerPassedPosition;  // 位置通過
    public event Action<Vector2Int> OnPlayerTouchedWall;     // 壁接触
    public event Action OnDoorOpened;        // 扉開放
    public event Action OnTreasureTaken;     // 宝箱取得

    // 発火メソッド（プレファブから呼ぶ）
    public void TriggerEnemyDied(EnemyBase enemy) => OnEnemyDied?.Invoke(enemy);
    public void TriggerEnemyKilled(string enemyId) => OnEnemyKilled?.Invoke(enemyId);
    public void TriggerItemPicked(string itemId) => OnItemPicked?.Invoke(itemId);
    public void TriggerPlayerPassedPosition(Vector2Int pos) => OnPlayerPassedPosition?.Invoke(pos);
    public void TriggerPlayerTouchedWall(Vector2Int pos) => OnPlayerTouchedWall?.Invoke(pos);
    public void TriggerDoorOpened() => OnDoorOpened?.Invoke();
    public void TriggerTreasureTaken() => OnTreasureTaken?.Invoke();
}