using UnityEngine;
public class PlayerController : MonoBehaviour
{
    [SerializeField] private EnvironmentSO environment;
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float doubleJumpForce = 12f;
    [SerializeField] private float runSpeed = 7f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float JUMP_BUFFER = 0.5f;
    private int jumpCount = 0;
    private bool isJumpInput = false;
    private bool isGrounded = true;
    private bool canJump = false;
    private float jumpBufferCounter = 0f;
    private float coyoteTimeCounter = 0f;

    private void FixedUpdate()
    {
        ApplyBetterHall();
    }
    private void ApplyBetterHall()
    {
        rb.velocity += Vector2.up * Physics2D.gravity.y * environment.gravityScale * Time.fixedDeltaTime;
    }
    private void Update()
    {
        ApplyJump();
        jumpBuffer();
    }
    private void ApplyJump()
    {
        if (!InputManager.Instance.IsJumpPressed()) return;
        if (jumpCount >= 2) return;
        Jump();
    }
    private void Jump()
    {
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
}