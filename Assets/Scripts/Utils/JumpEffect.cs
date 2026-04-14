using System.Collections;
using UnityEngine;
public class JumpEffect : MonoBehaviour
{
    [SerializeField] private GameObject prefabJumpEffect;
    private Animator animator;
    private Coroutine despawnCoroutine;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void OnEnable()
    {
        if (despawnCoroutine != null)
        {
            StopCoroutine(despawnCoroutine);
        }
        despawnCoroutine = StartCoroutine(despawn());
    }
    private void Update()
    {
        transform.position += Vector3.left * EnvironmentManager.Instance.currentSpeedWorld * Time.deltaTime;
    }
    private IEnumerator despawn()
    {
        yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
        PoolManager.Instance.Despawn("JumpEffect", gameObject);
    }
}