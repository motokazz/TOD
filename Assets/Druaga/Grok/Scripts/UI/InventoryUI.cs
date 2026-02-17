using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    [Header("UI Settings")]
    [SerializeField] private Transform itemSlotContainer;          // Horizontal Layout Group が付いた親
    [SerializeField] private GameObject itemSlotPrefab;            // 1スロットのプレハブ（Image + Text + Outlineなど）

    [Header("Visuals")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color highlightColor = new Color(1f, 0.9f, 0.4f);
    [SerializeField] private float highlightDuration = 1.5f;       // 新規取得時の点滅時間
    [SerializeField] private int maxVisibleSlots = 8;              // 画面に同時に表示する最大数

    [Header("References")]
    [SerializeField] private ItemDatabase itemDatabase;            // アイコン参照用（なくても可）

    private List<ItemSlot> activeSlots = new List<ItemSlot>();
    private Dictionary<string, ItemSlot> itemToSlotMap = new Dictionary<string, ItemSlot>();

    private void Awake()
    {
        ClearAllSlots();
    }

    private void OnEnable()
    {
        // ItemInventoryからのイベント購読
        if (ItemInventory.Instance != null)
        {
            ItemInventory.Instance.OnItemAdded += OnItemAdded;
            ItemInventory.Instance.OnItemRemoved += OnItemRemoved;
            ItemInventory.Instance.OnItemUsed += OnItemUsed;
            ItemInventory.Instance.OnInventoryChanged += RefreshAll;
        }
    }

    private void OnDisable()
    {
        if (ItemInventory.Instance != null)
        {
            ItemInventory.Instance.OnItemAdded -= OnItemAdded;
            ItemInventory.Instance.OnItemRemoved -= OnItemRemoved;
            ItemInventory.Instance.OnItemUsed -= OnItemUsed;
            ItemInventory.Instance.OnInventoryChanged -= RefreshAll;
        }
    }

    /// <summary>
    /// 所持アイテムが変更されたときに全表示を更新
    /// </summary>
    public void RefreshAll()
    {
        ClearAllSlots();

        var owned = ItemInventory.Instance.GetAllOwnedItems();
        int index = 0;

        foreach (var item in owned)
        {
            if (index >= maxVisibleSlots) break;

            AddOrUpdateSlot(item);
            index++;
        }
    }

    private void OnItemAdded(string itemId)
    {
        var itemData = itemDatabase?.GetItem(itemId);
        if (itemData == null) return;

        AddOrUpdateSlot(itemData);

        // 新規取得演出
        if (itemToSlotMap.TryGetValue(itemId, out var slot))
        {
            StartCoroutine(HighlightNewItem(slot));
        }
    }

    private void OnItemRemoved(string itemId)
    {
        if (itemToSlotMap.TryGetValue(itemId, out var slot))
        {
            slot.gameObject.SetActive(false);
            itemToSlotMap.Remove(itemId);
            activeSlots.Remove(slot);
        }
    }

    private void OnItemUsed(string itemId, int remaining)
    {
        if (itemToSlotMap.TryGetValue(itemId, out var slot))
        {
            slot.UpdateCount(remaining);
        }
    }

    private void AddOrUpdateSlot(ItemData itemData)
    {
        if (itemToSlotMap.TryGetValue(itemData.itemId, out var existingSlot))
        {
            // 既存なら更新（回数など）
            existingSlot.UpdateIcon(itemData.icon);
            existingSlot.UpdateCount(itemData.effectParameters.useCount);
            return;
        }

        // 新規スロット作成
        var slotObj = Instantiate(itemSlotPrefab, itemSlotContainer);
        var slot = slotObj.GetComponent<ItemSlot>();
        if (slot == null) return;

        slot.Initialize(itemData);
        activeSlots.Add(slot);
        itemToSlotMap[itemData.itemId] = slot;
    }

    private void ClearAllSlots()
    {
        foreach (var slot in activeSlots)
        {
            if (slot != null) Destroy(slot.gameObject);
        }
        activeSlots.Clear();
        itemToSlotMap.Clear();
    }

    private IEnumerator HighlightNewItem(ItemSlot slot)
    {
        var outline = slot.GetComponent<Outline>();
        if (outline == null) yield break;

        float elapsed = 0f;
        Color startColor = outline.effectColor;

        while (elapsed < highlightDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 4f, 1f); // 点滅
            outline.effectColor = Color.Lerp(startColor, highlightColor, t);
            yield return null;
        }

        outline.effectColor = startColor;
    }

    // テスト用（デバッグキー）
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            RefreshAll();
        }
    }
}