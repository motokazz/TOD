using UnityEngine;

public class KnightAI : EnemyBase
{
    [Header("Knight Settings")]
    [SerializeField] protected bool rightHandRule = true;  // 右手/左手ルール（Inspectorで設定）

    protected Vector2Int lastDirection = Vector2Int.right;

    protected override void Awake()
    {
        base.Awake();
        lastDirection = Vector2Int.right;
    }

    protected override void PerformAI()
    {
        Vector2Int nextDir = GetNextWallFollowDirection();
        if (nextDir != Vector2Int.zero)
        {
            if (TryMove(nextDir))
            {
                lastDirection = nextDir;
            }
            else
            {
                lastDirection = RotateDirection(lastDirection, rightHandRule ? -90 : 90);
            }
        }
        else
        {
            lastDirection = GridUtils.FourDirections[Random.Range(0, 4)];
        }
    }

    protected Vector2Int GetNextWallFollowDirection()
    {
        Vector2Int sideDir = RotateDirection(lastDirection, rightHandRule ? 90 : -90);
        Vector2Int sidePos = currentGridPos + sideDir;

        var currentData = FloorManager.Instance?.CurrentData;
        bool hasWallOnSide = currentData?.GetWall(sidePos) ?? true;

        return hasWallOnSide ? lastDirection : sideDir;
    }

    protected Vector2Int RotateDirection(Vector2Int dir, float degrees)
    {
        degrees = (degrees % 360 + 360) % 360;

        if (degrees == 90f)
            return new Vector2Int(dir.y, -dir.x);
        if (degrees == -90f || degrees == 270f)
            return new Vector2Int(-dir.y, dir.x);
        if (degrees == 180f)
            return new Vector2Int(-dir.x, -dir.y);

        return dir;
    }
}