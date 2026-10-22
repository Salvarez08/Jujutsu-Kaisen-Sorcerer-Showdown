using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public GameObject pauseMenu;
    public GameObject pauseButton;

   public void PauseGame()
    {
        Time.timeScale = 0f; // Pause the game
        pauseMenu.SetActive(true); // Show the pause menu
        pauseButton.SetActive(false); // Hide the pause button
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f; // Resume the game
        pauseMenu.SetActive(false); // Hide the pause menu
        pauseButton.SetActive(true); // Show the pause button
    }
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
        Time.timeScale = 1;
    }
    public void QuitGame()
    {
        Debug.Log("Adios Tonotos");
        Application.Quit(); // Quit the application
    }
}
