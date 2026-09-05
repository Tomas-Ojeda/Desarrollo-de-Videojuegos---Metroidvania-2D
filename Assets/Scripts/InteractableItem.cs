using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    public string noteTitle = "Carta de mi Hermana";
    [TextArea(3, 5)]
    public string noteMessage = "Aquí dice: 'Elisa, si estás leyendo esto...'";
    public Sprite itemDialogueSprite;
    public KeyCode interactKey = KeyCode.E;

    [Header("Efecto de Titileo / Brillo")]
    public float pulseSpeed = 3f;
    public float minIntensity = 0.3f;
    public float maxIntensity = 1f;

    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;
    private bool isPlayerInRange = false;
    private bool isReading = false;
    private bool isCollected = false;
    private bool justOpened = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        // Animar brillo solo si no fue recogida
        if (!isCollected)
        {
            AnimateSparkle();
        }

        // Para abrir: requiere estar cerca, no haber sido recogida y presionar la tecla
        if (isPlayerInRange && !isCollected && Input.GetKeyDown(interactKey))
        {
            OpenNote();
        }
        // Para cerrar: basta con estar leyendo, que no sea el mismo frame que se abrió y presionar la tecla
        else if (isReading && !justOpened && Input.GetKeyDown(interactKey))
        {
            CloseNoteAndDestroy();
        }

        // Reseteamos el flag en el siguiente frame
        if (justOpened)
        {
            justOpened = false;
        }
    }

    private void AnimateSparkle()
    {
        if (spriteRenderer == null) return;

        float lerpFactor = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float currentBrightness = Mathf.Lerp(minIntensity, maxIntensity, lerpFactor);

        Color color = spriteRenderer.color;
        color.a = Mathf.Clamp01(currentBrightness);
        spriteRenderer.color = color;
    }

    private void OpenNote()
    {
        isReading = true;
        isCollected = true;
        justOpened = true; // Evita que se cierre en este mismo frame

        // 1. Mostrar texto en la interfaz de diálogo
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.ShowDialogue(noteMessage, itemDialogueSprite);
        }

        // 2. Registrar en el inventario
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddNote(noteTitle, noteMessage, itemDialogueSprite);
        }

        // 3. Ocultar el objeto del escenario
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (itemCollider != null) itemCollider.enabled = false;
    }

    private void CloseNoteAndDestroy()
    {
        isReading = false;

        // Ocultar caja de diálogo
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.HideDialogue();
        }

        // Destruir el GameObject de la escena
        Destroy(gameObject);
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
            if (isReading)
            {
                CloseNoteAndDestroy();
            }
        }
    }
}