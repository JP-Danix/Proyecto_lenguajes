using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Dibuja en pantalla el contenido de la pila de corazones.
// El primer icono (izquierda) es la CIMA de la pila: el corazón que recibe el daño.
//
// Como la pila es ilimitada (un PDA de verdad), puede tener cualquier profundidad:
// se muestran los iconos de la CIMA (hasta Max Visible Icons) y, si hay más corazones
// por debajo, un contador los resume. La pantalla sigue siendo función pura de la pila.
public class HealthUI : MonoBehaviour
{
    public RectTransform heartsContainer; // panel con Horizontal Layout Group
    public GameObject heartIconPrefab;
    public int maxVisibleIcons = 5;       // cuántos iconos de la cima se dibujan como máximo
    public Text counterText;              // opcional: muestra "+N" con los corazones no visibles

    public void Refresh(Stack<Heart> hearts)
    {
        // Redibuja los iconos de la cima
        foreach (Transform child in heartsContainer)
        {
            Destroy(child.gameObject);
        }

        int mostrados = 0;
        foreach (Heart heart in hearts) // Stack se itera de la cima hacia abajo
        {
            if (mostrados >= maxVisibleIcons) break;
            GameObject icon = Instantiate(heartIconPrefab, heartsContainer);
            icon.GetComponent<Image>().color = GetColor(heart);
            mostrados++;
        }

        // El resto de la pila (debajo de los visibles) se resume en el contador
        if (counterText != null)
        {
            int ocultos = hearts.Count - mostrados;
            counterText.text = ocultos > 0 ? "+" + ocultos : "";
        }
    }

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
