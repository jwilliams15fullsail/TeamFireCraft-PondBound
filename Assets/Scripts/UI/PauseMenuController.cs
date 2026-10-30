using UnityEngine;
using UnityEngine.SceneManagement;


public class PauseMenuController : MonoBehaviour
{
    [Header("Menu panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("Game settings")]
    [SerializeField] private bool isOnlineGame;

    [Header("HUD")]
    [SerializeField] private GameObject hud;
   private GameObject currentMenu;

    private bool isPaused;
   

    private void Start()
    {
        ShowPauseMenu(false);
        Time.timeScale = 1f;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (settingsPanel.activeSelf || howToPlayPanel.activeSelf)
                ShowPauseMenu(true); // Escape from a sub-screen returns to pause menu
            else
                TogglePause();
        }
    }

    public void BackToPauseMenu()
{
    ShowOnly(pausePanel);
}

    public void SwitchMenu(GameObject nextMenu)
    {
        currentMenu.SetActive(false);
        nextMenu.SetActive(true);
        currentMenu = nextMenu;
    }

    public void TogglePause()
    {
        ShowPauseMenu(!isPaused);
    }

    public void Resume()
    {
        ShowPauseMenu(false);
    }

    public void OpenSettings()
    {
        ShowOnly(settingsPanel);
    }

    public void OpenHowToPlay()
    {
        ShowOnly(howToPlayPanel);
    }

    public void OpenMainMenu()
    {
        // Restore time before loading another scene.
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenuScene");
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void ShowPauseMenu(bool show)
    {
        isPaused = show;

        if (pausePanel != null)
            pausePanel.SetActive(show);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (howToPlayPanel != null)
            howToPlayPanel.SetActive(false);

        //Deactivates HUD when in Pause Menu
        if (hud != null)
            hud.SetActive(!show);

        // Offline: freeze time. Online: leave the simulation running.
        Time.timeScale = show && !isOnlineGame ? 0f : 1f;

        Cursor.visible = show;
        Cursor.lockState = show ? CursorLockMode.None : CursorLockMode.Locked;
    }

    private void ShowOnly(GameObject panel)
    {
        if (pausePanel != null) pausePanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (howToPlayPanel != null) howToPlayPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

        if (panel != null) panel.SetActive(true);
    }

}
