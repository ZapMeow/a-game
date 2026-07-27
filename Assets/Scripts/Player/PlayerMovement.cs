using System;
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private static readonly int JumpAnimation = Animator.StringToHash("Jump");
    private static readonly int MovementAnimation = Animator.StringToHash("Movement");
    private static readonly int SmoothFallAnimation = Animator.StringToHash("SmoothFall");
    private static readonly int WallSlideAnimation = Animator.StringToHash("WallSlide");
    private static readonly int MovementYAnimation = Animator.StringToHash("MovementY");
    
    [Header("Player state")]
    [SerializeField] private PlayerStates state;
    [Space(2)]
    //Components
    [SerializeField] private BoxCollider2D boxCollider2D;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private Transform wallCheck;

    //Movement
    [SerializeField] private Vector2 movement;
    [SerializeField, Range(-1f, 1f)] private float horizontalInput;
    [SerializeField, Range(0.1f, 20f)] private float speed = 7.7f;
    [SerializeField, Range(0.1f, 10f)] private float jumpForce = 5;
    [SerializeField, Range(0, 10)] private int maxJumps = 1;
    [SerializeField] private int availableJumps;
    [SerializeField] private bool rightMovement;
    
    //low fall
    [SerializeField, Range(0.1f, 1f)] private float lowGravity = 0.5f;
    [SerializeField] private float gravity;
    [SerializeField, Range(0.1f, 5f)] private float timeFalling;
    [SerializeField] private bool isLowFalling = false;
    [SerializeField] private bool canLowFallingAgain = false;
    
    //Booleans states
    [SerializeField] private bool isGrounded = false;
    [SerializeField] private bool isWalled = false;
    
    //Layers
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float radiusChecker;
    
    //Wall sliding attributes
    [SerializeField] private bool isWalling;
    [SerializeField] private bool isWallJumping = false;
    [SerializeField, Range(0.1f, 1f)] private float wallJumpTime = 0.2f;
    [SerializeField, Range(0.1f, 20f)] private float wallJumpForceX = 10f;
    [SerializeField, Range(0.1f, 20f)] private float wallJumpForceY = 10f;
    [SerializeField, Range(-1f, -0.1f)] private float smoothSliding;
    [SerializeField, Range(1f, 10f)] private float timeSliding;
    [SerializeField] private bool canSlidingAgain;
    
    //Animator
    [SerializeField] private Animator animator;


    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Start()
    {
        gravity = rb.gravityScale;
        state = PlayerStates.Jump;
        canSlidingAgain = false;
        rightMovement = true;
        horizontalInput = 0;
        movement = new Vector2(0, 0);
        availableJumps = maxJumps;
        animator.SetBool(SmoothFallAnimation, false);
        animator.SetBool(JumpAnimation, false);
        
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        animator.SetFloat(MovementAnimation, Mathf.Abs(horizontalInput));
        animator.SetFloat(MovementYAnimation, rb.linearVelocityY);
        FlipState();
        IsGrounded();
        IsWalled();

        if (isWalled && !isGrounded && canSlidingAgain && !isWalling)
        {
            StartCoroutine(WallSliding());
        }

        if (Input.GetButtonDown("Jump"))
        {
            DoStateJump();
        }
    }

    private void FixedUpdate()
    {
        if (!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontalInput * speed, rb.linearVelocityY);
        }
    }

    private void DoStateJump()
    {
        //Jump State
        if (availableJumps > 0 && !isWalling)
        {
            animator.SetBool(JumpAnimation, true);
            animator.SetBool(SmoothFallAnimation, false);
            animator.Play("Base Layer.jump-test", 0, 0f);
            state = PlayerStates.Jump;
            if (!isGrounded)
            {
                availableJumps--;
            }
            rb.linearVelocity = new Vector2(rb.linearVelocityX, 0); 
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); 
        }
        
        //Low Falling state
        if (availableJumps <= 0 && !isWalling && !isLowFalling && canLowFallingAgain && rb.linearVelocityY < 0f)
        {
            state = PlayerStates.LowFall;
            StartCoroutine(LowFalling());
        }
        
        //Wall sliding state
        if (isWalled && !isGrounded && canSlidingAgain && !isWalling)
        {
            StartCoroutine(WallSliding());
        }
    }

    private void FlipState()
    {
        if (!isWallJumping && ((rb.linearVelocityX < 0f && rightMovement) || (rb.linearVelocityX > 0f && !rightMovement)))
        {
            rightMovement = !rightMovement;
            transform.localScale =
                new Vector3(transform.localScale.x * -1f, transform.localScale.y, transform.localScale.z);
        }
    }

    private IEnumerator WallSliding()
    {
        isWalling = true;
        float timer = 0;
        animator.SetBool(WallSlideAnimation, true);
        while (timer < timeSliding && isWalled && !isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                animator.Play("Base Layer.jump-test", 0, 0f);
                isWallJumping = true;
                rb.linearVelocity = Vector2.zero;
                
                float forceX = -transform.localScale.x * wallJumpForceX;
                rb.AddForce(new Vector2(forceX, wallJumpForceY), ForceMode2D.Impulse);
                
                Invoke(nameof(StopWallJumping), wallJumpTime);
                break;
            }

            timer += Time.deltaTime;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, smoothSliding);
            yield return null;
        }
        animator.SetBool(WallSlideAnimation, false);
        canSlidingAgain = false;
        isWalling = false;
    }
    
    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private IEnumerator LowFalling()
    {
        isLowFalling = true;
        float timer = 0;
        rb.gravityScale = lowGravity;
        animator.SetBool(SmoothFallAnimation, true);
        while (timer < timeFalling && !isGrounded)
        {
            Debug.Log((timer < timeFalling) + " Falling");
            timer += Time.deltaTime;
            yield return null;
        }
        canLowFallingAgain = false;
        isLowFalling = false;
        animator.SetBool(SmoothFallAnimation, false);
        rb.gravityScale = gravity;
    }

    private void IsGrounded()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, radiusChecker, groundLayer) && rb.linearVelocityY <= 0.01f;

        if (/*!wasGrounded && */isGrounded)
        {
            animator.SetBool(JumpAnimation, false);
            isWallJumping = false;
            canLowFallingAgain = true;
            availableJumps = maxJumps;
            canSlidingAgain = true;
        }
    }

    private void IsWalled()
    {
        isWalled = Physics2D.OverlapCircle(wallCheck.position, radiusChecker, wallLayer);
    }

    public bool GetIsGrounded()
    {
        return isGrounded;
    }



    public bool GetIsWalled()
    {
        return isWalled;
    }
}

public enum PlayerStates{
    Ground,
    Jump,
    LowFall,
    WallSliding
}