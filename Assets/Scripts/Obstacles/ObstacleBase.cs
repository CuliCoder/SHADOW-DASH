using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour
{
    [SerializeField] public ObstacleBaseSO obstacleBaseSO;
    [SerializeField]public double weight;
    private void Awake()
    {
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
            HandleCollision();
        }
    }

    public abstract void HandleCollision();
}