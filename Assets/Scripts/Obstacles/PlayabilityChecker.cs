using UnityEngine;
public class PlayabilityChecker : MonoBehaviour
{
    public bool CanPlayerAvoid(PlayerStateInfo playerStateInfo, float distanceToObstacle, float currentSpeed, ObstacleType obstacleType, BoxCollider2D hitboxObstacle)
    {
        float timeToReach = distanceToObstacle / currentSpeed;
        return obstacleType switch
        {
            ObstacleType.Low => SimulateBestJump(timeToReach, playerStateInfo).y > getTopBottomObstacle(hitboxObstacle).x,
            ObstacleType.Spike => SimulateBestJump(timeToReach, playerStateInfo).y > getTopBottomObstacle(hitboxObstacle).x,
            ObstacleType.High => playerStateInfo.isGrounded,
            ObstacleType.Floating => CanJumpOverOrRunUnder(playerStateInfo, timeToReach, hitboxObstacle),
            _ => true
        };
    }
    private Vector2 SimulateBestJump(float timeToReach, PlayerStateInfo stateInfo)
    {
        float vy = stateInfo.isGrounded ? stateInfo.jumpForce : stateInfo.rb.velocity.y;
        float y = stateInfo.rb.position.y;
        float gravity = Physics2D.gravity.y * EnvironmentManager.Instance.environmentSO.gravityScale;
        float elapsed = 0f;
        bool usedDoubleJump = !stateInfo.canDoubleJump;
        while (elapsed < timeToReach)
        {
            vy += gravity * Time.fixedDeltaTime;
            y += vy * Time.fixedDeltaTime;
            if (y <= 0f) { y = 0; vy = 0; }
            if (!usedDoubleJump && vy < 0 && elapsed < timeToReach * 0.6f)
            {
                usedDoubleJump = true;
                vy = stateInfo.doubleJumpForce;
            }
            elapsed += Time.fixedDeltaTime;
        }
        return getTopBottomPlayer(stateInfo.col, y);
    }
    private bool CanJumpOverOrRunUnder(PlayerStateInfo playerStateInfo, float timeToReach, BoxCollider2D hitboxObstacle)
    {
        Vector2 playerH = SimulateBestJump(timeToReach, playerStateInfo);
        bool canOver = playerH.y > getTopBottomObstacle(hitboxObstacle).x;
        bool canUnder = playerH.x < getTopBottomObstacle(hitboxObstacle).y;
        return canOver || canUnder;
    }
    public Vector2 getTopBottomObstacle(BoxCollider2D boxCollider2D)
    {
        float top = boxCollider2D.bounds.max.y;
        float bottom = boxCollider2D.bounds.min.y;
        return new Vector2(top, bottom);
    }
    public Vector2 getTopBottomPlayer(BoxCollider2D boxCollider2D, float posY)
    {
        float top = posY + boxCollider2D.offset.y + boxCollider2D.size.y / 2.0f;
        float bottom = posY + boxCollider2D.offset.y - boxCollider2D.size.y / 2.0f;
        return new Vector2(top, bottom);
    }
}