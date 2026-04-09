using UnityEngine;
public class PlayerSlideState : PlayerState
{

    public PlayerSlideState(PlayerStateManager stateManager, Animator animator) : base(stateManager, animator)
    {
    }
    public override void Enter()
    {
        playAnimation("slide");
    }
    public override void Update()
    {
        if (!stateManager.playerController.stateInfo.isSliding)
        {
            stateManager.changeState(stateManager.runState);
        }
    }
    public override void Exit()
    {

    }
}