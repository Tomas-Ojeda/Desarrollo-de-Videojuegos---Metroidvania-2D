using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Referencias a Jugador")]
    public ElisaHealth playerHealth;
    public ElisaStamina playerStamina; // Asegúrate de tener la referencia a tu script de Stamina

    [Header("Elementos de UI (Barras)")]
    public Image healthBarFill;
    public Image staminaBarFill;

    [Header("Retrato y Estados de Salud")]
    public Image portraitImage;
    public Sprite portraitFullHealth;    // Cara normal / vida alta (barra_de_vida1)
    public Sprite portraitMediumHealth;  // Cara herida ligera (barra_de_vida2)
    public Sprite portraitLowHealth;     // Cara muy herida (barra_de_vida3)

    [Header("Velocidad de Suavizado (Lerp)")]
    public float smoothSpeed = 10f;

    private void Update()
    {
        UpdateHealthBar();
        UpdateStaminaBar();
        UpdatePortrait();
    }

    private void UpdateHealthBar()
    {
        if (playerHealth == null || healthBarFill == null) return;

        float targetFill = Mathf.Clamp01(playerHealth.currentHealth / playerHealth.maxHealth);
        // Suaviza el movimiento del llenado de la barra
        healthBarFill.fillAmount = Mathf.Lerp(healthBarFill.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
    }

    private void UpdateStaminaBar()
    {
        if (playerStamina == null || staminaBarFill == null) return;

        // Ajusta las variables si en tu script de Stamina tienen otros nombres (ej. currentStamina / maxStamina)
        float targetFill = Mathf.Clamp01(playerStamina.currentStamina / playerStamina.maxStamina);
        staminaBarFill.fillAmount = Mathf.Lerp(staminaBarFill.fillAmount, targetFill, Time.deltaTime * smoothSpeed);
    }

    private void UpdatePortrait()
    {
        if (playerHealth == null || portraitImage == null) return;

        float healthPercentage = playerHealth.currentHealth / playerHealth.maxHealth;

        // Cambio de caras según porcentaje de vida restante
        if (healthPercentage > 0.65f)
        {
            if (portraitFullHealth != null) portraitImage.sprite = portraitFullHealth;
        }
        else if (healthPercentage > 0.30f)
        {
            if (portraitMediumHealth != null) portraitImage.sprite = portraitMediumHealth;
        }
        else
        {
            if (portraitLowHealth != null) portraitImage.sprite = portraitLowHealth;
        }
    }
}