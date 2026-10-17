using UnityEngine;

public class buttonFunctions : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject creditsMenu;

    private void ShowScreen(GameObject screenToShow)
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        creditsMenu.SetActive(false);
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

    public void BackButton()
    {
        ShowScreen(mainMenu);
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
