public class Minotaur: ObstacleBase
{
    public override void HandleCollision()
    {
        PoolManager.Instance.Despawn(base.obstacleBaseSO.key, gameObject);
    }
}