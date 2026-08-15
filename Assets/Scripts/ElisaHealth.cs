using System.Collections;
using UnityEngine;

public class ElisaHealth : MonoBehaviour
{
    [Header("Salud")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Punto de Reaparición")]
    public Transform spawnPoint;

    [Header("Invulnerabilidad (i-frames)")]
    public float invincibilityDuration = 1f;
    public float flashInterval = 0.1f;

    private bool isInvincible = false;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        // Si no asignaste un spawnPoint manualmente, usa la posición inicial de Elisa
        if (spawnPoint == null)
        {
            GameObject defaultSpawn = new GameObject("DefaultSpawnPoint");
            defaultSpawn.transform.position = transform.position;
            spawnPoint = defaultSpawn.transform;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead) return;

        currentHealth -= damage;
        Debug.Log($"¡Elisa recibió {damage} de daño! Vida restante: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    private IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;

        // Efecto visual de parpadeo
        float elapsedTime = 0f;
        while (elapsedTime < invincibilityDuration)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(flashInterval);
            elapsedTime += flashInterval;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true; // Asegurar que quede visible
        }

        isInvincible = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("¡Elisa ha muerto! Respawn en el punto de inicio...");

        // Frenar la velocidad actual
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // Reposicionar en el spawn point
        transform.position = spawnPoint.position;

        // Restaurar salud
        currentHealth = maxHealth;
        isDead = false;

        // Si el personaje quedó oculto durante el parpadeo, restaurarlo
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Elisa recuperó salud. Vida actual: {currentHealth}/{maxHealth}");
    }
}