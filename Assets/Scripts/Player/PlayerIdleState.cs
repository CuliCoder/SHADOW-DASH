using UnityEngine;
public class PlayerIdleState : PlayerState
{

    public PlayerIdleState(PlayerStateManager stateManager, Animator animator) : base(stateManager, animator)
    {
    }
    public override void Enter()
    {
        playAnimation("idle");
    }
    public override void Update()
    {
        if (InputManager.Instance.IsJumpPressed())
        {
            stateManager.changeState(stateManager.jumpState);
        }
    }
    public override void Exit()
    {

    }
}