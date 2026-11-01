
using UnityEngine;

public class VictoryUI : MonoBehaviour
{
    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;


    private void Start()
    {
        // Register this scene's victory panel with the GameManager.
        if (GameManager.instance != null)
        {
            // Reset the win condition for this new gameplay session.
            GameManager.instance.ResetWinState();

            GameManager.instance.RegisterVictoryPanel(victoryPanel);
        }
        else
        {
            Debug.LogWarning("GameManager not found! Victory UI could not register.");
        }
    }

    public void ReturnToMainMenu()
    {
        // Restore normal time before leaving the level.
        Time.timeScale = 1f;

        // Use the existing multiplayer session cleanup and menu transition.
        GameSession.LeaveToMenu();
    }

}
