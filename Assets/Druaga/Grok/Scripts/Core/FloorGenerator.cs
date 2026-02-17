using UnityEngine;
using System.Collections.Generic;

public class FloorGenerator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MapBuilder3D mapBuilder;

    [Header("Prefabs")]
    [SerializeField] private GameObject keyPrefab;
    [SerializeField] private GameObject doorPrefab;
    [SerializeField] private GameObject treasurePrefab;

    // 生成オブジェクトの保持
    private GameObject doorObject;
    private GameObject spawnedTreasure;
    private List<GameObject> spawnedEnemies = new List<GameObject>();

    public void BuildFloor(FloorData data)
    {
        if (data == null) return;

        ClearFloor(); // 念のためクリア

        if (data.useProcedural)
            data.GenerateProceduralMap();

        BuildFloorCore(data);
    }

    void BuildFloorCore(FloorData data)
    {
        ClearFloor(); // 念のためクリア
                      // マップクリア & 再構築
        mapBuilder.ClearMap();
        ClearEnemies();  // ★ 追加：敵破棄
        ClearTreasure();
        mapBuilder.BuildMap(data);

        SpawnKey(data);
        SpawnDoor(data, true); // 最初は閉じた状態
        SpawnEnemies(data);

        Debug.Log($"FloorGenerator: フロア構築完了");
    }


    public void ClearFloor()
    {
        mapBuilder.ClearMap();

        if (doorObject != null) { Destroy(doorObject); doorObject = null; }
        ClearEnemies();
        ClearTreasure();
    }

    private void SpawnKey(FloorData data)
    {
        var keyObj = Instantiate(keyPrefab, GridUtils.GridToWorld(data.keyPos), Quaternion.identity);
        var pickup = keyObj.GetComponent<PickupItem>();
        pickup?.onPicked.AddListener(() => FloorManager.Instance.OnKeyPickedUp());
    }

    private void SpawnDoor(FloorData data, bool active)
    {
        if (doorObject != null) Destroy(doorObject);
        doorObject = Instantiate(doorPrefab, GridUtils.GridToWorld(data.doorPos), Quaternion.identity);
        doorObject.SetActive(active);
    }

    private void SpawnEnemies(FloorData data)
    {
        ClearEnemies();
        foreach (var spawn in data.enemies)
        {
            var enemyData = EnemyDatabase.Instance?.GetEnemy(spawn.enemyId);
            if (enemyData?.modelPrefab == null) continue;

            var enemyObj = Instantiate(enemyData.modelPrefab, GridUtils.GridToWorld(spawn.pos), Quaternion.identity);
            spawnedEnemies.Add(enemyObj);

            var enemy = enemyObj.GetComponent<EnemyBase>();
            enemy?.Initialize(enemyData);
        }
    }

    private void ClearEnemies()
    {
        foreach (var e in spawnedEnemies) if (e) Destroy(e);
        spawnedEnemies.Clear();
    }

    private void ClearTreasure()
    {
        if (spawnedTreasure) { Destroy(spawnedTreasure); spawnedTreasure = null; }
    }

    // 宝箱だけ後から出現させる用
    public void SpawnTreasure(FloorData data)
    {
        ClearTreasure();
        spawnedTreasure = Instantiate(treasurePrefab, GridUtils.GridToWorld(data.treasurePos), Quaternion.identity);
        var pickup = spawnedTreasure.GetComponent<PickupItem>();
        pickup?.onPicked.AddListener(() => FloorManager.Instance.OnTreasurePickedUp());
    }

    // 再スタート用
    public void RestartFloor(FloorData data) => BuildFloorCore(data);

    // 外部から扉を開く見た目変更したいとき用
    public void OpenDoorVisual()
    {
        if (doorObject == null) return;
        var r = doorObject.GetComponentInChildren<Renderer>();
        if (r) r.material.color = Color.green;
    }
}