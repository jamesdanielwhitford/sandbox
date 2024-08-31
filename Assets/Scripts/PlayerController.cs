using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 10f;
    public float acceleration = 7f;
    public float deceleration = 7f;
    public float velocityPower = 0.9f;
    public float frictionAmount = 0.2f;

    [Header("Jump")]
    public float jumpForce = 15f;
    public float jumpCutMultiplier = 0.5f;
    public float fallGravityMultiplier = 1.5f;
    public float maxFallSpeed = 25f;
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Checks")]
    [SerializeField] private Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    // Private variables
    private Rigidbody2D rb;
    private float horizontal;
    private bool isFacingRight = true;
    private bool isJumping = false;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private Animator animator;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // Jump input
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Jump cut
        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
        }

        // Flip character
        if (horizontal > 0f && !isFacingRight)
        {
            Flip();
        }
        else if (horizontal < 0f && isFacingRight)
        {
            Flip();
        }

        // Coyote time
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }
        animator.SetFloat("Speed", Mathf.Abs(rb.velocity.x));
        animator.SetBool("IsJumping", !IsGrounded());
    }

    private void FixedUpdate()
    {
        // Apply movement
        float targetSpeed = horizontal * moveSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        float movement = Mathf.Pow(Mathf.Abs(speedDiff) * accelRate, velocityPower) * Mathf.Sign(speedDiff);
        rb.AddForce(movement * Vector2.right);

        // Apply friction
        if (IsGrounded() && Mathf.Abs(horizontal) < 0.01f)
        {
            float amount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(frictionAmount));
            amount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -amount, ForceMode2D.Impulse);
        }

        // Handle jump
        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping)
        {
            isJumping = true;
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // Apply extra gravity when falling
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallGravityMultiplier;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = 1f;
        }

        isJumping = !IsGrounded();
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1f;
        transform.localScale = localScale;
    }

    private void OnDrawGizmos()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}