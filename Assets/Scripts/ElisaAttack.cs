using System.Collections;
using UnityEngine;

public class ElisaAttack : MonoBehaviour
{
    [Header("Referencias")]
    public Transform attackPoint;
    public LayerMask enemyLayers;

    [Header("Ataque Normal Melee")]
    public float attackRange = 0.8f;
    public float attackDamage = 25f;
    public float attackCooldown = 0.3f;
    public float comboResetTime = 1.0f; // Tiempo para reiniciar el combo al golpe 1 si no atacas

    [Header("Power Shot / Estocada Cargada")]
    public float powerShotDamage = 75f;
    public float powerShotRange = 1.3f;
    public float powerShotStaminaCost = 35f;
    public float maxChargeTime = 1.2f;

    [Header("Ataque a Distancia (Shoot)")]
    public KeyCode shootKey = KeyCode.L; // Tecla para disparar
    public float shootCooldown = 0.25f;  // Cadencia de disparo
    private float lastShootTime;

    [Header("Ralentización de Carga (Slow Motion)")]
    [Range(0.1f, 1f)] public float chargeTimeScale = 0.4f;

    [Header("Efecto Embestida (Lunge)")]
    public float lungeForce = 18f; // Fuerza del impulso tipo disparo

    private float lastAttackTime;
    private float chargeTimer;
    private bool isCharging;
    private int comboStep = 0; // 0 = Golpe 1, 1 = Golpe 2

    private ElisaStamina staminaSystem;
    private Rigidbody2D rb;
    private Animator anim;

    private void Start()
    {
        staminaSystem = GetComponent<ElisaStamina>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // Reiniciar el combo al Golpe 1 si pasa mucho tiempo sin atacar
        if (Time.time - lastAttackTime > comboResetTime && comboStep != 0)
        {
            ResetCombo();
        }

        // 1. ATAQUE NORMAL MELEE (J o Clic Izquierdo)
        if (Input.GetKeyDown(KeyCode.J) || Input.GetMouseButtonDown(0))
        {
            if (Time.time - lastAttackTime > attackCooldown)
            {
                NormalAttack();
            }
        }

        // 2. DISPARO A DISTANCIA (Únicamente la tecla asignada en shootKey)
        if (Input.GetKeyDown(shootKey))
        {
            if (Time.time - lastShootTime > shootCooldown)
            {
                RangedAttack();
            }
        }

        // 3. INICIAR CARGA DE POWER SHOT (K o Clic Derecho)
        if (Input.GetKeyDown(KeyCode.K) || Input.GetMouseButtonDown(1))
        {
            if (staminaSystem != null && staminaSystem.HasStamina(powerShotStaminaCost))
            {
                isCharging = true;
                chargeTimer = 0f;

                // Ralentizar el juego
                Time.timeScale = chargeTimeScale;
                Time.fixedDeltaTime = 0.02f * Time.timeScale;

                Debug.Log("Cargando Power Shot...");
            }
        }

        // 4. PROCESAR CARGA Y SOLTAR ATAQUE
        if (isCharging)
        {
            if (Input.GetKey(KeyCode.K) || Input.GetMouseButton(1))
            {
                chargeTimer += Time.unscaledDeltaTime;
            }

            if (Input.GetKeyUp(KeyCode.K) || Input.GetMouseButtonUp(1))
            {
                // Restaurar velocidad normal ANTES de aplicar la embestida
                ResetTimeScale();

                if (chargeTimer >= maxChargeTime)
                {
                    ExecutePowerShot();
                }
                else
                {
                    Debug.Log("Ataque cargado cancelado (no cargó suficiente tiempo).");
                }

                isCharging = false;
                chargeTimer = 0f;
            }
        }
    }

    private void NormalAttack()
    {
        lastAttackTime = Time.time;

        if (anim != null)
        {
            // Asignar el índice del combo actual ANTES de activar el Trigger
            anim.SetInteger("ComboIndex", comboStep);
            anim.SetTrigger("AttackMele");
        }

        Debug.Log($"¡Ataque Normal Golpe {comboStep + 1}!");

        PerformAttack(attackDamage, attackRange);

        // Avanzar el paso del combo
        comboStep = (comboStep == 0) ? 1 : 0;
    }

    private void RangedAttack()
    {
        lastShootTime = Time.time;

        if (anim != null)
        {
            // Activa el Trigger exacto del Animator (rangedAttack)
            anim.SetTrigger("rangedAttack");
        }

        Debug.Log("¡Disparo Realizado!");
    }

    private void ResetCombo()
    {
        comboStep = 0;
        if (anim != null)
        {
            anim.SetInteger("ComboIndex", 0);
            anim.ResetTrigger("AttackMele");
        }
    }

    private void ExecutePowerShot()
    {
        if (staminaSystem != null && staminaSystem.UseStamina(powerShotStaminaCost))
        {
            lastAttackTime = Time.time;
            Debug.Log("¡POWER SHOT / ESTOCADA EJECUTADA!");

            if (anim != null)
            {
                anim.SetInteger("ComboIndex", 1);
                anim.SetTrigger("AttackMele");
            }

            // Aplicar el impulso de embestida directo
            ApplyLungeImpulse();

            // Realizar el ataque y daño
            PerformAttack(powerShotDamage, powerShotRange);

            // Reiniciar combo después del Power Shot
            comboStep = 0;
        }
    }

    private void ApplyLungeImpulse()
    {
        if (rb != null)
        {
            // Determinar dirección basada en la escala X de Elisa
            float facingDirection = Mathf.Sign(transform.localScale.x);

            // Reseteamos la velocidad X previa para que la embestida sea uniforme
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);

            // Aplicamos un impulso instantáneo hacia adelante
            rb.AddForce(new Vector2(facingDirection * lungeForce, 0f), ForceMode2D.Impulse);
        }
    }

    private void PerformAttack(float damage, float range)
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemyLife groundEnemy = enemy.GetComponent<EnemyLife>();
            if (groundEnemy != null)
            {
                groundEnemy.TakeDamage(damage);
                continue;
            }

            FlyingEnemyShooter flyingEnemy = enemy.GetComponent<FlyingEnemyShooter>();
            if (flyingEnemy != null)
            {
                flyingEnemy.TakeDamage(damage);
            }
        }
    }

    private void ResetTimeScale()
    {
        Time.timeScale = 1.0f;
        Time.fixedDeltaTime = 0.02f;
    }

    private void OnDisable()
    {
        ResetTimeScale();
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(attackPoint.position, powerShotRange);
    }
}