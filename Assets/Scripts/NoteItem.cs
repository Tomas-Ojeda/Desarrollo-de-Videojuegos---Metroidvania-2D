using UnityEngine;

public class NoteItem : MonoBehaviour
{
    [Header("Configuración de la Nota")]
    public string noteTitle = "Nota de la Hermana";
    [TextArea(3, 6)]
    public string noteMessage = "Elisa, si estás leyendo esto, tuve que adentrarme en las ruinas. Ten cuidado.";
    public KeyCode interactKey = KeyCode.E;

    [Header("Ícono para el Inventario")]
    public Sprite inventoryIcon; // Ícono pequeño para el slot del inventario

    [Header("Retrato para Diálogo")]
    public Sprite notePortrait; // Caja/Retrato para el diálogo

    [Header("Efecto de Titileo / Brillo")]
    public float pulseSpeed = 3f;
    public float minIntensity = 0.3f;
    public float maxIntensity = 1f;

    private SpriteRenderer spriteRenderer;
    private Collider2D itemCollider;
    private bool isPlayerInRange = false;
    private bool isCollected = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        itemCollider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (!isCollected)
        {
            AnimateSparkle();
        }

        if (isPlayerInRange && !isCollected && Input.GetKeyDown(interactKey))
        {
            CollectNote();
            return;
        }

        if (isCollected && Input.GetKeyDown(interactKey))
        {
            CloseDialogueAndDestroy();
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

    private void CollectNote()
    {
        isCollected = true;

        // Determinamos el ícono del inventario: si no asignaste uno, intenta usar el SpriteRenderer del objeto
        Sprite iconForInventory = inventoryIcon != null ? inventoryIcon : (spriteRenderer != null ? spriteRenderer.sprite : null);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddNote(noteTitle, noteMessage, iconForInventory);
        }

        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(noteMessage))
        {
            DialogueManager.Instance.ShowDialogue(noteMessage, notePortrait);
        }

        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (itemCollider != null) itemCollider.enabled = false;
    }

    private void CloseDialogueAndDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.HideDialogue();
        }

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
        }
    }
}