using UnityEngine;
using UnityEngine.SceneManagement;

// Una letra del alfabeto Σ hecha pickup (la llave, la moneda). Al tocarla, el
// jugador la escribe en su cinta (InventarioJugador) y queda disponible para
// ser leída por una puerta lectora. Regla de cadena bien formada: una sola
// letra por estado — si en este nivel ya se escribió una, este pickup queda
// inerte (rechazo con log, sin destruirse: sigue existiendo en el mapa).
public class LetraPickup : MonoBehaviour
{
    [Tooltip("Letra que este pickup escribe en la cinta (p. ej. \"llave\", \"moneda\")")]
    public string letra;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        InventarioJugador inventario = collision.GetComponent<InventarioJugador>();
        if (inventario == null) return;

        // Los estados del AFD de mapas son escenas: el estado actual es la escena activa
        string estado = SceneManager.GetActiveScene().name;
        if (!inventario.PuedeRecogerEn(estado))
        {
            Debug.LogWarning("Rechazo del pickup: en '" + estado + "' ya se escribió una letra en esta visita — este símbolo queda inerte hasta reingresar al estado");
            return;
        }

        inventario.Recoger(letra, estado);
        Destroy(gameObject); // recogido: la letra vive en la cinta, no en el mundo
    }
}
