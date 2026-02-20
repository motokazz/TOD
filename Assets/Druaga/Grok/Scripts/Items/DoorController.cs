using UnityEngine;

public class DoorController : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!FloorManager.Instance.IsDoorOpen)
        {
            return;
        }

        if (!FloorManager.Instance.IsDoorOpen) return;

        if (other.CompareTag("Player"))
        {
            // イベント発火（クリア判定はマネージャーに委任）
            EventManager.Instance?.TriggerPlayerPassedPosition(FloorManager.Instance.CurrentData.doorPos);
            EventManager.Instance?.TriggerDoorOpened();

            // 元の処理
            FloorManager.Instance.OnFloorCleared();
        }
    }
}