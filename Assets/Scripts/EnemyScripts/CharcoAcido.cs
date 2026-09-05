using UnityEngine;

public class CharcoAcido : MonoBehaviour
{
    [Header("⚙️ Configuración del Charco")]
    public float dañoPorSegundo = 5f;
    public float intervaloDaño = 0.5f;
    public float tiempoVida = 5f;

    private float contadorTiempo;

    private void Start()
    {
        Destroy(gameObject, tiempoVida);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // Verifica que el objeto que entra tenga el Tag "Player"
        if (collision.CompareTag("Player"))
        {
            contadorTiempo += Time.deltaTime;

            if (contadorTiempo >= intervaloDaño)
            {
                ElisaHealth vidaElisa = collision.GetComponent<ElisaHealth>();
                if (vidaElisa != null)
                {
                    vidaElisa.TakeDamage(dañoPorSegundo * intervaloDaño);
                    Debug.Log("💥 Elisa está sufriendo daño por ácido!");
                }
                else
                {
                    Debug.LogWarning("⚠️ Se detectó a Player pero no tiene el componente ElisaHealth.");
                }

                contadorTiempo = 0f;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            contadorTiempo = 0f;
        }
    }
}