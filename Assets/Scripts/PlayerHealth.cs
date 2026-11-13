using System.Collections.Generic;
using UnityEngine;

public enum HeartColor
{
    Amarillo = 3, // aguanta 3 golpes
    Azul = 2,     // aguanta 2 golpes
    Rojo = 1      // aguanta 1 golpe
}

[System.Serializable]
public class Heart
{
    public HeartColor type;
    public int hitsLeft;

    public Heart(HeartColor type)
    {
        this.type = type;
        hitsLeft = (int)type;
    }
}

// Sistema de vida modelado como autómata de pila (PDA):
// - PUSH: recoger un corazón (queda en la cima)
// - POP: el corazón de la cima se agota por el daño
// - Rechazo: pila vacía = Game Over
public class PlayerHealth : MonoBehaviour
{
    public int maxHearts = 3;
    public List<HeartColor> initialHearts = new List<HeartColor>
    {
        HeartColor.Amarillo, HeartColor.Azul, HeartColor.Rojo
    };
    public HealthUI healthUI;

    private readonly Stack<Heart> hearts = new Stack<Heart>(); // la cima es el último apilado

    void Start()
    {
        // Apila los corazones iniciales: el último de la lista queda en la cima
        foreach (HeartColor type in initialHearts)
        {
            hearts.Push(new Heart(type));
        }
        NotifyUI();
    }

    public void TakeDamage(int damage)
    {
        for (int i = 0; i < damage && hearts.Count > 0; i++)
        {
            Heart top = hearts.Peek();
            top.hitsLeft--;

            if (top.hitsLeft <= 0)
            {
                hearts.Pop(); // POP: el corazón de la cima se rompe
                Debug.Log("Se rompió un corazón " + top.type);
            }
        }

        if (hearts.Count == 0)
        {
            Debug.Log("Game Over");
            Destroy(gameObject); // luego: respawn o pantalla de muerte
        }
        NotifyUI();
    }

    // PUSH: devuelve true si se pudo apilar (falso si la pila está llena)
    public bool PushHeart(HeartColor type)
    {
        if (hearts.Count >= maxHearts) return false;

        hearts.Push(new Heart(type));
        Debug.Log("PUSH: corazón " + type + " (vida total: " + TotalHits() + " golpes)");
        NotifyUI();
        return true;
    }

    private int TotalHits()
    {
        int total = 0;
        foreach (Heart heart in hearts) total += heart.hitsLeft;
        return total;
    }

    private void NotifyUI()
    {
        if (healthUI != null) healthUI.Refresh(hearts);
    }
}
