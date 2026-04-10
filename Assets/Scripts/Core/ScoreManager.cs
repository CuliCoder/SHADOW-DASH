using TMPro;
using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    private float scoreRate = 0.5f;
    public float currentScoreRate { get; private set; }
    public long currentScore { get; private set; }
    private int nextMilestone = 10;
    private float rawScore;

    [SerializeField] private TextMeshProUGUI textMeshPro;

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

        currentScore = 0;
        rawScore = 0f;
        currentScoreRate = 1;
        textMeshPro.text = currentScore.ToString();
    }

    private void Update()
    {
        UpdateScore();
    }

    public void UpdateScore()
    {
        float timeCounter = EnvironmentManager.Instance.timeCounter;
        while (timeCounter >= nextMilestone)
        {
            currentScoreRate += scoreRate;
            nextMilestone += 10;
        }

        rawScore += currentScoreRate * Time.deltaTime;
        currentScore = (long)rawScore;
        textMeshPro.text = currentScore.ToString();
    }
}