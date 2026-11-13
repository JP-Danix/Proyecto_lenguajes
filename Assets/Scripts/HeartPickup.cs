using UnityEngine;

// Objeto recogible: al tocarlo el jugador, hace PUSH del corazón a la pila.
public class HeartPickup : MonoBehaviour
{
    public HeartColor type = HeartColor.Rojo;

    private void Start()
    {
        // Tinta el sprite del mundo con el color del tipo (mismos colores que la UI)
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = type switch
            {
                HeartColor.Amarillo => new Color(1f, 0.92f, 0.23f),
                HeartColor.Azul => new Color(0.2f, 0.55f, 1f),
                _ => new Color(0.95f, 0.25f, 0.25f)
            };
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        PlayerHealth player = collision.GetComponent<PlayerHealth>();
        if (player != null && player.PushHeart(type))
        {
            Destroy(gameObject); // recogido: PUSH hecho
        }
        // Si la pila está llena, el corazón NO se consume (sigue en el mapa)
    }
}
