using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

// El AFN-lambda de los mapas: el traspaso de niveles modelado como autómata
// finito no determinista con transiciones espontáneas.
//
//   Q  = las escenas de Unity (cada nivel es un estado)
//   Σ  = las letras del alfabeto (llave, moneda): los pickups las escriben en la
//        cinta del jugador y una puerta lectora (las casas) las lee al cruzarla.
//        El no determinismo (qué puerta tomar) lo resuelve el jugador caminando.
//   δ  = la función de transición POR LETRA, serializada: (estadoOrigen, letra) ->
//        (escenaDestino, spawnId). La dispara la puerta lectora al leer la cinta.
//   λ  = la lista de transiciones ESPONTÁNEAS (las torres): cambian de estado sin
//        leer nada. Cada una lleva un Id Puerta interno para distinguirlas en la
//        serialización — no es una letra de Σ.
//   q0 = la escena inicial
//   F  = el nivel final: llegar a él acepta la partida (el recorrido entero es una cadena de Σ*)
//
// Una entrada (letra o puerta) sin transición definida desde el estado actual es
// RECHAZADA: el autómata no tiene δ para ella y la ignora (con log) — el mismo
// rechazo que el PDA de la vida. El jugador persiste entre niveles
// (DontDestroyOnLoad): el PDA de vida sobrevive mientras este autómata cambia de
// estado; la cinta, en cambio, se reinicia en cada estado.
public class MapManager : MonoBehaviour
{
    public static MapManager Instance { get; private set; }

    // El jugador persistente que este autómata globalizó (null hasta la primera
    // carga de escena). PlayerHealth lo consulta para saber si es el sobreviviente:
    // las copias que trae cada escena no deben dibujar UI ni apilar Z0.
    public GameObject JugadorPersistente => jugador;

    [System.Serializable]
    public class TransicionLetra
    {
        public string estadoOrigen;  // la escena de la que se sale (un estado de Q)
        public string letra;         // la letra de Σ que la puerta lectora lee
        public string escenaDestino; // el estado destino (otra escena de Q)
        public string spawnId;       // el SpawnPoint donde aparece el jugador en la escena destino
    }

    [System.Serializable]
    public class TransicionLambda
    {
        public string estadoOrigen;  // la escena de la que se sale (un estado de Q)
        public string idPuerta;      // bookkeeping interno: qué puerta λ es (no es una letra de Σ)
        public string escenaDestino; // el estado destino (otra escena de Q)
        public string spawnId;       // el SpawnPoint donde aparece el jugador en la escena destino
    }

    [Header("δ: transiciones por letra (dispara la puerta lectora al leer la cinta)")]
    public List<TransicionLetra> transicionesLetra = new List<TransicionLetra>();

    [Header("λ: transiciones espontáneas (las torres; Id Puerta es bookkeeping, no letra de Σ)")]
    public List<TransicionLambda> transicionesLambda = new List<TransicionLambda>();

    [Header("Estado inicial q0 (nombre de la escena donde arranca el juego)")]
    public string estadoInicial;

    private Dictionary<(string, string), TransicionLetra> deltaLetra;     // índice (origen, letra) -> transición
    private Dictionary<(string, string), TransicionLambda> deltaLambda;   // índice (origen, idPuerta) -> transición λ
    private string estadoActual;
    private GameObject jugador;      // el jugador persistente (la pila viaja con él)
    private string spawnPendiente;   // el spawnId de la transición en curso

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // al recargar escenas, sobrevive solo el primero
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // "Compilar" ambas tablas serializadas a índices de consulta O(1)
        deltaLetra = new Dictionary<(string, string), TransicionLetra>();
        foreach (TransicionLetra t in transicionesLetra)
        {
            if (string.IsNullOrEmpty(t.estadoOrigen) || string.IsNullOrEmpty(t.letra)) continue;
            deltaLetra[(t.estadoOrigen, t.letra)] = t;
        }
        deltaLambda = new Dictionary<(string, string), TransicionLambda>();
        foreach (TransicionLambda t in transicionesLambda)
        {
            if (string.IsNullOrEmpty(t.estadoOrigen) || string.IsNullOrEmpty(t.idPuerta)) continue;
            deltaLambda[(t.estadoOrigen, t.idPuerta)] = t;
        }

        // q0: la escena donde vive el MapManager
        if (string.IsNullOrEmpty(estadoInicial)) estadoInicial = SceneManager.GetActiveScene().name;
        estadoActual = estadoInicial;

        SceneManager.sceneLoaded += AlCargarEscena;
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= AlCargarEscena;
    }

    // δ(estado, letra): la dispara la puerta lectora al leer la letra pendiente
    // en la cinta del jugador. Sin transición definida = rechazo.
    public bool Transitar(string letra, GameObject quien)
    {
        if (!deltaLetra.TryGetValue((estadoActual, letra), out TransicionLetra t))
        {
            Debug.LogWarning("Rechazo del AFN-lambda de mapas: no existe δ('" + estadoActual + "', '" + letra + "') — la puerta no lleva a ningún estado");
            return false;
        }
        Ejecutar(t.escenaDestino, t.spawnId, "δ('" + estadoActual + "', '" + letra + "')");
        return true;
    }

    // λ(estado): transición espontánea de una puerta libre. El Id Puerta solo
    // distingue la puerta en la serialización; formalmente no consume entrada.
    public bool TransitarLambda(string idPuerta)
    {
        if (!deltaLambda.TryGetValue((estadoActual, idPuerta), out TransicionLambda t))
        {
            Debug.LogWarning("Rechazo del AFN-lambda de mapas: no existe λ('" + estadoActual + "') para la puerta '" + idPuerta + "'");
            return false;
        }
        Ejecutar(t.escenaDestino, t.spawnId, "λ('" + estadoActual + "') → '" + idPuerta + "'");
        return true;
    }

    private void Ejecutar(string destino, string spawnId, string detalle)
    {
        Debug.Log("Transición del AFN-lambda de mapas: " + detalle + " = '" + destino + "'");
        estadoActual = destino;
        spawnPendiente = spawnId;
        SceneManager.LoadScene(destino);
    }

    // Acciones al entrar a un estado (escena): ubicar al jugador persistente en su
    // spawn y reconectar la cámara local. La escena nueva trae su propia copia del
    // jugador y de la UI: se descarta la copia, sobrevive el jugador global.
    private void AlCargarEscena(Scene escena, LoadSceneMode modo)
    {
        // El jugador persistente: el primero que se encuentra se vuelve global
        if (jugador == null)
        {
            PlayerHealth ph = FindFirstObjectByType<PlayerHealth>();
            if (ph != null)
            {
                jugador = ph.gameObject;
                DontDestroyOnLoad(jugador);
            }
        }

        // Deduplicación: descartar el jugador que vino con la escena nueva
        if (jugador != null)
        {
            foreach (PlayerHealth ph in FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None))
            {
                if (ph.gameObject != jugador) Destroy(ph.gameObject);
            }
        }

        // Cinta por estado: las letras no leídas se pierden al cambiar de estado
        if (jugador != null)
        {
            InventarioJugador inventario = jugador.GetComponent<InventarioJugador>();
            if (inventario != null) inventario.ReiniciarCinta();
        }

        // El orden de dibujo de la FSM de la colina es POR ESCENA, pero el jugador
        // persistente viaja con él: si salió del estado anterior estando "sobre" una
        // colina (subió y se fue por una puerta sin pasar por su HillExit), llega con
        // sortingOrder 15 y las colinas del estado nuevo dejan de taparlo. Al entrar
        // a un estado, la FSM de la colina arranca de cero: orden base 4 (HillExit
        // usa el mismo valor al bajar por la escalera). Si el spawn cae dentro de un
        // trigger de entrada, ese HillEntry vuelve a subirlo a 15 después.
        if (jugador != null)
        {
            SpriteRenderer sprite = jugador.GetComponent<SpriteRenderer>();
            if (sprite != null) sprite.sortingOrder = 4;
        }

        // Salida de la transición: colocar al jugador en el spawn que dictó
        if (jugador != null && !string.IsNullOrEmpty(spawnPendiente))
        {
            SpawnPoint spawn = EncontrarSpawn(spawnPendiente);
            if (spawn != null)
            {
                jugador.transform.position = spawn.transform.position;
            }
            else
            {
                Debug.LogWarning("No hay SpawnPoint con id '" + spawnPendiente + "' en '" + escena.name + "'");
            }
            spawnPendiente = null;
        }

        // La cámara de esta escena vuelve a seguir al jugador persistente
        if (jugador != null)
        {
            CinemachineCamera camara = FindFirstObjectByType<CinemachineCamera>();
            if (camara != null) camara.Target.TrackingTarget = jugador.transform;
        }
    }

    private SpawnPoint EncontrarSpawn(string id)
    {
        foreach (SpawnPoint sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
        {
            if (sp.id == id) return sp;
        }
        return null;
    }
}
