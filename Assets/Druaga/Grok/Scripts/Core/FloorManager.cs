using UnityEngine;

public class FloorManager : MonoBehaviour
{
    public static FloorManager Instance { get; private set; }

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

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
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
}