using UnityEngine;

public class EnemyContactDamage : MonoBehaviour
{
    public int damage = 1;
    public float attackCooldown = 1f; // segundos entre golpes que puede dar el enemigo

    private float cooldownTimer;

    void Update()
    {
        // Descuenta el cooldown del ataque
        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // No puede atacar mientras dure su cooldown
        if (cooldownTimer > 0f) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            // Dirección del golpe: del enemigo hacia el jugador (para el knockback)
            Vector2 hitDirection = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
            collision.gameObject.GetComponent<PlayerHealth>()?.TakeDamage(damage, hitDirection);
            cooldownTimer = attackCooldown;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Solo al INICIAR el contacto: golpea y da media vuelta (no se queda pegado drenando)
        if (collision.gameObject.CompareTag("Player"))
        {
            GetComponent<EnemyPatrol>()?.TurnAround();
        }
    }
}
