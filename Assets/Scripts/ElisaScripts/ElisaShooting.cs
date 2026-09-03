using System.Collections;
using UnityEngine;

public class ElisaShooting : MonoBehaviour
{
    [Header("Referencias")]
    public GameObject bulletPrefab;
    public Transform firePoint;

    [Header("Configuración de Munición")]
    public int maxClipSize = 12;            // Balas por cargador
    public int currentAmmo = 12;           // Balas en el cargador actual
    public int extraClips = 2;              // Cargadores de repuesto (máx 2)

    [Header("Tiempos y Cadencia")]
    public float fireRate = 0.25f;          // Tiempo mínimo entre disparos
    public float reloadTime = 1.5f;         // Tiempo que tarda en recargar

    private float nextFireTime;
    private bool isReloading;
    private Animator anim;

    private void Start()
    {
        currentAmmo = maxClipSize;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isReloading) return;

        // Disparar con la tecla L
        if (Input.GetKeyDown(KeyCode.L) && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                Shoot();
            }
            else if (extraClips > 0)
            {
                StartCoroutine(Reload());
            }
            else
            {
                Debug.Log("¡Sin munición ni cargadores de repuesto!");
            }
        }

        // Recargar manualmente con R
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxClipSize && extraClips > 0)
        {
            StartCoroutine(Reload());
        }
    }

    private void Shoot()
    {
        nextFireTime = Time.time + fireRate;
        currentAmmo--;

        // Activar el Trigger de la animación de disparo a distancia
        if (anim != null)
        {
            anim.SetTrigger("rangedAttack");
        }

        // Determinar orientación de disparo (según hacia dónde mira Elisa)
        float facingDirection = Mathf.Sign(transform.localScale.x);
        Quaternion bulletRotation = facingDirection > 0 ? Quaternion.identity : Quaternion.Euler(0, 180, 0);

        // Instanciar bala
        Instantiate(bulletPrefab, firePoint.position, bulletRotation);

        Debug.Log($"¡Pum! Balas restantes en cargador: {currentAmmo}/{maxClipSize} | Cargadores extra: {extraClips}");
    }

    private IEnumerator Reload()
    {
        isReloading = true;
        if (anim != null) anim.SetBool("isReloading", true);

        Debug.Log("Recargando pistola...");

        yield return new WaitForSeconds(reloadTime);

        if (extraClips > 0)
        {
            extraClips--;
            currentAmmo = maxClipSize;
            Debug.Log($"¡Pistola recargada! Cargadores extra restantes: {extraClips}");
        }

        if (anim != null) anim.SetBool("isReloading", false);
        isReloading = false;
    }

    public void AddClips(int amount)
    {
        extraClips = Mathf.Clamp(extraClips + amount, 0, 2);
    }
}