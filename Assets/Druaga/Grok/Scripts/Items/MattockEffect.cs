using UnityEngine;

[CreateAssetMenu(fileName = "MattockEffect", menuName = "Druaga/ItemEffects/Mattock")]
public class MattockEffect : ItemEffectBase
{
    [Header("マトック パラメータ")]
    [SerializeField] private int initialUses = 3; // 初期使用回数

    public override void Apply(PlayerController player)
    {
        player.SetMattockUses(initialUses);
        Debug.Log($"マトック装備！ 使用回数: {initialUses}");
    }

    public override void Remove(PlayerController player)
    {
        base.Remove(player);
        player.SetMattockUses(0);
        Debug.Log("マトック効果解除");
    }
}