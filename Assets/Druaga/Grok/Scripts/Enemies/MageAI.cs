using UnityEngine;

public class MageAI : EnemyBase
{
    [Header("Mage Settings (Druaga Style)")]
    [SerializeField] private GameObject magicProjectilePrefab;  // 魔法弾プレハブ
    [SerializeField] private float attackRange = 8f;           // 攻撃可能距離
    [SerializeField] private float projectileSpeed = 5f;       // 魔法弾速度
    [SerializeField] private float sightAngle = 120f;          // 視野角（度）
    [SerializeField] private Vector2 attackCooldown= new Vector2(1f,3f);          // 視野角（度）
    [SerializeField] private float minSafeDistance = 3f;       // 近づきすぎたら後退する距離

    private float lastAttackTime = 0f;
    private bool hasLineOfSight = false;

    protected override void PerformAI()
    {
        if (player == null || player.IsDead) return;

        Vector3 toPlayer = player.transform.position - transform.position;
        float distance = toPlayer.magnitude;

        // 視線チェック
        hasLineOfSight = CanSeePlayer();

        if (hasLineOfSight)
        {
            // プレイヤーを見ている → 攻撃可能
            float ac = Random.Range(attackCooldown.x,attackCooldown.y);
            if (Time.time - lastAttackTime >= ac)
            {
                ShootMagic();
                lastAttackTime = Time.time;
            }

            // 距離調整：近すぎたら後退
            if (distance < minSafeDistance)
            {
                TryMoveAwayFromPlayer();
            }
            else
            {
                // 適度な距離を保ちつつ、少しランダム移動
                if (Random.value < 0.3f)
                {
                    TryMoveRandom();
                }
            }
        }
        else
        {
            // 見えない → ランダム移動
            TryMoveRandom();
        }
    }

    private bool CanSeePlayer()
    {
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        // 視野角内か
        if (angle > sightAngle / 2f) return false;

        // 壁チェック（Raycastでプレイヤーまで直線が見えるか）
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, directionToPlayer);
        if (Physics.Raycast(ray, out RaycastHit hit, attackRange))
        {
            if (hit.collider.CompareTag("Player") || hit.collider.CompareTag("Sword")|| hit.collider.CompareTag("Shield"))
            {
                return true;
            }
        }

        return false;
    }

    private void ShootMagic()
    {
        if (magicProjectilePrefab == null) return;

        Vector3 spawnPos = transform.position + Vector3.up * 1.2f;
        Vector3 direction = new Vector3(player.transform.position.x - spawnPos.x,0f,player.transform.position.z - spawnPos.z).normalized;

        var projectileObj = Instantiate(magicProjectilePrefab, spawnPos, Quaternion.LookRotation(direction));
        var projectile = projectileObj.GetComponent<MagicProjectile>();
        if (projectile != null)
        {
            projectile.Initialize(direction, projectileSpeed, contactDamage);  // ダメージは contactDamage を使う
        }

        // 魔法発射音やエフェクト（任意）
        // if (audioSource && magicSound) audioSource.PlayOneShot(magicSound);
    }

    private bool TryMoveAwayFromPlayer()
    {
        Vector2Int playerGrid = GridUtils.WorldToGrid(player.transform.position);
        Vector2Int dirAway = currentGridPos - playerGrid;  // 逆方向

        if (dirAway == Vector2Int.zero) return false;

        Vector2Int normalizedDir = new Vector2Int(
            dirAway.x != 0 ? (dirAway.x > 0 ? 1 : -1) : 0,
            dirAway.y != 0 ? (dirAway.y > 0 ? 1 : -1) : 0
        );

        return TryMove(normalizedDir);
    }

    // 既存の TryMove, TryMoveRandom などは EnemyBase から継承
}