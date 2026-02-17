using UnityEngine;

public class DoorController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!FloorManager.Instance.IsDoorOpen)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log("扉に到達！ 次フロアへ移動");

            // ★ ここで FloorManager に通知（エラー解消）
            if (FloorManager.Instance != null)
            {
                FloorManager.Instance.OnFloorCleared();
            }
            else
            {
                Debug.LogError("FloorManager.Instance が null です");
            }
        }
    }
}