using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Configuración del mazo")]
    [SerializeField] private int cantidadCartasIniciales = 5;
    [SerializeField] private int cartasPorRonda = 5;

    private const int CantidadDeCartas = 30;

    public static GameManager Instance { get; private set; }

    // Eventos para que la interfaz se actualice
    public static event Action OnManoCambio;       // cambió la mano, el mazo o el descarte
    public static event Action OnSeleccionCambio;  // cambió solo la carta seleccionada

    private int[] mazo = new int[CantidadDeCartas];
    private int[] mano = new int[CantidadDeCartas];
    private int[] descarte = new int[CantidadDeCartas];

    private int cantidadMazo;
    private int cantidadMano;
    private int cantidadDescarte;

    private int cartaSeleccionada = -1;

    // Lectura del estado (para la interfaz)
    public int CantidadMazo => cantidadMazo;
    public int CantidadMano => cantidadMano;
    public int CantidadDescarte => cantidadDescarte;
    public int CartaSeleccionada => cartaSeleccionada;

    // Número de carta (0 a 29) que hay en una posición de la mano
    public int IdCartaEnMano(int indice)
    {
        return mano[indice];
    }

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
        if (Instance != this)
            return;

        IniciarPartida();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    // ============================================================
    //  API PÚBLICA (botones y otros scripts)
    // ============================================================

    public void IniciarPartida()
    {
        cantidadMazo = CantidadDeCartas;
        cantidadMano = 0;
        cantidadDescarte = 0;
        cartaSeleccionada = -1;

        for (int i = 0; i < CantidadDeCartas; i++)
            mazo[i] = i;

        MezclarMazo();

        for (int i = 0; i < cantidadCartasIniciales; i++)
            RobarInterno();

        OnManoCambio?.Invoke();
    }

    public void BarajarTodo()
    {
        int total = cantidadMano + cantidadDescarte + cantidadMazo;
        int[] todas = new int[total];
        int indice = 0;

        for (int i = 0; i < cantidadMano; i++) todas[indice++] = mano[i];
        for (int i = 0; i < cantidadDescarte; i++) todas[indice++] = descarte[i];
        for (int i = 0; i < cantidadMazo; i++) todas[indice++] = mazo[i];

        for (int i = 0; i < total; i++)
            mazo[i] = todas[i];

        cantidadMazo = total;
        cantidadMano = 0;
        cantidadDescarte = 0;
        cartaSeleccionada = -1;

        MezclarMazo();
        OnManoCambio?.Invoke();
    }

    public void RobarCarta()
    {
        RobarInterno();
        OnManoCambio?.Invoke();
    }

    public void SeleccionarCarta(int indice)
    {
        if (indice < 0 || indice >= cantidadMano)
            return;

        cartaSeleccionada = indice;
        OnSeleccionCambio?.Invoke();
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

        OnManoCambio?.Invoke();
    }

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

        for (int i = 0; i < cartasPorRonda; i++)
        {
            if (!RobarInterno())
                break;
        }

        OnManoCambio?.Invoke();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // ============================================================
    //  MECÁNICAS INTERNAS
    // ============================================================

    private bool RobarInterno()
    {
        if (cantidadMazo == 0)
            RecuperarDescarte();

        if (cantidadMazo == 0 || cantidadMano >= CantidadDeCartas)
            return false;

        mano[cantidadMano++] = mazo[--cantidadMazo];
        return true;
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

    private void MezclarMazo()
    {
        for (int i = cantidadMazo - 1; i > 0; i--)
        {
            int aleatorio = UnityEngine.Random.Range(0, i + 1);
            (mazo[i], mazo[aleatorio]) = (mazo[aleatorio], mazo[i]);
        }
    }
}