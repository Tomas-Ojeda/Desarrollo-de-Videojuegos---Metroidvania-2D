using System.Collections;
using UnityEngine;

public class MiniJefeBase : EnemigoBase
{
    [Header("🤢 Invocación de Súbditos")]
    public GameObject prefabSubditoZombie;
    public float tiempoEntreInvocaciones = 6f;
    public Transform puntoInvocacion;
    public int maxSubditosSimultaneos = 3;

    [Header("🧪 Disparo de Ácido a Distancia")]
    public GameObject prefabProyectilAcido;
    public Transform puntoDisparo;
    public float fuerzaDisparo = 7f;
    public float tiempoEntreDisparos = 3.5f;

    [Header("🫠 Rastro de Ácido Pasivo")]
    public GameObject prefabCharcoAcido;
    public float tiempoEntreCharcos = 0.8f;
    public Vector3 offsetPies = new Vector3(0f, -0.8f, 0f);

    private float contadorInvocacion;
    private float contadorDisparo;
    private float contadorCharco;
    private bool enFase2 = false;

    protected override void Update()
    {
        ComprobarCambioDeFase();

        // 1. Rastro continuo por donde camina despacio
        GenerarRastroAcido();

        // 2. Lógica de ataque a distancia e invocación si detecta a Elisa
        if (objetivoElisa != null)
        {
            float distancia = Vector2.Distance(transform.position, objetivoElisa.position);

            if (distancia <= rangoDeteccion)
            {
                ManejarAtaquesApdf(distancia);
            }
        }

        // 3. Movimiento lento base (patrulla / seguimiento pesado)
        base.Update();
    }

    private void ManejarAtaquesApdf(float distancia)
    {
        contadorDisparo += Time.deltaTime;
        contadorInvocacion += Time.deltaTime;

        // Disparo de ácido si Elisa está a cierta distancia
        if (contadorDisparo >= tiempoEntreDisparos)
        {
            DispararAcido();
            contadorDisparo = 0f;
        }

        // Invocación de zombies desde la masa
        if (contadorInvocacion >= tiempoEntreInvocaciones)
        {
            InvocarSubdito();
            contadorInvocacion = 0f;
        }
    }

    private void DispararAcido()
    {
        if (prefabProyectilAcido == null) return;

        Vector3 origen = puntoDisparo != null ? puntoDisparo.position : transform.position;
        GameObject proyectil = Instantiate(prefabProyectilAcido, origen, Quaternion.identity);

        // Dirección en arco o directa hacia Elisa
        Vector2 direccion = (objetivoElisa.position - origen).normalized;
        direccion.y += 0.6f; // Ligera parábola al disparar

        Rigidbody2D rb = proyectil.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(direccion * fuerzaDisparo, ForceMode2D.Impulse);
        }

        Debug.Log("🤮 El Minijefe disparó una bola de ácido.");
    }

    private void InvocarSubdito()
    {
        if (prefabSubditoZombie == null) return;

        // Limita la cantidad de enemigos simultáneos en pantalla
        int zombiesActivos = GameObject.FindGameObjectsWithTag("Enemy").Length;
        if (zombiesActivos >= maxSubditosSimultaneos + 1) return; 

        Vector3 origen = puntoInvocacion != null ? puntoInvocacion.position : transform.position;
        Instantiate(prefabSubditoZombie, origen, Quaternion.identity);

        Debug.Log("🧟 Un zombie emergió de la masa corporal!");
    }

    private void GenerarRastroAcido()
    {
        contadorCharco += Time.deltaTime;
        if (contadorCharco >= tiempoEntreCharcos)
        {
            if (prefabCharcoAcido != null)
            {
                Instantiate(prefabCharcoAcido, transform.position + offsetPies, Quaternion.identity);
            }
            contadorCharco = 0f;
        }
    }

    private void ComprobarCambioDeFase()
    {
        if (!enFase2 && vidaActual <= vidaMaxima * 0.5f)
        {
            enFase2 = true;
            tiempoEntreDisparos *= 0.6f;     // Dispara más rápido
            tiempoEntreInvocaciones *= 0.7f; // Expulsa zombies más rápido
            Debug.Log("🤢 ¡FASE 2: La masa empieza a convulsionar e invocar más rápido!");
        }
    }
}