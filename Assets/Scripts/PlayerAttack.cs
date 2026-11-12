using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public Animator animator;
    public Rigidbody2D rb;
    public float attackCooldown = 0.3f;
    public float attackDuration = 0.4f; // un poco más que lo que dura tu clip

    private float cooldownTimer;
    private float attackTimer;
    private bool isAttacking;

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (isAttacking)
        {
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0f) ResetAttack(); // seguro por si el evento falla
            return;
        }

        if (Input.GetMouseButtonDown(0) && cooldownTimer <= 0f)
        {
            isAttacking = true;
            attackTimer = attackDuration;
            cooldownTimer = attackCooldown;
            animator.SetBool("isAttacking", true);
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Lo llama el Animation Event al final del clip (y el temporizador como respaldo)
    public void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }
}