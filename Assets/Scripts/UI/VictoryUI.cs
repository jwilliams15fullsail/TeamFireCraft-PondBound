using UnityEngine;

// The victory screen in a level. Registers its panel with the GameManager, which shows it when
// the level is won. When it appears it celebrates: a fanfare and, optionally, confetti.
public class VictoryUI : MonoBehaviour
{
    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Celebration")]
    [Tooltip("Played once when the victory screen appears: a fanfare, a cheer, happy frog croaks.")]
    [SerializeField] private AudioClip winSound;
    [SerializeField, Range(0f, 1f)] private float winVolume = 0.9f;
    [Tooltip("Optional: confetti or fireworks. A particle effect in the scene (or under this canvas) played when the victory screen appears.")]
    [SerializeField] private ParticleSystem confetti;

    private AudioSource winSource;
    private bool celebrated;

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

    private void Update()
    {
        if (victoryPanel == null || !victoryPanel.activeSelf)
            return;

        if (!celebrated)
        {
            celebrated = true;
            Celebrate();
        }

        // Keep the mouse free while the victory screen is up, so the button can be clicked.
        // Every frame, because closing the pause menu captures the mouse again.
        if (!PauseMenu.IsOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Celebrate()
    {
        if (winSound != null)
        {
            if (winSource == null)
            {
                winSource = gameObject.AddComponent<AudioSource>();
                winSource.playOnAwake = false;
                winSource.spatialBlend = 0f;
            }
            winSource.PlayOneShot(winSound, winVolume);
        }
        if (confetti != null)
            confetti.Play(true);
    }

    public void ReturnToMainMenu()
    {
        // Restore normal time before leaving the level.
        Time.timeScale = 1f;

        // Use the existing multiplayer session cleanup and menu transition.
        GameSession.LeaveToMenu();
    }
}