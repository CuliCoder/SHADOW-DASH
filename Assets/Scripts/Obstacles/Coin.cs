using UnityEngine;
public class Coin : ObstacleBase
{
    public override void HandleCollision()
    {
        PoolManager.Instance.Despawn(base.obstacleBaseSO.key, gameObject);
    }
}