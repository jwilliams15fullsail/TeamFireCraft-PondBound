using FishNet.Object;
using TMPro;
using UnityEngine;

// The "Checkpoint reached" popup on the HUD. It pops in when any frog reaches a checkpoint (the
// checkpoint is the whole team's), stays a moment, then fades out. Purely local, like PlayerHUD:
// put it on the HUD, with the panel as a child.
public class CheckpointPopup : MonoBehaviour
{
    [Tooltip("The popup: a child panel with a Canvas Group (for the fade). Hidden until a checkpoint is reached.")]
    [SerializeField] CanvasGroup panel;
    [Tooltip("Optional: the text. {0} becomes the checkpoint's number.")]
    [SerializeField] TMP_Text text;
    [SerializeField] string message = "Checkpoint {0} reached!";
    [Tooltip("Seconds it stays fully shown.")]
    [SerializeField, Min(0f)] float showTime = 2.5f;
    [Tooltip("Seconds to fade in and out.")]
    [SerializeField, Min(0.01f)] float fadeTime = 0.35f;
    [Tooltip("How big it starts when it pops in (1 = no pop).")]
    [SerializeField, Min(0.1f)] float popScale = 1.3f;

    float shownAt = -100f;

    void Awake()
    {
        if (panel != null)
        {
            panel.alpha = 0f;
            panel.gameObject.SetActive(false);
        }
    }

    void OnEnable() => CheckpointManager.Reached += Show;
    void OnDisable() => CheckpointManager.Reached -= Show;

    void Show(CheckpointManager checkpoint, NetworkObject frog)
    {
        if (panel == null)
            return;
        if (text != null)
            text.text = string.Format(message, checkpoint.Number);
        shownAt = Time.unscaledTime;
        panel.gameObject.SetActive(true);
    }

    void Update()
    {
        if (panel == null || !panel.gameObject.activeSelf)
            return;
        float t = Time.unscaledTime - shownAt;
        float total = fadeTime + showTime + fadeTime;
        if (t >= total)
        {
            panel.alpha = 0f;
            panel.gameObject.SetActive(false);
            return;
        }
        float fadeIn = Mathf.Clamp01(t / fadeTime);
        float fadeOut = Mathf.Clamp01((total - t) / fadeTime);
        panel.alpha = Mathf.Min(fadeIn, fadeOut);
        float pop = Mathf.Lerp(popScale, 1f, Mathf.SmoothStep(0f, 1f, fadeIn));
        panel.transform.localScale = Vector3.one * pop;
    }
}