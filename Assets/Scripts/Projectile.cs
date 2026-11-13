using UnityEngine;

// Proyectil genérico: se mueve en línea recta y daña al primer enemigo que toca.
// Lo instancia ComboController cuando una cadena de la gramática de combos es aceptada.
public class Projectile : MonoBehaviour
{
    public float velocidad = 6f;
    public float vidaMaxima = 2f; // guarda temporal: el proyectil muere aunque no golpee

    private int danio;
    private bool lanzado;

    // Lo llama ComboController justo después de instanciar
    public void Lanzar(Vector2 direccion, int danioDelCombo)
    {
        danio = danioDelCombo;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direccion.normalized * velocidad;
        }

        // El arte apunta a la derecha: rota el sprite hacia la dirección de vuelo
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);

        lanzado = true;
        Destroy(gameObject, vidaMaxima);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!lanzado) return;
        if (collision.CompareTag("Player")) return; // no se daña a sí mismo

        // Golpea al enemigo: daño y desaparece
        EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
        if (enemy != null)
        {
            enemy.TakeDamage(danio);
            Destroy(gameObject);
            return;
        }

        // Los triggers "fantasma" (espada del jugador, pickups, zonas de colina)
        // NO detienen el proyectil: solo lo detienen los muros sólidos (colliders normales)
        if (!collision.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}
