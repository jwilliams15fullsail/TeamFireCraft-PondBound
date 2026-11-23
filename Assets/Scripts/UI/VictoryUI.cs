using UnityEngine;
using UnityEngine.Audio;

// The victory screen in a level. Registers its panel with the GameManager, which shows it when
// the level is won. When it appears it celebrates: a success sound and, optionally, confetti.
public class VictoryUI : MonoBehaviour
{
    [Header("Victory Screen")]
    [SerializeField] private GameObject victoryPanel;

    [Header("Celebration")]
    [Tooltip("Played once when the victory screen appears: a fanfare, a cheer, happy frog croaks.")]
    [SerializeField] private AudioClip winSound;
    [SerializeField, Range(0f, 1f)] private float winVolume = 0.9f;
    [Tooltip("Mixer group the win sound plays through (your SFX group), so the volume settings apply to it.")]
    [SerializeField] private AudioMixerGroup output;
    [Tooltip("Optional: confetti or fireworks. A particle effect in the scene (or under this canvas) played when the victory screen appears.")]
    [SerializeField] private ParticleSystem confetti;

    private AudioSource winSource;
    private bool celebrated;

    // This level's victory screen, so the GameManager or the level exit can always find it.
    public static VictoryUI Current { get; private set; }
    public GameObject Panel => victoryPanel;

    private void OnEnable() => Current = this;

    private void OnDisable()
    {
        if (Current == this)
            Current = null;
    }

    private void Start()
    {
        if (victoryPanel == null)
            Debug.LogWarning("VictoryUI: Victory Panel is empty, so there's nothing to show when the level is won.", this);

        // Register this scene's victory panel with the GameManager.
        if (GameManager.instance != null)
        {
            // Reset the win condition for this new gameplay session.
            GameManager.instance.ResetWinState();

            GameManager.instance.RegisterVictoryPanel(victoryPanel);
        }
        else
        {
            Debug.LogWarning("GameManager not found! Victory UI could not register (start Play mode from MainMenuScene).");
            if (victoryPanel != null)
                victoryPanel.SetActive(false);
        }
    }

    // Shows the victory screen without the GameManager (Play mode started in the level itself).
    public void Show()
    {
        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }

    private void Update()
    {
        if (victoryPanel == null || !victoryPanel.activeInHierarchy)
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
                winSource.spatialBlend = 0f;          // 2D: same loudness wherever the camera is
                winSource.ignoreListenerPause = true; // still plays if the game is paused
                winSource.outputAudioMixerGroup = output;
            }
            winSource.PlayOneShot(winSound, winVolume);
        }
        else
        {
            Debug.LogWarning("VictoryUI: no Win Sound assigned, so the victory screen is silent.");
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