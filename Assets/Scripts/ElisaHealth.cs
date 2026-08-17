using System.Collections;
using UnityEngine;

public class ElisaHealth : MonoBehaviour
{
    [Header("Salud")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Punto de Reaparición")]
    public Transform spawnPoint;
    public float respawnDelay = 1.5f; // Tiempo que dura la animación de muerte antes de reaparecer

    [Header("Invulnerabilidad (i-frames)")]
    public float invincibilityDuration = 1f;
    public float flashInterval = 0.1f;

    private bool isInvincible = false;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator anim;
    private CharacterController2d controller;
    private ElisaAttack attackScript;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController2d>();
        attackScript = GetComponent<ElisaAttack>();

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
            StartCoroutine(DeathRoutine());
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

    private IEnumerator DeathRoutine()
    {
        isDead = true;
        Debug.Log("¡Elisa ha muerto!");

        // 1. Disparar el Trigger de Muerte en el Animator
        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // 2. Desactivar controles de movimiento y ataque
        if (controller != null) controller.enabled = false;
        if (attackScript != null) attackScript.enabled = false;

        // 3. Frenar la física para que no siga deslizándose
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        // 4. Esperar el tiempo de reproducción de la animación de muerte
        yield return new WaitForSeconds(respawnDelay);

        // 5. Reposicionar en el spawn point y restaurar salud
        transform.position = spawnPoint.position;
        currentHealth = maxHealth;

        // 6. Volver a activar controles y visuales
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        if (controller != null) controller.enabled = true;
        if (attackScript != null) attackScript.enabled = true;

        // Resetear Animator a estado normal (Idle)
        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
        }

        isDead = false;
    }

    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"Elisa recuperó salud. Vida actual: {currentHealth}/{maxHealth}");
    }
}