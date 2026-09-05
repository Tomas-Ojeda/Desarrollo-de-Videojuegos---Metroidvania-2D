using UnityEngine;
using UnityEngine.UI;

public class ZonaFinal : MonoBehaviour
{
    [Header("UI del Mensaje Final")]
    public GameObject panelFinal; // El Canvas/Panel que contiene el texto
    
    private bool juegoTerminado = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (juegoTerminado) return;

        if (collision.CompareTag("Player"))
        {
            juegoTerminado = true;
            MostrarContinuara();
        }
    }

    private void MostrarContinuara()
    {
        // Activa el panel UI con el cartel "Continuará..."
        if (panelFinal != null)
        {
            panelFinal.SetActive(true);
        }

        // Corta el audio de pasos si el jugador venía caminando
        if (PlayerAudio.Instance != null)
        {
            PlayerAudio.Instance.StopPasos();
        }

        // Frenar el juego o el movimiento del jugador
        Time.timeScale = 0f; 
    }
}