using UnityEngine;

public class Door : MonoBehaviour
{
    [Header("Configuración de la Puerta")]
    public string requiredKeyID = "LlaveNivel1";
    public KeyCode interactKey = KeyCode.E;

    [Header("Mensajes de Interacción")]
    [TextArea(2, 4)]
    public string lockedMessage = "La puerta está trancada. Necesitás la llave correcta.";
    [TextArea(2, 4)]
    public string unlockedMessage = "Usaste la llave. La puerta se ha abierto.";

    private bool isPlayerInRange = false;
    private bool isUnlocked = false;
    private bool isDialogueActive = false;

    private SpriteRenderer spriteRenderer;
    private Collider2D[] doorColliders;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        doorColliders = GetComponents<Collider2D>();
    }

    private void Update()
    {
        if (!isPlayerInRange) return;

        if (Input.GetKeyDown(interactKey))
        {
            if (!isDialogueActive)
            {
                // Si el diálogo no está abierto, intentamos interactuar / abrir
                InteractWithDoor();
            }
            else
            {
                // Si el diálogo ya está abierto, lo cerramos
                CloseDialogue();
            }
        }
    }

    private void InteractWithDoor()
    {
        if (isUnlocked) return;

        // Verificar si el jugador tiene la llave requerida
        if (InventoryManager.Instance != null && InventoryManager.Instance.HasKey(requiredKeyID))
        {
            isUnlocked = true;
            isDialogueActive = true;

            // Mostrar mensaje de éxito
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowDialogue(unlockedMessage, null);
            }

            // Ocultar la imagen de la puerta
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = false;
            }

            // Desactivar las paredes/colisiones para permitir el paso
            if (doorColliders != null)
            {
                foreach (Collider2D col in doorColliders)
                {
                    col.enabled = false;
                }
            }
        }
        else
        {
            // Si no tiene la llave, mostrar mensaje de trancado
            isDialogueActive = true;
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.ShowDialogue(lockedMessage, null);
            }
        }
    }

    private void CloseDialogue()
    {
        isDialogueActive = false;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.HideDialogue();
        }

        // Si la puerta ya fue abierta con éxito, destruimos el objeto una vez cerrado el diálogo
        if (isUnlocked)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Si el jugador se aleja mientras el cartel está visible, lo cerramos
            if (isDialogueActive)
            {
                CloseDialogue();
            }
        }
    }
}