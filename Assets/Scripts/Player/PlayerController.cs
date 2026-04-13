using System.Collections;
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
        if (stateInfo.isDead) return;
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
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("obstacle"))
        {
            StartCoroutine(TransitionPlayerWhenDie());
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
    public IEnumerator TransitionPlayerWhenDie()
    {
        ScoreManager.Instance.Pause(null);
        stateInfo.isDead = true;
        while (transform.position.y > stateInfo.originPositionY)
        {
            transform.position += Vector3.down * 10f * Time.unscaledDeltaTime;
            yield return null;
        }
        ResetState();
        yield return new WaitForSecondsRealtime(1.5f);
        ScoreManager.Instance.enableMenuPause();
    }
    private void ResetState()
    {
        stateInfo.isGrounded = true;
        stateInfo.canDoubleJump = true;
        stateInfo.jumpCount = 0;
        stateInfo.isSliding = false;
        stateInfo.slideCounter = 0f;
        stateInfo.col.size = stateInfo.runSize;
        stateInfo.rb.velocity = Vector2.zero;
        stateInfo.jumpBufferCounter = 0f;
        stateInfo.slideBufferCounter = 0f;
    }
}