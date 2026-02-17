using UnityEngine;

[CreateAssetMenu(fileName = "WhiteSwordEffect", menuName = "Druaga/ItemEffects/WhiteSword")]
public class WhiteSwordEffect : ItemEffectBase
{
    [Header("白の剣 パラメータ")]
    [SerializeField] private int swordPower = 2;

    public override void Apply(PlayerController player)
    {
        if (Use())
        {
            var sword = player.GetComponentInChildren<SwordController>();
            if (sword != null)
            {
                sword.UpgradeSword(swordPower);
                Debug.Log($"白の剣装備！ 剣威力: {swordPower}");
            }
        }
    }

    public override void Remove(PlayerController player)
    {
        base.Remove(player);
        var sword = player.GetComponentInChildren<SwordController>();
        if (sword != null)
        {
            sword.UpgradeSword(1);  // 威力リセット
        }
        Debug.Log("白の剣効果解除");
    }
}