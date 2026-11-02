using UnityEngine;

[System.Serializable]
public class DatosCarta
{
    [SerializeField] private string nombre;
    [SerializeField] private string tipo;
    [SerializeField, TextArea(2, 5)] private string descripcion;
    [SerializeField] private string energia;
    [SerializeField] private Sprite fondo;

    [SerializeField] private string copias;

    public string Nombre => nombre;
    public string Tipo => tipo;
    public string Descripcion => descripcion;
    public string Energia => energia;
    public Sprite Fondo => fondo;
    public string Copias => copias;
}

[CreateAssetMenu(fileName = "BaseDeCartas", menuName = "Cartas/Base de cartas")]
public class BaseDeCartas : ScriptableObject
{
    [SerializeField] private DatosCarta[] cartas;

    public int Cantidad => cartas == null ? 0 : cartas.Length;

    // El id es el número de carta (0 a 29)
    public DatosCarta Obtener(int id)
    {
        if (cartas == null || id < 0 || id >= cartas.Length)
            return null;

        return cartas[id];
    }
}