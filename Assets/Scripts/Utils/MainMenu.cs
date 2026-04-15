using UnityEngine;
using TMPro;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMeshProUGUI;
    private void Start()
    {
        string savedValue = PlayerPrefs.GetString(GamePrefs.HighScoreKey, "0");
        textMeshProUGUI.text = savedValue;
    }
    public void PlayGame()
    {
        SceneTransition.Instance.LoadScene("PlayGame", SceneTransition.TransitionState.End);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}