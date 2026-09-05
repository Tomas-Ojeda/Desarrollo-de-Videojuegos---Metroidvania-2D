using UnityEngine;

public class ProyectilAcido : MonoBehaviour
{
    public float daño = 10f;
    public float tiempoVida = 4f;

    private void Start()
    {
        // Se destruye automáticamente después de unos segundos si no choca con nada
        Destroy(gameObject, tiempoVida);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ElisaHealth vida = other.GetComponent<ElisaHealth>();
            if (vida != null)
            {
                vida.TakeDamage(daño);
                Debug.Log($"💥 El proyectil impactó a Elisa causando {daño} de daño!");
            }
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground")) // O cualquier capa de suelo que utilices
        {
            Destroy(gameObject);
        }
    }
}