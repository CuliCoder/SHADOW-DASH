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

    [SerializeField, Tooltip("Thời gian (giây) để đạt đến mức khó nhất (Min Interval)")]
    private float timeToMaxDifficulty = 60f;
    private float distanceToObstacle;
    private float Spawn_interval_low = 2f;
    private float Spawn_interval_high = 1.6f;
    private float Spawn_interval_spike = 1.2f;
    private float Spawn_interval_floating = 0.85f;
    private float timeToNextLow = 0f;
    private float timeToNextHigh = 0f;
    private float timeToNextSpike = 0f;
    private float timeToNextFloating = 0f;
    private float spawnTimer = 0f;
    private PlayabilityChecker playabilityChecker;
    private WeightRandom<ObstacleType> weightRandom;

    private void Awake()
    {
        playabilityChecker = new PlayabilityChecker();
    }

    private void Start()
    {
        if (environmentSO == null || playerController == null)
        {
            Debug.LogError("ObstacleSpawner is missing required references.");
            enabled = false;
            return;
        }

        weightRandom = new WeightRandom<ObstacleType>(new List<(ObstacleType, double)>());
        spawnTimer = maxSpawnInterval;

        foreach (var entry in obstaclePoolEntries)
        {
            PoolManager.Instance.prewarmPool(entry.obstacleBaseSO.key, entry.obstacleBaseSO.prefab, entry.poolSize);
        }

        distanceToObstacle = EnvironmentManager.Instance.environmentSO.posXSpawnObstacle - playerController.transform.position.x;
    }

    private void Update()
    {
        UpdateSpawnTimer();
    }

    private void SpawnObstacle()
    {
        if (obstaclePoolEntries == null)
        {
            return;
        }

        List<ObstacleType> playableTypes = CollectSpawnableObstacleTypes(true);
        if (playableTypes.Count == 0)
        {
            playableTypes = CollectSpawnableObstacleTypes(false);
        }

        if (playableTypes.Count == 0)
        {
            return;
        }

        List<(ObstacleType, double)> validEntriesWithWeights = LevelManager.Instance.GetCurrentLevelInfo();
        if (validEntriesWithWeights == null || validEntriesWithWeights.Count == 0)
        {
            Debug.LogWarning("ObstacleSpawner: No valid entries with weights found for the current level.");
            return;
        }

        List<(ObstacleType, double)> filteredEntriesWithWeights = FilterWeightsByPlayableTypes(validEntriesWithWeights, playableTypes);

        if (filteredEntriesWithWeights.Count == 0)
        {
            Debug.LogWarning("ObstacleSpawner: No playable weighted entries after filtering.");
            return;
        }

        weightRandom.UpdateWeights(filteredEntriesWithWeights);
        ObstacleType pickedType = weightRandom.Pick();
        TrySpawnPickedObstacle(pickedType);
    }

    private List<ObstacleType> CollectSpawnableObstacleTypes(bool respectPlayability)
    {
        List<ObstacleType> playableTypes = new();
        float currentSpeed = EnvironmentManager.Instance.currentSpeedWorld;

        foreach (var entry in obstaclePoolEntries)
        {
            if (!IsPoolEntryValid(entry))
            {
                Debug.LogWarning("ObstacleSpawner: ObstaclePoolEntry or its data is missing.");
                continue;
            }

            ObstacleType obstacleType = entry.obstacleBaseSO.obstacleType;
            if (!CanSpawn(obstacleType))
            {
                continue;
            }

            if (!respectPlayability)
            {
                playableTypes.Add(obstacleType);
                continue;
            }

            bool canAvoid = playabilityChecker.CanPlayerAvoid(
                playerController.stateInfo,
                distanceToObstacle,
                currentSpeed,
                obstacleType,
                entry.obstacleBaseSO.prefab.GetComponent<BoxCollider2D>());

            if (!canAvoid)
            {
                continue;
            }

            playableTypes.Add(obstacleType);
        }

        return playableTypes;
    }

    private static bool IsPoolEntryValid(ObstaclePoolEntry entry)
    {
        return entry != null && entry.obstacleBaseSO != null && entry.obstacleBaseSO.prefab != null;
    }

    private static List<(ObstacleType, double)> FilterWeightsByPlayableTypes(
        List<(ObstacleType, double)> weightedEntries,
        List<ObstacleType> playableTypes)
    {
        HashSet<ObstacleType> playableTypeSet = new(playableTypes);
        List<(ObstacleType, double)> filteredEntries = new();

        for (int i = 0; i < weightedEntries.Count; i++)
        {
            var weightedEntry = weightedEntries[i];
            if (playableTypeSet.Contains(weightedEntry.Item1))
            {
                filteredEntries.Add(weightedEntry);
            }
        }

        return filteredEntries;
    }

    private void TrySpawnPickedObstacle(ObstacleType pickedType)
    {
        foreach (var entry in obstaclePoolEntries)
        {
            if (!IsPoolEntryValid(entry))
            {
                continue;
            }

            if (entry.obstacleBaseSO.obstacleType != pickedType)
            {
                continue;
            }

            PoolManager.Instance.SpawnPool(entry.obstacleBaseSO.key, environmentSO.posXSpawnObstacle, Quaternion.identity);
            return;
        }
    }

    private void UpdateSpawnTimer()
    {
        float surviveTime = EnvironmentManager.Instance.timeCounter;
        float t = surviveTime / timeToMaxDifficulty;
        t = Mathf.Clamp01(t);
        float smoothedT = t * t * (3f - 2f * t); // Smoothing function for more gradual difficulty increase
        float currentSpawnInterval = Mathf.Lerp(maxSpawnInterval, minSpawnInterval, smoothedT);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = currentSpawnInterval;
            SpawnObstacle();
        }
    }

    private bool CanSpawn(ObstacleType obstacleType)
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