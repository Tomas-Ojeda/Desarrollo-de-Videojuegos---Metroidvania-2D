using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Referencias UI")]
    public GameObject inventoryPanel; // Panel del Inventario
    public Transform itemsContainer;   // Contenedor UI (Grid Layout Group)
    public GameObject slotPrefab;      // Prefab del slot guardado en Assets

    // Estructura para guardar llaves con su sprite correspondiente
    [System.Serializable]
    public struct KeyData
    {
        public string keyID;
        public Sprite icon;
    }

    private Dictionary<string, KeyData> collectedKeys = new Dictionary<string, KeyData>();
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

            Time.timeScale = activeState ? 0f : 1f;

            if (activeState)
            {
                RefreshInventoryUI();
            }
        }
    }

    private void RefreshInventoryUI()
    {
        if (itemsContainer == null || slotPrefab == null) return;

        // Limpiar la interfaz anterior
        foreach (Transform child in itemsContainer)
        {
            Destroy(child.gameObject);
        }

        // 1. Mostrar Llaves guardadas
        foreach (var keyPair in collectedKeys)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, itemsContainer);
            InventorySlot slot = newSlotObj.GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.SetupSlotAsKey(keyPair.Value.icon);
            }
        }

        // 2. Mostrar Notas guardadas
        foreach (NoteData note in collectedNotes)
        {
            GameObject newSlotObj = Instantiate(slotPrefab, itemsContainer);
            InventorySlot slot = newSlotObj.GetComponent<InventorySlot>();
            if (slot != null)
            {
                slot.SetupSlotAsNote(note);
            }
        }
    }

    // --- MÉTODOS PARA LLAVES ---
    public void AddKey(string keyID, Sprite keySprite)
    {
        if (!collectedKeys.ContainsKey(keyID))
        {
            KeyData newKey = new KeyData { keyID = keyID, icon = keySprite };
            collectedKeys.Add(keyID, newKey);
            Debug.Log($"Llave '{keyID}' agregada al inventario.");
        }
    }

    public bool HasKey(string keyID)
    {
        return collectedKeys.ContainsKey(keyID);
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