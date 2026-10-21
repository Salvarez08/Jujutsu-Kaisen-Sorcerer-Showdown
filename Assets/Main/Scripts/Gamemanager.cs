using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Contenedor de la mano")]
    [SerializeField] private Transform contenedorMano;

    [Header("Prefab de carta")]
    [SerializeField] private GameObject prefabCarta;

    [Header("Configuración del mazo")]
    [SerializeField] private int cantidadCartasIniciales = 5;
    [SerializeField] private int cartasPorRonda = 5;

    private const int CantidadDeCartas = 30;

    [Header("Distribución de la mano")]
    [SerializeField] private float anchoMano = 8f;
    [SerializeField] private float separacionMaxima = 1.5f;
    [SerializeField] private float separacionMinima = 0.5f;
    [SerializeField] private float desplazamientoSeleccion = 0.4f;

    public static GameManager Instance { get; private set; }

    private int[] mazo = new int[CantidadDeCartas];
    private int[] mano = new int[CantidadDeCartas];
    private int[] descarte = new int[CantidadDeCartas];

    private int cantidadMazo;
    private int cantidadMano;
    private int cantidadDescarte;

    private GameObject[] cartasVisibles = new GameObject[CantidadDeCartas];

    private int cartaSeleccionada = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        IniciarPartida();
    }

    // ============================================================
    //  API PÚBLICA PARA REINICIAR
    // ============================================================

    /// <summary>
    /// Reinicia todo: mazo mezclado, mano vacía, descarte vacío
    /// y reparte las cartas iniciales. No recarga la escena.
    /// </summary>
    public void IniciarPartida()
    {
        InicializarMazo();
        RepartirManoInicial();
    }

    /// <summary>
    /// Solo reinicia el mazo con las cartas actuales (mano + descarte),
    /// sin repartir. Útil para "barajar de nuevo" sin empezar partida.
    /// </summary>
    public void BarajarTodo()
    {
        // Juntar mano + descarte + lo que quede en mazo en un solo array
        int total = cantidadMano + cantidadDescarte + cantidadMazo;
        int[] todas = new int[total];
        int indice = 0;

        for (int i = 0; i < cantidadMano; i++) todas[indice++] = mano[i];
        for (int i = 0; i < cantidadDescarte; i++) todas[indice++] = descarte[i];
        for (int i = 0; i < cantidadMazo; i++) todas[indice++] = mazo[i];

        // Volver a llenar el mazo
        for (int i = 0; i < total; i++)
            mazo[i] = todas[i];

        cantidadMazo = total;
        cantidadMano = 0;
        cantidadDescarte = 0;
        cartaSeleccionada = -1;

        MezclarMazo();
        ActualizarCartas();
    }

    // ============================================================
    //  INICIALIZACIÓN
    // ============================================================

    private void InicializarMazo()
    {
        cantidadMazo = CantidadDeCartas;
        cantidadMano = 0;
        cantidadDescarte = 0;
        cartaSeleccionada = -1;

        for (int i = 0; i < CantidadDeCartas; i++)
            mazo[i] = i;

        MezclarMazo();
    }

    private void RepartirManoInicial()
    {
        for (int i = 0; i < cantidadCartasIniciales; i++)
            RobarCarta();
    }

    // ============================================================
    //  LÓGICA DE MAZO / MANO / DESCARTE
    // ============================================================

    private void MezclarMazo()
    {
        for (int i = cantidadMazo - 1; i > 0; i--)
        {
            int aleatorio = Random.Range(0, i + 1);
            (mazo[i], mazo[aleatorio]) = (mazo[aleatorio], mazo[i]);
        }
    }

    public void RobarCarta()
    {
        if (cantidadMazo == 0)
            RecuperarDescarte();

        if (cantidadMazo == 0 || cantidadMano >= CantidadDeCartas)
            return;

        mano[cantidadMano++] = mazo[--cantidadMazo];
        ActualizarCartas();
    }

    public void DescartarSeleccionada()
    {
        if (cartaSeleccionada < 0 ||
            cartaSeleccionada >= cantidadMano ||
            cantidadDescarte >= CantidadDeCartas)
            return;

        descarte[cantidadDescarte++] = mano[cartaSeleccionada];

        for (int i = cartaSeleccionada; i < cantidadMano - 1; i++)
            mano[i] = mano[i + 1];

        cantidadMano--;
        cartaSeleccionada = -1;

        ActualizarCartas();
    }

    private void RecuperarDescarte()
    {
        if (cantidadDescarte == 0)
            return;

        for (int i = 0; i < cantidadDescarte; i++)
            mazo[i] = descarte[i];

        cantidadMazo = cantidadDescarte;
        cantidadDescarte = 0;

        MezclarMazo();
    }

    // ============================================================
    //  VISUAL
    // ============================================================

    private void ActualizarCartas()
    {
        for (int i = 0; i < cartasVisibles.Length; i++)
        {
            if (cartasVisibles[i] != null)
                Destroy(cartasVisibles[i]);

            cartasVisibles[i] = null;
        }

        if (prefabCarta == null || contenedorMano == null)
        {
            Debug.LogWarning("GameManager: asigna el prefab y el contenedor de la mano.");
            return;
        }

        for (int i = 0; i < cantidadMano; i++)
        {
            GameObject carta = Instantiate(prefabCarta, contenedorMano);

            cartasVisibles[i] = carta;
            carta.name = "Carta_" + i;

            if (carta.TryGetComponent<Carta>(out var scriptCarta))
                scriptCarta.indiceEnMano = i;

            if (carta.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
                spriteRenderer.sortingOrder = i;
        }

        ActualizarSeleccionVisual();
    }

    public void SeleccionarCarta(int indice)
    {
        if (indice < 0 || indice >= cantidadMano)
            return;

        cartaSeleccionada = indice;
        ActualizarSeleccionVisual();
    }

    private void ActualizarSeleccionVisual()
    {
        if (contenedorMano == null)
            return;

        for (int i = 0; i < cantidadMano; i++)
        {
            if (cartasVisibles[i] == null)
                continue;

            cartasVisibles[i].transform.localPosition = new Vector3(
                CalcularPosicionX(i),
                i == cartaSeleccionada ? desplazamientoSeleccion : 0f,
                0f
            );
        }
    }

    private float CalcularPosicionX(int indiceCarta)
    {
        if (cantidadMano <= 1)
            return 0f;

        float separacion = Mathf.Clamp(
            anchoMano / (cantidadMano - 1),
            separacionMinima,
            separacionMaxima
        );

        return (indiceCarta - (cantidadMano - 1) / 2f) * separacion;
    }

    // ============================================================
    //  RONDAS
    // ============================================================

    public void TerminarRonda()
    {
        if (cantidadMano == 0)
            return;

        for (int i = 0; i < cantidadMano; i++)
        {
            if (cantidadDescarte >= CantidadDeCartas)
                break;

            descarte[cantidadDescarte++] = mano[i];
        }

        cantidadMano = 0;
        cartaSeleccionada = -1;

        ActualizarCartas();

        for (int i = 0; i < cartasPorRonda; i++)
        {
            RobarCarta();

            if (cantidadMano >= cartasPorRonda)
                break;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}