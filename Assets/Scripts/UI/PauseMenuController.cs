using UnityEngine;

public class PauseMenuController : MonoBehaviour
{
    [Header("Menu panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject howToPlayPanel;
    [SerializeField] private GameObject mainMenuPanel;

    [Header("HUD")]
    [SerializeField] private GameObject hud;

    private bool isPaused;

    // GameSession.IsOffline is true for offline play.
    private bool IsOnlineGame => !GameSession.IsOffline;

    private void Start()
    {
        SetPaused(false);
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if ((settingsPanel != null && settingsPanel.activeSelf) ||
            (howToPlayPanel != null && howToPlayPanel.activeSelf))
        {
            ShowPauseMenuPanel();
        }
        else
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        SetPaused(!isPaused);
    }

    public void Resume()
    {
        SetPaused(false);
    }

    public void OpenSettings()
    {
        ShowSubPanel(settingsPanel);
    }

    public void OpenHowToPlay()
    {
        ShowSubPanel(howToPlayPanel);
    }

    public void BackToPauseMenu()
    {
        ShowPauseMenuPanel();
    }

    // Connect your Leave to Main Menu button to this method.
    public void OpenMainMenu()
    {
        SetPaused(false);
        GameSession.LeaveToMenu();
    }

    // Connect your Quit button to this method.
    public void ExitGame()
    {
        SetPaused(false);
        GameSession.QuitGame();
    }

    private void SetPaused(bool paused)
    {
        isPaused = paused;

        SetActive(pausePanel, paused);
        SetActive(settingsPanel, false);
        SetActive(howToPlayPanel, false);
        SetActive(mainMenuPanel, false);
        SetActive(hud, !paused);

        // Offline gameplay pauses; online gameplay keeps running.
        Time.timeScale = paused && !IsOnlineGame ? 0f : 1f;

        Cursor.visible = paused;
        Cursor.lockState = paused
            ? CursorLockMode.None
            : CursorLockMode.Locked;
    }

    private void ShowPauseMenuPanel()
    {
        isPaused = true;

        SetActive(pausePanel, true);
        SetActive(settingsPanel, false);
        SetActive(howToPlayPanel, false);
        SetActive(mainMenuPanel, false);
        SetActive(hud, false);

        Time.timeScale = IsOnlineGame ? 1f : 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void ShowSubPanel(GameObject panel)
    {
        isPaused = true;

        SetActive(pausePanel, false);
        SetActive(settingsPanel, false);
        SetActive(howToPlayPanel, false);
        SetActive(mainMenuPanel, false);
        SetActive(panel, true);
        SetActive(hud, false);

        Time.timeScale = IsOnlineGame ? 1f : 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private static void SetActive(GameObject obj, bool active)
    {
        if (obj != null)
            obj.SetActive(active);
    }
}