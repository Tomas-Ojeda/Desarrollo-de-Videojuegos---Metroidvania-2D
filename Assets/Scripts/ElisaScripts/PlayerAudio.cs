using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio Instance;

    [Header("Audio Source principal (Ataques / Disparos)")]
    public AudioSource audioSource;

    [Header("Audio Source para Pasos (Continuo)")]
    public AudioSource movementAudioSource;

    [Header("Clips de Sonido")]
    public AudioClip sfxMelee;
    public AudioClip sfxDisparo;
    public AudioClip sfxCaminar;
    public AudioClip sfxCorrer;

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

        // Si no creaste un AudioSource secundario en el Inspector, creamos uno dinámicamente para los pasos
        if (movementAudioSource == null)
        {
            movementAudioSource = gameObject.AddComponent<AudioSource>();
            movementAudioSource.playOnAwake = false;
            movementAudioSource.loop = true; // Loop continuo mientras camine o corra
        }
    }

    public void PlayMelee()
    {
        if (audioSource != null && sfxMelee != null)
        {
            audioSource.PlayOneShot(sfxMelee);
        }
    }

    public void PlayDisparo()
    {
        if (audioSource != null && sfxDisparo != null)
        {
            audioSource.PlayOneShot(sfxDisparo);
        }
    }

    // Gestiona el audio de caminata y carrera
    public void UpdatePasos(bool isMoving, bool isRunning)
    {
        if (movementAudioSource == null) return;

        if (!isMoving)
        {
            // Si el personaje se detuvo, paramos el sonido de inmediato
            if (movementAudioSource.isPlaying)
            {
                movementAudioSource.Stop();
            }
            return;
        }

        AudioClip clipAUsar = isRunning ? sfxCorrer : sfxCaminar;

        // Si cambió entre caminar/correr o no estaba sonando nada, cambiamos el clip y reproducimos
        if (movementAudioSource.clip != clipAUsar || !movementAudioSource.isPlaying)
        {
            movementAudioSource.clip = clipAUsar;
            movementAudioSource.loop = true;
            movementAudioSource.Play();
        }
    }

    public void StopPasos()
    {
        if (movementAudioSource != null && movementAudioSource.isPlaying)
        {
            movementAudioSource.Stop();
        }
    }
}