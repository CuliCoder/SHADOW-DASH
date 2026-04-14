using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void prewarmPool(string key, GameObject prefab, int poolSize)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i <= poolSize; i++)
            {
                GameObject gameObject = Instantiate(prefab);
                gameObject.SetActive(false);
                objectPool.Enqueue(gameObject);
            }
            poolDictionary.Add(key, objectPool);
        }
    }
    public GameObject SpawnPool(string key, float posX, Quaternion rot)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"not found {key} in pool dictionary");
            return null;
        }
        Queue<GameObject> pools = poolDictionary[key];
        if (pools.Count == 0)
        {
            Debug.LogWarning($"{key} sold out");
            return null;
        }
        GameObject objtoSpawn = pools.Dequeue();
        Vector2 pos = new Vector2(posX, objtoSpawn.transform.position.y);
        objtoSpawn.transform.SetPositionAndRotation(pos, rot);
        objtoSpawn.SetActive(true);
        return objtoSpawn;
    }
    public GameObject SpawnPool(string key, Vector2 position, Quaternion rot)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"not found {key} in pool dictionary");
            return null;
        }
        Queue<GameObject> pools = poolDictionary[key];
        if (pools.Count == 0)
        {
            Debug.LogWarning($"{key} sold out");
            return null;
        }
        GameObject objtoSpawn = pools.Dequeue();
        objtoSpawn.transform.SetPositionAndRotation(position, rot);
        objtoSpawn.SetActive(true);
        return objtoSpawn;
    }
    public void Despawn(string key, GameObject gameObject)
    {
        if (!poolDictionary.ContainsKey(key))
        {
            Debug.LogWarning($"not found {key} in pool dictionary");
            Destroy(gameObject);
            return;
        }
        gameObject.SetActive(false);
        poolDictionary[key].Enqueue(gameObject);
    }
}