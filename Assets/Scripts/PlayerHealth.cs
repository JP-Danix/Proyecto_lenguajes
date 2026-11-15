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
// - PUSH: recoger un corazón (queda en la cima); la pila es ILIMITADA (sin tope de profundidad),
//   así que el número de configuraciones es infinito y el sistema NO se puede aplanar a un AFD:
//   con corazones de 1 golpe, sobrevivir exige #recoger >= #daño en todo prefijo de la cadena
//   de eventos (prefijos balanceados), un lenguaje que el lema del bombeo demuestra no regular.
// - POP: el corazón de la cima se agota por el daño
// - Configuración inicial: (q0, Z0) — la pila arranca con el único símbolo Z0 (corazón rojo)
// - Rechazo: daño con pila vacía = no existe la transición δ(q, daño, ε) = Game Over
public class PlayerHealth : MonoBehaviour
{
    [Header("Configuración inicial del PDA")]
    public HeartColor Z0 = HeartColor.Rojo; // símbolo inicial de pila: la configuración inicial es (q0, Z0)
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

        // Auto-configuración para prefabs: si no hay UI asignada, busca la única
        // HealthUI de la escena (así el prefab del jugador funciona en cualquier nivel)
        if (healthUI == null)
        {
            healthUI = FindFirstObjectByType<HealthUI>();
        }

        // Configuración inicial (q0, Z0): la pila arranca con el símbolo inicial Z0,
        // exactamente un elemento, como exige la definición del PDA. La serialización
        // es un solo HeartColor (no una lista): no se puede expresar una pila pre-cargada.
        hearts.Push(new Heart(Z0));
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

        // Cada unidad de daño es un símbolo de entrada. El rechazo NO es "quedarse
        // sin pila" (el POP que vacía la pila deja vivo al jugador, y un recoger lo
        // recupera: δ(q, recoger, ε) existe): es recibir daño cuando la pila YA está
        // vacía — δ(q, daño, ε) no existe, así que esa lectura muere en el acto.
        for (int i = 0; i < damage; i++)
        {
            if (hearts.Count == 0)
            {
                Debug.Log("Game Over");
                Destroy(gameObject); // luego: respawn o pantalla de muerte
                return;
            }

            Heart top = hearts.Peek();
            top.hitsLeft--;
            Debug.Log("Golpe al corazón " + top.type + " (le quedan " + top.hitsLeft + " golpes)");

            if (top.hitsLeft <= 0)
            {
                hearts.Pop(); // POP: el corazón de la cima se rompe
                Debug.Log("Se rompió un corazón " + top.type + (hearts.Count == 0 ? " (pila vacía: vivo, pero el próximo daño es rechazo)" : ""));
            }
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

    // PUSH: la pila es ilimitada, así que siempre se puede apilar
    public void PushHeart(HeartColor type)
    {
        hearts.Push(new Heart(type));
        Debug.Log("PUSH: corazón " + type + " (profundidad de la pila: " + hearts.Count + ", vida total: " + TotalHits() + " golpes)");
        NotifyUI();
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
