using TMPro;
using UnityEngine;

public class CardView : MonoBehaviour
{
    [SerializeField] private SpriteRenderer fondo;
    [SerializeField] private TMP_Text titulo;
    [SerializeField] private TMP_Text tipo;
    [SerializeField] private TMP_Text descripcion;
    [SerializeField] private TMP_Text energiaMaldita;
    [SerializeField] private TMP_Text copias;

    public void Configurar(DatosCarta datos)
    {
        if (datos == null)
            return;

        if (fondo != null)
            fondo.sprite = datos.Fondo;

        if (titulo != null)
            titulo.text = datos.Nombre;

        if (tipo != null)
            tipo.text = datos.Tipo;

        if (descripcion != null)
            descripcion.text = datos.Descripcion;

        if (energiaMaldita != null)
            energiaMaldita.text = datos.Energia;

        if (copias != null)
            copias.text = datos.Copias;
    }
}