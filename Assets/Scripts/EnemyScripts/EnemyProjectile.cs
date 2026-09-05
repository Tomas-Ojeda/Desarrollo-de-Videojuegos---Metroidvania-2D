using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float lifeTime = 3f;
    public float damage = 10f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si choca contra Elisa
        if (collision.gameObject.name == "Elisa" || collision.CompareTag("Player"))
        {
            ElisaHealth elisaHealth = collision.GetComponent<ElisaHealth>();
            if (elisaHealth != null)
            {
                elisaHealth.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
        // Si choca contra el suelo o plataformas
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            Destroy(gameObject);
        }
    }
}