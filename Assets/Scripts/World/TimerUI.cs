using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows a RoomTimer's countdown on the HUD with the number pictures (number_0 to number_9) in a
// box (UI Box 1). The digits turn red and pulse near the end, then flash 0:00 when it runs out.
// Hidden while no timer is counting. Purely local, like PlayerHUD.
public class TimerUI : MonoBehaviour
{
    [Tooltip("Optional: the timer to show. Empty: whichever room timer is counting down.")]
    [SerializeField] RoomTimer timer;
    [Tooltip("The whole timer panel (the UI Box 1 image), hidden while no timer is counting.")]
    [SerializeField] GameObject panel;

    [Header("Digits")]
    [Tooltip("The number pictures, in order: Element 0 is number_0, Element 9 is number_9.")]
    [SerializeField] Sprite[] digitSprites = new Sprite[10];
    [Tooltip("The digit Images, left to right. 3 shows M:SS (2:05), 4 shows MM:SS (12:05).")]
    [SerializeField] Image[] digitImages = new Image[3];
    [Tooltip("Optional: the colon between minutes and seconds (an Image or a text). It blinks while counting.")]
    [SerializeField] GameObject colon;

    [Header("Text (optional)")]
    [Tooltip("Optional: shows the timer's Label, like Lasers in.")]
    [SerializeField] TMP_Text labelText;
    [Tooltip("Optional: shown when the time runs out, like a LASERS! text or picture.")]
    [SerializeField] GameObject runOutMessage;
    [Tooltip("The run-out message shows once this many seconds are left (0 = only when the time runs out).")]
    [SerializeField, Min(0f)] float messageBelow = 10f;

    [Header("Look")]
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color warningColor = new Color(1f, 0.2f, 0.15f);
    [Tooltip("Below this many seconds the digits turn red and pulse.")]
    [SerializeField, Min(0f)] float warningBelow = 10f;
    [Tooltip("Seconds the timer stays up, flashing 0:00, after it runs out.")]
    [SerializeField, Min(0f)] float runOutShowTime = 4f;

    [Header("Sound (optional)")]
    [Tooltip("A clock tick, played every second while the timer counts down.")]
    [SerializeField] AudioClip tickSound;
    [Tooltip("Optional: a sharper tick for the last seconds (below Warning Below). Empty: uses Tick Sound.")]
    [SerializeField] AudioClip urgentTickSound;
    [Tooltip("Tick twice a second in the last seconds, so it speeds up.")]
    [SerializeField] bool fastTicksNearEnd = true;
    [Tooltip("Optional: played once when the time runs out (a buzzer or alarm hit).")]
    [SerializeField] AudioClip runOutSound;
    [SerializeField, Range(0f, 1f)] float tickVolume = 0.6f;
    [Tooltip("Optional: the mixer group, e.g. SFX, so the Settings sliders control it.")]
    [SerializeField] UnityEngine.Audio.AudioMixerGroup soundOutput;

    float runOutSince = -1f;
    Graphic[] colonGraphics;
    AudioSource audioSource;
    int lastTick = -1;
    bool playedRunOut;

    void Awake()
    {
        // A plain 2D sound, heard the same wherever you are: it is your countdown.
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.outputAudioMixerGroup = soundOutput;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
            audioSource.PlayOneShot(clip, tickVolume);
    }

    void Update()
    {
        RoomTimer shown = timer != null ? timer : RoomTimer.Shown;
        bool running = shown != null && shown.IsRunning;
        bool runOut = shown != null && shown.HasRunOut;

        if (runOut && runOutSince < 0f)
            runOutSince = Time.time;
        else if (!runOut)
            runOutSince = -1f;

        bool show = running || (runOut && Time.time - runOutSince < runOutShowTime);
        if (panel != null && panel.activeSelf != show)
            panel.SetActive(show);
        if (!running)
            lastTick = -1;
        if (!runOut)
            playedRunOut = false;
        if (!show)
            return;

        if (labelText != null)
            labelText.text = runOut ? "" : shown.Label;
        

        float left = runOut ? 0f : shown.SecondsLeft;
        int seconds = Mathf.CeilToInt(left);
        bool warning = runOut || left <= warningBelow;
        // The run-out message comes up for the last seconds (Message Below) and stays at 0:00.
        bool showMessage = runOut || left <= messageBelow;
        if (runOutMessage != null && runOutMessage.activeSelf != showMessage)
            runOutMessage.SetActive(showMessage);
        bool flashOff = runOut && Mathf.Repeat(Time.time, 0.5f) > 0.25f; // run out: 0:00 flashes
        float pulse = warning && !runOut ? 1f + 0.12f * Mathf.Abs(Mathf.Sin(Time.time * Mathf.PI * 2f)) : 1f;

        ShowDigits(seconds / 60, seconds % 60, warning ? warningColor : normalColor, flashOff, pulse);

        // Ticking: once a second, twice a second (and sharper) in the last seconds, a buzzer at 0:00.
        int tick = warning && fastTicksNearEnd ? Mathf.CeilToInt(left * 2f) : seconds;
        if (!runOut && tick != lastTick)
        {
            if (lastTick >= 0)
                PlaySound(warning && urgentTickSound != null ? urgentTickSound : tickSound);
            lastTick = tick;
        }
        if (runOut && !playedRunOut)
        {
            playedRunOut = true;
            PlaySound(runOutSound);
        }

        // The colon blinks once a second while counting, and stays on otherwise.
        if (colon != null)
        {
            // Hidden by switching its picture or text off, not the object, so it keeps its space
            // and the digits don't jump sideways each time it blinks.
            colonGraphics ??= colon.GetComponentsInChildren<Graphic>(true);
            bool visible = !flashOff && (runOut || Mathf.Repeat(left, 1f) > 0.5f);
            foreach (Graphic graphic in colonGraphics)
                graphic.enabled = visible;
        }
    }

    // Fills the digit Images right to left: two for the seconds, the rest for the minutes.
    void ShowDigits(int minutes, int seconds, Color color, bool hidden, float scale)
    {
        if (digitImages == null || digitSprites == null || digitSprites.Length < 10)
            return;
        int count = digitImages.Length;
        for (int i = 0; i < count; i++)
        {
            Image image = digitImages[i];
            if (image == null)
                continue;
            int fromRight = count - 1 - i;
            int digit = fromRight == 0 ? seconds % 10
                : fromRight == 1 ? seconds / 10
                : minutes / (int)Mathf.Pow(10, fromRight - 2) % 10;
            image.sprite = digitSprites[digit];
            image.color = color;
            image.enabled = !hidden;
            image.rectTransform.localScale = Vector3.one * scale;
        }
    }
}