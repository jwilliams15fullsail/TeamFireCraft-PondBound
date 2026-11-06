using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TestObserver : MonoBehaviour
{
    public static TestObserver instance { get; private set; }

    [Header("Respawn Settings")]
    [SerializeField] private InputActionReference respawnKey;
    [SerializeField] private float respawnHeightOffset = 1f;

    [Header("UI Integration")]
    [SerializeField] private TMP_Text respawnMessage;
    [Tooltip("The UI Image for the TENS place (ex, '3' in 30).")]
    [SerializeField] private Image tensImage;
    [Tooltip("The UI Image for the ONES place (ex, '0' in 30).")]
    [SerializeField] private Image onesImage;
    [Tooltip("The number pictures, in order: Element 0 is number_0, Element 9 is number_9.")]
    [SerializeField] private Sprite[] digits = new Sprite[10];
    [Tooltip("The number to start counting down from.")]
    [SerializeField, Range(1, 99)] private int deathCooldownDuration = 30;


    private Vector3 initialSpawnPosition;
    private CheckpointManager activeCheckpoint;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Rigidbody playerRigidbody;

    private bool _isPlayerDead = false;
    private float timeOfDeath = 0f; // Stores the exact hardware clock time of death
    // private Coroutine countdownCoroutine;
    public bool canRespawn { get; private set; } = false;        // flags when the 30s timer is up

    public bool isPlayerDead
    {
        get => _isPlayerDead;
        set
        {
            _isPlayerDead = value;
            if (_isPlayerDead)
            {
                timeOfDeath = Time.realtimeSinceStartup;
                canRespawn = false;
                ToggleCountdownUi(true);
                //// Stop any leftover countdown loop safely before starting a new one
                //if (countdownCoroutine != null) StopCoroutine(countdownCoroutine);
                //// AUTOMATIC TRIGGER: Start counting down from 30 immediately on death
                //countdownCoroutine = StartCoroutine(DeathCountdownRoutine());
            }
            else
            {
                //// Safely turn off UI if respawned by other means
                //if (countdownCoroutine != null) { StopCoroutine(countdownCoroutine); }
                ToggleCountdownUi(false);
                canRespawn = false;
            }
        }
    }

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else { Destroy(gameObject); return; }

        // Hide UI at the start
        ToggleCountdownUi(false);
        if (respawnMessage != null) { respawnMessage.gameObject.SetActive(false); }
    }

    // Method the networked Frog script will call when it spawns
    public void RegisterLocalPlayer(GameObject localPlayer, Rigidbody rb, Vector3 spawnPoint)
    {
        playerObject = localPlayer;
        playerRigidbody = rb;
        initialSpawnPosition = spawnPoint;

        Debug.Log($"[Test Observer] Successfully registered local player: {localPlayer.name}");
    }

    // Subscribe to CheckpointManager on creation
    private void OnEnable()
    {
        CheckpointManager.OnCheckpointActivated += HandleCheckpointTouched;
    }

    // Unsubscribe if this object is destroyed
    private void OnDisable()
    {
        CheckpointManager.OnCheckpointActivated -= HandleCheckpointTouched;
    }

    private void Update()
    {
        // If the player isn't dead, don't execute any tracking or visual updates
        if (!_isPlayerDead) return;

        if (!canRespawn)
        {
            // Calculate remaining time using the hardware clock
            float elapsed = Time.realtimeSinceStartup - timeOfDeath;
            int remainingSeconds = Mathf.CeilToInt(Mathf.Max(0f, deathCooldownDuration - elapsed));

            if (remainingSeconds > 0)
            {
                // Split digits just like the death screen does, but matching our 30s scale
                int tensValue = remainingSeconds / 10;
                int onesValue = remainingSeconds % 10;

                if (tensImage != null && tensValue < digits.Length)
                {
                    if (tensValue == 0) tensImage.gameObject.SetActive(false);
                    else
                    {
                        tensImage.gameObject.SetActive(true);
                        tensImage.sprite = digits[tensValue];
                    }
                }

                if (onesImage != null && onesValue < digits.Length)
                {
                    onesImage.sprite = digits[onesValue];
                }
            }
            else
            {
                // Timer finished! Clean up UI and unlock respawn capability
                ToggleCountdownUi(false);
                canRespawn = true;
                Debug.Log("[Test Observer] 30 seconds are up! You can press R to finalize respawn.");
            }
        }

        // Handle the R keypress unlock sequence
        if (respawnKey.action.WasPressedThisFrame() && canRespawn)
        {
            InitiateDeathScreenSequence();
        }
    }

    private void HandleCheckpointTouched(CheckpointManager newCheckpoint, GameObject interactingPlayer)
    {
        // CRITICAL MULTIPLAYER FILTER: Only process this event if it was triggered by our local player
        if (interactingPlayer != playerObject) return;

        if (activeCheckpoint != null && activeCheckpoint != newCheckpoint)
        {
            // Tell the server to deactivate visuals specifically for our local player
            activeCheckpoint.DeactivateForPlayer(playerObject);
            Debug.Log("[Test Observer] Cleared the previous checkpoint.");
        }
        activeCheckpoint = newCheckpoint;
        Debug.Log($"[Test Observer] New Checkpoint set at: {newCheckpoint.transform.position}");
    }

    //private IEnumerator DeathCountdownRoutine()
    //{
    //    ToggleCountdownUi(true);        // Show countdown timer

    //    float startTime = Time.realtimeSinceStartup;
    //    float endTime = startTime + deathCooldownDuration;
    //    int lastDisplayedValue = -1;

    //    // Start counting down from user defined start value
    //    while (Time.realtimeSinceStartup < endTime)
    //    {
    //        float remainingTime = endTime - Time.realtimeSinceStartup;
    //        int currentDisplayCount = Mathf.CeilToInt(remainingTime);

    //        if (currentDisplayCount != lastDisplayedValue && currentDisplayCount >= 0)
    //        {
    //            lastDisplayedValue = currentDisplayCount;

    //            // Split number into it's digits
    //            int tensValue = currentDisplayCount / 10;
    //            int onesValue = currentDisplayCount % 10;

    //            // Update tens image
    //            if (tensImage != null && tensValue < digits.Length && digits[tensValue] != null)
    //            {
    //                // Optional: Hide the tens digit completely if we hit single digits
    //                if (tensValue == 0)
    //                {
    //                    tensImage.gameObject.SetActive(false);
    //                }
    //                else
    //                {
    //                    tensImage.gameObject.SetActive(true);
    //                    tensImage.sprite = digits[tensValue];
    //                }
    //            }

    //            // Update ones image
    //            if (onesImage != null && onesValue < digits.Length && digits[onesValue] != null)
    //            {
    //                onesImage.sprite = digits[onesValue];
    //            }
    //        }
    //        yield return null;
    //    }
    //    ToggleCountdownUi(false);
    //    canRespawn = true;
    //    if (respawnMessage != null) { respawnMessage.gameObject.SetActive(true); }
    //}

    private void ToggleCountdownUi(bool show)
    {
        if (tensImage != null) { tensImage.gameObject.SetActive(show); }
        if (onesImage != null) { onesImage.gameObject.SetActive(show); }
    }

    private void InitiateDeathScreenSequence()
    {
        canRespawn = false;

        // Setup checkpoint offset destination before the screen takes over
        Vector3 spawnPosition = activeCheckpoint != null
            ? activeCheckpoint.transform.position + (Vector3.up * respawnHeightOffset)
            : initialSpawnPosition;

        playerObject.transform.position = spawnPosition;

        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
        }

        _isPlayerDead = false;
        canRespawn = false;
    }


    //private void RespawnPlayer()
    //{
    //    if (playerObject == null) { return; }

    //    // Prevent weird physics interactions when teleporting player back to checkpoint
    //    if (playerRigidbody != null)
    //    {
    //        playerRigidbody.linearVelocity = Vector3.zero;
    //        playerRigidbody.angularVelocity = Vector3.zero;
    //    }

    //    // Ternary operator checking if we have an activeCheckpoint or not
    //    Vector3 spawnPosition = activeCheckpoint != null
    //        ? activeCheckpoint.transform.position + (Vector3.up * respawnHeightOffset)
    //        : initialSpawnPosition;

    //    // Teleport player to the checkpoint position
    //    playerObject.transform.position = spawnPosition;    // MAY NEED UPDATED TO USE FROG NETWORKED SCRIPT FOR MOVEMENT

    //    // Reset death state after respawn
    //    isPlayerDead = false;
    //    if (respawnMessage != null) { respawnMessage.gameObject.SetActive(false); }
    //    Debug.Log($"[Test Observer] Player respawned at position {spawnPosition}");
    //}
}
