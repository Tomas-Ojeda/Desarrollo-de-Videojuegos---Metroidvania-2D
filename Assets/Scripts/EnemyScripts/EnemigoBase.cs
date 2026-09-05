using System.Collections;
using UnityEngine;

// Asegura que el GameObject tenga los componentes necesarios automáticamente
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class EnemigoBase : MonoBehaviour
{
    [Header("🎯 Puntos de Patrulla")]
    public Transform puntoA;
    public Transform puntoB;
    private Transform objetivoActual;

    [Header("📊 Estadísticas del Enemigo")]
    public float vidaMaxima = 100f;
    protected float vidaActual;
    public float velocidadPatrulla = 2f;
    public float velocidadPersecucion = 4f;
    public float dañoAtaque = 10f;

    [Header("👁️ Rangos de Detección (Gizmos)")]
    public float rangoDeteccion = 5f;
    public float rangoAtaque = 1.5f;
    public Transform objetivoElisa; // Referencia a Elisa

    [Header("⏱️ Tiempos de Ataque y Anticipación")]
    public float tiempoEntreAtaques = 1.5f; // Cooldown total
    public float tiempoAnticipacion = 0.4f; // Tiempo de aviso antes del golpe
    private float tiempoSiguienteAtaque = 0f;
    private bool estaAtacando = false;

    // Componentes protegidos
    protected Rigidbody2D rb;
    protected Animator anim;
    protected bool mirandoDerecha = true;

    // Estados posibles del enemigo
    protected enum EstadoEnemigo { Patrullando, Persiguiendo, Atacando }
    protected EstadoEnemigo estadoActual;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        vidaActual = vidaMaxima;
    }

    protected virtual void Start()
    {
        objetivoActual = puntoA;
        estadoActual = EstadoEnemigo.Patrullando;
    }

    protected virtual void Update()
    {
        if (objetivoElisa == null) return;

        // Si está en medio de la secuencia de ataque, no cambia de estado ni se mueve
        if (estaAtacando) return;

        // Calcular la distancia hacia Elisa
        float distanciaAElisa = Vector2.Distance(transform.position, objetivoElisa.position);

        // Control de la Máquina de Estados según la distancia
        if (distanciaAElisa <= rangoAtaque)
        {
            estadoActual = EstadoEnemigo.Atacando;
        }
        else if (distanciaAElisa <= rangoDeteccion)
        {
            estadoActual = EstadoEnemigo.Persiguiendo;
        }
        else
        {
            estadoActual = EstadoEnemigo.Patrullando;
        }

        // Ejecutar el comportamiento según el estado
        ManejarEstados();
    }

    private void ManejarEstados()
    {
        switch (estadoActual)
        {
            case EstadoEnemigo.Patrullando:
                Patrullar();
                break;
            case EstadoEnemigo.Persiguiendo:
                Perseguir();
                break;
            case EstadoEnemigo.Atacando:
                IntentarAtacar();
                break;
        }
    }

    private void Patrullar()
    {
        if (objetivoActual == null) return;

        float direccion = (objetivoActual.position.x - transform.position.x) > 0 ? 1 : -1;
        rb.linearVelocity = new Vector2(direccion * velocidadPatrulla, rb.linearVelocity.y);

        GirarSprite(direccion);

        if (Mathf.Abs(transform.position.x - objetivoActual.position.x) < 0.5f)
        {
            objetivoActual = (objetivoActual == puntoA) ? puntoB : puntoA;
        }
    }

    private void Perseguir()
    {
        float direccion = (objetivoElisa.position.x - transform.position.x) > 0 ? 1 : -1;
        rb.linearVelocity = new Vector2(direccion * velocidadPersecucion, rb.linearVelocity.y);

        GirarSprite(direccion);
    }

    private void IntentarAtacar()
    {
        // Detener movimiento para realizar el ataque
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Verificar si el tiempo de recarga (cooldown) ya pasó
        if (Time.time >= tiempoSiguienteAtaque)
        {
            StartCoroutine(RutinaAtaque());
            tiempoSiguienteAtaque = Time.time + tiempoEntreAtaques;
        }
    }

    // Corrutina que gestiona la anticipación (aviso) y el golpe real
    private IEnumerator RutinaAtaque()
    {
        estaAtacando = true;

        // 1. Iniciar animación de ataque (Anticipación)
        if (anim != null) anim.SetTrigger("Atacar");

        // 2. Esperar el tiempo de anticipación para que el jugador reaccione
        yield return new WaitForSeconds(tiempoAnticipacion);

        // 3. Ejecutar la lógica de daño
        Atacar();

        estaAtacando = false;
    }

    // Método virtual: Aplica el daño a Elisa
    public virtual void Atacar()
    {
        if (objetivoElisa == null) return;

        // Comprobar si Elisa sigue dentro del rango de ataque tras la anticipación
        float distanciaActual = Vector2.Distance(transform.position, objetivoElisa.position);
        if (distanciaActual <= rangoAtaque + 0.5f)
        {
            // Conexión con el script de salud de Elisa
            ElisaHealth saludElisa = objetivoElisa.GetComponent<ElisaHealth>();
            if (saludElisa != null)
            {
                saludElisa.TakeDamage(dañoAtaque);
            }
        }
    }

    public virtual void RecibirDaño(float cantidad)
    {
        vidaActual -= cantidad;
        Debug.Log($"🩸 ¡{gameObject.name} recibió {cantidad} de daño! Vida restante: {vidaActual}/{vidaMaxima}");

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    protected virtual void Morir()
    {
        Debug.Log($"☠️ {gameObject.name} ha sido derrotado.");
        Destroy(gameObject);
    }

    private void GirarSprite(float direccion)
    {
        if ((direccion > 0 && !mirandoDerecha) || (direccion < 0 && mirandoDerecha))
        {
            mirandoDerecha = !mirandoDerecha;
            Vector3 escala = transform.localScale;
            escala.x *= -1;
            transform.localScale = escala;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);
    }
}