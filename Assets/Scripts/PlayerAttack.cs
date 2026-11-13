using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    public GameObject slashEffectDown;
    public GameObject slashEffectUP;
    public GameObject slashEffectSide;
    public Transform attackSpawnPoint;
    public Animator animator;
    public Rigidbody2D rb;
    public float attackCooldown = 0.3f;
    public float attackDuration = 0.4f; // un poco más que lo que dura tu clip

    private Vector2 lastMovementDirection;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }
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
            PlaySlashEffect();
        }
    }

    void PlaySlashEffect()
    {
        GameObject prefabToSpawn = slashEffectSide; // Default

        if (lastMovementDirection.y > 0.1f)
        {
            prefabToSpawn = slashEffectUP;
        }
        else if (lastMovementDirection.y < -0.1f)
        {
            prefabToSpawn = slashEffectDown;
        }
        else
            if(Mathf.Abs(lastMovementDirection.x) > 0.5f)
        {
            prefabToSpawn = slashEffectSide;
        }

        Destroy(Instantiate(prefabToSpawn, attackSpawnPoint.position, Quaternion.identity), 0.3f);
    }

    public void SetLastMovementDirection(Vector2 direction)
    {
        if (direction != Vector2.zero)
        {
            lastMovementDirection = direction;
        }
    }
    // Lo llama el Animation Event al final del clip (y el temporizador como respaldo)
    public void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }
}