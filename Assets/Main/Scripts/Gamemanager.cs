using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private const int CantidadDeCartas = 30;

    [Header("Configuración del mazo")]
    [SerializeField] private int cantidadCartasIniciales = 5;
    [SerializeField] private int cartasPorRonda = 5;

    [Header("Energía")]
    [SerializeField] private BaseDeCartas baseDeCartas;
    [SerializeField] private int energiaMaxima = 5;

    public static GameManager Instance { get; private set; }

    public static event Action OnManoCambio;
    public static event Action OnSeleccionCambio;
    public static event Action OnEnergiaCambio;

    private readonly int[] mazo = new int[CantidadDeCartas];
    private readonly int[] mano = new int[CantidadDeCartas];
    private readonly int[] descarte = new int[CantidadDeCartas];
    private readonly int[] bufferBarajar = new int[CantidadDeCartas];

    public int cantidadMazo { get; private set; }
    public int cantidadMano { get; private set; }
    public int cantidadDescarte { get; private set; }
    public int cartaSeleccionada { get; private set; } = -1;
    public int energiaActual { get; private set; }

    public int EnergiaMaxima => energiaMaxima;

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
        if (Instance == this)
            IniciarPartida();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
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
        {
            if (!RobarInterno())
                break;
        }

        OnManoCambio?.Invoke();
        OnSeleccionCambio?.Invoke();
        OnEnergiaCambio?.Invoke();
    }

    public void BarajarTodo()
    {
        int totalCartas = cantidadMano + cantidadDescarte + cantidadMazo;

        Array.Copy(mano, 0, bufferBarajar, 0, cantidadMano);
        Array.Copy(descarte, 0, bufferBarajar, cantidadMano, cantidadDescarte);
        Array.Copy(
            mazo,
            0,
            bufferBarajar,
            cantidadMano + cantidadDescarte,
            cantidadMazo
        );

        Array.Copy(bufferBarajar, 0, mazo, 0, totalCartas);

        cantidadMazo = totalCartas;
        cantidadMano = 0;
        cantidadDescarte = 0;
        cartaSeleccionada = -1;

        MezclarMazo();

        OnManoCambio?.Invoke();
        OnSeleccionCambio?.Invoke();
    }

    public void RobarCarta()
    {
        if (RobarInterno())
            OnManoCambio?.Invoke();
    }

    public void SeleccionarCarta(int indice)
    {
        if (indice < 0 || indice >= cantidadMano)
            return;

        if (cartaSeleccionada == indice)
            return;

        cartaSeleccionada = indice;
        OnSeleccionCambio?.Invoke();
    }

    public void JugarSeleccionada()
    {
        if (cartaSeleccionada < 0 || cartaSeleccionada >= cantidadMano)
            return;

        if (baseDeCartas == null)
            return;

        DatosCarta datos = baseDeCartas.Obtener(mano[cartaSeleccionada]);

        if (datos == null || datos.Costo > energiaActual)
            return;

        energiaActual -= datos.Costo;
        DescartarSeleccionada();
        OnEnergiaCambio?.Invoke();
    }

    public void DescartarSeleccionada()
    {
        int indice = cartaSeleccionada;

        if (indice < 0 ||
            indice >= cantidadMano ||
            cantidadDescarte >= CantidadDeCartas)
        {
            return;
        }

        descarte[cantidadDescarte] = mano[indice];
        cantidadDescarte++;

        int cartasADesplazar = cantidadMano - indice - 1;

        if (cartasADesplazar > 0)
        {
            Array.Copy(
                mano,
                indice + 1,
                mano,
                indice,
                cartasADesplazar
            );
        }

        cantidadMano--;
        cartaSeleccionada = -1;

        OnManoCambio?.Invoke();
        OnSeleccionCambio?.Invoke();
    }

    public void TerminarRonda()
    {
        if (cantidadMano > 0)
        {
            Array.Copy(
                mano,
                0,
                descarte,
                cantidadDescarte,
                cantidadMano
            );

            cantidadDescarte += cantidadMano;
            cantidadMano = 0;
        }

        cartaSeleccionada = -1;

        for (int i = 0; i < cartasPorRonda; i++)
        {
            if (!RobarInterno())
                break;
        }

        energiaActual = energiaMaxima;

        OnManoCambio?.Invoke();
        OnSeleccionCambio?.Invoke();
        OnEnergiaCambio?.Invoke();
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public bool RobarInterno()
    {
        if (cantidadMazo == 0)
            RecuperarDescarte();

        if (cantidadMazo == 0 || cantidadMano >= CantidadDeCartas)
            return false;

        cantidadMazo--;
        mano[cantidadMano] = mazo[cantidadMazo];
        cantidadMano++;

        return true;
    }

    public void RecuperarDescarte()
    {
        if (cantidadMazo > 0 || cantidadDescarte == 0)
            return;

        Array.Copy(descarte, 0, mazo, 0, cantidadDescarte);

        cantidadMazo = cantidadDescarte;
        cantidadDescarte = 0;

        MezclarMazo();
    }

    public void MezclarMazo()
    {
        for (int indice = cantidadMazo - 1; indice > 0; indice--)
        {
            int indiceAleatorio = UnityEngine.Random.Range(0, indice + 1);

            (mazo[indice], mazo[indiceAleatorio]) =
                (mazo[indiceAleatorio], mazo[indice]);
        }
    }
}