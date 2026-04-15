using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneTransition : MonoBehaviour
{
    public enum TransitionState
    {
        Start,
        End
    }
    public static SceneTransition Instance { get; private set; }
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private Canvas canvas;
    private Coroutine transitionCoroutine;
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
            DontDestroyOnLoad(gameObject);
        }
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (transitionAnimator == null || canvas == null)
        {
            Debug.LogError("SceneTransition is missing required references.");
            return;
        }

        canvas.worldCamera = Camera.main;
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        transitionCoroutine = StartCoroutine(PlaySceneStartTransition());
    }
    public void LoadScene(string sceneName, TransitionState transitionState)
    {
        if (transitionAnimator == null)
        {
            Debug.LogError("Transition Animator is not assigned.");
            return;
        }

        transitionAnimator.gameObject.SetActive(true);
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        playTransitionAnimation(transitionState);
        if (transitionState == TransitionState.Start)
        {
            transitionCoroutine = StartCoroutine(WaitForAnimationAndDisable("startgame"));
            return;
        }

        transitionCoroutine = StartCoroutine(WaitForAnimationAndLoad(sceneName, "end"));
    }

    private IEnumerator PlaySceneStartTransition()
    {
        transitionAnimator.gameObject.SetActive(true);
        playTransitionAnimation(TransitionState.Start);
        yield return WaitForAnimationComplete("startgame");
        transitionAnimator.gameObject.SetActive(false);
        transitionCoroutine = null;
    }

    private IEnumerator WaitForAnimationAndLoad(string sceneName, string stateName)
    {
        yield return WaitForAnimationComplete(stateName);
        SceneManager.LoadScene(sceneName);
        transitionCoroutine = null;
    }

    private IEnumerator WaitForAnimationAndDisable(string stateName)
    {
        yield return WaitForAnimationComplete(stateName);
        transitionAnimator.gameObject.SetActive(false);
        transitionCoroutine = null;
    }

    private IEnumerator WaitForAnimationComplete(string stateName)
    {
        yield return null;

        yield return new WaitUntil(() => transitionAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName));
        yield return new WaitUntil(() => transitionAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f);
    }

    private void playTransitionAnimation(TransitionState transitionState)
    {
        if (transitionAnimator == null)
        {
            Debug.LogError("Transition Animator is not assigned.");
            return;
        }
        switch (transitionState)
        {
            case TransitionState.Start:
                transitionAnimator.Play("startgame", 0, 0f);
                break;
            case TransitionState.End:
                transitionAnimator.Play("end", 0, 0f);
                break;
        }
    }
}