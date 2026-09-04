using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [Header("Configuración de la Llave")]
    public string keyID = "LlaveNivel1";
    public KeyCode interactKey = KeyCode.E;

    [Header("Mensaje al Recoger")]
    [TextArea(2, 4)]
    public string pickupMessage = "¡Encontraste la llave antigua de la caverna!";
    public Sprite keyDialogueSprite;

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
        // Solo brilla si todavía no fue recogida
        if (!isCollected)
        {
            AnimateSparkle();
        }

        // Si está en rango y no se recogió -> Recoger al presionar E
        if (isPlayerInRange && !isCollected && Input.GetKeyDown(interactKey))
        {
            CollectKey();
            return;
        }

        // Si ya se recogió y el jugador presiona E -> Cerrar el mensaje
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

    private void CollectKey()
    {
        isCollected = true;

        // 1. Guardar la llave en el InventoryManager
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddKey(keyID);
        }

        // 2. Mostrar el cartel de diálogo
        if (DialogueManager.Instance != null && !string.IsNullOrEmpty(pickupMessage))
        {
            DialogueManager.Instance.ShowDialogue(pickupMessage, keyDialogueSprite);
        }

        // 3. Ocultar el sprite y desactivar el collider (la llave desaparece del mapa visualmente)
        if (spriteRenderer != null) spriteRenderer.enabled = false;
        if (itemCollider != null) itemCollider.enabled = false;
    }

    private void CloseDialogueAndDestroy()
    {
        // Ocultar el cartel
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.HideDialogue();
        }

        // Destruir el objeto definitivamente
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