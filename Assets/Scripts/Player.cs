using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class Player : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator anim;
    private float xInput;
    private float yInput;
    private int facingDir = 1;
    private bool facingRight = true;
    private bool isAirBorn;


    [Header("Movement")]
    [SerializeField]
    private float moveSpeed;
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float doubleJumpForce;
    private bool canDoubleJump;
    private bool isWallJumping;


    [Header("Buffer & Coyote Jumo")]
    [SerializeField]
    private float bufferJumpWindow = .025f;
    private float bufferJumpActivated = -1;
    [SerializeField]
    private float coyoteJumpWindow = 0.5f;
    private float coyoteJumpActivated = -1;
    

    [Header("Wall Interactions")]
    [SerializeField]
    private float wallJumpDuration = 0.6f;
    [SerializeField]
    private Vector2 wallJumpForce;
    

    [Header("Knockback")]
    [SerializeField]
    private float knockBackDuration;
    [SerializeField]
    private Vector2 knockBackForce;
    private bool isKnocked;
    

    [Header("Collision")]
    [SerializeField]
    private float groundCheckDistance;
    [SerializeField]
    private LayerMask whatIsGround;
    [SerializeField]
    private float wallCheckDistance;
    private bool isGrounded;
    private bool isWallDetected;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponentInChildren<Animator>();
        
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
            KnockBack();


        AirbornStatus();

        if (isKnocked)
            return;

        HandleInput();
        HandleWallSlide();
        HandleMovement();
        HandleFlip();
        HandleCollision();
        HandleAnimations();
    }

    public void KnockBack()
    {
        if (isKnocked)
            return;

        StartCoroutine(KnockBackRoutine());
        anim.SetTrigger("knockBack");
        rb.linearVelocity = new Vector2(knockBackForce.x * -facingDir, knockBackForce.y);
    }

    private IEnumerator KnockBackRoutine()
    {

        isKnocked = true;
        yield return new WaitForSeconds(knockBackDuration);
        isKnocked = false;
    }

    private void AirbornStatus()
    {
        if (isGrounded && isAirBorn)
            HandleLanding();

        if (!isGrounded && !isAirBorn)
            BecomeAirBorne();
    }

    private void BecomeAirBorne()
    {
        isAirBorn = true;

        if (rb.linearVelocity.y < 0)
        {

            ActivateCoyotejump();

        }
    }

    private void HandleLanding()
    {
        isAirBorn = false;
        canDoubleJump = true;

        AttemptBufferJump();
    }

    private void HandleInput()
    {
        xInput = Input.GetAxisRaw("Horizontal");
        yInput = Input.GetAxisRaw("Vertical");


        if (Input.GetKeyDown(KeyCode.Space))
        {

            JumpButton();
            BufferJumpRequest();

        }
    }

    private void JumpButton()
    {
        bool coyoteJumpAvailable = Time.time < coyoteJumpActivated + coyoteJumpWindow;
         
        if (isGrounded || coyoteJumpAvailable)
        {

            Jump();

        }else if(isWallDetected && !isGrounded)
        {

            WallJump();
        }
        else if (canDoubleJump)
        {
            DoubleJump();
        }

        DeactiveCoyoteJump();
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void WallJump()
    {
        canDoubleJump = true;
        rb.linearVelocity = new Vector2(wallJumpForce.x * -facingDir, wallJumpForce.y);

        Flip();

        StopAllCoroutines();
        StartCoroutine(WallJumpRoutine());
    }

    private IEnumerator WallJumpRoutine()
    {
        isWallJumping = true;

        yield return new WaitForSeconds(wallJumpDuration);

        isWallJumping = false;
    }


    private void HandleWallSlide()
    {
        bool canWallSlide = isWallDetected && rb.linearVelocity.y < 0;
        float yModifier = yInput < 0 ? 1 : 0;


        if (!canWallSlide)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * yModifier);

    }

    private void DoubleJump()
    {
        isWallJumping = false;
        canDoubleJump = false;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, doubleJumpForce);
    }

    #region Buffer & Coyote Jump

    private void BufferJumpRequest()
    {
        if (isAirBorn)
            bufferJumpActivated = Time.time;
    }

    private void AttemptBufferJump()
    {
        if (Time.time < bufferJumpActivated + bufferJumpWindow)
        {
            bufferJumpActivated = Time.time - 1;
            Jump();
        }
    }

    private void ActivateCoyotejump()
    {
        coyoteJumpActivated = Time.time;
    }

    private void DeactiveCoyoteJump()
    {
        coyoteJumpActivated = Time.time - 1;
    }

#endregion


    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x, transform.position.y - groundCheckDistance));
        Gizmos.DrawLine(transform.position, new Vector2(transform.position.x + (wallCheckDistance * facingDir), transform.position.y));
    }

    private void HandleCollision()
    {
        isGrounded = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, whatIsGround);
        isWallDetected = Physics2D.Raycast(transform.position, Vector2.right * facingDir, wallCheckDistance, whatIsGround);
    }

    private void HandleAnimations()
    {
        anim.SetFloat("xVelocity", rb.linearVelocity.x);
        anim.SetFloat("yVelocity", rb.linearVelocity.y);
        anim.SetBool("isGrounded", isGrounded);
        anim.SetBool("isWallDetected", isWallDetected);
    }

    private void HandleMovement()
    {
        if (isWallDetected)
            return;

        if (isWallJumping)
            return;

        rb.linearVelocity = new Vector2(xInput * moveSpeed, rb.linearVelocity.y);

    }

    private void HandleFlip()
    {
        if (xInput < 0 && facingRight || xInput > 0 && !facingRight)
        {
            Flip();
        }
    }
    private void Flip()
    {
        facingDir = facingDir * -1;
        transform.Rotate(0, 180, 0);
        facingRight = !facingRight;
    }


}
