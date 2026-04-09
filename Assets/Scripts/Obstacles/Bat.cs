public class Bat : ObstacleBase
{
    public override void HandleCollision()
    {
        PoolManager.Instance.Despawn(base.obstacleBaseSO.key, gameObject);
    }
}