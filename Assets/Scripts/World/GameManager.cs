using UnityEngine;

// Lives in MainMenuScene and stays loaded for the whole game. Shows the level's victory screen
// when the level is won (LevelExit), once per level.
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    // Prevent the win condition from triggering multiple times.
    private bool gameWon = false;

    public bool HasWon => gameWon;

    [Header("Victory UI")]
    [SerializeField] private GameObject victoryPanel;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Register the victory panel from the gameplay scene.
    public void RegisterVictoryPanel(GameObject panel)
    {
        victoryPanel = panel;

        // Keep the victory screen hidden until the game is won.
        if (victoryPanel != null)
            victoryPanel.SetActive(gameWon);
    }

    public void ResetWinState()
    {
        // Allow the win condition to trigger again in a new game.
        gameWon = false;

        // Hide the previous victory screen if it still exists.
        if (victoryPanel != null)
            victoryPanel.SetActive(false);

        Debug.Log("Win state reset for a new game.");
    }

    public void WinGame()
    {
        // Only allow the game to be completed once.
        if (gameWon)
            return;

        gameWon = true;

        Debug.Log("Level Complete");

        // The panel from an earlier level (or a level that was restarted) is gone: use this level's.
        if (victoryPanel == null && VictoryUI.Current != null)
            victoryPanel = VictoryUI.Current.Panel;

        //Trigger level complete UI
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
            if (!victoryPanel.activeInHierarchy)
                Debug.LogWarning($"Victory Panel '{victoryPanel.name}' is on, but something above it in the Hierarchy is switched off, so it can't be seen.", victoryPanel);
        }
        else
        {
            Debug.LogWarning("Victory Panel has not been registered!");
        }
    }
}