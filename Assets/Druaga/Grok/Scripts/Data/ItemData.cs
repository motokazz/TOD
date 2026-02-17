using UnityEngine;

[System.Serializable]
public class ItemData
{
    [Tooltip("一意の識別子（例: CopperMattock, JetBoots, WhiteSword）")]
    public string itemId;

    [Tooltip("表示名（日本語/英語どちらでも）")]
    public string itemName;

    [Tooltip("説明文（UI表示用）")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("インベントリに表示するアイコン（2Dスプライト）")]
    public Sprite icon;

    [Tooltip("3Dモデルとして表示する場合のプレハブ")]
    public GameObject modelPrefab;

    [Tooltip("拾ったときの効果音（AudioClip or AudioClipのID）")]
    public AudioClip pickupSound;

    [Tooltip("このアイテムが持続型（常時効果）か一時的か")]
    public bool isPermanent = true;

    [Tooltip("効果パラメータ（アイテムごとに異なる）")]
    public ItemEffectParameters effectParameters;

    [Tooltip("このアイテムがZAP時に失われるか")]
    public bool lostOnZap = true;

    [Tooltip("表示順やレア度（ソート用）")]
    public int displayOrder;

    // ★ ここを追加：アイテム効果（ScriptableObject）をインスペクターで割り当て可能に
    [Header("効果")]
    public ItemEffectBase effect;  // これで ItemDatabase.cs の itemData.effect が使える
}