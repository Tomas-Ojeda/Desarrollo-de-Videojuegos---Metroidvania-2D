using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 25f;
    public float damage = 15f;
    public float lifeTime = 3f;

    [Header("Empuje al Enemigo")]
    public float knockbackForce = 1.5f; // Fuerza sutil para moverlo apenas un poco hacia atrás

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.Translate(Vector3.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Ignorar colisión con Elisa
        if (collision.CompareTag("Player") || collision.gameObject.name == "Elisa") return;

        // 1. Buscar la interfaz IDamageable en el objeto o sus padres
        IDamageable damageable = collision.GetComponent<IDamageable>();
        if (damageable == null) damageable = collision.GetComponentInParent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damage);
            ApplySlightKnockback(collision);
            Destroy(gameObject);
            return;
        }

        // 2. Destruir si toca el suelo o paredes
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }

    private void ApplySlightKnockback(Collider2D enemyCollider)
    {
        Rigidbody2D enemyRb = enemyCollider.GetComponent<Rigidbody2D>();
        if (enemyRb == null) enemyRb = enemyCollider.GetComponentInParent<Rigidbody2D>();

        if (enemyRb != null)
        {
            // Determinar la dirección de la bala
            Vector2 knockbackDir = transform.right.normalized;

            // Frenar la inercia previa para que el empuje sea siempre pequeño y controlado
            enemyRb.linearVelocity = new Vector2(0f, enemyRb.linearVelocity.y);

            // Aplicar una fuerza muy leve
            enemyRb.AddForce(knockbackDir * knockbackForce, ForceMode2D.Impulse);
        }
    }
}