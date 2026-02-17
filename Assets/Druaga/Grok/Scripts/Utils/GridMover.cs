using UnityEngine;
using System;
using System.Collections;

public class GridMover : MonoBehaviour
{
    [Header("移動設定")]
    [SerializeField] private float moveDuration = 0.25f;          // 1マス移動にかかる時間
    [SerializeField] private AnimationCurve moveCurve = AnimationCurve.Linear(0, 0, 1, 1);

    [Header("即振り向き設定")]
    [SerializeField] private float turnThreshold = 0.85f;  // 移動の85%完了で転換可能（調整可）

    [Header("状態")]
    public bool IsMoving { get; private set; } = false;

    private Vector3 startPosition;
    private Vector3 targetPosition;
    private float moveTimer = 0f;
    private Action onMoveComplete;

    public Vector2Int CurrentGridPos { get; private set; }
    
    
    private PlayerController player; // ★ 追加：PlayerController参照


    private void Awake()
    {
        player = GetComponent<PlayerController>();
        if (player == null)
        {
            Debug.LogError("PlayerControllerが見つかりません");
        }
    }
    private void Update()
    {
        if (!IsMoving)
        {
            return;
        }

        moveTimer += Time.deltaTime;
        moveDuration = GetMoveDuration();
        float t = moveTimer / moveDuration;

        if (t >= 1f)
        {
            transform.position = targetPosition;
            IsMoving = false;  // ★ ここで false に確実に戻す
            onMoveComplete?.Invoke();
            onMoveComplete = null;
        }
        else
        {
            // 通常Lerp
            float easedT = moveCurve.Evaluate(t);
            transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);
        }
    }

    /// <summary>
    /// 特定のグリッド座標に移動する
    /// </summary>
    
      public void MoveTo(Vector2Int targetGrid, Action onComplete = null)
     {
         // 移動中なら新しい移動をスキップ（重複防止）
         if (IsMoving)
         {
             Debug.Log("移動中なので新しいMoveToをスキップ: 現在位置 = " + transform.position);
             return;
         }

         IsMoving = true;
         startPosition = transform.position;
         targetPosition = GridUtils.GridToWorld(targetGrid);
         moveTimer = 0f;
         onMoveComplete = onComplete;

         // 向きを更新（オプション）
         Vector3 moveDir = (targetPosition - startPosition).normalized;
         if (moveDir != Vector3.zero)
         {
             transform.rotation = Quaternion.LookRotation(moveDir);
         }
     }
    
    private float GetMoveDuration()
    {
        return player != null ? 1f / player.CurrentMoveSpeed : 0.25f; // ★ ここで速度反映
    }


    public void SnapTo(Vector2Int gridPos)
    {
        transform.position = GridUtils.GridToWorld(gridPos);
        CurrentGridPos = gridPos;
        IsMoving = false;
        moveTimer = 0f;
        Debug.Log($"スナップ移動: {gridPos}");
    }

    public void CancelMove()
    {
        if (IsMoving)
        {
            transform.position = targetPosition;
            IsMoving = false;
            onMoveComplete?.Invoke();
            onMoveComplete = null;
            Debug.Log("移動を強制中断");
        }
    }
}