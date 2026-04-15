using System.Collections;
using UnityEngine;

public class ObstacleAttack : ObstacleBase
{
    public bool Attacked = false;
    private void OnEnable()
    {
        Attacked = false;
    }
    public override void HandleCollision(GameObject player)
    {
        if (Attacked) return;
        Attacked = true;
        player.GetComponent<PlayerController>().whenDie();
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