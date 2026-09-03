using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [Tooltip("ID único para este punto de spawn (ej: 'StartFromLevel1', 'EntranceFromCave')")]
    public string spawnPointID;

    private void OnDrawGizmos()
    {
        // Dibujar un indicador visual en el editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}