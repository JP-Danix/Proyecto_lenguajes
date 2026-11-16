using System.Collections.Generic;
using UnityEngine;

// La cinta del AFN de mapas: la letra (llave, moneda) que el jugador lleva
// disponible para ser leída por una puerta lectora.
//
//   - Una letra por VISITA al estado (nivel): mientras permaneces en un estado,
//     solo un pickup deja escribir (el resto queda inerte). Al reingresar al
//     estado, la cinta y el registro se reinician: los pickups vuelven a estar
//     disponibles (siempre puedes regresar y escribir de nuevo).
//   - La letra se lee una vez y sale de la cinta: la puerta lectora la consume
//     al transitar.
//   - La cinta es por estado: al cambiar de escena, las letras no leídas se
//     pierden (nunca llegaron a leerse). Cada nivel escribe su propia palabra;
//     el alfabeto Σ es global, la cinta no viaja entre estados.
public class InventarioJugador : MonoBehaviour
{
    // La letra pendiente en la cinta (null = cinta vacía)
    public string LetraActual { get; private set; }

    // Los estados donde ya se escribió una letra: cierra las demás ramas del nivel
    private readonly HashSet<string> estadosConLetra = new HashSet<string>();

    public bool TieneLetra => LetraActual != null;

    // ¿Se puede escribir una letra en este estado? (una sola por nivel)
    public bool PuedeRecogerEn(string estado)
    {
        return !estadosConLetra.Contains(estado);
    }

    // Recoger: la letra queda disponible en la cinta y su rama queda marcada
    public void Recoger(string letra, string estado)
    {
        LetraActual = letra;
        estadosConLetra.Add(estado);
        Debug.Log("Letra disponible en la cinta: '" + letra + "' ∈ Σ — en '" + estado + "' ya no se puede escribir otra");
    }

    // La puerta lectora lee esto: la letra pendiente en la cinta
    public string LetraParaLeer() => LetraActual;

    // La letra se lee una vez y sale de la cinta (la puerta la consume al transitar)
    public void Consumir()
    {
        if (LetraActual == null) return;
        Debug.Log("Letra consumida: '" + LetraActual + "' salió de la cinta (se leyó una vez)");
        LetraActual = null;
    }

    // Cinta por estado: lo llama el MapManager al entrar a un estado (escena).
    // Las letras no leídas se pierden y el registro de letras escritas se
    // libera: cada visita al estado arranca con la cinta vacía y los pickups
    // disponibles de nuevo.
    public void ReiniciarCinta()
    {
        if (LetraActual != null)
        {
            Debug.Log("Cinta por estado: '" + LetraActual + "' nunca fue leída y se pierde al cambiar de estado");
        }
        LetraActual = null;
        estadosConLetra.Clear();
    }
}
