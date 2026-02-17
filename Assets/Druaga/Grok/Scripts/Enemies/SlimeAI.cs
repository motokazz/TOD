using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SlimeAI : EnemyBase
{
    [Header("Slime Settings")]
    [SerializeField] private float jumpChance = 0.3f;           // ジャンプする確率（一部スライムのみ）
    [SerializeField] private float jumpHeight = 0.8f;

    protected override void PerformAI()
    {
        // ランダムに動く
        if (Random.value < 0.7f)
        {
            TryMoveRandom();
        }
        else
        {
            // たまにプレイヤー方向に動く
            TryMoveTowardPlayer();
        }

        // ジャンプ（視覚的演出のみ、壁越えはしない）
        if (Random.value < jumpChance)
        {
            StartCoroutine(JumpAnimation());
        }
    }

    // ここが重要：非ジェネリックの IEnumerator を返す
    private IEnumerator JumpAnimation()
    {
        Vector3 startPos = transform.position;
        Vector3 peakPos = startPos + Vector3.up * jumpHeight;
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (t < 0.5f)
            {
                transform.position = Vector3.Lerp(startPos, peakPos, t * 2f);
            }
            else
            {
                transform.position = Vector3.Lerp(peakPos, startPos, (t - 0.5f) * 2f);
            }

            yield return null;
        }

        transform.position = startPos;
    }
}