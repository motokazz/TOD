using UnityEngine;
using System.Collections.Generic;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }
    public readonly List<ICondition> activeConditions = new List<ICondition>();

    [Header("Sub Managers")]
    [SerializeField] private FloorGenerator generator;
    [SerializeField] private ConditionManager conditionManager;

    [Header("All Floor Data")]
    [SerializeField] private FloorData[] allFloorData;

    // 状態
    private FloorData currentData;
    private int currentFloorNum;
    private bool treasureSpawned;
    private bool treasureTaken;
    private bool keyTaken;
    private bool isDoorOpen;

    public FloorData CurrentData => currentData;
    public int CurrentFloorNum => currentFloorNum;
    public bool TreasureSpawned => treasureSpawned;
    public bool HasKey => keyTaken;
    public bool IsDoorOpen => isDoorOpen;
    public List<ICondition> ActiveConditions => new List<ICondition>();  // 読み取り専用で公開

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);

        // イベント購読
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEnemyKilled += HandleEnemyKilled;
            EventManager.Instance.OnItemPicked += HandleItemPicked;
            EventManager.Instance.OnPlayerPassedPosition += HandlePlayerPassedPosition;
            EventManager.Instance.OnPlayerTouchedWall += HandlePlayerTouchedWall;
            EventManager.Instance.OnDoorOpened += HandleDoorOpened;
            EventManager.Instance.OnTreasureTaken += HandleTreasureTaken;
        }
    }

    public void LoadFloor(int floorNum)
    {
        UnloadCurrentFloor();

        currentFloorNum = floorNum;
        currentData = allFloorData[floorNum - 1];

        // 状態リセット
        treasureSpawned = false;
        treasureTaken = false;
        keyTaken = false;
        isDoorOpen = false;

        // 生成
        generator.BuildFloor(currentData);

        // 条件
        conditionManager.Initialize(currentData);

        // プレイヤーリスポーン
        FindAnyObjectByType<PlayerController>()?.Respawn(currentData.startPos);

        Debug.Log($"Floor {floorNum} ロード完了");
    }
    public void RestartCurrentFloor()
    {
        UnloadCurrentFloor();

       // currentData = allFloorData[currentFloorNum - 1];

        // 状態リセット
        //treasureSpawned = false;
        //treasureTaken = false;
        keyTaken = false;
        isDoorOpen = false;

        // 生成
        generator.RestartFloor(currentData);

        // 条件
        conditionManager.Initialize(currentData);

        // プレイヤーリスポーン
        FindAnyObjectByType<PlayerController>()?.Respawn(currentData.startPos);

        Debug.Log($"Floor {currentFloorNum} ロード完了");
    }
    public void UnloadCurrentFloor()
    {
        generator.ClearFloor();
        conditionManager.ResetCurrentFloorConditions();
    }

    // ==================== コールバック ====================

    public void OnKeyPickedUp()
    {
        keyTaken = true;
        Debug.Log("鍵取得");
    }

    public void OnTreasurePickedUp()
    {
        treasureTaken = true;
        treasureSpawned = true;
        Debug.Log("宝箱取得 → 表ルート");
    }

    public void SpawnTreasure()
    {
        if (treasureSpawned) return;
        generator.SpawnTreasure(currentData);
        treasureSpawned = true;
    }

    public void OpenDoor()
    {
        if (isDoorOpen) return;
        isDoorOpen = true;
        generator.OpenDoorVisual();
    }

    public void OnFloorCleared()
    {
        int next = treasureTaken ? currentFloorNum + 1 : currentData.nextBackFloor;
        if (next > 60) { GameManager.Instance.GameClear(); return; }
        GameManager.Instance.LoadFloor(next);
    }

    // PlayerControllerから呼ばれる
    public void NotifyPlayerPosition(Vector2Int pos)
    {
        conditionManager.OnPlayerMoved(pos);
    }

    // EnemyBaseから呼ばれる
    public void OnEnemyDied(EnemyBase enemy)
    {
        conditionManager.OnEnemyKilled(enemy);
    }


    // 追加EventVer
    private void HandleEnemyKilled(string enemyId)
    {
        // 条件チェック（例: KillCountCondition）
        foreach (var condition in activeConditions)
        {
            if (condition is KillCountCondition killCond) killCond.OnEnemyKilled(enemyId);
            if (condition.IsMet(this)) { /* 宝箱出現など */ SpawnTreasure(); }
        }

        // アチーブメントチェック
        AchievementManager.Instance?.CheckAchievements();
        Debug.Log($"敵殺害イベント受信: {enemyId}");
    }

    private void HandleItemPicked(string itemId)
    {
        // アイテム追加処理（ItemInventory.AddItem(itemId)）
        ItemInventory.Instance?.AddItem(itemId);
        Debug.Log($"アイテム取得イベント受信: {itemId}");
    }

    private void HandlePlayerTouchedWall(Vector2Int pos)
    {
        Debug.Log($"プレイヤーが壁に触れました: {pos}");

        // 壁接触系の条件を進める（TouchSpecificWallCondition など）
        foreach (var condition in activeConditions)
        {
            if (condition is TouchSpecificWallCondition touchCondition)
            {
                touchCondition.OnPlayerTouchedWall(pos);

                // 条件が満たされたら何かする（例: 宝箱出現、ログなど）
                if (condition.IsMet(this))
                {
                    Debug.Log("特定の壁接触条件クリア！");
                    // SpawnTreasureIfNeeded(); など
                }
            }
        }
    }

    private void HandleDoorOpened()
    {
        Debug.Log("扉が開いたイベントを受信しました");

        // ここに扉開放時の処理を書く（必要に応じて）
        if (currentData != null)
        {
            // 例: 扉通過条件を進める
            foreach (var condition in activeConditions)
            {
                if (condition is PassPositionCondition passCond)
                {
                    // 扉位置を通過したとみなす（または専用フラグを立てる）
                    passCond.UpdatePlayerPosition(currentData.doorPos);

                    if (condition.IsMet(this))
                    {
                        Debug.Log("扉通過条件クリア！");
                        // 必要なら宝箱出現や次のアクション
                        // SpawnTreasureIfNeeded();
                    }
                }
            }
        }

        // 扉の見た目を変更（すでに DoorController で処理している場合不要）
        // if (doorObject != null) { ... 開いたアニメーションや色変更 ... }

        // 次フロアへの移動は DoorController の OnTriggerEnter で既に呼んでいる場合が多い
    }

    // 他のハンドラー（HandlePlayerPassedPosition, HandlePlayerTouchedWallなど）も同様に実装
    // 例: HandlePlayerPassedPosition(Vector2Int pos) { if (pos == doorPos) OnFloorCleared(); }

    private void OnDisable()
    {
        // 購読解除（メモリリーク防止）
        if (EventManager.Instance != null)
        {
            EventManager.Instance.OnEnemyKilled -= HandleEnemyKilled;
            // 他の解除も
        }
    }
    private void HandlePlayerPassedPosition(Vector2Int pos)
    {
        // ここに「プレイヤーが指定位置を通過した」時の処理を書く
        Debug.Log($"プレイヤーが位置 {pos} を通過しました");

        // 例: 扉通過チェック
        if (currentData != null && pos == currentData.doorPos)
        {
            if (IsDoorOpen)
            {
                OnFloorCleared();
                Debug.Log("扉通過 → 次フロアへ！");
            }
        }

        // 条件チェック（PassPositionCondition など）
        foreach (var condition in activeConditions)
        {
            if (condition is PassPositionCondition passCond)
            {
                passCond.UpdatePlayerPosition(pos);
                if (condition.IsMet(this))
                {
                    // 条件クリア時の処理（宝箱出現など）
                    SpawnTreasureIfNeeded();
                }
            }
        }
    }

    private void HandleTreasureTaken()
    {
        Debug.Log("宝箱取得イベントを受信");
        treasureTaken = true;
        // クリア条件チェックやサウンドなど
    }
    // FloorManager.cs
    private void SpawnTreasureIfNeeded()
    {
        if (treasureSpawned || treasureTaken) return;

        var generator = GetComponent<FloorGenerator>(); // 同じGameObjectにある場合
                                                        // または FindObjectOfType<FloorGenerator>();

        if (generator == null)
        {
            Debug.LogError("FloorGeneratorが見つかりません");
            return;
        }

        generator.SpawnTreasure(currentData);
        treasureSpawned = true;
    }
}