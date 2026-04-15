using UnityEngine;
public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

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
    }
    public bool IsJumpPressed()
    {
        return Input.GetButtonDown("Jump");
    }

    public bool IsSlidePressed()
    {
        return Input.GetButtonDown("Slide");
    }
}