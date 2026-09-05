using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [HideInInspector]
    public string nextSpawnPointID = "";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Buscar el objeto Player en la escena recién cargada
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player != null && !string.IsNullOrEmpty(nextSpawnPointID))
        {
            // Método moderno para buscar los SpawnPoints sin advertencias de deprecación
            SpawnPoint[] spawnPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            foreach (SpawnPoint sp in spawnPoints)
            {
                if (sp.spawnPointID == nextSpawnPointID)
                {
                    player.transform.position = sp.transform.position;
                    break;
                }
            }
        }
    }
}