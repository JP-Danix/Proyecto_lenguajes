using UnityEngine;

// El estado de rechazo del PDA hecho pantalla.
//
// La mecánica: δ(q, daño, λ) no existe — recibir daño con la pila vacía no tiene
// transición, así que la lectura muere en el acto (el prefijo deja de ser viable).
// Este panel es la INTERFAZ de ese rechazo: PlayerHealth lo activa en el momento
// exacto del símbolo no viable (el reporte del compilador en la línea del error,
// no al final de la cadena). El botón VOLVER AL MENU es una nueva corrida:
// VolverAlMenu restaura la configuración inicial (pila nueva, corazones de vuelta).
//
// Vive DENTRO del prefab HealthCanvas (inactivo por defecto): cada escena trae
// el suyo, como la barra de corazones.
public class GameOverUI : MonoBehaviour
{
    // Lo llama PlayerHealth al momento del rechazo. El panel está INACTIVO en
    // la escena, por eso la búsqueda incluye objetos inactivos.
    public static void Mostrar()
    {
        GameOverUI ui = FindFirstObjectByType<GameOverUI>(FindObjectsInactive.Include);
        if (ui != null)
        {
            ui.gameObject.SetActive(true);
            Debug.Log("Rechazo del PDA: no existe δ(q, daño, λ) — la pila está vacía. Game Over.");
        }
        else
        {
            Debug.LogWarning("Game Over sin pantalla: no hay componente GameOverUI en esta escena (va dentro del HealthCanvas, desactivado)");
        }
    }
}
