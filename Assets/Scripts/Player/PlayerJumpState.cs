using UnityEngine;
public class PlayerJumpState : PlayerState
{

    public PlayerJumpState(PlayerStateManager stateManager, Animator animator) : base(stateManager, animator)
    {
    }
    public override void Enter()
    {
        playAnimation("jump_start");
    }
    public override void Update()
    {
        if (stateManager.playerController.rb.velocity.y > 0)
        {
            playAnimation("jump_start");
        }
        if (stateManager.playerController.rb.velocity.y < 0)
        {
            playAnimation("jump_end");
        }
        if (stateManager.playerController.isGrounded)
        {
            stateManager.changeState(stateManager.runState);
        }
    }
    public override void Exit()
    {

    }
}