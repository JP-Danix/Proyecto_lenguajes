using UnityEngine;

public class HillExit : MonoBehaviour
{
    public Collider2D[] hillColliders;
    public Collider2D[] boundaryColliders;

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
