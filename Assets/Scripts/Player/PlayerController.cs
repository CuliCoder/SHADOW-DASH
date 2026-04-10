using UnityEngine;
public class PlayerController : MonoBehaviour
{
    public PlayerStateInfo stateInfo;

    private void Awake()
    {
        stateInfo = new PlayerStateInfo(GetComponent<Rigidbody2D>(), GetComponent<BoxCollider2D>());
    }

    private void Start()
    {
        stateInfo.col.size = stateInfo.runSize;
    }

    private void FixedUpdate()
    {
        ApplyBetterFall();
        UpdateSlide();
    }

    private void ApplySlide()
    {
        if (InputManager.Instance.IsSlidePressed() && stateInfo.isGrounded)
        {
            StartSlide();
        }
    }

    private void ApplyBetterFall()
    {
        stateInfo.rb.velocity += Vector2.up * Physics2D.gravity.y * EnvironmentManager.Instance.environmentSO.gravityScale * Time.fixedDeltaTime;
    }

    private void Update()
    {
        ApplyJump();
        UpdateJumpBuffer();
        ApplySlide();
        UpdateSlideBuffer();
    }

    private void ApplyJump()
    {
        if (!InputManager.Instance.IsJumpPressed()) return;
        if (stateInfo.jumpCount >= 2) return;
        Jump();
    }

    private void Jump()
    {
        if (stateInfo.isSliding) return;

        stateInfo.jumpCount++;
        if (stateInfo.jumpCount >= 2)
        {
            stateInfo.canDoubleJump = false;
        }

        if (stateInfo.isGrounded)
        {
            stateInfo.isGrounded = false;
            stateInfo.rb.velocity = new Vector2(stateInfo.rb.velocity.x, stateInfo.jumpForce);
            return;
        }

        stateInfo.rb.velocity = new Vector2(stateInfo.rb.velocity.x, stateInfo.doubleJumpForce);
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("ground"))
        {
            stateInfo.isGrounded = true;
            stateInfo.jumpCount = 0;
            stateInfo.canDoubleJump = true;
        }
    }

    private void UpdateJumpBuffer()
    {
        if (InputManager.Instance.IsJumpPressed())
        {
            stateInfo.jumpBufferCounter = stateInfo.JUMP_BUFFER;
        }
        else
        {
            stateInfo.jumpBufferCounter -= Time.deltaTime;
        }
        if (stateInfo.jumpBufferCounter > 0f && stateInfo.isGrounded)
        {
            Jump();
            stateInfo.jumpBufferCounter = 0f;
        }
    }

    private void UpdateSlideBuffer()
    {
        if (InputManager.Instance.IsSlidePressed())
        {
            stateInfo.slideBufferCounter = stateInfo.SLIDE_BUFFER;
        }
        else
        {
            stateInfo.slideBufferCounter -= Time.deltaTime;
        }
        if (stateInfo.slideBufferCounter > 0f && stateInfo.isGrounded)
        {
            StartSlide();
            stateInfo.slideBufferCounter = 0f;
        }
    }

    private void UpdateSlide()
    {
        if (stateInfo.isSliding)
        {
            stateInfo.slideCounter -= Time.fixedDeltaTime;
            stateInfo.col.size = stateInfo.slideSize;
        }
        if (stateInfo.slideCounter <= 0f)
        {
            stateInfo.isSliding = false;
            stateInfo.col.size = stateInfo.runSize;
        }
    }

    private void StartSlide()
    {
        if (stateInfo.isSliding) return;

        stateInfo.isSliding = true;
        stateInfo.slideCounter = stateInfo.slideTime;
    }
}