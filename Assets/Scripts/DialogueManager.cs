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
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void ShowDialogue(string message, Sprite dialogueSprite)
    {
        if (dialoguePanel == null) return;

        // Cambiar la imagen del marco si se proporciona un sprite
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

        if (dialogueText != null)
        {
            typingCoroutine = StartCoroutine(TypeText(message));
        }
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
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (dialogueText != null)
        {
            dialogueText.text = "";
        }

        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        isDialogueActive = false;
    }

    public bool IsActive()
    {
        return isDialogueActive;
    }
}