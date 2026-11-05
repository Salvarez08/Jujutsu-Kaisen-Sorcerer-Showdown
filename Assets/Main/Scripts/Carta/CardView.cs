using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fondo;
    [SerializeField] private TMP_Text Titulo;
    [SerializeField] private TMP_Text Tipo;
    [SerializeField] private TMP_Text Descripcion;
    [SerializeField] private TMP_Text EnergiaMaldita;
    [SerializeField] private TMP_Text Copias;
    [SerializeField] private GameObject wrapper;

    public void Configurar(DatosCarta datos)
    {
        if (datos == null)
            return;

        if (fondo != null && datos.Fondo != null)
            fondo.sprite = datos.Fondo;

        if (Titulo != null)
            Titulo.text = datos.Nombre;

        if (Tipo != null)
            Tipo.text = datos.Tipo;

        if (Descripcion != null)
            Descripcion.text = datos.Descripcion;

        if (EnergiaMaldita != null)
            EnergiaMaldita.text = datos.Energia;

        if (Copias != null)
            Copias.text = datos.Copias;
    }
}