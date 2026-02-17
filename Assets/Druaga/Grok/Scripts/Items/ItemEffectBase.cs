using UnityEngine;

[CreateAssetMenu(fileName = "ItemEffectBase", menuName = "Druaga/ItemEffects/Base")]
public abstract class ItemEffectBase : ScriptableObject
{
    [Header("効果パラメータ")]
    [SerializeField] protected int useCount = 1;          // 使用回数（消耗品の場合）
    [SerializeField] protected bool isConsumable = true;  // 使い切りか（falseなら永続効果）

    [Header("ZAP時")]
    [SerializeField] protected bool lostOnZap = true;     // ZAP時に効果が失われるか

    /// <summary>
    /// 効果をプレイヤーに適用する
    /// </summary>
    public abstract void Apply(PlayerController player);

    /// <summary>
    /// 効果を解除する（ZAP時や期限切れ時）
    /// </summary>
    public virtual void Remove(PlayerController player)
    {
        // デフォルトでは何もしない（永続効果はここで解除）
        Debug.Log($"{name} の効果を解除しました");
    }

    /// <summary>
    /// 使用回数を1減らす（消耗品の場合）
    /// </summary>
    public virtual bool Use()
    {
        if (!isConsumable) return true;  // 永続効果は無限使用

        useCount--;
        Debug.Log($"{name} 使用残り: {useCount}");

        if (useCount <= 0)
        {
            Debug.Log($"{name} の使用回数が0になりました");
            return false;
        }

        return true;
    }

    /// <summary>
    /// ZAP時に効果が失われるかどうかを返す
    /// </summary>
    public bool IsLostOnZap => lostOnZap;
}