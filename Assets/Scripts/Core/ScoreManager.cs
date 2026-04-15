using System;
using System.Collections;
using TMPro;
using UnityEngine;
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject menuPause;
    [SerializeField] private GameObject menuGameOver;
    [SerializeField] private GameObject recordVFX;
    [SerializeField] private TextMeshProUGUI newRecordText;
    private float scoreRate = 0.5f;
    public float currentScoreRate { get; private set; }
    public long currentScore { get; private set; }
    public long highScore { get; private set; }
    private int nextMilestone = 10;
    private float rawScore;
    private Coroutine recordVFXCoroutine;

    [SerializeField] private TextMeshProUGUI textMeshPro;

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

        currentScore = 0;
        rawScore = 0f;
        currentScoreRate = 1;
        highScore = LoadHighScore();
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
    public void Pause(Action handlePause)
    {
        Time.timeScale = 0f;
        handlePause?.Invoke();
    }
    public void Resume(Action handleResume)
    {
        Time.timeScale = 1f;
        handleResume?.Invoke();
    }
    public void enableMenuPause()
    {
        menuPause.SetActive(true);
    }
    public void disableMenuPause()
    {
        menuPause.SetActive(false);
    }
    public void ResumeGameWhenGameOver()
    {
        disableMenuGameOver();
        Resume(ResetState);
    }
    private void ResetState()
    {
        playerController.stateInfo.isDead = false;
        playerController.transform.position = new Vector3(playerController.transform.position.x, playerController.stateInfo.originPositionY, playerController.transform.position.z);
        ParallaxController.Instance.resumeRunningParallax();
        LevelManager.Instance.ReduceLevel();
    }
    public void ResumeGame()
    {
        Resume(disableMenuPause);
    }
    public void PauseGame()
    {
        Pause(enableMenuPause);
    }
    public void disableMenuGameOver()
    {
        menuGameOver.SetActive(false);
    }
    public void enableMenuGameOver()
    {
        SaveHighScoreIfNeeded();
        menuGameOver.SetActive(true);
    }

    public void SaveHighScoreIfNeeded()
    {
        if (currentScore <= highScore)
        {
            return;
        }
        newRecordText.text = currentScore.ToString();
        if (recordVFXCoroutine != null)
        {
            StopCoroutine(recordVFXCoroutine);
        }
        recordVFX.SetActive(true);
        recordVFXCoroutine = StartCoroutine(waitRecordVFX());
        highScore = currentScore;
        PlayerPrefs.SetString(GamePrefs.HighScoreKey, highScore.ToString());
        PlayerPrefs.Save();
    }

    public long LoadHighScore()
    {
        string savedValue = PlayerPrefs.GetString(GamePrefs.HighScoreKey, "0");
        if (long.TryParse(savedValue, out long parsedScore))
        {
            highScore = parsedScore;
            return parsedScore;
        }

        highScore = 0;
        return 0;
    }

    public void ResetSavedHighScore()
    {
        PlayerPrefs.DeleteKey(GamePrefs.HighScoreKey);
        PlayerPrefs.Save();
        highScore = 0;
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        SceneTransition.Instance.LoadScene("MainMenu", SceneTransition.TransitionState.End);
    }
    private IEnumerator waitRecordVFX()
    {
        yield return new WaitForSeconds(1.1f);
        recordVFX.SetActive(false);
    }
}