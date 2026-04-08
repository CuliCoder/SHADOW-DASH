using UnityEngine;
public class EnvironmentManager : MonoBehaviour
{
    [SerializeField] public EnvironmentSO environmentSO;
    public static EnvironmentManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

    }
}