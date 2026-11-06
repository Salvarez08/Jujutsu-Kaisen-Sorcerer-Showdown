using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
    [SerializeField]
    private Vector3 escalaSeleccionada =
        new Vector3(1.2f, 1.2f, 1f);

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

        RedibujarMano();
        ActualizarEnergia();
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
        if (gm == null)
            return;

        if (prefabCarta == null || contenedorMano == null)
        {
            Debug.LogWarning(
                "UIManager: asigna el prefab y el contenedor de la mano."
            );
            return;
        }

        int cantidad = gm.cantidadMano;

        for (int i = 0; i < cantidad; i++)
        {
            GameObject obj;

            if (i < cartas.Count && cartas[i] != null)
            {
                obj = cartas[i];
                obj.SetActive(true);
            }
            else
            {
                obj = Instantiate(prefabCarta, contenedorMano);
                obj.name = "Carta_" + i;

                if (i < cartas.Count)
                    cartas[i] = obj;
                else
                    cartas.Add(obj);

                AsegurarCollider(obj);
            }

            Carta carta = obj.GetComponent<Carta>();
            if (carta == null)
                carta = obj.AddComponent<Carta>();

            carta.Inicializar(i, this);

            int id = gm.IdCartaEnMano(i);
            DatosCarta datos = ObtenerDatos(id);

            CardView vista = obj.GetComponentInChildren<CardView>(true);
            if (vista != null && datos != null)
                vista.Configurar(datos);
        }

        for (int i = cantidad; i < cartas.Count; i++)
        {
            if (cartas[i] != null)
                cartas[i].SetActive(false);
        }

        Distribuir();
    }

    public void AlClickear(int indice)
    {
        GameManager.Instance?.SeleccionarCarta(indice);
    }

    private void ActualizarEnergia()
    {
        if (textoEnergia == null)
            return;

        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;

        textoEnergia.SetText(
            "{0} / {1}",
            gm.energiaActual,
            gm.EnergiaMaxima
        );
    }

    private void Distribuir()
    {
        GameManager gm = GameManager.Instance;
        if (gm == null)
            return;

        int cantidad = Mathf.Min(gm.cantidadMano, cartas.Count);
        int seleccionada = gm.cartaSeleccionada;

        float centro = (cantidad - 1) / 2f;
        float separacion = cantidad <= 1
            ? 0f
            : Mathf.Clamp(
                anchoMano / (cantidad - 1),
                separacionMinima,
                separacionMaxima
            );

        for (int i = 0; i < cantidad; i++)
        {
            GameObject carta = cartas[i];
            if (carta == null)
                continue;

            bool esSeleccionada = i == seleccionada;
            float t = i - centro;
            float x = t * separacion;
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

            int orden = esSeleccionada
                ? ordenSeleccionada
                : ordenBase + i;

            AplicarOrdenDeDibujo(carta, orden);
        }
    }

    private void AplicarOrdenDeDibujo(GameObject carta, int orden)
    {
        int ordenSprite = orden * 10;
        int ordenTexto = ordenSprite + ordenTextoSobreSprite;

        Renderer[] renderers =
            carta.GetComponentsInChildren<Renderer>(true);

        for (int i = 0; i < renderers.Length; i++)
        {
            renderers[i].sortingOrder =
                renderers[i] is SpriteRenderer
                    ? ordenSprite
                    : ordenTexto;
        }

        Canvas[] canvases =
            carta.GetComponentsInChildren<Canvas>(true);

        for (int i = 0; i < canvases.Length; i++)
        {
            canvases[i].overrideSorting = true;
            canvases[i].sortingOrder = ordenTexto;
        }
    }

    private DatosCarta ObtenerDatos(int id)
    {
        DatosCarta datos = baseDeCartas != null
            ? baseDeCartas.Obtener(id)
            : null;

        if (datos == null)
        {
            Debug.LogWarning(
                "UIManager: no hay datos en la base de cartas para la carta "
                + id + "."
            );
        }

        return datos;
    }

    private void AsegurarCollider(GameObject carta)
    {
        if (carta.GetComponentInChildren<Collider2D>() != null)
            return;

        SpriteRenderer sprite =
            carta.GetComponentInChildren<SpriteRenderer>();

        if (sprite != null)
        {
            sprite.gameObject.AddComponent<BoxCollider2D>();
        }
        else
        {
            Debug.LogWarning(
                "UIManager: la carta no tiene Collider2D ni SpriteRenderer."
            );
        }
    }
}