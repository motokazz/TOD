using UnityEngine;

[CreateAssetMenu(fileName = "JetBootsEffect", menuName = "Druaga/ItemEffects/JetBoots")]
public class JetBootsEffect : ItemEffectBase
{
    [Header("JetBoots パラメータ")]
    [SerializeField] private float speedMultiplier = 1.5f;

    public override void Apply(PlayerController player)
    {
        if (Use())
        {
            player.ApplySpeedMultiplier(speedMultiplier);
            Debug.Log($"ジェットブーツ装備！ 速度倍率: {speedMultiplier}x");
        }
    }

    public override void Remove(PlayerController player)
    {
        base.Remove(player);
        player.ApplySpeedMultiplier(1f);  // 速度を元に戻す
        Debug.Log("ジェットブーツ効果解除");
    }
}