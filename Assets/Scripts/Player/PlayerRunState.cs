using UnityEngine;
public class PlayerRunState : PlayerState
{

    public PlayerRunState(PlayerStateManager stateManager, Animator animator) : base(stateManager, animator)
    {
    }
    public override void Enter()
    {
        playAnimation("run");
    }
    public override void Update()
    {
        if (!stateManager.playerController.isGrounded)
        {
            stateManager.changeState(stateManager.jumpState);
        }
        else if (stateManager.playerController.isSliding)
        {
            stateManager.changeState(stateManager.slideState);
        }
    }
    public override void Exit()
    {

    }
}