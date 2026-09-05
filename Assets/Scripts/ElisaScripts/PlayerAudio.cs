using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    public static PlayerAudio Instance;

    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Clips de Sonido")]
    public AudioClip sfxMelee;
    public AudioClip sfxDisparo;
    public AudioClip sfxCaminar;
    public AudioClip sfxCorrer;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    private void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    // Método general para reproducir un efecto sin cortar otros
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Funciones específicas
    public void PlayMelee() => PlaySFX(sfxMelee);
    public void PlayDisparo() => PlaySFX(sfxDisparo);
    public void PlayPasosCaminar() => PlaySFX(sfxCaminar);
    public void PlayPasosCorrer() => PlaySFX(sfxCorrer);
}