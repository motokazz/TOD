using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image backgroundImage;

    private string itemId;

    public void Initialize(ItemData itemData)
    {
        itemId = itemData.itemId;
        UpdateIcon(itemData.icon);
        UpdateCount(itemData.effectParameters.useCount);
    }

    public void UpdateIcon(Sprite sprite)
    {
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
            iconImage.enabled = sprite != null;
        }
    }

    public void UpdateCount(int count)
    {
        if (countText != null)
        {
            if (count <= 0 || count == -1) // -1 = 無制限
            {
                countText.text = "";
            }
            else
            {
                countText.text = count.ToString();
            }
        }
    }

    public string ItemId => itemId;
}