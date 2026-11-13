using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5;
    public Rigidbody2D rb;
    public Animator animator;
    public SpriteRenderer spriteRenderer;
    private Vector2 LastMoveDirection = Vector2.down;

    void FixedUpdate()
    {
        // Congela el movimiento y los parámetros de animación mientras ataca,
        // así el ataque conserva la dirección y el personaje no camina.
        if (animator.GetBool("isAttacking"))
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        Vector2 movement = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Solo actualizamos la dirección del Animator cuando nos movemos,
        // así el idle conserva la última dirección al detenerse.
        if (movement != Vector2.zero)
        {
            LastMoveDirection = movement;
            animator.SetFloat("Xinput", movement.x);
            animator.SetFloat("Yinput", movement.y);
        }

        // Voltea el sprite según la última dirección horizontal
        spriteRenderer.flipX = LastMoveDirection.x < 0;

        animator.SetFloat("Speed", movement.magnitude);

        rb.linearVelocity = movement * speed;
    }
}