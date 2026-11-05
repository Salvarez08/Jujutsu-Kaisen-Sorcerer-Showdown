using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Configuración del mazo")]
    [SerializeField] private int cantidadCartasIniciales = 5;
    [SerializeField] private int cartasPorRonda = 5;

    [Header("Energía")]
    [SerializeField] private BaseDeCartas baseDeCartas;
    [SerializeField] private int energiaMaxima = 5;

    private const int CantidadDeCartas = 30;

    public static GameManager Instance { get; private set; }

    public static event Action OnManoCambio;
    public static event Action OnSeleccionCambio;
    public static event Action OnEnergiaCambio;

    private readonly int[] mazo = new int[CantidadDeCartas];
    private readonly int[] mano = new int[CantidadDeCartas];
    private readonly int[] descarte = new int[CantidadDeCartas];
    // Buffer reutilizable para evitar allocations en BarajarTodo
    private readonly int[] bufferBarajar = new int[CantidadDeCartas];

    private int cantidadMazo;
    private int cantidadMano;
    private int cantidadDescarte;

    private int cartaSeleccionada = -1;
    private int energiaActual;

    public int CantidadMazo => cantidadMazo;
    public int CantidadMano => cantidadMano;
    public int CantidadDescarte => cantidadDescarte;
    public int CartaSeleccionada => cartaSeleccionada;
    public int EnergiaActual => energiaActual;
    public int EnergiaMaxima => energiaMaxima;

    public int IdCartaEnMano(int indice) => mano[indice];

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
        if (Instance != this) return;
        IniciarPartida();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void IniciarPartida()
    {
        cantidadMazo = CantidadDeCartas;
        cantidadMano = 0;
        cantidadDescarte = 0;
        cartaSeleccionada = -1;
        energiaActual = energiaMaxima;

        for (int i = 0; i < CantidadDeCartas; i++)
            mazo[i] = i;

        MezclarMazo();

        for (int i = 0; i < cantidadCartasIniciales; i++)
            RobarInterno();

        OnManoCambio?.Invoke();
        OnEnergiaCambio?.Invoke();
    }

    public void BarajarTodo()
    {
        int total = cantidadMano + cantidadDescarte + cantidadMazo;

        // Array.Copy es memcpy interno: mucho más rápido que bucles manuales
        Array.Copy(mano, 0, bufferBarajar, 0, cantidadMano);
        Array.Copy(descarte, 0, bufferBarajar, cantidadMano, cantidadDescarte);
        Array.Copy(mazo, 0, bufferBarajar, cantidadMano + cantidadDescarte, cantidadMazo);
        Array.Copy(bufferBarajar, 0, mazo, 0, total);

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
        if (indice < 0 || indice >= cantidadMano) return;
        cartaSeleccionada = indice;
        OnSeleccionCambio?.Invoke();
    }

    public void JugarSeleccionada()
    {
        if (cartaSeleccionada < 0 || cartaSeleccionada >= cantidadMano)
            return;

        DatosCarta datos = baseDeCartas != null
            ? baseDeCartas.Obtener(mano[cartaSeleccionada])
            : null;

        int costo = datos != null ? datos.Costo : 0;

        // Sin energía suficiente, la carta no se juega
        if (costo > energiaActual)
            return;

        energiaActual -= costo;
        DescartarSeleccionada();   // la manda al descarte y avisa a la UI
        OnEnergiaCambio?.Invoke();
    }
    public void DescartarSeleccionada()
    {
        if (cartaSeleccionada < 0 ||
            cartaSeleccionada >= cantidadMano ||
            cantidadDescarte >= CantidadDeCartas)
            return;

        descarte[cantidadDescarte++] = mano[cartaSeleccionada];

        // Shift de la mano usando Array.Copy en lugar de bucle
        int restantes = cantidadMano - cartaSeleccionada - 1;
        if (restantes > 0)
            Array.Copy(mano, cartaSeleccionada + 1, mano, cartaSeleccionada, restantes);

        cantidadMano--;
        cartaSeleccionada = -1;

        OnManoCambio?.Invoke();
    }

    public void TerminarRonda()
    {
        if (cantidadMano == 0) return;

        int espacio = CantidadDeCartas - cantidadDescarte;
        int copiar = cantidadMano < espacio ? cantidadMano : espacio;

        if (copiar > 0)
        {
            Array.Copy(mano, 0, descarte, cantidadDescarte, copiar);
            cantidadDescarte += copiar;
        }

        cantidadMano = 0;
        cartaSeleccionada = -1;

        for (int i = 0; i < cartasPorRonda; i++)
        {
            if (!RobarInterno()) break;
        }

        // Si quieres que la energía se recargue cada ronda, descomenta:
        // energiaActual = energiaMaxima;
        // OnEnergiaCambio?.Invoke();

        OnManoCambio?.Invoke();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

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
        if (cantidadDescarte == 0) return;

        Array.Copy(descarte, 0, mazo, 0, cantidadDescarte);
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