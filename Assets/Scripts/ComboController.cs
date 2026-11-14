using System.Collections.Generic;
using UnityEngine;

// Sistema de combos como GRAMÁTICA DE TRADUCCIÓN (tipo 3 de Chomsky + tabla de símbolos):
// - Sintaxis: toda cadena tiene la forma  DIRECCIÓN · COMBO · ⊢
//     * El PRIMER símbolo es la dirección del lanzamiento (flechas o WASD): fija hacia dónde
//       vuela el proyectil y viaja DENTRO de la cadena, como el resto.
//     * Le siguen los símbolos del combo (letras serializadas en el Inspector).
//     * El delimitador ⊢ (Espacio) cierra la cadena y dispara la aceptación.
//   Cada derivación termina en un SÍMBOLO SEMÁNTICO que viaja en la cadena:
//       S ⇒ w S_w ⇒ wb A_w ⇒ wbb B_w ⇒ wbbn⊢fuego↑
//   El efecto es, así, función EXCLUSIVA de la cadena aceptada — sin estado externo.
// - Compilación: en Awake(), las producciones se compilan a un trie determinista (el AFD de
//   Kleene). Como la dirección se elige al inicio, el trie del combo se construye en 4 copias
//   (una por dirección), y cada copia termina en su propio estado de aceptación que recuerda
//   la dirección elegida. El delimitador ⊢ se agrega automáticamente al final de cada producción.
// - Semántica: una TABLA DE SÍMBOLOS traduce el terminal semántico a su acción en el mundo
//   (prefab + daño). Sintaxis y semántica son capas separadas: una cadena aceptada cuyo
//   terminal no está en la tabla es un "error semántico" — válida pero sin efecto.
// - No hay lógica temporal en el reconocedor: cadenas finitas cerradas por el jugador con Espacio.
public class ComboController : MonoBehaviour
{
    // El símbolo delimitador ⊢ del alfabeto: cierra la cadena y dispara el reconocimiento
    public const KeyCode DELIMITADOR = KeyCode.Space;

    [System.Serializable]
    public class ComboRule
    {
        public string nombre;        // para logs (y el README)
        public KeyCode[] secuencia;  // los símbolos del combo (sin dirección ni delimitador)
        public string simboloSalida; // terminal semántico con el que termina la derivación (ej. "fuego")
    }

    [System.Serializable]
    public class AccionCombo
    {
        public string simbolo;       // el terminal semántico (ej. "fuego")
        public GameObject proyectil; // qué instancia en el mundo
        public int danio = 1;        // cuánto daño produce
    }

    // Los símbolos de dirección del alfabeto (flechas o WASD producen estos símbolos lógicos)
    // y el vector de vuelo que cada uno determina. Van emparejados por índice.
    private static readonly KeyCode[] SIMBOLOS_DIRECCION =
    {
        KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow
    };
    private static readonly Vector2[] VECTORES_DIRECCION =
    {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    [Header("Gramática de combos (producciones sintácticas)")]
    public List<ComboRule> combos;

    [Header("Tabla de símbolos (capa semántica: terminal -> acción)")]
    public List<AccionCombo> acciones = new List<AccionCombo>();

    // Una clase Nodo es un estado del autómata compilado
    private class Nodo
    {
        public readonly Dictionary<KeyCode, Nodo> transiciones = new Dictionary<KeyCode, Nodo>();
        public string simboloSalida; // != null en los estados que cierran una derivación
        public Vector2 direccion;    // la dirección elegida con el primer símbolo de la cadena
    }

    private Nodo raiz;
    private Nodo estadoActual;
    private Dictionary<string, AccionCombo> tabla;

    void Awake()
    {
        // Índice de la tabla de símbolos: consulta O(1) al aceptar una cadena
        tabla = new Dictionary<string, AccionCombo>();
        foreach (AccionCombo accion in acciones)
        {
            if (!string.IsNullOrEmpty(accion.simbolo) && !tabla.ContainsKey(accion.simbolo))
            {
                tabla[accion.simbolo] = accion;
            }
        }

        CompilarGramatica();
    }

    // "Compilar" las producciones al autómata. Como la dirección es el primer símbolo de la
    // cadena y debe recordarse hasta la aceptación, el camino de cada producción se construye
    // en 4 copias (una por dirección); cada copia comparte prefijos con las demás reglas de esa
    // misma dirección y termina en un estado de aceptación que guarda (terminal, dirección).
    private void CompilarGramatica()
    {
        raiz = new Nodo();
        estadoActual = raiz;

        foreach (ComboRule regla in combos)
        {
            if (regla.secuencia == null || regla.secuencia.Length == 0) continue; // producción vacía: se ignora

            for (int i = 0; i < SIMBOLOS_DIRECCION.Length; i++)
            {
                // La copia de esta regla para la dirección i arranca en la transición dir -> ...
                if (!raiz.transiciones.TryGetValue(SIMBOLOS_DIRECCION[i], out Nodo inicio))
                {
                    inicio = new Nodo();
                    raiz.transiciones[SIMBOLOS_DIRECCION[i]] = inicio;
                }

                Nodo actual = inicio;
                foreach (KeyCode simbolo in regla.secuencia)
                {
                    if (!actual.transiciones.TryGetValue(simbolo, out Nodo siguiente))
                    {
                        siguiente = new Nodo();
                        actual.transiciones[simbolo] = siguiente;
                    }
                    actual = siguiente;
                }

                // Si el usuario ya puso el delimitador al final, se respeta; si no, el compilador lo agrega
                if (regla.secuencia[regla.secuencia.Length - 1] != DELIMITADOR)
                {
                    Nodo cierre = new Nodo();
                    actual.transiciones[DELIMITADOR] = cierre;
                    actual = cierre;
                }

                // Estado de aceptación de esta copia: recuerda el terminal y la dirección
                actual.simboloSalida = regla.simboloSalida;
                actual.direccion = VECTORES_DIRECCION[i];
            }
        }
    }

    void Update()
    {
        KeyCode? simbolo = LeerSimbolo();
        if (simbolo == null) return;

        // Transición del autómata. Símbolo no esperado (incluye una letra sin dirección previa
        // o un ⊢ a media cadena): cadena rechazada; se reintenta desde el estado inicial
        // con ese mismo símbolo.
        if (!estadoActual.transiciones.TryGetValue(simbolo.Value, out Nodo siguiente))
        {
            estadoActual = raiz;
            if (!raiz.transiciones.TryGetValue(simbolo.Value, out siguiente)) return;
        }

        estadoActual = siguiente;

        // Aceptación: la cadena es válida -> pasa a la capa semántica (tabla de símbolos)
        if (estadoActual.simboloSalida != null)
        {
            Ejecutar(estadoActual.simboloSalida, estadoActual.direccion);
            estadoActual = raiz; // la cadena se consumió completa
        }
    }

    // El alfabeto físico: el delimitador (Espacio), los símbolos de dirección
    // (flechas o WASD, normalizados a símbolos de flecha) y las letras A-Z
    private KeyCode? LeerSimbolo()
    {
        // El delimitador de la gramática
        if (Input.GetKeyDown(KeyCode.Space)) return DELIMITADOR;

        // Símbolos de dirección: flechas o WASD producen el mismo símbolo lógico
        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) return KeyCode.UpArrow;
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) return KeyCode.DownArrow;
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) return KeyCode.LeftArrow;
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) return KeyCode.RightArrow;

        // Letras A-Z: permiten definir combos con cualquier tecla del alfabeto.
        // Ojo: W, A, S y D están RESERVADAS como símbolos de dirección (el bloque anterior
        // las intercepta), así que no producen símbolo de letra: una producción que las use
        // como letras de combo sería muerta (inalcanzable). El alfabeto efectivo de letras
        // es A-Z menos {W, A, S, D}.
        for (int i = (int)KeyCode.A; i <= (int)KeyCode.Z; i++)
        {
            KeyCode letra = (KeyCode)i;
            if (letra == KeyCode.W || letra == KeyCode.A || letra == KeyCode.S || letra == KeyCode.D) continue; // reservadas
            if (Input.GetKeyDown(letra)) return letra;
        }

        return null;
    }

    // Capa semántica: la cadena ya fue validada por la sintaxis y trae consigo la dirección
    // (su primer símbolo) y el terminal semántico. La tabla de símbolos decide qué significa:
    // un terminal sin entrada en la tabla es un error semántico — cadena válida, sin efecto.
    private void Ejecutar(string terminal, Vector2 direccion)
    {
        if (string.IsNullOrEmpty(terminal)) return;

        if (!tabla.TryGetValue(terminal, out AccionCombo accion))
        {
            Debug.LogWarning("Error semántico: la cadena fue aceptada por la gramática, pero el terminal '" + terminal + "' no está en la tabla de símbolos (no produce acción).");
            return;
        }

        if (accion.proyectil == null) return;

        GameObject proyectil = Instantiate(
            accion.proyectil,
            (Vector2)transform.position + direccion.normalized * 0.5f,
            Quaternion.identity
        );

        proyectil.GetComponent<Projectile>()?.Lanzar(direccion, accion.danio);
        Debug.Log("Cadena aceptada: terminal '" + terminal + "', dirección " + direccion + " -> " + accion.proyectil.name);
    }
}
