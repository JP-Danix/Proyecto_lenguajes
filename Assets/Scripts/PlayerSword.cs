using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public Animator playerAnimator;
    public int damage = 1;
    public float reach = 0.6f;         // distancia del golpe desde el centro del jugador
    public float hitCooldown = 0.5f;   // segundos mínimos entre golpes conectados
    public float enemyKnockbackForce = 3f;    // empujón que recibe el enemigo
    public float enemyKnockbackDuration = 0.15f; // tiempo que el enemigo no controla su movimiento

    private bool wasAttacking;
    private bool hasHit;    // un solo golpe por ataque
    private float hitTimer; // cooldown entre golpes conectados

    void Update()
    {
        if (hitTimer > 0f) hitTimer -= Time.deltaTime;

        bool attacking = playerAnimator.GetBool("isAttacking");

        // Cada vez que inicia un ataque, habilita un golpe nuevo
        if (attacking && !wasAttacking)
        {
            hasHit = false;
        }
        wasAttacking = attacking;

        // Coloca el trigger delante del jugador, según la dirección del ataque
        Vector2 dir = new Vector2(playerAnimator.GetFloat("Xinput"), playerAnimator.GetFloat("Yinput"));
        if (dir == Vector2.zero) dir = Vector2.down;
        transform.localPosition = dir.normalized * reach;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!playerAnimator.GetBool("isAttacking") || hasHit || hitTimer > 0f) return;
        if (!collision.CompareTag("Enemy")) return;

        EnemyHealth health = collision.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            hasHit = true;
            hitTimer = hitCooldown;

            // Knockback al enemigo: se aleja del jugador
            EnemyPatrol patrol = collision.GetComponent<EnemyPatrol>();
            Vector2 direction = ((Vector2)collision.transform.position - (Vector2)transform.parent.position).normalized;
            if (patrol != null)
            {
                patrol.ApplyKnockback(direction, enemyKnockbackForce, enemyKnockbackDuration);
            }
            else
            {
                Rigidbody2D enemyRb = collision.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.linearVelocity = direction * enemyKnockbackForce;
                }
            }
        }
    }
}
