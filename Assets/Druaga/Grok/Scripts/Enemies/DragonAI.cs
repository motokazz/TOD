using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DragonAI : EnemyBase
{
    [Header("Dragon Boss Settings")]
    [SerializeField] private GameObject breathProjectilePrefab;
    [SerializeField] private GameObject miniDragonPrefab;
    [SerializeField] private float breathRange = 8f;
    [SerializeField] private int summonCount = 3;
    [SerializeField] private List<Vector2Int> summonPositions;

    [Header("Phases")]
    [SerializeField] private float phase2HpThreshold = 0.5f;
    [SerializeField] private float phase3HpThreshold = 0.25f;

    private enum DragonState { Patrol, Chase, BreathAttack, Summon, Roar }
    private DragonState currentState = DragonState.Patrol;
    private float stateTimer = 0f;

    private float lastBreathTime = 0f;
    private float lastSummonTime = 0f;
    private float breathCooldown = 3f;
    private float summonCooldown = 10f;

    private int currentPhase = 1;

    protected override void Awake()
    {
        base.Awake();
        maxHp = 20;
        currentHp = maxHp;
    }

    protected override void PerformAI()
    {
        UpdatePhase();
        UpdateState();

        switch (currentState)
        {
            case DragonState.Patrol:
                TryMoveRandom();
                break;
            case DragonState.Chase:
                TryMoveTowardPlayer();
                break;
            case DragonState.BreathAttack:
                if (Time.time - lastBreathTime >= breathCooldown / currentPhase)
                {
                    StartCoroutine(ShootBreath(3 + currentPhase - 1));
                    lastBreathTime = Time.time;
                }
                break;
            case DragonState.Summon:
                if (Time.time - lastSummonTime >= summonCooldown)
                {
                    SummonMiniDragons();
                    lastSummonTime = Time.time;
                }
                break;
            case DragonState.Roar:
                break;
        }

        stateTimer -= Time.deltaTime;
    }

    private void UpdatePhase()
    {
        if ((float)currentHp / maxHp <= phase3HpThreshold) currentPhase = 3;
        else if ((float)currentHp / maxHp <= phase2HpThreshold) currentPhase = 2;
    }

    private void UpdateState()
    {
        if (stateTimer <= 0f)
        {
            float rand = Random.value;
            if (rand < 0.4f) currentState = DragonState.Chase;
            else if (rand < 0.7f) currentState = DragonState.BreathAttack;
            else if (rand < 0.9f) currentState = DragonState.Summon;
            else currentState = DragonState.Patrol;

            stateTimer = 2f;
        }
    }

    private IEnumerator ShootBreath(int shotCount)
    {
        Vector3 dir = (player.transform.position - transform.position).normalized;
        for (int i = 0; i < shotCount; i++)
        {
            if (breathProjectilePrefab == null) continue;
            Vector3 spawnPos = transform.position + dir * 0.5f + Vector3.up * 1.5f;
            var projectileObj = Instantiate(breathProjectilePrefab, spawnPos, Quaternion.identity);
            var projectile = projectileObj.GetComponent<MagicProjectile>();
            if (projectile != null)
            {
                projectile.Initialize(dir, 5f, 999);  // dmg は無視されるのでOK
            }
            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SummonMiniDragons()
    {
        int summons = Mathf.Min(summonCount, summonPositions.Count);
        for (int i = 0; i < summons; i++)
        {
            int idx = Random.Range(0, summonPositions.Count);
            Vector2Int pos = summonPositions[idx];

            var currentData = FloorManager.Instance?.CurrentData;
            if (currentData == null || currentData.GetWall(pos))
            {
                // 壁なので召喚不可
                return;
            }

            if (miniDragonPrefab)
            {
                var mini = Instantiate(miniDragonPrefab, GridUtils.GridToWorld(pos), Quaternion.identity);
                var miniAI = mini.GetComponent<SlimeAI>();
                if (miniAI != null)
                {
                    miniAI.MultiplyMoveSpeed(1.5f);
                }
            }
        }
    }

    protected override void Die()
    {
        base.Die();

        // ★ 修正箇所：this を渡す（これで CS7036 解消）
        OnEnemyDied?.Invoke(this);

        // ボス死亡時の特別処理
        GameManager.Instance.OnGameClear(1);
    }

    public override void Initialize(EnemyData data)
    {
        base.Initialize(data);
    }
}