using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Header("Configuración de Cámara")]
    [Tooltip("Arrastrá la Main Camera o la cámara que sigue a Elisa.")]
    public Transform cameraTransform;

    [Header("Intensidad del Parallax")]
    [Tooltip("0 = Se mueve pegado a la cámara. 1 = Se queda completamente estático en el fondo. Valores sugeridos para fondo lejano: 0.6 a 0.8")]
    [Range(0f, 1f)]
    public float parallaxFactor = 0.7f;

    [Header("Ejes de Movimiento")]
    public bool moverEnX = true;
    public bool moverEnY = true;

    private Vector3 lastCameraPosition;
    private bool initialized = false;

    void Start()
    {
        // Si no se asignó manualmente la cámara, busca la Main Camera del juego
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
    }

    void LateUpdate()
    {
        if (cameraTransform == null) return;

        // En el primer cuadro sólo capturamos la posición inicial de la cámara 
        // para evitar el salto/teletransporte de Cinemachine al dar Play.
        if (!initialized)
        {
            lastCameraPosition = cameraTransform.position;
            initialized = true;
            return;
        }

        // Calcula cuánto se movió la cámara desde el último cuadro
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;

        // Calculamos el desplazamiento del fondo
        float moveX = moverEnX ? deltaMovement.x * (1f - parallaxFactor) : 0f;
        float moveY = moverEnY ? deltaMovement.y * (1f - parallaxFactor) : 0f;

        // Aplicamos el movimiento al objeto padre
        transform.position += new Vector3(moveX, moveY, 0f);

        // Guardamos la posición actual de la cámara para el siguiente cuadro
        lastCameraPosition = cameraTransform.position;
    }
}