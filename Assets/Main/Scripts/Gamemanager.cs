using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Contenedor de la mano")]
    [SerializeField] private Transform contenedorMano;

    [Header("Prefab de carta")]
    [SerializeField] private GameObject prefabCarta;

    [Header("Botones")]
    [SerializeField] private Button botonRobar;
    [SerializeField] private Button botonDescartar;

    [Header("Configuración")]
    [SerializeField] private int cantidadCartasIniciales = 7;
    [SerializeField] private float separacionCartas = 340f;

    private const int CantidadDeCartas = 30;

    private int[] mazo = new int[CantidadDeCartas];
    private int[] mano = new int[CantidadDeCartas];
    private int[] descarte = new int[CantidadDeCartas];

    private int cantidadMazo;
    private int cantidadMano;
    private int cantidadDescarte;

    private GameObject[] cartasVisibles = new GameObject[CantidadDeCartas];
    private int cartaSeleccionada = -1;

    private void Start()
    {
        // Inicializar mazo: cada carta tiene un ID único (0..29)
        cantidadMazo = CantidadDeCartas;
        for (int idCarta = 0; idCarta < CantidadDeCartas; idCarta++)
            mazo[idCarta] = idCarta;

        MezclarMazo();

        if (botonRobar != null) botonRobar.onClick.AddListener(RobarCarta);
        if (botonDescartar != null) botonDescartar.onClick.AddListener(DescartarSeleccionada);

        for (int n = 0; n < cantidadCartasIniciales; n++)
            RobarCarta();
    }

    private void MezclarMazo()
    {
        for (int posicionActual = cantidadMazo - 1; posicionActual > 0; posicionActual--)
        {
            int posicionAleatoria = Random.Range(0, posicionActual + 1);
            (mazo[posicionActual], mazo[posicionAleatoria]) =
            (mazo[posicionAleatoria], mazo[posicionActual]);
        }
    }

    public void RobarCarta()
    {
        if (cantidadMazo == 0) RecuperarDescarte();
        if (cantidadMazo == 0 || cantidadMano >= CantidadDeCartas) return;

        mano[cantidadMano++] = mazo[--cantidadMazo];
        ActualizarCartas();
    }

    public void DescartarSeleccionada()
    {
        if (cartaSeleccionada < 0 || cartaSeleccionada >= cantidadMano || cantidadDescarte >= CantidadDeCartas)
            return;

        descarte[cantidadDescarte++] = mano[cartaSeleccionada];

        // Compactar la mano moviendo cada carta una posición a la izquierda
        for (int posicion = cartaSeleccionada; posicion < cantidadMano - 1; posicion++)
            mano[posicion] = mano[posicion + 1];

        cantidadMano--;
        cartaSeleccionada = -1;
        ActualizarCartas();
    }

    private void RecuperarDescarte()
    {
        if (cantidadDescarte == 0) return;

        for (int posicion = 0; posicion < cantidadDescarte; posicion++)
            mazo[posicion] = descarte[posicion];

        cantidadMazo = cantidadDescarte;
        cantidadDescarte = 0;
        MezclarMazo();
    }

    private void ActualizarCartas()
    {
        for (int indice = 0; indice < cartasVisibles.Length; indice++)
        {
            if (cartasVisibles[indice] != null) Destroy(cartasVisibles[indice]);
            cartasVisibles[indice] = null;
        }

        if (prefabCarta == null || contenedorMano == null) return;

        for (int indiceCarta = 0; indiceCarta < cantidadMano; indiceCarta++)
        {
            GameObject carta = Instantiate(prefabCarta, contenedorMano);
            cartasVisibles[indiceCarta] = carta;
            carta.name = "Carta_" + indiceCarta;

            if (carta.TryGetComponent<RectTransform>(out var rect))
            {
                rect.sizeDelta = new Vector2(100f, 150f);
                rect.anchoredPosition = new Vector2(
                    CalcularPosicionX(indiceCarta), 0f);
            }

            if (carta.TryGetComponent<Button>(out var botonCarta))
            {
                int indiceCapturado = indiceCarta; // evita el bug de closure
                botonCarta.onClick.RemoveAllListeners();
                botonCarta.onClick.AddListener(() => SeleccionarCarta(indiceCapturado));
            }

            if (carta.TryGetComponent<Image>(out var imagen))
                imagen.color = Color.white;
        }

        ActualizarSeleccionVisual();
    }

    private void SeleccionarCarta(int indice)
    {
        if (indice < 0 || indice >= cantidadMano) return;
        cartaSeleccionada = indice;
        ActualizarSeleccionVisual();
    }

    private void ActualizarSeleccionVisual()
    {
        for (int indiceCarta = 0; indiceCarta < cantidadMano; indiceCarta++)
        {
            if (cartasVisibles[indiceCarta] == null) continue;
            if (cartasVisibles[indiceCarta].TryGetComponent<RectTransform>(out var rect))
            {
                rect.anchoredPosition = new Vector2(
                    CalcularPosicionX(indiceCarta),
                    indiceCarta == cartaSeleccionada ? 30f : 0f);
            }
        }
    }

    // Calcula la posición X centrada de una carta según su índice en la mano
    private float CalcularPosicionX(int indiceCarta)
    {
        return (indiceCarta - (cantidadMano - 1) / 2f) * separacionCartas;
    }

    private void OnDestroy()
    {
        if (botonRobar != null) botonRobar.onClick.RemoveListener(RobarCarta);
        if (botonDescartar != null) botonDescartar.onClick.RemoveListener(DescartarSeleccionada);
    }
}