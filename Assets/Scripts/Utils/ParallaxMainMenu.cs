using UnityEngine;
public class ParallaxMainMenu : MonoBehaviour
{
    private Material[] mat;
    private GameObject[] backgrounds;
    private float[] backSpeed;
    private Transform cam;
    private void Start()
    {
        cam = Camera.main.transform;
        BuildParallaxData(gameObject);
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
    private void BackSpeedCalculate(int backCount)
    {
        float farthestBack = 0f;

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
            backSpeed[i] = (backgrounds[i].transform.position.z - cam.position.z) / farthestBack;
        }
    }
    private void RunParallax()
    {
        for (int i = 0; i < backgrounds.Length; i++)
        {
            Vector2 offset = mat[i].mainTextureOffset;
            offset.x += backSpeed[i] * Time.deltaTime;
            mat[i].mainTextureOffset = offset;
        }
    }
    private void Update()
    {
        if (backgrounds != null)
        {
            RunParallax();
        }
    }
}