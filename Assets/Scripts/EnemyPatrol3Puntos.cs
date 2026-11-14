using UnityEngine;

// Patrulla de exactamente 3 puntos: ida P1 -> P2 -> P3 y regreso P3 -> P2 -> P1.
// Pensado para rutas en L (dos tramos rectos que se recorren y se des-recorren),
// a diferencia del circuito circular de EnemyPatrol (donde el último punto enlaza con el primero).
public class EnemyPatrol3Puntos : MonoBehaviour, IEnemigoMovil
{
    public float speed = 2f;
    public Animator animator;

    [Header("Los 3 puntos de la ruta, en orden (P1 -> P2 -> P3)")]
    public Transform punto1;
    public Transform punto2;
    public Transform punto3;

    [Header("Patrulla automática (si falta algún punto)")]
    public float patrolDistance = 3f; // separación de los puntos auto-creados

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private int indiceObjetivo;  // a qué punto vamos: 0, 1 o 2
    private int direccion = 1;   // +1 = avanzar por la ruta, -1 = regresar
    private float knockbackTimer;

    public bool IsKnockedBack => knockbackTimer > 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Auto-configuración: si falta algún punto, crea una ruta de 3 en línea
        // (izquierda, centro, derecha) alrededor del punto inicial.
        // Los puntos quedan SUELTOS en la escena (no hijos) para que el knockback no los arrastre.
        if (punto1 == null || punto2 == null || punto3 == null)
        {
            Vector2 basePos = transform.position;
            punto1 = CrearPunto("P1", basePos + Vector2.left * patrolDistance);
            punto2 = CrearPunto("P2", basePos);
            punto3 = CrearPunto("P3", basePos + Vector2.right * patrolDistance);
        }

        indiceObjetivo = 1; // arranca hacia el punto central
    }

    private Transform CrearPunto(string nombre, Vector2 posicion)
    {
        GameObject punto = new GameObject(nombre + "_" + name);
        punto.transform.position = posicion;
        return punto.transform;
    }

    // Lo llama PlayerSword al golpear: el enemigo sale despedido y no controla su rumbo un momento
    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        rb.linearVelocity = direction * force;
        knockbackTimer = duration;
    }

    void Update()
    {
        if (knockbackTimer > 0f) knockbackTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Durante el retroceso no controla su propio movimiento
        if (knockbackTimer > 0f)
        {
            return;
        }

        Vector2 destino = DestinoActual().position;
        Vector2 direction = (destino - (Vector2)transform.position).normalized;
        rb.linearVelocity = direction * speed;

        // Le dice al Animator qué tan rápido se mueve (para idle <-> correr)
        if (animator != null)
        {
            animator.SetFloat("Speed", rb.linearVelocity.magnitude);
        }

        // Voltea el sprite hacia donde camina (el arte mira a la izquierda por defecto)
        if (spriteRenderer != null && direction.x != 0)
        {
            spriteRenderer.flipX = direction.x > 0;
        }

        // Si llegó al punto, invierte el rumbo en los extremos (P1 y P3)
        // y avanza/regresa al siguiente; en P2 (el codo de la L) simplemente continúa
        if (Vector2.Distance(transform.position, destino) < 0.1f)
        {
            if (indiceObjetivo == 2 && direccion > 0) direccion = -1;
            else if (indiceObjetivo == 0 && direccion < 0) direccion = 1;

            indiceObjetivo = Mathf.Clamp(indiceObjetivo + direccion, 0, 2);
        }
    }

    private Transform DestinoActual()
    {
        if (indiceObjetivo == 0) return punto1;
        if (indiceObjetivo == 2) return punto3;
        return punto2;
    }

    // Invierte el rumbo de la patrulla (lo usa EnemyContactDamage al golpear al jugador):
    // pasa a caminar hacia el punto anterior de la ruta, por donde venía
    public void TurnAround()
    {
        direccion = -direccion;
        indiceObjetivo = Mathf.Clamp(indiceObjetivo + direccion, 0, 2);
    }
}
