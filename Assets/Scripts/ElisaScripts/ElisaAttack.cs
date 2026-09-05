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
    public float attackCooldown = 0.45f; // Tiempo mínimo entre ataques para evitar spameo
    public float comboResetTime = 1.0f;   // Tiempo para reiniciar el combo si no atacas

    [Header("Ataque a Distancia (Shoot)")]
    public KeyCode shootKey = KeyCode.L;
    public float shootCooldown = 0.3f;
    private float lastShootTime;

    private float lastAttackTime;
    private int comboStep = 0; // 0 = Golpe 1, 1 = Golpe 2
    private bool isAttacking;

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

        // Liberar el estado de ataque si ya pasó el cooldown
        if (isAttacking && Time.time - lastAttackTime >= attackCooldown)
        {
            isAttacking = false;
        }

        // 1. ATAQUE NORMAL MELEE (J, K o Clic Izquierdo/Derecho)
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K) || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            // Solo se puede atacar si NO está atacando actualmente y se cumplió el cooldown
            if (!isAttacking && Time.time - lastAttackTime >= attackCooldown)
            {
                NormalAttack();
            }
        }

        // 2. DISPARO A DISTANCIA
        if (Input.GetKeyDown(shootKey))
        {
            if (Time.time - lastShootTime >= shootCooldown)
            {
                RangedAttack();
            }
        }
    }

    private void NormalAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        if (anim != null)
        {
            // Asignar el índice del combo actual ANTES de activar el Trigger
            anim.SetInteger("ComboIndex", comboStep);
            anim.SetTrigger("AttackMele");
        }

        // Reproducir sonido de ataque melee
        if (PlayerAudio.Instance != null)
        {
            PlayerAudio.Instance.PlayMelee();
        }

        PerformAttack(attackDamage, attackRange);

        // Alternar el paso del combo entre Golpe 1 (0) y Golpe 2 (1)
        comboStep = (comboStep == 0) ? 1 : 0;
    }

    private void RangedAttack()
    {
        lastShootTime = Time.time;

        if (anim != null)
        {
            anim.SetTrigger("rangedAttack");
        }

        if (PlayerAudio.Instance != null)
        {
            PlayerAudio.Instance.PlayDisparo();
        }
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

    private void PerformAttack(float damage, float range)
    {
        if (attackPoint == null) return;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, range, enemyLayers);

        foreach (Collider2D enemy in hitEnemies)
        {
            EnemigoBase enemyBase = enemy.GetComponentInParent<EnemigoBase>();
            if (enemyBase == null) enemyBase = enemy.GetComponent<EnemigoBase>();

            if (enemyBase != null)
            {
                enemyBase.RecibirDaño(damage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}