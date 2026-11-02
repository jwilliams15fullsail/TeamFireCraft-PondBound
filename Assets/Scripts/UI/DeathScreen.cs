using UnityEngine;
using UnityEngine.UI;

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

    [Header("Respawn countdown")]
    [Tooltip("The Image that shows the number: a child of the panel.")]
    [SerializeField] Image number;
    [Tooltip("The number pictures, in order: Element 0 is number_0, Element 9 is number_9.")]
    [SerializeField] Sprite[] digits = new Sprite[10];
    [Tooltip("The number to start from. Each one shows for a second, down to 0, then the frog respawns.")]
    [SerializeField, Range(1, 9)] int countFrom = 5;

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
        if (shownFor >= countFrom + 1 && !respawnRequested)
        {
            respawnRequested = true;
            driver.RequestRespawn(); // the panel closes when the frog is alive again
        }
    }
}