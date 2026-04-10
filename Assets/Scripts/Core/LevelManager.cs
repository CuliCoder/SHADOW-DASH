using System.Collections.Generic;
using UnityEngine;

public enum levels
{
    normal,
    mid,
    hard,
}

[System.Serializable]
public struct ObstacleWeight
{
    public ObstacleType type;
    public float weight;
}

[System.Serializable]
public struct LevelInfoEntry
{
    public levels level;
    public List<ObstacleWeight> obstacleWeights;
}

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private List<LevelInfoEntry> levelInfos;

    private readonly Dictionary<levels, List<(ObstacleType, double)>> runtimeLevelInfos = new();
    private levels currentLevel = levels.normal;

    public List<(ObstacleType, double)> CurrentLevelInfo { get; private set; } = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildRuntimeLevelInfos();
        SetLevel(currentLevel);
    }

    public void SetLevel(levels level)
    {
        if (currentLevel == level && CurrentLevelInfo != null && CurrentLevelInfo.Count > 0)
        {
            return;
        }

        currentLevel = level;
        if (!runtimeLevelInfos.TryGetValue(currentLevel, out var info))
        {
            CurrentLevelInfo = new List<(ObstacleType, double)>();
            Debug.LogError($"No level info found for level: {currentLevel}");
            return;
        }

        CurrentLevelInfo = info;
    }

    private void Update()
    {
        ChangeLevelByTime();
    }

    private void ChangeLevelByTime()
    {
        float timeCounter = EnvironmentManager.Instance.timeCounter;

        if (timeCounter < 30f)
        {
            SetLevel(levels.normal);
        }
        else if (timeCounter < 60f)
        {
            SetLevel(levels.mid);
        }
        else
        {
            SetLevel(levels.hard);
        }
    }

    private void BuildRuntimeLevelInfos()
    {
        runtimeLevelInfos.Clear();
        if (levelInfos == null)
        {
            return;
        }

        for (int i = 0; i < levelInfos.Count; i++)
        {
            LevelInfoEntry entry = levelInfos[i];
            List<(ObstacleType, double)> weights = new();

            if (entry.obstacleWeights != null)
            {
                for (int j = 0; j < entry.obstacleWeights.Count; j++)
                {
                    ObstacleWeight item = entry.obstacleWeights[j];
                    weights.Add((item.type, item.weight));
                }
            }

            runtimeLevelInfos[entry.level] = weights;
        }
    }

    public List<(ObstacleType, double)> GetCurrentLevelInfo()
    {
        if (CurrentLevelInfo == null || CurrentLevelInfo.Count == 0)
        {
            Debug.LogError($"No level info found for level: {currentLevel}");
            return null;
        }

        return CurrentLevelInfo;
    }
}
