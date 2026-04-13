using System.Collections;
using UnityEngine;

public class ObstacleAttack: ObstacleBase
{
    public override void HandleCollision(GameObject player)
    {
        // PoolManager.Instance.Despawn(base.obstacleBaseSO.key, gameObject);
        ParallaxController.Instance.stopRunningParallax();
        PlayAnimation("attack");
        StartCoroutine(waitAttackAnimation());
    }
    private IEnumerator waitAttackAnimation()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("attack") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        PlayAnimation("idle");
    }
}