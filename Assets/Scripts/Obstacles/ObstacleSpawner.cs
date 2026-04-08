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
    [SerializeField] private List<ObstaclePoolEntry> obstaclePoolEntries;
    [SerializeField] private float spawnInterval = 1f;
    private void Start()
    {
        foreach (var entry in obstaclePoolEntries)
        {
            PoolManager.Instance.prewarmPool(entry.obstacleBaseSO.key, entry.obstacleBaseSO.prefab, entry.poolSize);
        }
        InvokeRepeating(nameof(SpawnObstacle), spawnInterval, spawnInterval);
    }
    private void FixedUpdate()
    {
    }
    private void SpawnObstacle()
    {
        foreach (var entry in obstaclePoolEntries)
        {
            PoolManager.Instance.SpawnPool(entry.obstacleBaseSO.key, new Vector3(environmentSO.posXSpawnObstacle, UnityEngine.Random.Range(-10f, 3f), 0f), Quaternion.identity);
        }
    }
}