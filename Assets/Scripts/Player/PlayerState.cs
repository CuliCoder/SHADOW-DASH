using UnityEngine;

public class PlayerState : IState
{
    public PlayerStateManager stateManager { get; private set; }
    public Animator animator { get; private set; }
    public PlayerState(PlayerStateManager stateManager, Animator animator)
    {
        this.stateManager = stateManager;
        this.animator = animator;
    }
    public void playAnimation(string animationName)
    {
        animator.Play(animationName);
    }
    public virtual void Enter()
    {
        
    }
    public virtual void Update()
    {

    }
    public virtual void Exit()
    {

    }
}