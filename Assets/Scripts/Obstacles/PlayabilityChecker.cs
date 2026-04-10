using UnityEngine;
public class PlayabilityChecker
{
    private const float MinReactionTime = 0.2f;
    private const float MaxJumpLeadTime = 1.0f;
    private const float SimulationStep = 0.02f;
    private const float VerticalMargin = 0.05f;

    public bool CanPlayerAvoid(PlayerStateInfo playerStateInfo, float distanceToObstacle, float currentSpeed, ObstacleType obstacleType, BoxCollider2D hitboxObstacle)
    {
        if (playerStateInfo.rb == null || playerStateInfo.col == null || hitboxObstacle == null || currentSpeed <= 0f)
        {
            return false;
        }

        float timeToReach = Mathf.Max(0f, distanceToObstacle / currentSpeed);
        if (timeToReach < MinReactionTime)
        {
            return false;
        }

        Vector2 obstacleTopBottom = GetTopBottomObstacle(hitboxObstacle);
        return obstacleType switch
        {
            ObstacleType.Low => CanJumpOverWithinReactionWindow(playerStateInfo, timeToReach, obstacleTopBottom),
            ObstacleType.Spike => CanJumpOverWithinReactionWindow(playerStateInfo, timeToReach, obstacleTopBottom),
            ObstacleType.High => CanSlideUnder(playerStateInfo, obstacleTopBottom),
            ObstacleType.Floating => CanJumpOverWithinReactionWindow(playerStateInfo, timeToReach, obstacleTopBottom) || CanSlideUnder(playerStateInfo, obstacleTopBottom),
            _ => true
        };
    }

    private bool CanJumpOverWithinReactionWindow(PlayerStateInfo stateInfo, float timeToReach, Vector2 obstacleTopBottom)
    {
        float earliestJumpStart = Mathf.Max(0f, timeToReach - MaxJumpLeadTime);
        float latestJumpStart = Mathf.Max(0f, timeToReach - MinReactionTime);

        if (stateInfo.isSliding)
        {
            earliestJumpStart = Mathf.Max(earliestJumpStart, stateInfo.slideCounter);
        }

        for (float firstJumpStartTime = earliestJumpStart; firstJumpStartTime <= latestJumpStart; firstJumpStartTime += SimulationStep)
        {
            if (SimulateJumpAtTime(stateInfo, timeToReach, firstJumpStartTime, -1f).y > obstacleTopBottom.x + VerticalMargin)
            {
                return true;
            }

            for (float secondJumpStartTime = firstJumpStartTime + SimulationStep; secondJumpStartTime <= latestJumpStart; secondJumpStartTime += SimulationStep)
            {
                if (SimulateJumpAtTime(stateInfo, timeToReach, firstJumpStartTime, secondJumpStartTime).y > obstacleTopBottom.x + VerticalMargin)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private Vector2 SimulateJumpAtTime(PlayerStateInfo stateInfo, float timeToReach, float firstJumpStartTime, float secondJumpStartTime)
    {
        float y = stateInfo.rb.position.y;
        float vy = stateInfo.rb.velocity.y;
        float gravity = Physics2D.gravity.y * EnvironmentManager.Instance.environmentSO.gravityScale;
        float elapsed = 0f;
        bool grounded = stateInfo.isGrounded;
        bool canDoubleJump = stateInfo.canDoubleJump;
        bool firstJumpTriggered = false;
        bool secondJumpTriggered = false;

        while (elapsed < timeToReach)
        {
            if (!firstJumpTriggered && elapsed >= firstJumpStartTime)
            {
                if (grounded)
                {
                    vy = stateInfo.jumpForce;
                    grounded = false;
                    firstJumpTriggered = true;
                }
                else if (canDoubleJump)
                {
                    vy = stateInfo.doubleJumpForce;
                    canDoubleJump = false;
                    firstJumpTriggered = true;
                    secondJumpTriggered = true;
                }
            }

            if (!secondJumpTriggered && secondJumpStartTime >= 0f && elapsed >= secondJumpStartTime && !grounded && canDoubleJump)
            {
                vy = stateInfo.doubleJumpForce;
                canDoubleJump = false;
                secondJumpTriggered = true;
                if (!firstJumpTriggered)
                {
                    firstJumpTriggered = true;
                }
            }

            if (!grounded)
            {
                vy += gravity * Time.fixedDeltaTime;
                y += vy * Time.fixedDeltaTime;
            }

            elapsed += Time.fixedDeltaTime;
        }

        return GetTopBottomPlayer(stateInfo.col, y, stateInfo.runSize);
    }

    private bool CanSlideUnder(PlayerStateInfo playerStateInfo, Vector2 obstacleTopBottom)
    {
        if (!playerStateInfo.isGrounded)
        {
            return false;
        }

        Vector2 playerSlideTopBottom = GetTopBottomPlayer(playerStateInfo.col, playerStateInfo.rb.position.y, playerStateInfo.slideSize);
        return playerSlideTopBottom.y < obstacleTopBottom.y - VerticalMargin;
    }

    public Vector2 GetTopBottomObstacle(BoxCollider2D boxCollider2D)
    {
        float top = boxCollider2D.transform.position.y + boxCollider2D.offset.y + boxCollider2D.size.y / 2.0f;
        float bottom = boxCollider2D.transform.position.y + boxCollider2D.offset.y - boxCollider2D.size.y / 2.0f;
        return new Vector2(top, bottom);
    }

    public Vector2 GetTopBottomPlayer(BoxCollider2D boxCollider2D, float posY, Vector2 size)
    {
        float top = posY + boxCollider2D.offset.y + size.y / 2.0f;
        float bottom = posY + boxCollider2D.offset.y - size.y / 2.0f;
        return new Vector2(top, bottom);
    }
}