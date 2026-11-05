using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Prefab y contenedor")]
    [SerializeField] private Transform contenedorMano;
    [SerializeField] private GameObject prefabCarta;
    [SerializeField] private BaseDeCartas baseDeCartas;

    [Header("Distribución de la mano")]
    [SerializeField] private float anchoMano = 8f;
    [SerializeField] private float separacionMaxima = 1.5f;
    [SerializeField] private float separacionMinima = 0.5f;

    [Header("Abanico")]
    [SerializeField] private float curvatura = 0.12f;
    [SerializeField] private float anguloPorCarta = 4f;

    [Header("Selección")]
    [SerializeField] private float desplazamientoSeleccion = 0.4f;
    [SerializeField] private Vector3 escalaNormal = Vector3.one;
    [SerializeField] private Vector3 escalaSeleccionada = new Vector3(1.2f, 1.2f, 1f);

    [Header("Orden de dibujo")]
    [SerializeField] private int ordenBase = 0;
    [SerializeField] private int ordenSeleccionada = 100;
    [SerializeField] private int ordenTextoSobreSprite = 5;

    [Header("Energía maldita")]
    [SerializeField] private TMP_Text textoEnergia;


    private readonly List<GameObject> cartas = new List<GameObject>();

    private void OnEnable()
    {
        GameManager.OnManoCambio += RedibujarMano;
        GameManager.OnSeleccionCambio += Distribuir;
        GameManager.OnEnergiaCambio += ActualizarEnergia;

        ActualizarEnergia(); // por si el GameManager ya arrancó
    }

    private void OnDisable()
    {
        GameManager.OnManoCambio -= RedibujarMano;
        GameManager.OnSeleccionCambio -= Distribuir;
        GameManager.OnEnergiaCambio -= ActualizarEnergia;
    }
    private void RedibujarMano()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        if (prefabCarta == null || contenedorMano == null)
        {
            Debug.LogWarning("UIManager: asigna el prefab y el contenedor de la mano.");
            return;
        }

        // for en lugar de foreach (List<T> foreach asigna enumerador)
        for (int i = 0; i < cartas.Count; i++)
        {
            if (cartas[i] != null)
                Destroy(cartas[i]);
        }
        cartas.Clear();

        for (int i = 0; i < gm.CantidadMano; i++)
        {
            GameObject obj = Instantiate(prefabCarta, contenedorMano);
            obj.name = "Carta_" + i;

            Carta carta = obj.GetComponent<Carta>();
            if (carta == null)
                carta = obj.AddComponent<Carta>();

            carta.Inicializar(i, this);

            int id = gm.IdCartaEnMano(i);
            DatosCarta datos = ObtenerDatos(id);

            CardView vista = obj.GetComponentInChildren<CardView>(true);
            if (vista != null && datos != null)
                vista.Configurar(datos);

            AsegurarCollider(obj);
            cartas.Add(obj);
        }

        Distribuir();
    }

    public void AlClickear(int indice)
    {
        GameManager.Instance?.SeleccionarCarta(indice);
    }

    private void ActualizarEnergia()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null || textoEnergia == null) return;

        textoEnergia.text = gm.EnergiaActual + " / " + gm.EnergiaMaxima;
    }

    private void Distribuir()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null) return;

        int cantidad = cartas.Count;
        int seleccionada = gm.CartaSeleccionada;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject carta = cartas[i];
            if (carta == null) continue;

            bool esSeleccionada = (i == seleccionada);

            float t = i - (cantidad - 1) / 2f;
            float x = CalcularPosicionX(i, cantidad);
            float y = -curvatura * t * t;
            float angulo = -t * anguloPorCarta;
            Vector3 escala = escalaNormal;

            if (esSeleccionada)
            {
                y += desplazamientoSeleccion;
                angulo = 0f;
                escala = escalaSeleccionada;
            }

            Transform tr = carta.transform;
            tr.localPosition = new Vector3(x, y, 0f);
            tr.localRotation = Quaternion.Euler(0f, 0f, angulo);
            tr.localScale = escala;

            AplicarOrdenDeDibujo(carta, esSeleccionada ? ordenSeleccionada : ordenBase + i);
        }
    }

    private float CalcularPosicionX(int indiceCarta, int cantidad)
    {
        if (cantidad <= 1) return 0f;

        float separacion = Mathf.Clamp(
            anchoMano / (cantidad - 1),
            separacionMinima,
            separacionMaxima
        );

        return (indiceCarta - (cantidad - 1) / 2f) * separacion;
    }

    private void AplicarOrdenDeDibujo(GameObject carta, int orden)
    {
        int ordenSprite = orden * 10;
        int ordenTexto = ordenSprite + ordenTextoSobreSprite;

        Renderer[] renderers = carta.GetComponentsInChildren<Renderer>(true);
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].sortingOrder = (renderers[i] is SpriteRenderer) ? ordenSprite : ordenTexto;

        Canvas[] canvases = carta.GetComponentsInChildren<Canvas>(true);
        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].overrideSorting = true;
            canvases[i].sortingOrder = ordenTexto;
        }
    }

    private DatosCarta ObtenerDatos(int id)
    {
        DatosCarta datos = baseDeCartas != null ? baseDeCartas.Obtener(id) : null;

        if (datos == null)
            Debug.LogWarning("UIManager: no hay datos en la base de cartas para la carta " + id + ".");

        return datos;
    }

    private void AsegurarCollider(GameObject carta)
    {
        if (carta.GetComponentInChildren<Collider2D>() != null) return;

        SpriteRenderer sr = carta.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
            sr.gameObject.AddComponent<BoxCollider2D>();
        else
            Debug.LogWarning("UIManager: la carta no tiene Collider2D ni SpriteRenderer.");
    }
}