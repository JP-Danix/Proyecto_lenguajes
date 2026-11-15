using UnityEngine;

// Una puerta del AFN-lambda de mapas. Hay dos clases, y cada una es una pieza formal:
//
//   PuertaLibre (las torres): transición λ — el cambio de estado ocurre sin leer
//   ningún símbolo. Su Id Puerta es el bookkeeping interno que distingue esta
//   puerta de las demás en la lista de λ: no es una letra de Σ, es la elección
//   no determinista que el jugador resuelve caminando.
//
//   PuertaLectora (las puertas de las casas): el momento de lectura — la puerta
//   LEE la letra que el jugador lleva en la cinta (llave, moneda) y consulta
//   δ(estado, letra). Sin letra que leer no hay transición: rechazo. La letra se
//   consume al transitar (se lee una vez y sale de la cinta).
public class DoorTransition : MonoBehaviour
{
    public enum TipoPuerta { PuertaLibre, PuertaLectora }

    public TipoPuerta tipo = TipoPuerta.PuertaLibre;

    [Tooltip("Puertas libres (λ): identificador interno de esta puerta (no es una letra de Σ)")]
    public string idPuerta;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        if (tipo == TipoPuerta.PuertaLibre)
        {
            MapManager.Instance?.TransitarLambda(idPuerta);
            return;
        }

        // Puerta lectora: lee la letra pendiente en la cinta del jugador
        InventarioJugador inventario = collision.GetComponent<InventarioJugador>();
        if (inventario == null || !inventario.TieneLetra)
        {
            Debug.LogWarning("Rechazo del AFN-lambda de mapas: la puerta no lee ninguna letra — la cinta del jugador está vacía");
            return;
        }

        string letra = inventario.LetraParaLeer();
        if (MapManager.Instance != null && MapManager.Instance.Transitar(letra, collision.gameObject))
        {
            inventario.Consumir(); // la letra se leyó una vez: sale de la cinta
        }
    }
}
