using UnityEngine;

[CreateAssetMenu(fileName = "ObstacleBase", menuName = "ObstacleBase")]
public class ObstacleBaseSO: ScriptableObject
{
    public string key;
    public GameObject prefab;
    public ObstacleType obstacleType;
}