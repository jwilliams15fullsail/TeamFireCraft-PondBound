using UnityEngine;

public class buttonFunctions : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;
    [SerializeField] private GameObject multiplayerMenu;
    [SerializeField] private GameObject howToPlayMenu;

    private void ShowScreen(GameObject screenToShow)
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
        multiplayerMenu.SetActive(false);
        howToPlayMenu.SetActive(false);
        screenToShow.SetActive(true);
    }
    public void OpenSettings()
    {
        ShowScreen(settingsMenu);
    }

    public void OpenCredits()
    {
        ShowScreen(creditsMenu);
    }

    public void OpenMultiplayer()
    {
        ShowScreen(multiplayerMenu);
    }

    public void BackButton()
    {
        ShowScreen(mainMenu);
    }

    public void OpenHowToPlay()
    {
        ShowScreen(howToPlayMenu);
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
