using System.Collections;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public Animator animator;      // Animator del enemigo
    public float hurtDuration = 0.3f; // cuánto dura la animación de daño

    private int health;
    private bool dead; // la FSM entró al estado sumidero Death: de aquí no se sale

    void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        if (dead) return; // un cadáver no recibe golpes

        health -= damage;
        if (health <= 0)
        {
            Die();
            return;
        }
        StartCoroutine(HurtRoutine());
    }

    // Transición al estado sumidero Death de la FSM del enemigo (la guarda `isDead`
    // del Animator dispara Idle/Run/Hit -> Death desde cualquier estado de vida).
    //
    // Acciones de entrada del estado: apagar todo lo que mueve o hace dañar al
    // cuerpo (patrulla, daño por contacto, colisiones) — el cadáver no patrulla,
    // no daña ni recibe golpes. La SALIDA del estado es la animación de muerte
    // (salida de Moore); la destrucción del GameObject es la capa de aplicación
    // que corre cuando el clip termina.
    private void Die()
    {
        dead = true;
        StopAllCoroutines(); // cancela HurtRoutine si estaba a media guarda

        foreach (MonoBehaviour script in GetComponentsInChildren<MonoBehaviour>())
        {
            if (script != null && script != this) script.enabled = false; // patrullas, contacto, etc.
        }
        foreach (Collider2D col in GetComponentsInChildren<Collider2D>())
        {
            col.enabled = false;
        }

        if (animator == null)
        {
            Destroy(gameObject);
            return;
        }

        animator.SetBool("isDead", true); // guarda del AFD: Idle/Run/Hit -> Death
        StartCoroutine(DeathRoutine());
    }

    // Espera a que la FSM entre a Death y destruye el objeto cuando termina el
    // clip: la duración se lee del propio Animator (no hay número mágico).
    private IEnumerator DeathRoutine()
    {
        // Deja que el Animator procese la transición hacia EnemyDie (timeout de 2 s por si acaso)
        float waited = 0f;
        while (waited < 2f)
        {
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("EnemyDie")) break;
            waited += Time.deltaTime;
            yield return null;
        }

        AnimatorStateInfo death = animator.GetCurrentAnimatorStateInfo(0);
        if (death.IsName("EnemyDie"))
        {
            // Solo el tiempo del clip que queda por reproducir
            float restante = Mathf.Max(0f, (1f - death.normalizedTime) * death.length);
            yield return new WaitForSeconds(restante);
        }

        Destroy(gameObject);
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
