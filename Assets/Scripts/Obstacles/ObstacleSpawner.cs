using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class ObstaclePoolEntry
{
    public ObstacleBaseSO obstacleBaseSO;
    public int poolSize;
}
public class ObstacleSpawner : MonoBehaviour
{
    [SerializeField] private EnvironmentSO environmentSO;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private List<ObstaclePoolEntry> obstaclePoolEntries;
    [SerializeField] private float maxSpawnInterval = 3.0f; // Lúc mới chơi, 3s ra 1 chướng ngại vật
    [SerializeField] private float minSpawnInterval = 2.5f; // Mức khó nhất, 2.5s ra 1 cái
    
    [Tooltip("Thời gian (giây) để đạt đến mức khó nhất (Min Interval)")]
    private float timeToMaxDifficulty = 90f;
    private float distanceToObstacle;
    private float Spawn_interval_low = 2f;
    private float Spawn_interval_high = 1.6f;
    private float Spawn_interval_spike = 1.2f;
    private float Spawn_interval_floating = 0.85f;
    private float timeToNextLow = 0f;
    private float timeToNextHigh = 0f;
    private float timeToNextSpike = 0f;
    private float timeToNextFloating = 0f;
    private float TimeCounter = 0f;
    private PlayabilityChecker playabilityChecker;
    private WeightRandom<ObstaclePoolEntry> weightRandom;
    private void Awake()
    {
        playabilityChecker = new PlayabilityChecker();

    }
    private void Start()
    {
        weightRandom = new WeightRandom<ObstaclePoolEntry>(new List<(ObstaclePoolEntry, double)>());
        TimeCounter = maxSpawnInterval;
        foreach (var entry in obstaclePoolEntries)
        {
            PoolManager.Instance.prewarmPool(entry.obstacleBaseSO.key, entry.obstacleBaseSO.prefab, entry.poolSize);
        }
        distanceToObstacle = EnvironmentManager.Instance.environmentSO.posXSpawnObstacle - playerController.transform.position.x;
        // InvokeRepeating(nameof(SpawnObstacle), spawnInterval, spawnInterval);
    }
    private void FixedUpdate()
    {

    }
    private void Update()
    {
        reduceSpawnInterval();
    }
    private void SpawnObstacle()
    {
        if (obstaclePoolEntries == null) return;
        List<(ObstaclePoolEntry entry, double weight)> validEntries = new();
        foreach (var entry in obstaclePoolEntries)
        {
            if (entry == null || entry.obstacleBaseSO == null || entry.obstacleBaseSO.prefab == null)
            {
                Debug.LogWarning("ObstacleSpawner: ObstaclePoolEntry or its data is missing.");
                continue;
            }

            if (!canSpawn(entry.obstacleBaseSO.obstacleType)) continue;
            bool checker = playabilityChecker.CanPlayerAvoid(playerController.stateInfo, distanceToObstacle,
            EnvironmentManager.Instance.currentSpeedWorld, entry.obstacleBaseSO.obstacleType,
            entry.obstacleBaseSO.prefab.GetComponent<BoxCollider2D>());
            Debug.Log($"Checking playability for obstacle type: {entry.obstacleBaseSO.obstacleType}, can player avoid: {checker}");
            if (!checker) continue;
            (ObstaclePoolEntry, double) weightedEntry = (entry, entry.obstacleBaseSO.prefab.GetComponent<ObstacleBase>().weight);
            validEntries.Add(weightedEntry);
        }
        if (validEntries.Count == 0) return;
        weightRandom.UpdateWeights(validEntries);
        ObstaclePoolEntry itemPicked = weightRandom.Pick();
        GameObject obj = PoolManager.Instance.SpawnPool(itemPicked.obstacleBaseSO.key, environmentSO.posXSpawnObstacle, Quaternion.identity);

        if (obj == null)
        {
            Debug.LogWarning($"ObstacleSpawner: SpawnPool returned null for key '{itemPicked.obstacleBaseSO.key}'.");
        }

        BoxCollider2D boxCollider = obj.GetComponent<BoxCollider2D>();
        if (boxCollider == null)
        {
            Debug.LogWarning($"ObstacleSpawner: Spawned object '{obj.name}' has no BoxCollider2D.");
        }

        Debug.Log($"{boxCollider.bounds.min.y}, {boxCollider.bounds.max.y}");
    }

    private void reduceSpawnInterval()
    {
        float surviveTime = EnvironmentManager.Instance.timeCounter;
        float t = surviveTime / timeToMaxDifficulty;
        t = Mathf.Clamp01(t);
        float smoothedT = t * t * (3f - 2f * t); // Smoothing function for more gradual difficulty increase
        float currentSpawnInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, smoothedT);
        TimeCounter -= Time.deltaTime;
        if (TimeCounter <= 0f)
        {
            TimeCounter = currentSpawnInterval;
            SpawnObstacle();
        }
    }
    private bool canSpawn(ObstacleType obstacleType)
    {
        float timecounter = EnvironmentManager.Instance.timeCounter;
        switch (obstacleType)
        {
            case ObstacleType.Low:
                if (timecounter >= 0 && timecounter >= timeToNextLow)
                {
                    timeToNextLow += Spawn_interval_low;
                    return true;
                }
                return false;
            case ObstacleType.High:
                if (timecounter >= 10 && timecounter >= timeToNextHigh)
                {
                    timeToNextHigh += Spawn_interval_high;
                    return true;
                }
                return false;
            case ObstacleType.Spike:
                if (timecounter >= 30 && timecounter >= timeToNextSpike)
                {
                    timeToNextSpike += Spawn_interval_spike;
                    return true;
                }
                return false;
            case ObstacleType.Floating:
                if (timecounter >= 45 && timecounter >= timeToNextFloating)
                {
                    timeToNextFloating += Spawn_interval_floating;
                    return true;
                }
                return false;
            default:
                return true;
        }
    }
}