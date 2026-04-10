public class ObstacleAttack: ObstacleBase
{
    public override void HandleCollision()
    {
        PoolManager.Instance.Despawn(base.obstacleBaseSO.key, gameObject);
    }
}