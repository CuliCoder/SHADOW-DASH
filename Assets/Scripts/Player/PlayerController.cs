using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [SerializeField] private EnvironmentSO environment;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float doubleJumpForce = 12f;
    [SerializeField] public float runSpeed = 7f;
    [SerializeField] private float JUMP_BUFFER = 0.5f;
    [SerializeField] private float SLIDE_BUFFER = 0.1f;
    [SerializeField] private Vector2 runSize;
    [SerializeField] private Vector2 slideSize;
    [SerializeField] private float slideTime = 0.6f;
    public Rigidbody2D rb { get; private set; }
    public BoxCollider2D col { get; private set; }
    private int jumpCount = 0;
    public bool isGrounded { get; private set; } = true;
    private float jumpBufferCounter = 0f;
    private float coyoteTimeCounter = 0f;
    private float slideBufferCounter = 0f;
    private float slideCounter = 0f;
    public bool isSliding { get; private set; } = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }
    private void Start()
    {
        col.size = runSize;
    }
    private void FixedUpdate()
    {
        ApplyBetterHall();
        slide();
    }
    private void ApplySlide()
    {
        if (InputManager.Instance.IsSlidePressed() && isGrounded)
        {
            startSlide();
        }
    }
    private void ApplyBetterHall()
    {
        rb.velocity += Vector2.up * Physics2D.gravity.y * environment.gravityScale * Time.fixedDeltaTime;
    }
    private void Update()
    {
        ApplyJump();
        jumpBuffer();
        ApplySlide();
        slideBuffer();
    }
    private void ApplyJump()
    {
        if (!InputManager.Instance.IsJumpPressed()) return;
        if (jumpCount >= 2) return;
        Jump();
    }
    private void Jump()
    {
        if (isSliding) return;
        jumpCount++;
        if (isGrounded)
        {
            isGrounded = false;
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            return;
        }
        rb.velocity = new Vector2(rb.velocity.x, doubleJumpForce);
    }
    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.name == "ground")
        {
            isGrounded = true;
            jumpCount = 0;
        }
    }
    private void jumpBuffer()
    {
        if (InputManager.Instance.IsJumpPressed())
        {
            jumpBufferCounter = JUMP_BUFFER;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }
        if (jumpBufferCounter > 0f && isGrounded)
        {
            Jump();
            jumpBufferCounter = 0f;
        }
    }
    private void slideBuffer()
    {
        if (InputManager.Instance.IsSlidePressed())
        {
            slideBufferCounter = SLIDE_BUFFER;
        }
        else
        {
            slideBufferCounter -= Time.deltaTime;
        }
        if (slideBufferCounter > 0f && isGrounded)
        {
            startSlide();
            slideBufferCounter = 0f;
        }
    }
    private void slide()
    {
        if (isSliding)
        {
            slideCounter -= Time.fixedDeltaTime;
            col.size = slideSize;
        }
        if (slideCounter <= 0f)
        {
            isSliding = false;
            col.size = runSize;
        }
    }
    private void startSlide()
    {
        if(isSliding) return;
        isSliding = true;
        slideCounter = slideTime;
    }
}