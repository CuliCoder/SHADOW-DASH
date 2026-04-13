using UnityEngine;
[System.Serializable]
public struct PlayerStateInfo
{
    [SerializeField] public float jumpForce;
    [SerializeField] public float doubleJumpForce;
    [SerializeField] public float JUMP_BUFFER;
    [SerializeField] public float SLIDE_BUFFER;
    [SerializeField] public Vector2 runSize;
    [SerializeField] public Vector2 slideSize;
    [SerializeField] public float slideTime;
    public Rigidbody2D rb { get; private set; }
    public BoxCollider2D col { get; private set; }
    public int jumpCount;
    public bool isGrounded;
    public float jumpBufferCounter;
    public float coyoteTimeCounter;
    public float slideBufferCounter;
    public float slideCounter;
    public bool isSliding;
    public bool canDoubleJump;
    public float originPositionY;
    public bool isDead;
    public PlayerStateInfo(Rigidbody2D rb, BoxCollider2D col)
    {
        this.rb = rb;
        this.col = col;
        jumpBufferCounter = 0f;
        coyoteTimeCounter = 0f;
        slideBufferCounter = 0f;
        slideCounter = 0f;
        isSliding = false;
        jumpCount = 0;
        isGrounded = true;
        jumpForce = 25f;
        doubleJumpForce = 20f;
        slideTime = 0.7f;
        JUMP_BUFFER = 0.5f;
        SLIDE_BUFFER = 0.1f;
        runSize = new Vector2(2.464787f, 4.749633f);
        slideSize = new Vector2(3.932135f, 3.755673f);
        canDoubleJump = true;
        originPositionY = -10.24046f;
        isDead = false;
    }
}