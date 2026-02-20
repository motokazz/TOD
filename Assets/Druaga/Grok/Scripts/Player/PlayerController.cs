using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections; // ★ Coroutine用追加
public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GridMover mover;
    [SerializeField] private SwordController swordController;

    [Header("Movement")]
    [SerializeField] private float baseMoveSpeed = 4f;

    // ★ 新規追加：HPシステム
    [Header("HP System")]
    [SerializeField] private int maxHP = 4; // ドルアーガ風HP4（調整可）
    [SerializeField] private float invincibleDuration = 1.5f; // 無敵時間（接触後1.5秒）

    [Header("Other")]
    [SerializeField] private int mattockRemainingUses = 0;

    private int currentHP;
    private Renderer playerRenderer; // 点滅用
    private Vector2Int currentGridPos;
    private Vector2Int currentFacing = Vector2Int.right;
    private Coroutine invincibleCoroutine;
    private bool isDead = false;
    private bool hasOneTimeInvincibility = false;
    private bool isInvincible = false;
    private float speedMultiplier = 1f;
    private Vector2 moveInput;

    public float CurrentMoveSpeed => baseMoveSpeed * speedMultiplier;
    public bool IsMoving => mover != null && mover.IsMoving;
    public bool IsDead => isDead;
    public Vector2Int CurrentGridPos => currentGridPos;
    public Vector2Int CurrentFacing => currentFacing;
    public bool IsSwordDrawn => swordController != null && swordController.IsDrawn;
    public bool HasOneTimeInvincibility => hasOneTimeInvincibility;
    public int CurrentHP => currentHP;
    public int MaxHP => maxHP;
    public bool IsInvincible => isInvincible;

    private void Awake()
    {
        if (mover == null) mover = GetComponent<GridMover>();
        if (swordController == null) swordController = GetComponentInChildren<SwordController>();
        currentHP = maxHP;
        playerRenderer = GetComponentInChildren<Renderer>(); // 点滅用
    }
    private void Update()
    {
        //if (isDead || IsMoving || !GameManager.Instance.IsGameActive) return;
        HandleMovementInput();
    }
    // 移動関連
    private void HandleMovementInput()
    {
        if (moveInput != Vector2.zero)
        {
            Vector2Int inputDir = Vector2Int.zero;
            // 入力の大きさで4方向に丸める
            if (Mathf.Abs(moveInput.x) > Mathf.Abs(moveInput.y))
            {
                inputDir = moveInput.x > 0 ? Vector2Int.right : Vector2Int.left;
            }
            else if (moveInput.y != 0)
            {
                inputDir = moveInput.y > 0 ? Vector2Int.up : Vector2Int.down;
            }

            if (inputDir != Vector2Int.zero)
            {
                currentFacing = inputDir;
                UpdateFacingVisual();
                Vector2Int nextPos = currentGridPos + inputDir;
                bool outOfBounds = GridUtils.IsOutOfBounds(nextPos);
                if (outOfBounds)
                {
                    //範囲外なので移動不可
                    return;
                }
                var currentData = FloorManager.Instance?.CurrentData;
                if (currentData == null || currentData.GetWall(nextPos))
                {
                    // 壁なので移動不可
                    return;
                }
                mover.MoveTo(nextPos, OnMoveComplete);
            }
        }
    }
    private void UpdateFacingVisual()
    {
        transform.forward = new Vector3(currentFacing.x, 0f, currentFacing.y);
    }
    private void OnMoveComplete()
    {
        currentGridPos = GridUtils.WorldToGrid(transform.position);
        // フロアマネージャに自分の居場所を伝える。Condition判定など
        FloorManager.Instance?.NotifyPlayerPosition(currentGridPos);
        // 扉チェック
        var floorManager = FloorManager.Instance;
        if (floorManager == null) return;
        Vector2Int doorPos = floorManager.CurrentData.doorPos;
        // 1. 扉の隣にいるか判定
        bool isAdjacentToDoor = IsAdjacent(currentGridPos, doorPos);
        if (isAdjacentToDoor && floorManager.HasKey && !floorManager.IsDoorOpen)
        {
            floorManager.OpenDoor();
        }
        // 2. 扉のマスにいる（＝入った）場合
        if (currentGridPos == doorPos)
        {
            if (floorManager.IsDoorOpen)
            {
                floorManager.OnFloorCleared();
            }
        }
    }
   
    // 補助メソッド：隣接判定（上下左右のみ）
    private bool IsAdjacent(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy == 1); // ちょうど1マス隣
    }
    // ★ 新規：ダメージ処理
    public void TakeDamage(int damage)
    {
        if (isInvincible) return; // ★ 無敵中は無視
        currentHP -= damage;
        Debug.Log($"プレイヤーダメージ！ 残りHP: {currentHP}/{maxHP}");
        if (currentHP <= 0)
        {
            if (hasOneTimeInvincibility)
            {
                hasOneTimeInvincibility = false;
                StartInvincibility(); // 無敵開始 + 点滅エフェクト
                currentHP = maxHP;
                Debug.Log("回復の薬発動！ 死を回避");
                return;
            }
            Die();
        }
        else
        {
            StartInvincibility(); // 無敵開始 + 点滅エフェクト
        }
    }
   
    // 無敵
    private void StartInvincibility()
    {
        isInvincible = true;
        if (invincibleCoroutine != null) StopCoroutine(invincibleCoroutine);
        invincibleCoroutine = StartCoroutine(InvincibilityCoroutine());
    }
    private IEnumerator InvincibilityCoroutine()
    {
        float timer = 0f;
        while (timer < invincibleDuration)
        {
            // 点滅エフェクト
            if (playerRenderer != null)
            {
                playerRenderer.enabled = Mathf.Sin(timer * 10f) > 0; // 高速点滅
            }
            timer += Time.deltaTime;
            yield return null;
        }
        // 無敵終了
        isInvincible = false;
        if (playerRenderer != null) playerRenderer.enabled = true;
        invincibleCoroutine = null;
    }
   
    // 死亡判定
    public void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("プレイヤー死亡");
        GameManager.Instance.OnPlayerDeath();
    }
    public void Respawn(Vector2Int startPos)
    {
        isDead = false;
        currentHP = maxHP; // ★ HP全快
        isInvincible = false; // ★ 無敵解除
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
        }
        if (playerRenderer != null) playerRenderer.enabled = true;
        mover.SnapTo(startPos);
        currentGridPos = startPos;
        if (swordController != null)
        {
            swordController.DrawSword(false);
        }
        currentFacing = Vector2Int.right;
        UpdateFacingVisual();
    }

    // ZAP
    public void OnZapReset()
    {
        hasOneTimeInvincibility = false;
        speedMultiplier = 1f;
        mattockRemainingUses = 0;
        isInvincible = false;
        if (invincibleCoroutine != null)
        {
            StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = null;
        }
        if (swordController != null)
        {
            swordController.ResetOnZap();
        }
    }
   
    // アイテム使用
    public void GrantOneTimeInvincibility()
    {
        hasOneTimeInvincibility = true;
        Debug.Log("1回無敵権利を付与（回復の薬）");
    }
    public void ApplySpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
        Debug.Log($"移動速度倍率適用: {multiplier}x");
    }
    public void SetMattockUses(int uses)
    {
        mattockRemainingUses = uses;
        Debug.Log($"マトック残り回数: {uses}");
    }
    public void UseMattock()
    {

        
        if (mattockRemainingUses <= 0)
        {
            Debug.Log("マトック残り0回です");
            return;
        }
        Vector2Int frontPos = currentGridPos + currentFacing;
        var currentData = FloorManager.Instance?.CurrentData;
        if (currentData == null || !currentData.GetWall(frontPos))
        {
            // 壁では無いので使用不可
            return;
        }
        // 壁を壊す
        MapBuilder3D.Instance.DestroyWallAt(frontPos);
        // 1回減算
        mattockRemainingUses--;
        Debug.Log($"壁破壊成功！ マトック残り: {mattockRemainingUses}");
        if (mattockRemainingUses <= 0)
        {
            Debug.Log("マトック使用回数0！ 効果終了");
        }
        

        // ... (壁破壊処理)
        if (true/* 壁破壊成功 */) EventManager.Instance?.TriggerPlayerTouchedWall(frontPos);  // 壁接触通知
    }

    private void UpdateGridPosition()
    {
        currentGridPos = GridUtils.WorldToGrid(transform.position);
        EventManager.Instance?.TriggerPlayerPassedPosition(currentGridPos);  // 位置通過通知
    }

    // 使用キー用コールバック
    public void OnMattock(InputAction.CallbackContext context)
    {
        if (context.performed && mattockRemainingUses > 0)
        {
            UseMattock();
        }
    }
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (swordController != null)
            {
                swordController.Attack(); // 剣攻撃処理
            }
        }
    }

}
