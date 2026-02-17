using UnityEngine;
using UnityEngine.Events;

public class PickupItem : MonoBehaviour
{
    [Header("Item Settings")]
    [SerializeField] private string itemId = "UnknownItem"; // 通常アイテム用ID
    [SerializeField] private bool isKey = false;
    [SerializeField] private bool isTreasure = false;
    [SerializeField] private bool destroyOnPickup = true;

    [Header("Visuals & Effects")]
    [SerializeField] private GameObject pickupEffectPrefab;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float rotationSpeed = 60f;
    [SerializeField] private float bobSpeed = 1f;
    [SerializeField] private float bobAmount = 0.15f;

    [Header("Events")]
    public UnityEvent onPicked;

    private bool hasBeenPicked = false;
    private Vector3 startPosition;
    private PlayerController player;

    private void Awake()
    {
        startPosition = transform.position;
    }

    private void Update()
    {
        if (!hasBeenPicked)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = startPosition + new Vector3(0, bobOffset, 0);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasBeenPicked) return;

        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerController>();
            if (player == null)
            {
                Debug.LogError("PlayerControllerが見つかりません");
                return;
            }

            PickUp();
        }
    }

    public void PickUp()
    {
        if (hasBeenPicked) return;
        hasBeenPicked = true;

        onPicked?.Invoke();

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        if (pickupEffectPrefab != null)
        {
            Instantiate(pickupEffectPrefab, transform.position, Quaternion.identity);
        }

        // 通常アイテム追加
        if (ItemDatabase.Instance != null && ItemDatabase.Instance.Exists(itemId))
        {
            ItemInventory.Instance?.AddItem(itemId);
        }

        // 鍵・宝箱の特別処理
        if (isKey)
        {
            FloorManager.Instance?.OnKeyPickedUp();
        }

        if (isTreasure)
        {
            FloorManager.Instance?.OnTreasurePickedUp();

            // ★ 修正：宝箱の中身は FloorData.treasureItemId から取得
            string treasureItemId = FloorManager.Instance?.CurrentData?.treasureItemId;
            if (string.IsNullOrEmpty(treasureItemId))
            {
                Debug.LogWarning("宝箱の中身 itemId が設定されていません (FloorData.treasureItemId)");
                return;
            }

            var itemEffect = ItemDatabase.Instance?.GetItemEffect(treasureItemId);
            if (itemEffect == null)
            {
                Debug.LogError($"itemEffect が null です。itemId = '{treasureItemId}' を確認してください");
                return;
            }

            if (player == null)
            {
                Debug.LogError("player が null です");
                return;
            }

            itemEffect.Apply(player);
            Debug.Log($"宝箱からアイテム効果適用: {treasureItemId}");
        }

        if (destroyOnPickup)
        {
            Destroy(gameObject, 0.1f);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    public void ForcePickup()
    {
        PickUp();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
#if UNITY_EDITOR
        UnityEditor.Handles.Label(transform.position + Vector3.up * 1.2f, itemId);
#endif
    }
}