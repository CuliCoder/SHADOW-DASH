using UnityEngine;
public class PlayerDeadState : PlayerState
{

    public PlayerDeadState(PlayerStateManager stateManager, Animator animator) : base(stateManager, animator)
    {
    }
    public override void Enter()
    {
        playAnimation("die");
    }
    public override void Update()
    {
        if (!stateManager.playerController.stateInfo.isDead)
        {
            stateManager.changeState(stateManager.runState);
        }
    }
    public override void Exit()
    {

    }
}