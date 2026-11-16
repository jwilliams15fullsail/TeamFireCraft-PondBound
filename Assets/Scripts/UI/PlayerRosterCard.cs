using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One player's card in the PlayerRoster: their frog's picture, name, health / hunger /
// hydration bars and a status line. Put it on the card template inside the roster's panel
// and drag its parts in. The roster copies the template for each player and fills it in.
public class PlayerRosterCard : MonoBehaviour
{
    [Tooltip("The frog picture.")]
    [SerializeField] Image portrait;
    [Tooltip("Optional: a frame or strip that takes the player's frog colour.")]
    [SerializeField] Image colourStrip;
    [SerializeField] TMP_Text nameText;
    [Tooltip("Optional: DOWN while the frog is dead, otherwise how far away it is.")]
    [SerializeField] TMP_Text statusText;

    [Header("Bars (Image Type: Filled, Fill Method: Horizontal)")]
    [SerializeField] Image healthFill;
    [SerializeField] Image hungerFill;
    [SerializeField] Image hydrationFill;

    [Tooltip("Optional: fades the card while the frog is dead.")]
    [SerializeField] CanvasGroup group;

    FrogIdentity frog;
    FrogStats stats;
    FrogMotor motor;
    string shownName;  // what the name text was built from, so it's only rebuilt on a change
    int shownSlot = -2;
    int shownStatus = -2; // -1: DOWN, otherwise the distance shown in metres

    // Called every frame by PlayerRoster. local: your own frog (null before it spawns).
    public void Refresh(FrogIdentity who, FrogIdentity local, Sprite picture, bool tintPicture, float barSpeed, float deadAlpha)
    {
        bool newFrog = who != frog;
        if (newFrog)
        {
            frog = who;
            stats = who.GetComponent<FrogStats>();
            motor = who.GetComponent<FrogMotor>();
            shownName = null;
            shownSlot = -2;
            shownStatus = -2;
        }

        Color colour = frog.Tint;
        if (portrait != null)
        {
            portrait.sprite = picture;
            portrait.preserveAspect = true;
            portrait.color = tintPicture ? colour : Color.white;
        }
        if (colourStrip != null)
            colourStrip.color = colour;

        if (nameText != null && (frog.PlayerName.Value != shownName || frog.Slot.Value != shownSlot))
        {
            shownName = frog.PlayerName.Value;
            shownSlot = frog.Slot.Value;
            nameText.text = frog.IsOwner ? $"{frog.DisplayName} (You)" : frog.DisplayName;
        }

        bool dead = motor != null && motor.IsDead;
        if (group != null)
            group.alpha = dead ? deadAlpha : 1f;
        UpdateStatus(dead, local);

        if (stats == null || !stats.IsSpawned)
            return;
        // A new frog in this card: jump straight to its values instead of sliding from the last one's.
        float step = newFrog ? 1f : barSpeed * Time.unscaledDeltaTime;
        UpdateBar(healthFill, stats.HealthFraction, step);
        UpdateBar(hungerFill, stats.HungerFraction, step);
        UpdateBar(hydrationFill, stats.HydrationFraction, step);
    }

    void UpdateStatus(bool dead, FrogIdentity local)
    {
        if (statusText == null)
            return;
        int status = -2; // nothing: your own card, or your frog isn't in yet
        if (dead)
            status = -1;
        else if (local != null && local != frog)
            status = Mathf.RoundToInt(Vector3.Distance(local.transform.position, frog.transform.position));
        if (status == shownStatus)
            return;
        shownStatus = status;
        statusText.text = status == -1 ? "DOWN" : status >= 0 ? $"{status} m" : "";
    }

    static void UpdateBar(Image fill, float value, float step)
    {
        if (fill != null)
            fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, Mathf.Clamp01(value), step);
    }
}