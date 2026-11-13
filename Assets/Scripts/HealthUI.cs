using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Dibuja en pantalla el contenido de la pila de corazones.
// El primer icono (izquierda) es la CIMA de la pila: el corazón que recibe el daño.
public class HealthUI : MonoBehaviour
{
    public RectTransform heartsContainer; // panel con Horizontal Layout Group

    public void Refresh(Stack<Heart> hearts)
    {
        // Redibuja todos los iconos
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (Heart heart in hearts) // Stack se itera de la cima hacia abajo
        {
            GameObject icon = Instantiate(heartIconPrefab, heartsContainer);
            icon.GetComponent<Image>().color = GetColor(heart);
        }
    }

    public GameObject heartIconPrefab;

    private Color GetColor(Heart heart)
    {
        Color baseColor = heart.type switch
        {
            HeartColor.Amarillo => new Color(1f, 0.92f, 0.23f),
            HeartColor.Azul => new Color(0.2f, 0.55f, 1f),
            _ => new Color(0.95f, 0.25f, 0.25f)
        };

        // Cuantos menos golpes le quedan al corazón, más oscuro se ve
        float damageRatio = 1f - (float)heart.hitsLeft / (int)heart.type;
        return Color.Lerp(baseColor, Color.black, damageRatio * 0.6f);
    }
}
