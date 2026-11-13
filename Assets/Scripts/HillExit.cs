using UnityEngine;

public class HillExit : MonoBehaviour
{
    public Collider2D[] hillColliders;
    public Collider2D[] boundaryColliders;

    [Header("Nombres convención (solo si no cableas a mano)")]
    public string hillTilemapName = "HillsCollition";
    public string boundaryTilemapName = "HillBoundary";

    // Mismo auto-cableado que HillEntry: listas vacías = busca por nombre convención
    void Awake()
    {
        if (hillColliders == null || hillColliders.Length == 0)
        {
            GameObject hills = GameObject.Find(hillTilemapName);
            if (hills != null) hillColliders = hills.GetComponentsInChildren<Collider2D>();
        }

        if (boundaryColliders == null || boundaryColliders.Length == 0)
        {
            GameObject limites = GameObject.Find(boundaryTilemapName);
            if (limites != null) boundaryColliders = limites.GetComponentsInChildren<Collider2D>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            foreach (Collider2D hill in hillColliders)
            {
                hill.enabled = true;
            }

            foreach (Collider2D boundary in boundaryColliders)
            {
                boundary.enabled = false;
            }

            // Vuelve al sorting order base (4, igual que los árboles): con el sort
            // por eje Y configurado, la posición del jugador decide quién se dibuja encima
            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 4;
        }
    }
}
