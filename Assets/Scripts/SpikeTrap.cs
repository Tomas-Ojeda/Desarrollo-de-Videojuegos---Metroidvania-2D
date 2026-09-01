using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    [Header("Configuración del Daño")]
    public float damageAmount = 25f; // Cantidad de vida que saca

    [Header("Empuje / Knockback (Opcional)")]
    public bool applyKnockback = true;
    public float knockbackForce = 6f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Por si el jugador se queda parado encima de los pinchos después de los i-frames
        TryDamagePlayer(collision.gameObject);
    }

    private void TryDamagePlayer(GameObject playerObject)
    {
        if (playerObject.CompareTag("Player"))
        {
            ElisaHealth health = playerObject.GetComponent<ElisaHealth>();
            if (health != null)
            {
                health.TakeDamage(damageAmount);

                // Empujoncito opcional hacia arriba/atrás para evitar que se quede pegado
                if (applyKnockback)
                {
                    Rigidbody2D rb = playerObject.GetComponent<Rigidbody2D>();
                    if (rb != null)
                    {
                        rb.linearVelocity = new Vector2(rb.linearVelocity.x, knockbackForce);
                    }
                }
            }
        }
    }
}