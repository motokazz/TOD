using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Druaga/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    // Singleton（どこからでも ItemDatabase.Instance でアクセス可能）
    private static ItemDatabase _instance;
    public static ItemDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<ItemDatabase>("ItemDatabase");
                if (_instance == null)
                {
                    Debug.LogError("Resources/ItemDatabase.asset が見つかりません。作成してください");
                }
            }
            return _instance;
        }
    }

    [SerializeField]
    private List<ItemData> items = new List<ItemData>();

    // IDでアイテムデータを取得
    public ItemData GetItem(string itemId)
    {
        foreach (var item in items)
        {
            if (item.itemId == itemId)
            {
                return item;
            }
        }
        Debug.LogWarning($"アイテムが見つかりません: {itemId}");
        return null;
    }

    // アイテム効果を取得（これでエラー解消）
    public ItemEffectBase GetItemEffect(string itemId)
    {
        var item = GetItem(itemId);
        return item?.effect;  // ItemData.effect を返す
    }

    // 存在チェック（Existsメソッド）
    public bool Exists(string itemId)
    {
        return GetItem(itemId) != null;
    }
}