using UnityEngine;

public class MagicProjectile : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private int damage;

    public void Initialize(Vector3 dir, float spd, int dmg)
    {
        direction = dir;
        speed = spd;
        damage = dmg;

        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = direction * speed;  // ← linearVelocity → velocity に自動修正
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);  // 即死
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Shield"))
        {
            // ★ FloorManager経由で通知
            if (ConditionManager.Instance != null)
            {
               ConditionManager.Instance.OnMagicProjectileHit();
            }

            // 魔法弾は消滅（貫通しない）
            Destroy(gameObject);
            Debug.Log("呪文を盾で防御！");

            // SEやエフェクト（火花など）をここで再生しても良い
            return;
        }
        else if (other.CompareTag("Wall") || other.CompareTag("IndestructibleWall") )
        {
            Destroy(gameObject);
        }
    }
}