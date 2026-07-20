using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // Components
    private Rigidbody2D rb;
    private BoxCollider2D collider;

    // Movement attributes
    [SerializeField, Range(0f, 100f)] private float speed = 5f;
    [SerializeField, Range(0f, 100f)] private float jumpForce = 10f;
    [SerializeField] private float horizontalInput;
    [SerializeField] private bool isFacingRight = true;

    // Jump attributes
    [SerializeField, Range(1, 10)] private int maxJumps = 2;
    [SerializeField] private int jumpsRemaining;

    // Ground check
    [SerializeField] private LayerMask groundLayer;

    // Wall interaction
    [Header("Wall Interaction")]
    [SerializeField] private Transform wallCheck;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckRadius = 0.2f;
    [SerializeField] private float wallSlidingSpeed = 2f;
    private bool _isWallSliding;

    // Falling attributes
    [Header("Falling")]
    [SerializeField, Range(0.5f, 5f)] private float normalGravityScale = 1f;
    [SerializeField, Range(0.1f, 2f)] private float fallGravityMultiplier = 0.5f;
    [SerializeField] private float smoothFallDuration = 2f;
    [SerializeField] private Coroutine smoothFallCoroutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<BoxCollider2D>();
        rb.gravityScale = normalGravityScale;
        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        if (IsGrounded() && rb.linearVelocityY <= 0.1f)
        {
            jumpsRemaining = maxJumps;
            _isWallSliding = false;
        }

        HandleInput();
        HandleJump();
        HandleSmoothFall();
        Flip();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleWallSliding();
    }

    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
    }

    private void HandleMovement()
    {
        if (!_isWallSliding)
        {
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocityY);
        }
    }

    private void HandleWallSliding()
    {
        if (IsTouchingWall() && !IsGrounded() && horizontalInput != 0)
        {
            _isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -wallSlidingSpeed));
            jumpsRemaining = maxJumps;
        }
        else
        {
            _isWallSliding = false;
        }
    }

    private void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, 0f);
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            jumpsRemaining--;
        }
    }

    private void HandleSmoothFall()
    {
        if (Input.GetButtonDown("Jump") && IsFalling() && smoothFallCoroutine == null && jumpsRemaining == 0)
        {
            smoothFallCoroutine = StartCoroutine(SmoothFallRoutine());
        }

        if (IsGrounded() && smoothFallCoroutine != null)
        {
            StopCoroutine(smoothFallCoroutine);
            smoothFallCoroutine = null;
            ResetGravity();
        }
    }

    private bool IsGrounded()
    {
        float extraHeight = 0.1f;
        RaycastHit2D raycastHit = Physics2D.BoxCast(collider.bounds.center, collider.bounds.size, 0f, Vector2.down, extraHeight, groundLayer);
        return raycastHit.collider != null;
    }

    private bool IsTouchingWall()
    {
        return Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
    }

    private bool IsFalling()
    {
        return rb.linearVelocityY < -0.1f;
    }

    private void Flip()
    {
        if ((isFacingRight && horizontalInput < 0f) || (!isFacingRight && horizontalInput > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator SmoothFallRoutine()
    {
        rb.gravityScale = normalGravityScale * fallGravityMultiplier;
        yield return new WaitForSeconds(smoothFallDuration);
        ResetGravity();
        smoothFallCoroutine = null;
    }

    private void ResetGravity()
    {
        rb.gravityScale = normalGravityScale;
    }

    private void OnDrawGizmos()
    {
        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}
