using UnityEngine;

public class ObstacleAttack: ObstacleBase
{
    public override void HandleCollision(GameObject player)
    {
        // PoolManager.Instance.Despawn(base.obstacleBaseSO.key, gameObject);
        ParallaxController.Instance.stopRunningParallax();
        PlayAnimation("attack");
        
    }
}