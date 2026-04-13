using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public struct ParallaxLayer
{
    public GameObject layerObject;
    public levels level;
}
public class ParallaxController : MonoBehaviour
{
    [SerializeField] List<ParallaxLayer> backgroundsParent; //Parent of all background layers
    [SerializeField] float transitionDuration = 0.75f;
    [SerializeField] Vector2 transitionOffset = new Vector2(100f, 0f);
    [SerializeField] Transform backgroundContainer;
    Transform cam; //Main Camera
    GameObject[] backgrounds;
    Material[] mat;
    float[] backSpeed;

    float farthestBack;
    GameObject currentBackgroundInstance;
    Coroutine transitionRoutine;
    private bool isStopping = false;
    [Range(0.01f, 0.05f)]
    public float parallaxSpeed;
    private float scrollX;
    // Start is called before the first frame update
    public static ParallaxController Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    void Start()
    {
        cam = Camera.main.transform;
    }

    void BackSpeedCalculate(int backCount)
    {
        farthestBack = 0f;

        for (int i = 0; i < backCount; i++) //find the farthest background
        {
            if ((backgrounds[i].transform.position.z - cam.position.z) > farthestBack)
            {
                farthestBack = backgrounds[i].transform.position.z - cam.position.z;
            }
        }

        for (int i = 0; i < backCount; i++) //set the speed of bacground
        {
            if (Mathf.Approximately(farthestBack, 0f))
            {
                backSpeed[i] = 1f;
                continue;
            }

            backSpeed[i] = 1 - (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }

    private void Update()
    {
        RunParallax();
    }
    public void changeMap(levels level)
    {
        foreach (var layer in backgroundsParent)
        {
            Debug.Log($"Checking layer {layer.layerObject.name} for level {layer.level}");
            if (layer.level == level)
            {
                setBackground(layer.layerObject);
                return;
            }
        }
    }
    private void setBackground(GameObject background)
    {
        if (background == null)
        {
            Debug.LogError("Background is null.");
            return;
        }

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        Transform parent = backgroundContainer != null ? backgroundContainer : transform;
        GameObject nextBackgroundInstance = Instantiate(background, parent);
        Vector3 basePosition = nextBackgroundInstance.transform.position;


        if (currentBackgroundInstance == null)
        {
            currentBackgroundInstance = nextBackgroundInstance;
            BuildParallaxData(currentBackgroundInstance);
            scrollX = EnvironmentManager.Instance.currentSpeedWorld * Time.deltaTime;
            return;
        }

        Vector3 offset = new Vector3(transitionOffset.x, transitionOffset.y, 0f);
        nextBackgroundInstance.transform.position = new Vector3(basePosition.x + offset.x, basePosition.y, basePosition.z);
        transitionRoutine = StartCoroutine(TransitionBackground(currentBackgroundInstance, nextBackgroundInstance, basePosition, offset));
    }

    private void BuildParallaxData(GameObject background)
    {
        int backCount = background.transform.childCount;
        mat = new Material[backCount];
        backSpeed = new float[backCount];
        backgrounds = new GameObject[backCount];

        for (int i = 0; i < backCount; i++)
        {
            backgrounds[i] = background.transform.GetChild(i).gameObject;
            Renderer renderer = backgrounds[i].GetComponent<Renderer>();
            if (renderer != null)
            {
                mat[i] = renderer.material;
            }
        }

        BackSpeedCalculate(backCount);
    }

    private IEnumerator TransitionBackground(GameObject oldBackground, GameObject newBackground, Vector3 basePosition, Vector3 offset)
    {
        float elapsed = 0f;
        float duration = Mathf.Max(transitionDuration, 0.01f);
        Vector3 oldStart = oldBackground.transform.position;
        Vector3 oldEnd = sumOffset(oldStart, new Vector3(-offset.x, -offset.y, 0f));
        Vector3 newStart = sumOffset(newBackground.transform.position, offset);
        Vector3 newEnd = basePosition;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            oldBackground.transform.position = Vector3.Lerp(oldStart, oldEnd, t);
            newBackground.transform.position = Vector3.Lerp(newStart, newEnd, t);
            yield return null;
        }

        oldBackground.transform.position = oldEnd;
        newBackground.transform.position = newEnd;

        Destroy(oldBackground);
        currentBackgroundInstance = newBackground;
        BuildParallaxData(currentBackgroundInstance);
        scrollX = EnvironmentManager.Instance.currentSpeedWorld * Time.deltaTime;
        transitionRoutine = null;
    }

    private void RunParallax()
    {
        if (isStopping) return;
        if (backgrounds == null) return;
        if (mat == null) return;
        if (backSpeed == null) return;
        for (int i = 0; i < backgrounds.Length; i++)
        {
            if (mat[i] == null) continue;
            float speed = backSpeed[i] * parallaxSpeed;
            mat[i].SetTextureOffset("_MainTex", new Vector2(scrollX * speed, 0));
        }
        scrollX += EnvironmentManager.Instance.currentSpeedWorld * Time.deltaTime;
    }
    private Vector3 sumOffset(Vector3 a, Vector3 offset)
    {
        return new Vector3(a.x + offset.x, a.y, a.z);
    }
    public void stopRunningParallax()
    {
        isStopping = true;
    }
    public void resumeRunningParallax()
    {
        isStopping = false;
    }
}