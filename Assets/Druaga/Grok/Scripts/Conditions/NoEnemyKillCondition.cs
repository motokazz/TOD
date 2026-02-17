using UnityEngine;

[CreateAssetMenu(fileName = "NoEnemyKilledCondition", menuName = "Druaga/Conditions/NoEnemyKilled")]
public class NoEnemyKilledCondition : ScriptableObject, ICondition
{
    private int killCount = 0;
    [SerializeField] private string description = "敵を1匹も倒さない";

    public bool IsMet(FloorManager floorManager)
    {
        return killCount == 0;
    }

    public void Reset()
    {
        killCount = 0;
    }

    public string GetDescription() => description;

    public float GetProgress()
    {
        return killCount == 0 ? 1f : 0f;
    }

    public void OnEnemyKilled(string anyEnemyId)
    {
        killCount++;
    }
}