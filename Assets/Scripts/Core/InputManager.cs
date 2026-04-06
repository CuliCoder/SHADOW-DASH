using UnityEngine;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public bool IsJumpPressed()
    {
        return Input.GetButtonDown("Jump");
    }

    public bool IsFirePressed()
    {
        return Input.GetButtonDown("Fire1");
    }
}