using UnityEngine;

[System.Serializable]
public class ItemEffectParameters
{
    // 共通でよく使うパラメータ例（必要に応じて追加・削除）
    [Header("基本効果")]
    public float moveSpeedMultiplier = 1f;          // Jet Bootsなど
    public int swordPower = 1;                      // 剣の強さ（WhiteSword=2, GoldSword=3など）
    public int shieldPower = 1;                     // 盾の強さ
    public int breakWallCount = 0;                  // マトック類

    [Header("特殊効果")]
    public bool invincible = false;                 // Potion of Invisibilityなど
    public bool revealHidden = false;               // Potion of Reveal
    public bool noEnemyDamage = false;              // Potion of Healing（1回無敵）
    public bool attractTreasure = false;            // Potion of Magnet
    public int extraLives = 0;                      // Potion of Life

    [Header("制限・回数")]
    public int useCount = -1;                       // -1=無制限
    public float durationSeconds = 0f;              // 時間制限がある場合

    [Header("その他")]
    public string customEffectId;                   // 特殊効果はスクリプト側で対応
}