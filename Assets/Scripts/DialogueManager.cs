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

    // --- NUEVO PARA SISTEMA DE PÁGINAS ---
    private string[] currentPages;
    private int currentPageIndex = 0;
    private bool isTyping = false;

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

    private void Update()
    {
        // Si el diálogo está activo y el jugador presiona interacción (E, Espacio o Click)
        if (isDialogueActive && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)))
        {
            // Si todavía se está escribiendo la página actual, la muestra de golpe
            if (isTyping)
            {
                StopCoroutine(typingCoroutine);
                dialogueText.text = currentPages[currentPageIndex];
                isTyping = false;
            }
            // Si ya terminó de escribirse, pasa a la siguiente página
            else
            {
                NextPage();
            }
        }
    }

    // Método para mostrar múltiples páginas de texto
    public void ShowDialoguePages(string[] pages, Sprite dialogueSprite)
    {
        if (dialoguePanel == null || pages == null || pages.Length == 0) return;

        if (dialogueSprite != null && dialogueBoxImage != null)
        {
            dialogueBoxImage.sprite = dialogueSprite;
        }

        currentPages = pages;
        currentPageIndex = 0;
        dialoguePanel.SetActive(true);
        isDialogueActive = true;

        DisplayCurrentPage();
    }

    // Sobrecarga por si quieres mandar un solo texto directo como antes
    public void ShowDialogue(string message, Sprite dialogueSprite)
    {
        ShowDialoguePages(new string[] { message }, dialogueSprite);
    }

    private void DisplayCurrentPage()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine = StartCoroutine(TypeText(currentPages[currentPageIndex]));
    }

    private void NextPage()
    {
        currentPageIndex++;

        if (currentPageIndex < currentPages.Length)
        {
            DisplayCurrentPage();
        }
        else
        {
            HideDialogue();
        }
    }

    private IEnumerator TypeText(string message)
    {
        isTyping = true;
        dialogueText.text = "";

        foreach (char letter in message.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
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
        isTyping = false;
    }

    public bool IsActive()
    {
        return isDialogueActive;
    }
}