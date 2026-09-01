using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Referencias UI")]
    public GameObject dialoguePanel;
    public Image dialogueBoxImage;
    public TextMeshProUGUI dialogueText;

    [Header("Configuración")]
    public float textSpeed = 0.03f;

    private Coroutine typingCoroutine;
    private bool isDialogueActive = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowDialogue(string message, Sprite dialogueSprite)
    {
        // Cambiar la imagen del marco según la emoción del diálogo
        if (dialogueSprite != null && dialogueBoxImage != null)
        {
            dialogueBoxImage.sprite = dialogueSprite;
        }

        dialoguePanel.SetActive(true);
        isDialogueActive = true;

        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(message));
    }

    private IEnumerator TypeText(string message)
    {
        dialogueText.text = "";
        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);
        dialogueText.text = "";
        isDialogueActive = false;
    }

    public bool IsActive()
    {
        return isDialogueActive;
    }
}