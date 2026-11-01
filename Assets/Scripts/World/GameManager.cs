using UnityEngine;

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

        //Trigger level complete UI
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        else
        {
            Debug.LogWarning("Victory Panel has not been registered!");
        }
    }

    void Start()
    {

    }

    // Update is called once per frame.
    void Update()
    {

    }
}