using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour
{
    [SerializeField] public ObstacleBaseSO obstacleBaseSO;
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void FixedUpdate()
    {
        Move();
    }
    private void Update()
    {
        if (transform.position.x < -35f)
        {
            PoolManager.Instance.Despawn(obstacleBaseSO.key, gameObject);
        }
    }
    public void Move()
    {
        // float speed = obstacleBaseSO.runSpeed;
        transform.position += Vector3.left * EnvironmentManager.Instance.currentSpeedWorld * Time.fixedDeltaTime;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            HandleCollision(other.gameObject);
        }
    }
    public void PlayAnimation(string animationName)
    {
        if (animator != null)
        {
            animator.Play(animationName);
        }
    }
    public abstract void HandleCollision(GameObject player);
}