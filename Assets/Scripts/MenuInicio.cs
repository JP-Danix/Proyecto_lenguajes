using UnityEngine;
using UnityEngine.SceneManagement;

// El interruptor ON/OFF del curso (Unidad 1): "la entrada o interacción es la
// pulsación del interruptor". Este menú NO es un estado del AFN-λ de mapas —
// vive FUERA de la quíntupla: no es una escena de Q, no lleva filas δ ni λ.
// Desde aquí solo se enciende la máquina (Jugar: arranca la corrida en q0)
// o se apaga del todo (Salir). La escena de la que arranca la corrida la
// adopta el MapManager de esa escena (su campo estadoInicial está vacío).
public class MenuInicio : MonoBehaviour
{
    public void Jugar()
    {
        // Encender: cargar el estado inicial q0 (SampleScene). El MapManager
        // que despierta ahí compila sus tablas y adopta la escena como q0.
        SceneManager.LoadScene("SampleScene");
    }

    public void Salir()
    {
        // Apagar del todo. En el build cierra la aplicación; en el editor no
        // hace nada (Unity no cierra el editor por un Application.Quit).
        Application.Quit();
    }
}
