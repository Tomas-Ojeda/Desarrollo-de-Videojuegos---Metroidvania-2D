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
    [Range(0f, 1f)]
    public float jumpCutMultiplier = 0.5f;

    [Header("Detección de Suelo y Pared")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public Transform wallCheck;
    public float wallCheckRadius = 0.2f;
    public LayerMask groundLayer;

    [Header("Dash (Aire y Tierra)")]
    public float dashSpeed = 22f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    public float dashStaminaCost = 20f;

    [Header("Voltereta / Roll (En Piso con Movimiento)")]
    public float rollSpeed = 12f;
    public float rollDuration = 0.4f;
    public float rollCooldown = 0.6f;
    public float rollStaminaCost = 15f;

    [Header("Estados Generales")]
    public bool isInvulnerable { get; private set; }

    public bool IsPerformingAction => isDashing || isRolling || isWallJumping;

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
        if (IsPerformingAction) return;

        // 1. Inputs
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Detección física
        CheckSurroundings();

        // 3. Mecánica Unificada de Dash / Roll (Teclas C o S)
        bool dashOrRollInput = Input.GetKeyDown(KeyCode.C) || Input.GetKeyDown(KeyCode.S);

        if (dashOrRollInput)
        {
            if (isGrounded && canRoll && Mathf.Abs(horizontalInput) > 0.1f)
            {
                if (staminaSystem == null || staminaSystem.UseStamina(rollStaminaCost))
                {
                    StartCoroutine(PerformRoll());
                    return;
                }
            }
            else if (canDash)
            {
                if (staminaSystem == null || staminaSystem.UseStamina(dashStaminaCost))
                {
                    StartCoroutine(PerformDash());
                    return;
                }
            }
        }

        // 4. Mecánica de Salto Variable y Wall Jump
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

        // Control de Salto Variable
        if (Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.W))
        {
            OnJumpUp();
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
        if (IsPerformingAction) return;

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
            isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, groundLayer);
        }
    }

    private void CheckWallSlide()
    {
        // Se puede deslizar en pared si está en el aire, tocando la pared y cayendo (o mantenido contra ella)
        bool pushingAgainstWall = (facingRight && horizontalInput > 0) || (!facingRight && horizontalInput < 0);
        
        if (isTouchingWall && !isGrounded && rb.linearVelocity.y <= 0 && pushingAgainstWall)
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

    private void OnJumpUp()
    {
        if (rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    private IEnumerator PerformWallJump()
    {
        isWallJumping = true;
        isWallSliding = false;

        // Salta impulsándose en la dirección opuesta hacia la que mira
        float wallJumpDirection = facingRight ? -1f : 1f;

        Flip(); // Voltea el personaje para que mire en la dirección del salto

        rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpForce.x, wallJumpForce.y);

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

        float dashDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : (facingRight ? 1f : -1f);

        if (anim != null) 
        {
            anim.SetFloat("velocityY", 0f);
            anim.SetTrigger("Dash");
        }

        float elapsedTime = 0f;
        while (elapsedTime < dashDuration)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        rb.gravityScale = originalGravity;
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
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

        float rollDirection = horizontalInput != 0 ? Mathf.Sign(horizontalInput) : (facingRight ? 1f : -1f);

        if (anim != null && isGrounded)
        {
            anim.SetTrigger("Roll");
        }

        float elapsedTime = 0f;
        while (elapsedTime < rollDuration)
        {
            rb.linearVelocity = new Vector2(rollDirection * rollSpeed, rb.linearVelocity.y);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

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