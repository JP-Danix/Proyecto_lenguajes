using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    public float speed = 2f;
    public Transform pointA;
    public Transform pointB;
    public Animator animator;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Vector2 target;
    private float knockbackTimer;

    public bool IsKnockedBack => knockbackTimer > 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        target = pointB.position;
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

        Vector2 direction = (target - (Vector2)transform.position).normalized;
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

        // Si llegó cerca del punto, cambia al otro
        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            target = target == (Vector2)pointA.position
                ? (Vector2)pointB.position
                : (Vector2)pointA.position;
        }
    }

    // Invierte el rumbo de la patrulla (lo usa EnemyContactDamage al golpear al jugador)
    public void TurnAround()
    {
        target = target == (Vector2)pointA.position
            ? (Vector2)pointB.position
            : (Vector2)pointA.position;
    }
}
