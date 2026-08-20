using UnityEngine;

public class InfectedBase : MonoBehaviour
{
    [Header("Componentes 🧩")]
    protected Animator animator;

    [Header("Estado Actual 🔄")]
    public EnemyState currentState = EnemyState.Idle;

    [Header("Configuración ⚙️")]
    public float moveSpeed = 2f;

    [Header("Detección y Rangos 👁️")]
    [SerializeField] protected float detectionRange = 5f;
    [SerializeField] protected float attackRange = 1.2f;
    [SerializeField] protected LayerMask playerLayer;

    protected Transform playerTransform;

    protected virtual void Awake()
    {
        animator = GetComponent<Animator>();
    }

    protected virtual void Start()
    {
        FindPlayerReference();
    }

    protected virtual void Update()
    {
        DetectPlayerAndEvaluateState();
        HandleStateLogic();
    }

    protected virtual void FindPlayerReference()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, 50f, playerLayer);
        if (playerCollider != null)
        {
            playerTransform = playerCollider.transform;
        }
    }

    protected virtual void DetectPlayerAndEvaluateState()
    {
        if (playerTransform == null)
        {
            FindPlayerReference();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (currentState == EnemyState.Death) return;

        if (distanceToPlayer <= attackRange)
        {
            if (currentState != EnemyState.Attack)
            {
                ChangeState(EnemyState.Attack);
            }
        }
        else if (distanceToPlayer <= detectionRange)
        {
            if (currentState != EnemyState.Chase)
            {
                ChangeState(EnemyState.Chase);
            }
        }
        else
        {
            if (currentState != EnemyState.Idle)
            {
                ChangeState(EnemyState.Idle);
            }
        }
    }

    protected virtual void HandleStateLogic()
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                // En reposo no hace nada
                break;

            case EnemyState.Chase:
                // 1. Moverse hacia el jugador si tenemos la referencia
                if (playerTransform != null)
                {
                    MoveTowardsPlayer();
                    FlipTowardsPlayer();
                }
                break;

            case EnemyState.Attack:
                // Durante el ataque se detiene y mira al jugador
                if (playerTransform != null)
                {
                    FlipTowardsPlayer();
                }
                break;

            case EnemyState.Death:
                break;
        }
    }

    /// <summary>
    /// Desplaza al enemigo en línea recta hacia la posición de Elisa.
    /// </summary>
    protected virtual void MoveTowardsPlayer()
    {
        transform.position = Vector2.MoveTowards(
            transform.position, 
            playerTransform.position, 
            moveSpeed * Time.deltaTime
        );
    }

    /// <summary>
    /// Gira la escala del enemigo para que siempre mire hacia donde está Elisa.
    /// </summary>
    protected virtual void FlipTowardsPlayer()
    {
        if (playerTransform.position.x > transform.position.x)
        {
            // El jugador está a la derecha
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (playerTransform.position.x < transform.position.x)
        {
            // El jugador está a la izquierda
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    public virtual void ChangeState(EnemyState newState)
    {
        currentState = newState;
        UpdateAnimatorParameters();
    }

    protected virtual void UpdateAnimatorParameters()
    {
        if (animator == null) return;

        animator.SetBool("isAlert", currentState == EnemyState.Alert);
        animator.SetBool("isChasing", currentState == EnemyState.Chase);
    }

    public virtual void TriggerAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    public virtual void TriggerDeath()
    {
        if (animator != null)
        {
            animator.SetTrigger("Die");
        }
    }

    protected virtual void OnValidate()
    {
        if (Application.isPlaying && animator != null)
        {
            UpdateAnimatorParameters();
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}