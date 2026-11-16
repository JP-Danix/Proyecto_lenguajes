using UnityEngine;
using UnityEngine.SceneManagement;

// La pulsación que arranca una NUEVA corrida desde la pantalla de victoria.
//
// Formalmente: Victory sigue siendo absorbente (nadie sale de ella por δ ni λ —
// la tabla no cambia). Lo que hace este botón es APAGAR la máquina y volver a
// encenderla: restaura la configuración inicial (pila nueva con Z0, cinta nueva,
// corazones de vuelta al mundo, q0 de nuevo) y carga el menú, que vive fuera de
// la quíntupla. De ahí, el botón Jugar arranca la corrida en el estado inicial.
public class VolverAlMenu : MonoBehaviour
{
    public void Volver()
    {
        // 1. La partida anterior terminó: los corazones vuelven al mundo.
        //    (el registro estático de HeartPickup es de la corrida, no del juego)
        HeartPickup.ReiniciarRegistro();

        // 2. Apagar el AFN-λ de la corrida anterior: su MapManager persistente
        //    (y el jugador que globalizó, con su pila y su cinta) no deben
        //    sobrevivir — la nueva corrida despierta los que traiga SampleScene.
        if (MapManager.Instance != null)
        {
            if (MapManager.Instance.JugadorPersistente != null)
                Destroy(MapManager.Instance.JugadorPersistente);
            Destroy(MapManager.Instance.gameObject);
        }

        // 3. Al menú (fuera de Q). Allí, Jugar → LoadScene("SampleScene") y el
        //    MapManager nuevo adopta q0 con la configuración restaurada.
        SceneManager.LoadScene("Menu");
    }
}
