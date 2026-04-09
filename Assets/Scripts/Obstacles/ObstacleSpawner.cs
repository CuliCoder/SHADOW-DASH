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
    [SerializeField] private float spawnInterval = 1f;
    private float distanceToObstacle;
    private float timecounter;
    private PlayabilityChecker playabilityChecker;
    private void Awake()
    {
        playabilityChecker = new PlayabilityChecker();
    }
    private void Start()
    {
        timecounter = 0f;
        foreach (var entry in obstaclePoolEntries)
        {
            PoolManager.Instance.prewarmPool(entry.obstacleBaseSO.key, entry.obstacleBaseSO.prefab, entry.poolSize);
        }
        distanceToObstacle = EnvironmentManager.Instance.environmentSO.posXSpawnObstacle - playerController.transform.position.x;
        InvokeRepeating(nameof(SpawnObstacle), spawnInterval, spawnInterval);
    }
    private void FixedUpdate()
    {

    }
    private void Update()
    {
        timecounter+= Time.deltaTime;
    }
    private void SpawnObstacle()
    {
        foreach (var entry in obstaclePoolEntries)
        {
            if (!canSpawn(entry.obstacleBaseSO.obstacleType)) continue;
            bool checker = playabilityChecker.CanPlayerAvoid(playerController.stateInfo, distanceToObstacle,
            EnvironmentManager.Instance.environmentSO.speedWorld, entry.obstacleBaseSO.obstacleType,
            entry.obstacleBaseSO.prefab.GetComponent<BoxCollider2D>());
            if (!checker) continue;
            GameObject obj = PoolManager.Instance.SpawnPool(entry.obstacleBaseSO.key, environmentSO.posXSpawnObstacle, Quaternion.identity);
            Debug.Log($"{obj.GetComponent<BoxCollider2D>().bounds.min.y}, {obj.GetComponent<BoxCollider2D>().bounds.max.y}");
        }
    }
    private bool canSpawn(ObstacleType obstacleType)
    {
        return obstacleType switch
        {
            ObstacleType.Low => timecounter >= 0,
            ObstacleType.High => timecounter >= 10,
            ObstacleType.Spike => timecounter >= 30,
            ObstacleType.Floating => timecounter >= 45,
            _ => true
        };
    }
}