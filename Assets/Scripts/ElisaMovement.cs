using UnityEngine;

public class ElisaMovement : MonoBehaviour
{
    [Header("Movimiento Base")]
    public float walkSpeed = 4f;
    public float runSpeed = 8f;
    private float currentSpeed;

    [Header("Salto y Físicas")]
    public float jumpForce = 12f;
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    private bool isGrounded;

    [Header("Referencias de Componentes")]
    private Rigidbody2D rb;
    private ElisaStamina staminaSystem;
    private Animator anim;
    private CharacterController2d controller;

    private float horizontalInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        staminaSystem = GetComponent<ElisaStamina>();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController2d>();
    }

    private void Update()
    {
        // Si el controller está ejecutando una maniobra especial (Dash, Roll, WallJump), pausamos la lógica de movimiento base
        if (controller != null && controller.IsPerformingAction) return;

        // 1. Lectura de Inputs horizontales
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // 2. Detección de Suelo
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }

        // 3. Determinar Velocidad (Caminar vs Correr con Shift)
        bool isRunningInput = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        
        if (isRunningInput && Mathf.Abs(horizontalInput) > 0.1f)
        {
            currentSpeed = runSpeed;
        }
        else
        {
            currentSpeed = walkSpeed;
        }

        // 4. Salto (Espacio o W)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.W)) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        // 5. Voltear el Sprite según la dirección
        if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // 6. ACTUALIZACIÓN DEL ANIMATOR CONTROLLER
        UpdateAnimatorParameters();
    }

    private void FixedUpdate()
    {
        // Si se está ejecutando Dash, Roll o WallJump, se deja que CharacterController2d maneje el Rigidbody
        if (controller != null && controller.IsPerformingAction) return;

        // Aplicar movimiento horizontal en el ciclo de físicas
        rb.linearVelocity = new Vector2(horizontalInput * currentSpeed, rb.linearVelocity.y);
    }

    private void UpdateAnimatorParameters()
    {
        if (anim == null) return;

        // Velocidad horizontal absoluta para transiciones entre Idle, Walk y Run
        float actualSpeed = Mathf.Abs(rb.linearVelocity.x);
        anim.SetFloat("Speed", actualSpeed);

        // Estado en el aire / en el suelo para el salto
        anim.SetBool("isGrounded", isGrounded);

        // Agacharse (Reservado para Flecha Abajo; 'S' ahora activa el Dash)
        bool isCrouching = Input.GetKey(KeyCode.DownArrow);
        anim.SetBool("isCrouching", isCrouching);
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}