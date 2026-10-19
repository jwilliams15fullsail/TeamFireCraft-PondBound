using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

// In-game pause menu, opened with Esc or the gamepad Start button.
// Online, the game keeps running for everyone else: your frog just stops taking input
// (FrogNetworkDriver ignores input while the mouse is free). Offline, time stops too.
public class PauseMenu : MonoBehaviour
{
    public static bool IsOpen { get; private set; }

    [SerializeField] GameObject pausePanel;
    [SerializeField] GameObject settingsPanel;
    [Tooltip("Optional: shows the join code and player count in online games.")]
    [SerializeField] TMP_Text sessionInfo;

    // Enter Play Mode Options can skip the domain reload; don't carry IsOpen over.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun() => IsOpen = false;

    void Start()
    {
        Close();
    }

    void OnDestroy()
    {
        if (IsOpen)
        {
            IsOpen = false;
            Time.timeScale = 1f;
        }
    }

    void Update()
    {
        bool pressed = (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
                    || (Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame);
        if (!pressed)
            return;

        if (!IsOpen)
            Open();
        else if (settingsPanel.activeSelf)
            ShowPause(); // Esc in Settings goes back one step
        else
            Resume();
    }

    public void Open()
    {
        IsOpen = true;
        pausePanel.SetActive(true);
        settingsPanel.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (GameSession.IsOffline)
            Time.timeScale = 0f; // nobody else is playing, so really stop the game
        ShowSessionInfo();
    }

    public void Resume()
    {
        Close();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowSettings()
    {
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ShowPause()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
        ShowSessionInfo();
    }

    public void LeaveToMainMenu()
    {
        Close();
        GameSession.LeaveToMenu();
    }

    public void ExitGame()
    {
        Close();
        GameSession.QuitGame();
    }

    void Close()
    {
        IsOpen = false;
        pausePanel.SetActive(false);
        settingsPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void ShowSessionInfo()
    {
        if (sessionInfo == null)
            return;
        if (GameSession.IsOffline)
            sessionInfo.text = "Offline";
        else if (GameSession.Current != null)
            sessionInfo.text = $"Join code: {GameSession.Current.Code}\nPlayers: {GameSession.Current.PlayerCount}/{GameSession.Current.MaxPlayers}";
        else
            sessionInfo.text = "";
    }
}