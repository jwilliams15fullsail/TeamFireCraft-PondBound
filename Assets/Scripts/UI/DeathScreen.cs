using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

// Your own death screen. A moment after your frog dies, so the death animation shows first, a
// full-screen picture fades in: the Hydration one if the hydration bar is empty, otherwise the
// Health one. A number counts down over it, and when it's done the host brings the frog back.
// Online, the game carries on for everyone else. Purely local, like PlayerHUD: put it on the
// HUD canvas, with the panel as a child.
public class DeathScreen : MonoBehaviour
{
    [Tooltip("The full-screen panel: a child of this object with an Image and a Canvas Group.")]
    [SerializeField] CanvasGroup panel;
    [SerializeField] Sprite healthScreen;
    [SerializeField] Sprite hydrationScreen;
    [Tooltip("Seconds after dying before the panel starts to fade in, so the animation can play.")]
    [SerializeField] float showDelay = 1.5f;
    [SerializeField] float fadeTime = 0.5f;

    [Header("Input Settings")]
    [SerializeField] private InputActionReference respawnKey; // Drag your respawn key input action here

    [Header("Respawn countdown")]
    [Tooltip("The Image that shows the number: a child of the panel.")]
    [SerializeField] Image number;
    [SerializeField] private TMP_Text respawnMessage;
    [Tooltip("The UI Image for the TENS place (ex, '3' in 30).")]
    [SerializeField] private Image tensImage;
    [Tooltip("The UI Image for the ONES place (ex, '0' in 30).")]
    [SerializeField] private Image onesImage;
    [Tooltip("The number pictures, in order: Element 0 is number_0, Element 9 is number_9.")]
    [SerializeField] Sprite[] digits = new Sprite[10];
    [Tooltip("The number to start from. Each one shows for a second, down to 0, then the frog respawns.")]
    [SerializeField, Range(1, 9)] int countFrom = 5;
    [SerializeField, Range(1, 99)] int deathCooldownDuration = 30;
    // Tracks whether we already started the 30-second lockout coroutine this death lifecycle
    private bool startedDeathCountdown = false;
    private bool countdownFinished = false;
    public bool canRespawn { get; private set; } = false;    // flags when the 30s timer is up
    FrogStats stats;
    FrogMotor motor;
    FrogNetworkDriver driver;
    Image picture;
    float deadTime;
    bool respawnRequested;

    void Start()
    {
        picture = panel.GetComponent<Image>();
        panel.gameObject.SetActive(false);
        canRespawn = false;
        ToggleCountdownUi(false);
        if (respawnMessage != null) { respawnMessage.gameObject.SetActive(false); }
    }

    void Update()
    {
        if (stats == null)
        {
            stats = PlayerHUD.FindLocalFrogStats();
            if (stats != null)
            {
                motor = stats.GetComponent<FrogMotor>();
                driver = stats.GetComponent<FrogNetworkDriver>();
            }
        }

        bool dead = stats != null && stats.IsSpawned && motor.IsDead;

        // RESET STATE: If the frog becomes alive again, reset tracking flags
        if (!dead)
        {
            deadTime = 0f;
            canRespawn = false;
            startedDeathCountdown = false;
            respawnRequested = false;

            ToggleCountdownUi(false);
            if (respawnMessage != null) respawnMessage.gameObject.SetActive(false);
            panel.gameObject.SetActive(false);
            return;
        }
        // --------------------------------------------------------------------
        // INTERCEPTION LAYER: the 30-second lockout
        if (!canRespawn)
        {
            // If we just detected a death but haven't started the routine, fire it off now!
            if (!startedDeathCountdown)
            {
                startedDeathCountdown = true;
                StartCoroutine(DeathCountdownRoutine());
            }

            if (countdownFinished)
            {
                if (respawnKey != null && respawnKey.action.WasPressedThisFrame())
                {
                    canRespawn = true; // Unlock the final 5s death screen phase!
                    if (respawnMessage != null) respawnMessage.gameObject.SetActive(false);
                }
            }

            // HARD GUARD: While counting down, force everything else to wait
            panel.gameObject.SetActive(false);
            deadTime = 0f;
            return;
        }
        // ---------------------------------------------------------------------

        deadTime = dead ? deadTime + Time.deltaTime : 0f;
        bool show = dead && deadTime >= showDelay;

        if (panel.gameObject.activeSelf != show)
        {
            panel.gameObject.SetActive(show);
            if (show)
            {
                picture.sprite = stats.Hydration.Value <= 0f ? hydrationScreen : healthScreen;
                respawnRequested = false;
            }
        }
        if (!show)
            return;

        float shownFor = deadTime - showDelay;
        panel.alpha = Mathf.Clamp01(shownFor / fadeTime);
        number.sprite = digits[Mathf.Max(0, countFrom - (int)shownFor)];
        if (shownFor >= countFrom + 1 && !respawnRequested && canRespawn)
        {
            respawnRequested = true;
            driver.RequestRespawn(); // the panel closes when the frog is alive again
        }
    }

    private IEnumerator DeathCountdownRoutine()
    {
        ToggleCountdownUi(true);        // Show countdown timer
        if (respawnMessage != null) respawnMessage.gameObject.SetActive(false);

        float startTime = Time.realtimeSinceStartup;
        float endTime = startTime + deathCooldownDuration;
        int lastDisplayedValue = -1;

        // Start counting down from user defined start value
        while (Time.realtimeSinceStartup < endTime)
        {
            float remainingTime = endTime - Time.realtimeSinceStartup;
            int currentDisplayCount = Mathf.CeilToInt(remainingTime);

            if (currentDisplayCount != lastDisplayedValue && currentDisplayCount >= 0)
            {
                lastDisplayedValue = currentDisplayCount;

                // Split number into it's digits
                int tensValue = currentDisplayCount / 10;
                int onesValue = currentDisplayCount % 10;

                // Update tens image
                if (tensImage != null && tensValue < digits.Length && digits[tensValue] != null)
                {
                    // Optional: Hide the tens digit completely if we hit single digits
                    if (tensValue == 0)
                    {
                        tensImage.gameObject.SetActive(false);
                    }
                    else
                    {
                        tensImage.gameObject.SetActive(true);
                        tensImage.sprite = digits[tensValue];
                    }
                }

                // Update ones image
                if (onesImage != null && onesValue < digits.Length && digits[onesValue] != null)
                {
                    onesImage.sprite = digits[onesValue];
                }
            }
            yield return null;
        }
        ToggleCountdownUi(false);
        countdownFinished = true;
        if (respawnMessage != null) { respawnMessage.gameObject.SetActive(true); }
    }

    private void ToggleCountdownUi(bool show)
    {
        if (tensImage != null) { tensImage.gameObject.SetActive(show); }
        if (onesImage != null) { onesImage.gameObject.SetActive(show); }
    }
}