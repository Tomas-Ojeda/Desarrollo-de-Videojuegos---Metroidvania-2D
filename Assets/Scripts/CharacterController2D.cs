using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2d : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float sprintSpeedMultiplier = 1.5f;
    public float sprintStaminaCost = 25f;

    [Header("Salto")]
    public float jumpForce = 12f;

    [Header("Detección de Suelo y Pared")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Dash (En Aire)")]
    public float dashSpeed = 16f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.8f;
    public float dashStaminaCost = 20f;

    [Header("Voltereta / Roll (En Piso con Movimiento)")]
    public float rollSpeed = 10f;
    public float rollDuration = 0.4f;
    public float rollCooldown = 0.6f;
    public float rollStaminaCost = 15f;

    [Header("Estados Generales")]
    public bool isInvulnerable { get; private set; }

    [Header("Wall Slide & Wall Jump")]
    public float wallSlideSpeed = 2f;
    public Vector2 wallJumpForce = new Vector2(10f, 12f);
    public float wallJumpDuration = 0.15f;

    // Componentes y referencias
    private Rigidbody2D rb;
    private Animator anim;
    private ElisaStamina staminaSystem;

    // Estados
    private float horizontalInput;
    private bool isGrounded;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isWallJumping;
    private bool isDashing;
    private bool canDash = true;
    private bool isRolling;
    private bool canRoll = true;
    private bool facingRight = true;
    private bool isSprinting;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        staminaSystem = GetComponent<ElisaStamina>();
    }

    private void Update()
    {
        // Si está haciendo Dash, Roll o Wall Jump, se bloquea la captura estándar de inputs
        if (isDashing || isRolling || isWallJumping) return;

        // 1. Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Detección física
        CheckSurroundings();

        // 3. Mecánica Unificada de Dash / Roll (Tecla C)
        if (Input.GetKeyDown(KeyCode.C))
        {
            // Solo hace Roll si está en el suelo Y moviéndose
            if (isGrounded && canRoll && Mathf.Abs(horizontalInput) > 0.1f)
            {
                if (staminaSystem == null || staminaSystem.UseStamina(rollStaminaCost))
                {
                    StartCoroutine(PerformRoll());
                    return;
                }
            }
            // Si está en el aire -> Hace Dash
            else if (!isGrounded && canDash)
            {
                if (staminaSystem == null || staminaSystem.UseStamina(dashStaminaCost))
                {
                    StartCoroutine(PerformDash());
                    return;
                }
            }
        }

        // 4. Mecánica de Salto y Wall Jump
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (isWallSliding)
            {
                StartCoroutine(PerformWallJump());
            }
        }

        // 5. Lógica de Sprint
        bool wantToSprint = Input.GetKey(KeyCode.LeftShift);
        if (wantToSprint && Mathf.Abs(horizontalInput) > 0.1f && staminaSystem != null)
        {
            isSprinting = staminaSystem.UseStamina(sprintStaminaCost * Time.deltaTime);
        }
        else
        {
            isSprinting = false;
        }

        // 6. Lógica de Wall Slide
        CheckWallSlide();

        // 7. Volteo de personaje y actualización del Animator
        if (!isWallSliding)
        {
            CheckFlip(horizontalInput);
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDashing || isRolling || isWallJumping) return;

        float currentSpeed = moveSpeed * (isSprinting ? sprintSpeedMultiplier : 1f);

        if (isWallSliding)
        {
            if (rb.linearVelocity.y < -wallSlideSpeed)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -wallSlideSpeed);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
        }
    }

    private void CheckSurroundings()
    {
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        if (wallCheck != null)
        {
            isWallSliding = false;
            isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
        }
    }

    private void CheckWallSlide()
    {
        bool pushingAgainstWall = (facingRight && horizontalInput > 0) || (!facingRight && horizontalInput < 0);
        
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y < 0 && pushingAgainstWall)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private IEnumerator PerformWallJump()
    {
        isWallJumping = true;
        isWallSliding = false;

        float wallJumpDirection = facingRight ? -1f : 1f;

        rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);
        Flip();

        yield return new WaitForSeconds(wallJumpDuration);

        isWallJumping = false;
    }

    private IEnumerator PerformDash()
    {
        canDash = false;
        isDashing = true;
        isInvulnerable = true;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float dashDirection = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);

        if (anim != null) 
        {
            // Seteamos velocityY en 0 para evitar que el Animator corte la transición por velocityY < -0.1
            anim.SetFloat("velocityY", 0f);
            anim.SetTrigger("Dash");
        }

        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
        isInvulnerable = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private IEnumerator PerformRoll()
    {
        canRoll = false;
        isRolling = true;
        isInvulnerable = true;

        float rollDirection = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(rollDirection * rollSpeed, rb.linearVelocity.y);

        if (anim != null && isGrounded)
        {
            anim.SetTrigger("Roll");
        }

        yield return new WaitForSeconds(rollDuration);

        isRolling = false;
        isInvulnerable = false;

        yield return new WaitForSeconds(rollCooldown);
        canRoll = true;
    }

    private void CheckFlip(float direction)
    {
        if (direction > 0.1f && !facingRight)
        {
            Flip();
        }
        else if (direction < -0.1f && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private void UpdateAnimator()
    {
        if (anim == null) return;

        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        
        // Solo actualizamos velocityY si no estamos haciendo dash ni roll
        if (!isDashing && !isRolling)
        {
            anim.SetFloat("velocityY", rb.linearVelocity.y);
        }

        anim.SetBool("isWallSliding", isWallSliding);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(wallCheck.position, wallCheckRadius);
        }
    }
}