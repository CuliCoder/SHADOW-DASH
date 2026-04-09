using UnityEngine;

public abstract class ObstacleBase : MonoBehaviour
{
    [SerializeField] public ObstacleBaseSO obstacleBaseSO;
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
        transform.position += Vector3.left * EnvironmentManager.Instance.environmentSO.speedWorld * Time.fixedDeltaTime;
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleCollision();
    }
    public abstract void HandleCollision();
}