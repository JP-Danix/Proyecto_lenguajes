using System.Collections.Generic;
using UnityEngine;

// Sistema de combos modelado como GRAMÁTICA REGULAR (tipo 3 de Chomsky, lineal por la derecha):
// - Alfabeto: símbolos de flecha (↑ ↓ ← →, leídos de flechas o WASD)
// - Producciones: cada ComboRule del Inspector es una cadena del lenguaje de combos
// - Compilación: en Awake(), las producciones se convierten en un autómata de
//   reconocimiento (trie determinista de prefijos) — gramática -> AFD
// - Salida: al aceptar una cadena se lanza el proyectil asociado (transición tipo Mealy)
public class ComboController : MonoBehaviour
{
    [System.Serializable]
    public class ComboRule
    {
        public string nombre;        // para logs (y el README)
        public KeyCode[] secuencia;  // la producción, símbolo por símbolo (usar flechas: Up/Down/Left/Right)
        public GameObject proyectil; // salida al aceptar la cadena completa
        public int danio = 1;
    }

    [Header("Gramática de combos (producciones)")]
    public List<ComboRule> combos;

    [Header("Referencias")]
    public Animator animator; // lee Xinput/Yinput para la dirección del lanzamiento

    [Header("Guarda temporal")]
    public float timeout = 0.4f; // sin teclas en X segundos, la cadena en progreso se cancela

    // Una clase Nodo es un estado del autómata compilado
    private class Nodo
    {
        public readonly Dictionary<KeyCode, Nodo> transiciones = new Dictionary<KeyCode, Nodo>();
        public ComboRule acepta; // != null si este estado es de aceptación (fin de una producción)
    }

    private Nodo raiz;
    private Nodo estadoActual;
    private float timeoutTimer;

    void Awake()
    {
        if (animator == null) animator = GetComponent<Animator>();
        CompilarGramatica();
    }

    // "Compilar" las producciones al autómata: cada cadena construye (o reutiliza)
    // el camino de estados correspondiente a sus prefijos — un trie determinista
    private void CompilarGramatica()
    {
        raiz = new Nodo();
        estadoActual = raiz;

        foreach (ComboRule regla in combos)
        {
            Nodo actual = raiz;
            foreach (KeyCode simbolo in regla.secuencia)
            {
                if (!actual.transiciones.TryGetValue(simbolo, out Nodo siguiente))
                {
                    siguiente = new Nodo();
                    actual.transiciones[simbolo] = siguiente;
                }
                actual = siguiente;
            }
            actual.acepta = regla;
        }
    }

    void Update()
    {
        // Guarda temporal: si expira, la cadena a medias se descarta (vuelta al estado inicial)
        if (timeoutTimer > 0f)
        {
            timeoutTimer -= Time.deltaTime;
        }
        else
        {
            estadoActual = raiz;
        }

        KeyCode? simbolo = LeerSimbolo();
        if (simbolo == null) return;

        timeoutTimer = timeout;

        // Transición del autómata. Símbolo no esperado: cadena rechazada;
        // se reintenta desde el estado inicial con ese mismo símbolo.
        if (!estadoActual.transiciones.TryGetValue(simbolo.Value, out Nodo siguiente))
        {
            estadoActual = raiz;
            if (!raiz.transiciones.TryGetValue(simbolo.Value, out siguiente)) return;
        }

        estadoActual = siguiente;

        // Estado de aceptación: la cadena es válida -> salida (proyectil)
        if (estadoActual.acepta != null)
        {
            LanzarProyectil(estadoActual.acepta);
            estadoActual = raiz; // la cadena se consumió completa
        }
    }

    // El alfabeto físico: flechas, WASD y letras A-Z producen el símbolo lógico correspondiente
    private KeyCode? LeerSimbolo()
    {
        // Flechas y WASD (normalizados: ambas formas producen el mismo símbolo)
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return KeyCode.UpArrow;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return KeyCode.DownArrow;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return KeyCode.LeftArrow;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return KeyCode.RightArrow;

        // Letras A-Z: permiten definir producciones con cualquier tecla del alfabeto
        for (int i = (int)KeyCode.A; i <= (int)KeyCode.Z; i++)
        {
            KeyCode letra = (KeyCode)i;
            if (Input.GetKeyDown(letra)) return letra;
        }

        return null;
    }

    private void LanzarProyectil(ComboRule regla)
    {
        if (regla.proyectil == null) return;

        // Dirección de lanzamiento: la última dirección de mira del jugador (igual que la espada)
        Vector2 dir = new Vector2(animator.GetFloat("Xinput"), animator.GetFloat("Yinput"));
        if (dir == Vector2.zero) dir = Vector2.down;

        GameObject proyectil = Instantiate(
            regla.proyectil,
            (Vector2)transform.position + dir.normalized * 0.5f,
            Quaternion.identity
        );

        proyectil.GetComponent<Projectile>()?.Lanzar(dir, regla.danio);
        Debug.Log("Combo '" + regla.nombre + "': cadena aceptada por la gramática -> " + regla.proyectil.name);
    }
}
