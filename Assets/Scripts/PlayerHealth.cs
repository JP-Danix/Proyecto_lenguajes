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

    [Header("Efectos al recibir daño")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.15f;      // flash fuerte al recibir el golpe
    public float invulnerabilityTime = 0.5f; // parpadeo + invulnerabilidad después
    public float knockbackForce = 4f;        // fuerza del retroceso
    public float knockbackDuration = 0.2f;   // cuánto tiempo el jugador no controla el movimiento

    private readonly Stack<Heart> hearts = new Stack<Heart>(); // la cima es el último apilado
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Color originalColor;
    private float invulnerabilityTimer;
    private float flashTimer;
    private float knockbackTimer;

    // El movimiento consulta esto para no pelear con el retroceso
    public bool IsKnockedBack => knockbackTimer > 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalColor = spriteRenderer.color;
        rb = GetComponent<Rigidbody2D>();

        // Apila los corazones iniciales: el último de la lista queda en la cima
        foreach (HeartColor type in initialHearts)
        {
            hearts.Push(new Heart(type));
        }
        NotifyUI();
    }

    void Update()
    {
        if (invulnerabilityTimer > 0f) invulnerabilityTimer -= Time.deltaTime;
        if (knockbackTimer > 0f) knockbackTimer -= Time.deltaTime;

        // Color del sprite: flash fuerte -> parpadeo -> color normal
        if (flashTimer > 0f)
        {
            flashTimer -= Time.deltaTime;
            spriteRenderer.color = flashColor;
        }
        else if (invulnerabilityTimer > 0f)
        {
            spriteRenderer.color = Mathf.Repeat(invulnerabilityTimer * 10f, 1f) > 0.5f
                ? flashColor
                : originalColor;
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (invulnerabilityTimer > 0f) return; // invulnerable: ignora el golpe

        for (int i = 0; i < damage && hearts.Count > 0; i++)
        {
            Heart top = hearts.Peek();
            top.hitsLeft--;
            Debug.Log("Golpe al corazón " + top.type + " (le quedan " + top.hitsLeft + " golpes)");

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
            return;
        }

        invulnerabilityTimer = invulnerabilityTime;
        flashTimer = flashDuration;

        // Retroceso: empuja al jugador en la dirección del golpe
        if (rb != null && hitDirection != Vector2.zero)
        {
            rb.linearVelocity = hitDirection * knockbackForce;
            knockbackTimer = knockbackDuration;
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
