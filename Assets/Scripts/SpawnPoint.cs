using UnityEngine;

// Marca invisible: el punto donde aparece el jugador al entrar a esta escena.
// La tabla δ del AFD de mapas (MapManager) dice qué spawn corresponde a cada puerta.
public class SpawnPoint : MonoBehaviour
{
    public string id;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.4f);
    }
}
