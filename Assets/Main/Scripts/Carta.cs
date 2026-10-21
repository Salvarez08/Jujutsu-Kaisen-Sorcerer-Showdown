using UnityEngine;
using UnityEngine.EventSystems;

public class Carta : MonoBehaviour, IPointerClickHandler
{
    public int indiceEnMano;

    public void OnPointerClick(PointerEventData eventData)
    {
        GameManager.Instance.SeleccionarCarta(indiceEnMano);
    }
}