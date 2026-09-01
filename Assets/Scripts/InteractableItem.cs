using UnityEngine;

public class InteractableItem : MonoBehaviour
{
    [Header("Configuración del Diálogo")]
    [TextArea(3, 5)]
    public string noteMessage = "Aquí dice: 'Elisa, si estás leyendo esto...'";
    public Sprite itemDialogueSprite; // Sprite con la expresión (alegre, triste, etc.)
    public KeyCode interactKey = KeyCode.E;

    [Header("Efecto de Titileo / Brillo")]
    public float pulseSpeed = 3f;
    public float minIntensity = 0.3f;
    public float maxIntensity = 1f;

    private SpriteRenderer spriteRenderer;
    private bool isPlayerInRange = false;
    private bool isReading = false;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        AnimateSparkle();

        if (isPlayerInRange && Input.GetKeyDown(interactKey))
        {
            if (!isReading)
            {
                OpenNote();
            }
            else
            {
                CloseNote();
            }
        }
    }

    private void AnimateSparkle()
    {
        if (spriteRenderer == null) return;

        // Oscila suavemente el canal Alpha para simular el destello
        float lerpFactor = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
        float currentBrightness = Mathf.Lerp(minIntensity, maxIntensity, lerpFactor);

        Color color = spriteRenderer.color;
        color.a = Mathf.Clamp01(currentBrightness);
        spriteRenderer.color = color;
    }

    private void OpenNote()
    {
        isReading = true;
        DialogueManager.Instance.ShowDialogue(noteMessage, itemDialogueSprite);
    }

    private void CloseNote()
    {
        isReading = false;
        DialogueManager.Instance.HideDialogue();
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
                CloseNote();
            }
        }
    }
}