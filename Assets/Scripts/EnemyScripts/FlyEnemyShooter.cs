using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class FlyingEnemyShooter : MonoBehaviour
{
    [Header("Health")]
    [Range(1f, 200f)] public float health = 50f;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float patrolSpeed = 2f;

    [Header("Persecución")]
    public float chaseSpeed = 3.5f;
    public float detectionRange = 7f;

    [Header("Ataque a Distancia")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float attackRange = 5f;
    public float fireRate = 1f; // Disparos por segundo
    public float projectileSpeed = 8f;

    [Header("General")]
    public float stoppingDistance = 0.2f;

    private Rigidbody2D rb;
    private Transform player;
    private int currentPointIndex = 0;
    private bool chasing = false;
    private float nextFireTime = 0f;
    private bool facingRight = true;
    private bool isDead = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Busca automáticamente a Elisa por nombre
        GameObject playerObj = GameObject.Find("Elisa");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            Debug.LogWarning("FlyingEnemyShooter: Asigna al menos 2 puntos de patrulla en el Inspector.");
        }
    }

    private void Update()
    {
        if (isDead) return;

        DetectPlayer();

        if (chasing && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            CheckFlip(player.position.x - transform.position.x);

            if (distanceToPlayer <= attackRange)
            {
                Attack();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isDead) return;

        if (chasing && player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > attackRange)
            {
                MoveTowards(player.position, chaseSpeed);
            }
            else
            {
                rb.linearVelocity = Vector2.zero; // Frena para disparar en el aire
            }
        }
        else
        {
            Patrol();
        }
    }

    private void DetectPlayer()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        chasing = distanceToPlayer <= detectionRange;
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Vector2 targetPos = patrolPoints[currentPointIndex].position;
        MoveTowards(targetPos, patrolSpeed);

        CheckFlip(targetPos.x - transform.position.x);

        if (Vector2.Distance(transform.position, targetPos) < stoppingDistance)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }
    }

    private void MoveTowards(Vector2 target, float speed)
    {
        Vector2 direction = (target - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    private void Attack()
    {
        if (Time.time >= nextFireTime && projectilePrefab != null && firePoint != null)
        {
            nextFireTime = Time.time + (1f / fireRate);

            // Crear proyectil
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);

            // Dirección hacia Elisa
            Vector2 direction = (player.position - firePoint.position).normalized;

            // Asignar velocidad
            Rigidbody2D prb = projectile.GetComponent<Rigidbody2D>();
            if (prb != null)
            {
                prb.linearVelocity = direction * projectileSpeed;
            }

            // Rotar proyectil hacia la dirección del disparo
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    // Método para recibir daño desde ElisaAttack
    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;
        Debug.Log($"Enemigo volador recibió {damage} de daño. Vida restante: {health}");

        if (health <= 0)
        {
            DieBehavior();
        }
    }

    private void DieBehavior()
    {
        isDead = true;
        Debug.Log("¡Enemigo volador derrotado!");
        Destroy(gameObject);
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
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}