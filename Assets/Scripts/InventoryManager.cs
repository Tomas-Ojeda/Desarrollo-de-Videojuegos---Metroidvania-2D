using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject inventoryPanel; // Panel en el Canvas del Inventario

    // Guardado de llaves y notas recabadas
    private HashSet<string> collectedKeys = new HashSet<string>();
    private List<NoteData> collectedNotes = new List<NoteData>();

    [System.Serializable]
    public struct NoteData
    {
        public string title;
        public string message;
        public Sprite portrait;
    }

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

    private void Update()
    {
        // Abrir/Cerrar inventario con la tecla G
        if (Input.GetKeyDown(KeyCode.G))
        {
            ToggleInventory();
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel != null)
        {
            bool activeState = !inventoryPanel.activeSelf;
            inventoryPanel.SetActive(activeState);

            // Opcional: Pausar el juego o congelar al jugador si el inventario está abierto
            Time.timeScale = activeState ? 0f : 1f;
        }
    }

    // --- MÉTODOS PARA LLAVES ---
    public void AddKey(string keyID)
    {
        if (!collectedKeys.Contains(keyID))
        {
            collectedKeys.Add(keyID);
            Debug.Log($"Llave '{keyID}' agregada al inventario.");
        }
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.Contains(keyID);
    }

    // --- MÉTODOS PARA NOTAS ---
    public void AddNote(string title, string message, Sprite portrait)
    {
        NoteData newNote = new NoteData { title = title, message = message, portrait = portrait };
        collectedNotes.Add(newNote);
        Debug.Log($"Nota '{title}' guardada en el inventario.");
    }

    public List<NoteData> GetCollectedNotes()
    {
        return collectedNotes;
    }
}