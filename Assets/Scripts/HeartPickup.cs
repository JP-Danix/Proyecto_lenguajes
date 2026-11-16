using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Objeto recogible: al tocarlo el jugador, hace PUSH del corazón a la pila.
//
// Cada corazón se puede recoger UNA SOLA VEZ por partida: el registro global
// `recogidos` (estático, sobrevive a la descarga y recarga de escenas) marca los
// ya consumidos, y al despertar en una escena nueva cada copia se autodestruye
// si su clave ya está en el registro. Las letras (LetraPickup) NO usan este
// mecanismo: reaparecen por visita (regla de la cinta) porque el AFN-lambda
// necesita re-escribirlas para re-entrar a niveles.
public class HeartPickup : MonoBehaviour
{
    public HeartColor type = HeartColor.Rojo;

    // Corazones recogidos en esta partida. La clave (escena + posición + nombre)
    // es estable dentro de la sesión aunque la escena se descargue y vuelva a
    // cargar: el InstanceID de un objeto recargado cambia, la posición no.
    private static readonly HashSet<string> recogidos = new HashSet<string>();

    // Nueva partida: vacía el registro. Lo llama el botón de volver al menú
    // (VolverAlMenu.cs) — la nueva corrida arranca con todos los corazones
    // en el mundo, porque el registro es de LA CORRIDA, no del juego entero.
    public static void ReiniciarRegistro()
    {
        recogidos.Clear();
    }

    private string Clave()
    {
        return SceneManager.GetActiveScene().name + "|" +
               transform.position.ToString("F2") + "|" +
               gameObject.name;
    }

    private void Awake()
    {
        // Ya recogido en una visita anterior de esta partida: esta copia no debería existir.
        if (recogidos.Contains(Clave()))
        {
            Destroy(gameObject);
        }
    }

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
        if (player != null)
        {
            recogidos.Add(Clave()); // marcado ANTES de destruirse: si la escena se recarga, no vuelve
            player.PushHeart(type);
            Destroy(gameObject); // recogido: PUSH hecho (la pila es ilimitada, siempre acepta)
        }
    }
}
