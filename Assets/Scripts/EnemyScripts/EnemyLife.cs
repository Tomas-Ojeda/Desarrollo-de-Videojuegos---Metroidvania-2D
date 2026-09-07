using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class EnemyLife : MonoBehaviour, IDamageable
{
    public enum AIState
    {
        Patrolling,
        Chasing,
        Attacking,
        Idle
    }

    [Header("AI Configuration")]
    public AIState currentState = AIState.Patrolling;

    [Header("Movement Settings")]
    [Range(0.5f, 5f)] public float patrolSpeed = 2f;
    [Range(0.5f, 10f)] public float chaseSpeed = 3.5f;

    [Header("Jump Settings")]
    public float jumpForce = 12f;
    [Range(0.1f, 2f)] public float jumpCooldown = 1.2f; // Tiempo entre saltos
    [Range(0.1f, 1f)] public float jumpDelay = 0.35f;   // Retraso para reaccionar antes de saltar

    [Header("Attack Settings")]
    [Range(0.5f, 5f)] public float attackRange = 1.2f;
    [Range(0f, 3f)] public float attackCooldown = 1f;
    public int attackDamage = 15;

    [Header("Sight Settings")]
    [Range(5f, 20f)] public float sightRange = 8f;

    [Header("Health")]
    [Range(1f, 200f)] public float health = 100f;

    [Header("Patrol Points")]
    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    [Header("Ground Check")]
    public Transform groundCheck;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;

    private float lastAttackTime;
    private float nextJumpTime;
    private float jumpTimer;
    private Transform player;
    private Rigidbody2D rb;
    private Animator anim;
    private bool isDead;
    private bool facingRight = true;
    private bool isGrounded;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        GameObject playerObj = GameObject.Find("Elisa");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        lastAttackTime = Time.time;
        currentPatrolIndex = 0;
        isDead = false;
    }

    private void Update()
    {
        if (isDead || player == null) return;

        // Comprobar si está tocando el suelo usando el objeto asignado
        if (groundCheck != null)
        {
            isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        }
        else
        {
            isGrounded = Mathf.Abs(rb.linearVelocity.y) < 0.01f;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        HandleStateTransitions(distanceToPlayer);

        switch (currentState)
        {
            case AIState.Patrolling:
                PatrolBehavior();
                break;
            case AIState.Chasing:
                ChaseBehavior();
                break;
            case AIState.Attacking:
                AttackBehavior();
                break;
            case AIState.Idle:
                IdleBehavior();
                break;
        }

        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (anim == null) return;

        // Evaluamos si el enemigo se está moviendo horizontalmente
        float horizontalSpeed = Mathf.Abs(rb.linearVelocity.x);
        bool isMoving = horizontalSpeed > 0.1f;

        // Actualizamos los parámetros exactamente con los nombres de tu Animator
        anim.SetBool("isMoving", isMoving);
        anim.SetBool("isRunnig", currentState == AIState.Chasing && isMoving);
    }

    private void HandleStateTransitions(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange)
        {
            currentState = AIState.Attacking;
        }
        else if (distanceToPlayer < sightRange)
        {
            currentState = AIState.Chasing;
        }
        else
        {
            currentState = AIState.Patrolling;
        }
    }

    private void IdleBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    private void PatrolBehavior()
    {
        jumpTimer = 0f; // Reinicia intención de salto si patrulla

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        Transform targetPoint = patrolPoints[currentPatrolIndex];
        float direction = targetPoint.position.x - transform.position.x;

        rb.linearVelocity = new Vector2(Mathf.Sign(direction) * patrolSpeed, rb.linearVelocity.y);

        CheckFlip(direction);

        if (Mathf.Abs(transform.position.x - targetPoint.position.x) < 0.3f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    private void ChaseBehavior()
    {
        float directionX = player.position.x - transform.position.x;

        rb.linearVelocity = new Vector2(Mathf.Sign(directionX) * chaseSpeed, rb.linearVelocity.y);

        CheckFlip(directionX);

        // LÓGICA DE SALTO CON DELAY Y COOLDOWN
        float heightDifference = player.position.y - transform.position.y;

        if (heightDifference > 1.2f && isGrounded && Time.time >= nextJumpTime)
        {
            jumpTimer += Time.deltaTime;

            if (jumpTimer >= jumpDelay)
            {
                Jump();
                jumpTimer = 0f;
                nextJumpTime = Time.time + jumpCooldown;
            }
        }
        else
        {
            jumpTimer = 0f;
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private void AttackBehavior()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        float directionX = player.position.x - transform.position.x;
        CheckFlip(directionX);

        if (Time.time - lastAttackTime > attackCooldown)
        {
            // Trigger del Animator
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            // Infligir daño a Elisa
            if (player != null)
            {
                ElisaHealth playerHealth = player.GetComponent<ElisaHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(attackDamage);
                }
            }

            lastAttackTime = Time.time;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log($"Enemigo recibió {damage} de daño. Vida restante: {health}");

        if (health <= 0)
        {
            DieBehavior();
        }
    }

    private void DieBehavior()
    {
        isDead = true;

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // Desactivar el Collider para que Elisa pueda atravesarlo tras morir
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;

        Debug.Log("¡Enemigo derrotado!");
        Destroy(gameObject, 2f);
    }

    private void CheckFlip(float moveDirection)
    {
        if (moveDirection > 0.1f && !facingRight)
        {
            Flip();
        }
        else if (moveDirection < -0.1f && facingRight)
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);

        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}