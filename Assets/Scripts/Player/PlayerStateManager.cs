using UnityEngine;
public class PlayerStateManager : MonoBehaviour
{
    public PlayerController playerController { get; private set; }
    private Animator animator;
    public PlayerIdleState idleState { get; private set; }
    public PlayerJumpState jumpState { get; private set; }
    public PlayerRunState runState { get; private set; }
    public PlayerSlideState slideState { get; private set; }
    public PlayerDeadState deadState { get; private set; }
    public IState currentState { get; private set; }
    private void Awake()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        idleState = new PlayerIdleState(this, animator);
        jumpState = new PlayerJumpState(this, animator);
        runState = new PlayerRunState(this, animator);
        slideState = new PlayerSlideState(this, animator);
        deadState = new PlayerDeadState(this, animator);
    }
    private void Start()
    {
        currentState = runState;
        currentState.Enter();
    }
    private void Update()
    {
        currentState.Update();
    }
    public void changeState(IState newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }
}