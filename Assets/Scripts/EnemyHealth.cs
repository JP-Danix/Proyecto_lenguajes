using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public Animator animator;      // Animator del enemigo
    public float hurtDuration = 0.3f; // cuánto dura la animación de daño

    private int health;

    void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Destroy(gameObject);
            return;
        }
        StartCoroutine(HurtRoutine());
    }

    private IEnumerator HurtRoutine()
    {
        if (animator != null)
        {
            animator.SetBool("isHurt", true);
            yield return new WaitForSeconds(hurtDuration);
            animator.SetBool("isHurt", false);
        }
    }
}
