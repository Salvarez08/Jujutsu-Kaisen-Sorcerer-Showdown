using UnityEngine;
using UnityEngine.EventSystems;

public class Carta : MonoBehaviour, IPointerClickHandler
{
    private int indice;
    private UIManager ui;

    public void Inicializar(int indice, UIManager ui)
    {
        this.indice = indice;
        this.ui = ui;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Null-conditional: equivalente a "if (ui != null) ui.AlClickear(indice);"
        ui?.AlClickear(indice);
    }
}