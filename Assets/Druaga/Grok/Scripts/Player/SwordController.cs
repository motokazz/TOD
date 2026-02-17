using UnityEngine;
using UnityEngine.InputSystem;

public class SwordController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject swordMesh;          // 剣のモデル（インスペクターで割り当て）
    [SerializeField] private GameObject shieldMesh;
    [SerializeField] private float attackRange = 2f;        // 攻撃範囲
    [SerializeField] public int currentPower = 1;  // 現在の剣の威力（初期1）
    [SerializeField] private LayerMask enemyLayer;

   
    private bool isDrawn = false;                           // 剣を抜いているか

    // Input System 用（PlayerInputで設定）
    private bool attackInput;

    private void Awake()
    {
        if (swordMesh == null)
        {
            Debug.LogError("swordMesh が割り当てられていません");
        }
        if (shieldMesh == null)
        {
            Debug.LogError("shieldMesh が割り当てられていません");
        }
        swordMesh.SetActive(false);  // 初期は納剣状態
    }

    private void Update()
    {
        if (attackInput)
        {
            attackInput = false;  // 1フレームだけ
            Attack();
        }
    }

    public void Attack()
    {
        Debug.Log("Attack() が呼ばれました！ isDrawn = " + isDrawn);
        if (isDrawn)
        {
            PerformAttack();  // 攻撃実行
            DrawSword(false);  // 抜剣
        }
        else
        {
            DrawSword(true);  // 抜剣
        }
    }

    public void DrawSword(bool drawn)
    {
        if (isDrawn == drawn) return;

        isDrawn = drawn;
        swordMesh.SetActive(drawn);
        shieldMesh.SetActive(!drawn);
        Debug.Log(drawn ? "剣を抜いた！" : "剣を納めた");
    }

    private void PerformAttack()
    {
        Debug.Log("PerformAttack() が呼ばれました！ 攻撃実行開始");
        /*
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;
        Vector3 halfExtents = new Vector3(1f, 1f, attackRange);

        if (Physics.BoxCast(origin, halfExtents, direction, out RaycastHit hit, Quaternion.identity, attackRange, enemyLayer))
        {
            if (hit.collider.TryGetComponent<EnemyBase>(out var enemy))
            {
                int damage = currentPower;  // 威力に応じたダメージ
                enemy.TakeDamage(damage);
                Debug.Log($"剣攻撃ヒット！ ダメージ: {damage}");
            }
        }
        else
        {
            Debug.Log("攻撃ヒットなし（敵がいない or 範囲外）");  // ← これが出ている可能性
        }
        */
    }

    public bool IsDrawn => isDrawn;

    public void ResetOnZap()
    {
        DrawSword(false);
    }

    // ★ 追加：剣の威力をアップグレード
    public void UpgradeSword(int newPower)
    {
        currentPower = newPower;
        Debug.Log($"剣の威力がアップグレード！ 現在の威力: {currentPower}");
        // 必要なら視覚効果やSE
        // swordMesh.GetComponent<Renderer>().material.color = Color.yellow; など
    }
}