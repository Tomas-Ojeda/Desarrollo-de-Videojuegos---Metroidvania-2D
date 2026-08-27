using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Configuración de Escena")]
    [Tooltip("Nombre EXACTO de la escena de tu juego donde está Elisa")]
    public string gameSceneName = "ElisaMetroid";

    // Se ejecuta al tocar "Jugar / Nueva Partida"
    public void PlayGame()
    {
        // Reestablece la escala de tiempo por si acaso
        Time.timeScale = 1f;
        
        // Carga la escena del juego
        SceneManager.LoadScene(gameSceneName);
    }

    // Se ejecuta al tocar "Salir"
    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}