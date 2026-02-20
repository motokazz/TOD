using UnityEngine;
using System;
using System.Collections.Generic;

public class ItemInventory : Singleton<ItemInventory>
{
    // イベント（UI更新用）
    public event Action OnInventoryChanged;
    public event Action<string> OnItemAdded;
    public event Action<string> OnItemRemoved;
    public event Action<string, int> OnItemUsed;

    // 所持アイテム（ID → データ）
    private Dictionary<string, ItemData> ownedItems = new Dictionary<string, ItemData>();

    // 追加順を保持（UI表示順用）
    private List<string> ownedOrder = new List<string>();

    private void Awake()
    {
        base.Awake();
    }


    public void OnZapReset()
    {
        // 所持アイテムのうちZAPで失われるものを削除（例）
        var toRemove = new List<string>();
        foreach (var kvp in ownedItems)
        {
            var item = kvp.Value;
            if (item.lostOnZap)  // ItemDataに lostOnZap フラグがある前提
            {
                toRemove.Add(kvp.Key);
            }
        }

        foreach (var id in toRemove)
        {
            ownedItems.Remove(id);
            ownedOrder.Remove(id);
        }

        OnInventoryChanged?.Invoke();

        // プレイヤーに通知（速度、無敵などリセット）
        var player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            player.OnZapReset();  // PlayerController側で速度・無敵リセット
        }

        Debug.Log("ZAPによりアイテム効果をリセットしました");
    }


    /// <summary>
    /// アイテムを追加（重複は無視）
    /// </summary>
    public void AddItem(string itemId)
    {
        if (ownedItems.ContainsKey(itemId)) return; // 重複防止など

        var itemData = ItemDatabase.Instance?.GetItem(itemId);
        if (itemData == null)
        {
            Debug.LogWarning("アイテムデータが見つかりません: " + itemId);
            return;
        }

        ownedItems[itemId] = itemData;
        ownedOrder.Add(itemId);

        // ★ ここで効果適用
        if (itemData.effect != null)
        {
            var player = FindObjectOfType<PlayerController>();
            if (player != null)
            {
                itemData.effect.Apply(player);
                Debug.Log($"マトック取得 → 効果適用: {itemId}");
            }
        }

        OnItemAdded?.Invoke(itemId);
        OnInventoryChanged?.Invoke();
    }

    /// <summary>
    /// アイテムを削除
    /// </summary>
    public void RemoveItem(string itemId)
    {
        if (ownedItems.Remove(itemId))
        {
            ownedOrder.Remove(itemId);
            OnItemRemoved?.Invoke(itemId);
            OnInventoryChanged?.Invoke();
            Debug.Log($"アイテム削除: {itemId}");
        }
    }

    /// <summary>
    /// アイテムを使用（回数減らす例）
    /// </summary>
    public void UseItem(string itemId)
    {
        if (ownedItems.TryGetValue(itemId, out var item))
        {
            int remaining = item.effectParameters.useCount - 1;
            item.effectParameters.useCount = remaining;

            OnItemUsed?.Invoke(itemId, remaining);
            OnInventoryChanged?.Invoke();

            if (remaining <= 0)
            {
                RemoveItem(itemId);
            }
        }
    }

    /// <summary>
    /// すべての所持アイテムを取得（UI表示用）
    /// </summary>
    public List<ItemData> GetAllOwnedItems()
    {
        var list = new List<ItemData>();
        foreach (var id in ownedOrder)
        {
            if (ownedItems.TryGetValue(id, out var data))
            {
                list.Add(data);
            }
        }
        return list;
    }

    /// <summary>
    /// ZAP時などに全アイテムリセット（必要に応じて）
    /// </summary>
    public void ClearAll()
    {
        ownedItems.Clear();
        ownedOrder.Clear();
        OnInventoryChanged?.Invoke();
    }
}