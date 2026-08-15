using UnityEngine;

public class ElisaStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    public float maxStamina = 100f;
    public float currentStamina;
    public float regenRate = 15f;          // Cuánta stamina recupera por segundo
    public float regenDelay = 1f;          // Tiempo de espera para volver a regenerar tras usarla

    private float nextRegenTime;

    private void Start()
    {
        currentStamina = maxStamina;
    }

    private void Update()
    {
        // Regenerar stamina de forma constante si pasó el delay
        if (Time.time >= nextRegenTime && currentStamina < maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        }
    }

    public bool UseStamina(float amount)
    {
        if (currentStamina >= amount)
        {
            currentStamina -= amount;
            nextRegenTime = Time.time + regenDelay;
            return true; // Pudo consumir stamina
        }
        return false; // No hay suficiente stamina
    }

    public bool HasStamina(float amount)
    {
        return currentStamina >= amount;
    }
}