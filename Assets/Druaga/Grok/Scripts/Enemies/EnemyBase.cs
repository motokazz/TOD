using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    [Header("References")]
    [SerializeField] protected Animator animator;
    [SerializeField] protected AudioSource audioSource;

    [Header("Stats")]
    [SerializeField] protected int maxHp = 1;
    [SerializeField] public float baseMoveSpeed = 1f;
    [SerializeField] protected int contactDamage = 999;
    [SerializeField] protected int attackDamage = 1;

    [Header("Audio")]
    [SerializeField] protected AudioClip deathSound;

    [Header("AI Settings")]
    [SerializeField] protected float decisionInterval = 0.5f;

    // 状態
    protected EnemyData enemyData;
    protected int currentHp;
    protected bool isDead = false;
    protected bool isActive = true;
    protected float moveSpeedMultiplier = 1f;

    // 外部参照
    protected PlayerController player;
    protected Vector2Int currentGridPos;

    // イベント
    public System.Action<EnemyBase> OnEnemyDied;

    protected virtual void Awake()
    {
        currentHp = maxHp;
        player = FindObjectOfType<PlayerController>();
        UpdateGridPosition();
    }

    protected virtual void Start()
    {
        StartCoroutine(AIUpdateRoutine());
    }

    protected virtual void Update()
    {
        if (isDead || !isActive || !GameManager.Instance.IsGameActive) return;
    }

    protected virtual IEnumerator AIUpdateRoutine()
    {
        while (isActive && !isDead)
        {
            if (player == null || player.IsDead)
            {
                yield return new WaitForSeconds(decisionInterval);
                continue;
            }

            UpdateGridPosition();
            PerformAI();
            yield return new WaitForSeconds(decisionInterval);
        }
    }

    protected void UpdateGridPosition()
    {
        currentGridPos = GridUtils.WorldToGrid(transform.position);
    }

    protected abstract void PerformAI();

    protected bool TryMoveTowardPlayer()
    {
        Vector2Int playerGrid = GridUtils.WorldToGrid(player.transform.position);
        Vector2Int dir = playerGrid - currentGridPos;
        if (dir == Vector2Int.zero) return false;

        Vector2 dirNormalized = new Vector2(dir.x, dir.y).normalized;
        Vector2Int roundedDir = new Vector2Int(
            Mathf.RoundToInt(dirNormalized.x),
            Mathf.RoundToInt(dirNormalized.y)
        );
        if (roundedDir == Vector2Int.zero) return false;

        Vector2Int nextPos = currentGridPos + roundedDir;
        var currentData = FloorManager.Instance?.CurrentData;
        if (currentData == null || currentData.GetWall(nextPos))
        {
            return false;
        }

        if (GridUtils.IsInBounds(nextPos))
        {
            StartCoroutine(MoveTo(nextPos));
            return true;
        }
        return false;
    }

    protected bool TryMoveRandom()
    {
        var directions = GridUtils.FourDirections;
        int randomIndex = Random.Range(0, directions.Length);
        Vector2Int dir = directions[randomIndex];
        Vector2Int nextPos = currentGridPos + dir;
        var currentData = FloorManager.Instance?.CurrentData;
        if (currentData == null || currentData.GetWall(nextPos))
        {
            return false;
        }

        if (GridUtils.IsInBounds(nextPos))
        {
            StartCoroutine(MoveTo(nextPos));
            return true;
        }
        return false;
    }

    protected bool TryMove(Vector2Int direction)
    {
        Vector2Int nextPos = currentGridPos + direction;
        var currentData = FloorManager.Instance?.CurrentData;
        if (currentData == null || currentData.GetWall(nextPos))
        {
            return false;
        }

        if (GridUtils.IsInBounds(nextPos))
        {
            StartCoroutine(MoveTo(nextPos));
            return true;
        }
        return false;
    }

    protected IEnumerator MoveTo(Vector2Int targetGrid)
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = GridUtils.GridToWorld(targetGrid);
        float duration = 1f / (baseMoveSpeed * moveSpeedMultiplier);
        duration = 0.5f;
        float elapsed = 0f;

        Vector3 dir = (targetPos - startPos).normalized;
        if (dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(dir);
        }

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;
        UpdateGridPosition();
    }

    public void MultiplyMoveSpeed(float multiplier)
    {
        baseMoveSpeed *= multiplier;
    }

    public virtual void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHp -= damage;
        StartCoroutine(FlashRed());
        if (currentHp <= 0)
        {
            Die();
        }
    }

    protected virtual IEnumerator FlashRed()
    {
        var renderer = GetComponentInChildren<Renderer>();
        if (renderer == null) yield break;
        Color originalColor = renderer.material.color;
        renderer.material.color = Color.red;
        yield return new WaitForSeconds(0.15f);
        renderer.material.color = originalColor;
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;
        isActive = false;

        if (animator) animator.SetTrigger("Die");
        if (audioSource && deathSound)
        {
            audioSource.PlayOneShot(deathSound);
        }
        OnEnemyDied?.Invoke(this);
        FloorManager.Instance?.OnEnemyDied(this);
        StartCoroutine(DestroyAfterDelay(0.8f));

        // イベント発火（判定はマネージャーに委任）
        EventManager.Instance?.TriggerEnemyDied(this);
        EventManager.Instance?.TriggerEnemyKilled(GetEnemyId());

        // 視覚効果など（そのまま）
        //base.Die();  // 元のDie処理（エフェクトなど）
        Destroy(gameObject, 0.5f);  // 遅延破棄
    }

    protected virtual IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (isDead) return;
        
        if (other.CompareTag("Sword"))
        {
            var swordControl = other.GetComponentInParent<SwordController>();
            if (swordControl != null) TakeDamage(swordControl.currentPower);
        }
        if (other.CompareTag("Player"))
        {
            var playerControl = other.GetComponent<PlayerController>();
            if (playerControl == null) return;
            if (playerControl.IsSwordDrawn)//抜剣状態
            {
                playerControl.TakeDamage(attackDamage);
            }
            else //納剣状態
            {
                playerControl.TakeDamage(contactDamage);
            }

        }
    }

    public virtual void Initialize(EnemyData data)
    {
        this.enemyData = data;
        maxHp = data.maxHp;
        baseMoveSpeed = data.moveSpeed;
        currentHp = maxHp;
        contactDamage = data.contactDamage;
        attackDamage = data.attackDamage;
        if (data.bodyColor != Color.white)
        {
            var renderer = GetComponentInChildren<Renderer>();
            if (renderer) renderer.material.color = data.bodyColor;
        }
    }

    public bool IsDead => isDead;
    public Vector2Int GridPosition => currentGridPos;

    public string GetEnemyId()
    {
        if (enemyData != null)
        {
            return enemyData.enemyId;
        }
        return "UnknownEnemy";
    }
}