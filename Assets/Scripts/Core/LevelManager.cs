using System.Collections.Generic;
using UnityEngine;
using System.Collections;
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
    [SerializeField] private GameObject vfx1;
    [SerializeField] private GameObject vfx2;
    private readonly Dictionary<levels, List<(ObstacleType, double)>> runtimeLevelInfos = new();
    private levels currentLevel = levels.normal;
    public List<(ObstacleType, double)> CurrentLevelInfo { get; private set; } = new();
    private Coroutine levelChangeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        BuildRuntimeLevelInfos();
    }
    private void Start()
    {
        SetLevel(currentLevel);
    }
    public void SetLevel(levels level)
    {
        if (currentLevel == level && CurrentLevelInfo != null && CurrentLevelInfo.Count > 0)
        {
            return;
        }
        currentLevel = level;
        ParallaxController.Instance.changeMap(currentLevel);
        if (!runtimeLevelInfos.TryGetValue(currentLevel, out var info))
        {
            CurrentLevelInfo = new List<(ObstacleType, double)>();
            Debug.LogError($"No level info found for level: {currentLevel}");
            return;
        }

        CurrentLevelInfo = info;
        if (levelChangeCoroutine != null)
        {
            StopCoroutine(levelChangeCoroutine);
        }
        levelChangeCoroutine = StartCoroutine(ChangeLevelByTime());
        changeVFX();
    }
    private void changeVFX()
    {
        if(currentLevel == levels.mid)
        {
            vfx1.SetActive(true);
            vfx2.SetActive(false);
        }
        else if(currentLevel == levels.hard)
        {
            vfx1.SetActive(true);
            vfx2.SetActive(true);
        }
        else
        {
            vfx1.SetActive(false);
            vfx2.SetActive(false);
        }
    }
    private void Update()
    {
    }

    private IEnumerator ChangeLevelByTime()
    {
        if (currentLevel != levels.hard)
        {
            switch (currentLevel)
            {
                case levels.normal:
                    yield return new WaitForSeconds(30f);
                    SetLevel(levels.mid);
                    break;
                case levels.mid:
                    yield return new WaitForSeconds(30f);
                    SetLevel(levels.hard);
                    break;
                default:
                    Debug.LogError($"Unknown level: {currentLevel}");
                    break;
            }
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
    public void ReduceLevel()
    {
        switch (currentLevel)
        {
            case levels.normal:
                SetLevel(levels.normal);
                break;
            case levels.mid:
                SetLevel(levels.normal);
                break;
            case levels.hard:
                SetLevel(levels.mid);
                break;
            default:
                Debug.LogError($"Unknown level: {currentLevel}");
                break;
        }
    }
}
