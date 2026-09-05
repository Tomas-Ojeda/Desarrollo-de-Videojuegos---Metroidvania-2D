using System.Collections;
using UnityEngine;

public class ElisaHealth : MonoBehaviour
{
    [Header("Salud")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Punto de Reaparición")]
    public Transform spawnPoint;
    public float respawnDelay = 1.5f;

    [Header("Invulnerabilidad (i-frames)")]
    public float invincibilityDuration = 1f;
    public float flashInterval = 0.1f;

    private bool isInvincible = false;
    private bool isDead = false;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Animator anim;
    private CharacterController2d controller;
    private ElisaMovement elisaMovement;
    private ElisaAttack attackScript;

    private void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController2d>();
        elisaMovement = GetComponent<ElisaMovement>();
        attackScript = GetComponent<ElisaAttack>();

        if (spawnPoint == null)
        {
            GameObject defaultSpawn = new GameObject("DefaultSpawnPoint");
            defaultSpawn.transform.position = transform.position;
            spawnPoint = defaultSpawn.transform;
        }
    }

    public void TakeDamage(float damage)
    {
        if (isInvincible || isDead || (controller != null && controller.isInvulnerable)) return;

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
            spriteRenderer.enabled = true;
        }

        isInvincible = false;
    }

    private IEnumerator DeathRoutine()
    {
        isDead = true;
        Debug.Log("¡Elisa ha muerto!");

        if (anim != null)
        {
            anim.SetBool("isDead", true);
        }

        // Desactivar componentes de control
        if (controller != null) controller.enabled = false;
        if (elisaMovement != null) elisaMovement.enabled = false;
        if (attackScript != null) attackScript.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
        }

        yield return new WaitForSeconds(respawnDelay);

        transform.position = spawnPoint.position;
        currentHealth = maxHealth;

        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = true;
        }

        // Reactivar controles
        if (controller != null) controller.enabled = true;
        if (elisaMovement != null) elisaMovement.enabled = true;
        if (attackScript != null) attackScript.enabled = true;

        if (anim != null)
        {
            anim.SetBool("isDead", false);
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