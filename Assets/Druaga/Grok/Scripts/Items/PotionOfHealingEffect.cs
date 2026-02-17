using UnityEngine;

[CreateAssetMenu(fileName = "PotionOfHealingEffect", menuName = "Druaga/ItemEffects/PotionOfHealing")]
public class PotionOfHealingEffect : ItemEffectBase
{
    public override void Apply(PlayerController player)
    {
        if (Use())
        {
            player.GrantOneTimeInvincibility();
            Debug.Log("回復の薬使用！ 1回無敵権利獲得");
        }
    }

    // 無敵はZAP時も失われるので、Remove() で解除
    public override void Remove(PlayerController player)
    {
        base.Remove(player);
        // 無敵権利は1回使い切りなので特に解除不要（ただしZAP時はリセット）
    }
}