using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelExit : MonoBehaviour
{
    [Header("Escena Destino")]
    [Tooltip("Nombre exacto de la escena como figura en Build Settings")]
    [SerializeField] private string sceneToLoad;

    [Header("Configuración del Spawn")]
    [Tooltip("Identificador único del punto donde reaparece Elisa en la siguiente escena")]
    [SerializeField] private string targetSpawnPointID;

    [Header("Modo de Activación")]
    [Tooltip("Si es false, pasa de nivel apenas toca el Trigger. Si es true, exige presionar la tecla de interactuar.")]
    [SerializeField] private bool requireInteraction = false;
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool isPlayerInTrigger = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = true;

            if (!requireInteraction)
            {
                ChangeScene();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInTrigger = false;
        }
    }

    private void Update()
    {
        if (isPlayerInTrigger && requireInteraction && Input.GetKeyDown(interactKey))
        {
            ChangeScene();
        }
    }

    private void ChangeScene()
    {
        // Guardamos el ID del punto de destino en el GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.nextSpawnPointID = targetSpawnPointID;
        }

        SceneManager.LoadScene(sceneToLoad);
    }
}