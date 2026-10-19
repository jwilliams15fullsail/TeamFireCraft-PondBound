using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

// The local player's HUD: health, hunger and hydration bars, plus the level exit's
// "frogs at the exit" counter. Put one on a Screen Space canvas in each level.
// Purely local: it reads your own frog's FrogStats, which the host keeps up to date.
public class PlayerHUD : MonoBehaviour
{
    [Header("Bars (Image Type: Filled, Fill Method: Horizontal)")]
    [SerializeField] Image healthFill;
    [SerializeField] Image hungerFill;
    [SerializeField] Image hydrationFill;
    [SerializeField] float barSpeed = 1.5f;              // how fast a bar slides to its new value (full bars per second)
    [SerializeField, Range(0f, 1f)] float lowWarning = 0.25f; // a bar pulses below this

    [Header("Level exit (optional)")]
    [Tooltip("Hidden automatically in levels without a Level Exit.")]
    [SerializeField] TMP_Text exitCounter;

    FrogStats stats;
    LevelExit[] exits;

    void Start()
    {
        exits = FindObjectsByType<LevelExit>();
    }

    void Update()
    {
        if (stats == null)
            stats = FindLocalFrogStats();

        if (stats != null && stats.IsSpawned)
        {
            float dt = Time.unscaledDeltaTime;
            UpdateBar(healthFill, stats.HealthFraction, dt);
            UpdateBar(hungerFill, stats.HungerFraction, dt);
            UpdateBar(hydrationFill, stats.HydrationFraction, dt);
        }
        UpdateExitCounter();
    }

    static FrogStats FindLocalFrogStats()
    {
        NetworkManager manager = NetworkManager.Singleton;
        if (manager == null || !manager.IsClient || manager.SpawnManager == null)
            return null;
        NetworkObject player = manager.SpawnManager.GetLocalPlayerObject();
        return player != null ? player.GetComponent<FrogStats>() : null;
    }

    void UpdateBar(Image fill, float value, float dt)
    {
        if (fill == null)
            return;
        value = Mathf.Clamp01(value);
        fill.fillAmount = Mathf.MoveTowards(fill.fillAmount, value, barSpeed * dt);

        // Running low: pulse so it gets noticed.
        Color color = fill.color;
        color.a = value < lowWarning ? 0.45f + 0.55f * Mathf.Abs(Mathf.Sin(Time.unscaledTime * 5f)) : 1f;
        fill.color = color;
    }

    void UpdateExitCounter()
    {
        if (exitCounter == null)
            return;
        LevelExit exit = BusiestExit();
        bool show = exit != null;
        if (exitCounter.gameObject.activeSelf != show)
            exitCounter.gameObject.SetActive(show);
        if (!show)
            return;
        exitCounter.text = exit.SecondsLeft > 0f
            ? $"Next level in {exit.SecondsLeft:0}..."
            : $"Frogs at the exit: {exit.PlayersInside} / {exit.PlayersNeeded}";


        // With several exits, show the one counting down, otherwise the one with the most
        // frogs in it (the first one while they're all empty).
        LevelExit BusiestExit()
        {
            LevelExit best = null;
            foreach (LevelExit candidate in exits)
            {
                if (candidate == null || !candidate.IsSpawned)
                    continue;
                if (best == null || Busyness(candidate) > Busyness(best))
                    best = candidate;
            }
            return best;
        }

        static float Busyness(LevelExit exit) => exit.SecondsLeft > 0f ? 1000f : exit.PlayersInside;
    }
}