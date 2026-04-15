using UnityEngine;
public class EnvironmentManager : MonoBehaviour
{
    [SerializeField] public EnvironmentSO environmentSO;
    public static EnvironmentManager Instance { get; private set; }
    public float timeCounter { get; private set; }
    public float originalSpeedWorld { get; private set; }
    public float currentSpeedWorld { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
        }
        timeCounter = 0f;
        originalSpeedWorld = environmentSO.speedWorld;
    }
    private void Update()
    {
        timeCounter += Time.deltaTime;
        currentSpeedWorld = Mathf.Lerp(originalSpeedWorld, environmentSO.maxSpeedWorld, timeCounter / environmentSO.rampDuration);
    }
}