using UnityEngine;
using UnityEngine.SceneManagement;

public class StartBotton : MonoBehaviour
{
    [SerializeField] private string sceneName = "AmanteDeLosCaballos";

    public void LoadScene()
    {
        SceneManager.LoadScene(sceneName);
    }
}
