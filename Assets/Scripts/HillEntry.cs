using UnityEngine;

public class HillEntry : MonoBehaviour
{
    public Collider2D[] hillColliders;
    public Collider2D[] boundaryColliders;

    [Header("Nombres convención (solo si no cableas a mano)")]
    public string hillTilemapName = "HillsCollition";
    public string boundaryTilemapName = "HillBoundary";

    // Auto-configuración para reutilizar la colina en cualquier nivel:
    // si las listas están vacías en el Inspector, busca los tilemaps por nombre.
    // En niveles con varias colinas, renombra los tilemaps y asígnalos a mano.
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
                hill.enabled = false;
            }

            foreach (Collider2D boundary in boundaryColliders)
            {
                boundary.enabled = true;
            }

            collision.gameObject.GetComponent<SpriteRenderer>().sortingOrder = 15;
        }
    }
}
