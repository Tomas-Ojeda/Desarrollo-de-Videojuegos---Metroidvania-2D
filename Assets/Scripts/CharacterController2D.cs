using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CharacterController2d : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float sprintSpeedMultiplier = 1.5f; // Multiplicador al esprintar
    public float sprintStaminaCost = 25f;       // Stamina consumida por segundo

    [Header("Salto")]
    public float jumpForce = 12f;

    [Header("Detección de Suelo")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    private Rigidbody2D rb;
    private Animator anim;                      // Referencia al Animator
    private ElisaStamina staminaSystem;
    private float horizontalInput;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isSprinting;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();        // Obtiene el componente Animator
        staminaSystem = GetComponent<ElisaStamina>();
    }

    private void Update()
    {
        // 1. Capturar entrada horizontal (Flechas o A/D)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Comprobar si está tocando el suelo
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;
        }

        // 3. Salto (Tecla Espacio o W)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
        {
            Jump();
        }

        // 4. Lógica de Sprint / Esprintar (Shift Izquierdo)
        bool wantToSprint = Input.GetKey(KeyCode.LeftShift);

        if (wantToSprint && Mathf.Abs(horizontalInput) > 0.1f && staminaSystem != null)
        {
            if (staminaSystem.UseStamina(sprintStaminaCost * Time.deltaTime))
            {
                isSprinting = true;
            }
            else
            {
                isSprinting = false;
            }
        }
        else
        {
            isSprinting = false;
        }

        // 5. Girar el personaje según la dirección
        CheckFlip(horizontalInput);

        // 6. Actualizar variables en el Animator
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        // Calcular la velocidad final dependiendo de si está esprintando o no
        float currentSpeed = moveSpeed * (isSprinting ? sprintSpeedMultiplier : 1f);

        // Aplicar movimiento físico
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
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

        // Pasa la velocidad horizontal absoluta para Idle / Walk / Run
        anim.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));

        // Pasa el estado del suelo
        anim.SetBool("isGrounded", isGrounded);

        // Pasa la velocidad vertical para alternar entre Salto y Caída
        anim.SetFloat("velocityY", rb.linearVelocity.y);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}