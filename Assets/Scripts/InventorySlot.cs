using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [Header("Referencias Internas")]
    public Image iconImage;
    public Button slotButton;

    private InventoryManager.NoteData currentNote;
    private bool isNote = false;

    private void Awake()
    {
        if (slotButton == null)
            slotButton = GetComponent<Button>();

        if (slotButton != null)
        {
            // Limpiamos escuchas previas y agregamos la función
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(OnSlotClicked);
        }
    }

    // Configurar si el item es una llave
    public void SetupSlotAsKey(Sprite keySprite)
    {
        isNote = false;
        if (iconImage != null && keySprite != null)
        {
            iconImage.sprite = keySprite;
            iconImage.enabled = true;
        }
    }

    // Configurar si el item es una nota
    public void SetupSlotAsNote(InventoryManager.NoteData noteData)
    {
        isNote = true;
        currentNote = noteData;

        if (iconImage != null && noteData.portrait != null)
        {
            iconImage.sprite = noteData.portrait;
            iconImage.enabled = true;
        }
    }

    // Acción al hacer clic en la casilla dentro del inventario (PUBLIC para Unity)
    public void OnSlotClicked()
    {
        Debug.Log("¡Se hizo clic en el slot del inventario!");

        if (isNote)
        {
            Debug.Log($"Intentando leer la nota: {currentNote.message}");

            if (DialogueManager.Instance != null)
            {
                // 1. Cierra el panel de inventario si está abierto
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.ToggleInventory();
                }

                // 2. Muestra el diálogo con el texto de la nota
                DialogueManager.Instance.ShowDialogue(currentNote.message, currentNote.portrait);
            }
            else
            {
                Debug.LogWarning("No se encontró DialogueManager.Instance en la escena.");
            }
        }
        else
        {
            Debug.Log("Este ítem es una llave, no tiene texto de nota.");
        }
    }
}