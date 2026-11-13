using UnityEngine;

public class PlayerSword : MonoBehaviour
{
    public Animator playerAnimator;
    public int damage = 1;
    public float reach = 0.6f; // distancia del golpe desde el centro del jugador

    private bool wasAttacking;
    private bool hasHit; // un solo golpe por ataque

    void Update()
    {
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
        if (!playerAnimator.GetBool("isAttacking") || hasHit) return;
        if (!collision.CompareTag("Enemy")) return;

        EnemyHealth health = collision.GetComponent<EnemyHealth>();
        if (health != null)
        {
            health.TakeDamage(damage);
            hasHit = true;
        }
    }
}
