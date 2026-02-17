using UnityEngine;

[CreateAssetMenu(fileName = "TouchSpecificWallCondition", menuName = "Druaga/Conditions/TouchSpecificWall")]
public class TouchSpecificWallCondition : ScriptableObject, ICondition
{
    [SerializeField] private Vector2Int targetGridPos = new Vector2Int(1, 1);
    [SerializeField] private string description = "特定の壁に触れる";

    private bool hasTouched = false;

    public bool IsMet(FloorManager floorManager)
    {
        return hasTouched;
    }

    public void Reset()
    {
        hasTouched = false;
    }

    public string GetDescription() => description;

    public float GetProgress()
    {
        return hasTouched ? 1f : 0f;
    }

    // PlayerControllerから呼ばれる
    public void OnPlayerTouchedWall(Vector2Int gridPos)
    {
        if (gridPos == targetGridPos)
        {
            hasTouched = true;
        }
    }
}