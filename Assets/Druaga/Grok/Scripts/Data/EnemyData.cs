using UnityEngine;

[System.Serializable]
public class EnemyData
{
    [Tooltip("一意の識別子（例: GreenSlime, RedKnight, Mage）")]
    public string enemyId;

    [Tooltip("表示名")]
    public string enemyName;

    [Tooltip("3Dモデルプレハブ")]
    public GameObject modelPrefab;

    [Tooltip("移動速度（グリッド/秒）")]
    [Range(0.5f, 3f)]
    public float moveSpeed = 1f;

    [Tooltip("HP（剣ダメージ1 per hit）")]
    [Range(1, 10)]
    public int maxHp = 1;

    [Tooltip("AIタイプ（EnemyAI派生クラス名）")]
    public string aiType = "RandomWander"; // "RandomWander", "WallFollowRight", "WallFollowLeft", "PlayerChase", "MagicProjectile", "DragonBoss"

    [Tooltip("ジャンプ可能か（一部スライム）")]
    public bool canJump = false;

    [Tooltip("接触ダメージ量（納剣時プレイヤー即死=999）")]
    public int contactDamage = 999;

    [Tooltip("攻撃ダメージ量（抜剣時プレイヤーとの交差攻撃力）")]
    public int attackDamage = 1;

    [Tooltip("死亡時ドロップアイテムID（空=なし、複数=ランダム）")]
    public string[] possibleDropItems;

    [Tooltip("死亡エフェクト/サウンド")]
    public GameObject deathEffectPrefab;
    public AudioClip deathSound;

    [Tooltip("表示順（Editorソート用）")]
    public int displayOrder;

    [Tooltip("カラー変種（Red/Green/Blueで共有データ）")]
    public Color bodyColor = Color.white;
}