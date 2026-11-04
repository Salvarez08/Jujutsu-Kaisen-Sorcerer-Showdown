using UnityEngine;

public class PanelManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject panelScreen;
    [SerializeField] private GameObject panelSound;
    [SerializeField] private GameObject panelControls;
    [SerializeField] private GameObject panelAccounts;

    private void Start()
    {
        HideAllPanels();
    }

    public void ShowScreen()
    {
        HideAllPanels();
        panelScreen.SetActive(true);
    }

    public void ShowSound()
    {
        HideAllPanels();
        panelSound.SetActive(true);
    }

    public void ShowControls()
    {
        HideAllPanels();
        panelControls.SetActive(true);
    }

    public void ShowAccounts()
    {
        HideAllPanels();
        panelAccounts.SetActive(true);
    }

    
    public void Back()
    {
        HideAllPanels();
    }

    private void HideAllPanels()
    {
        panelScreen.SetActive(false);
        panelSound.SetActive(false);
        panelControls.SetActive(false);
        panelAccounts.SetActive(false);
    }
}