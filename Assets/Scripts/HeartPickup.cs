using UnityEngine;

// Objeto recogible: al tocarlo el jugador, hace PUSH del corazón a la pila.
public class HeartPickup : MonoBehaviour
{
    public HeartColor type = HeartColor.Rojo;

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
