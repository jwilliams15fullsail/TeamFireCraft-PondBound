using FishNet.Managing;
using FishNet.Managing.Scened;
using UnityEngine;
using UnityEngine.UI;

// The full-screen picture shown while a level loads, with a row of lily pads as its load bar.
// Purely local: each player sees it while their own machine loads, and it stays up until their
// frog is in the level. Lives on a canvas under the FishNet NetworkManager, which survives the
// scene change, and goes away with the manager when the player leaves to the menu.
public class LoadingScreen : MonoBehaviour
{
    // The last stretch of the bar is for your frog arriving, after the scene itself has loaded.
    const float SceneShareOfBar = 0.9f;

    [Tooltip("The full-screen Image: a child of this object.")]
    [SerializeField] GameObject picture;
    [Tooltip("Seconds it stays up after your frog appears, to hide the level's first slow frames.")]
    [SerializeField] float holdSeconds = 1f;
    [Tooltip("Hides anyway this long after loading ends, in case your frog never appears.")]
    [SerializeField] float giveUpSeconds = 15f;

    [Header("Lily pad load bar")]
    [Tooltip("The parent of the lily pad Images. They fill in Hierarchy order, top one first.")]
    [SerializeField] Transform padRow;
    [Tooltip("How a lily pad looks until the bar reaches it.")]
    [SerializeField] Color emptyPad = new Color(0f, 0f, 0f, 0.35f);
    [Tooltip("The fastest the bar can fill, in whole bars per second, so big jumps in progress stay smooth.")]
    [SerializeField] float fillSpeed = 1.5f;

    NetworkManager manager;
    Image[] pads;
    bool loading;
    float hideAt = -1f;
    float giveUpAt;
    float progress, shownProgress;

    // Start, not Awake: the NetworkManager creates its SceneManager in its own Awake.
    void Start()
    {
        pads = padRow.GetComponentsInChildren<Image>(true);
        picture.SetActive(false);
        manager = GetComponentInParent<NetworkManager>();
        manager.SceneManager.OnLoadStart += OnLoadStart;
        manager.SceneManager.OnLoadPercentChange += OnLoadPercentChange;
        manager.SceneManager.OnLoadEnd += OnLoadEnd;
    }

    void OnDestroy()
    {
        if (manager == null || manager.SceneManager == null)
            return;
        manager.SceneManager.OnLoadStart -= OnLoadStart;
        manager.SceneManager.OnLoadPercentChange -= OnLoadPercentChange;
        manager.SceneManager.OnLoadEnd -= OnLoadEnd;
    }

    void OnLoadStart(SceneLoadStartEventArgs args)
    {
        // The host gets this twice for one level, so only a fresh load empties the bar.
        if (!picture.activeSelf)
        {
            progress = shownProgress = 0f;
            ShowPads();
            picture.SetActive(true);
        }
        loading = true;
    }

    void OnLoadPercentChange(SceneLoadPercentEventArgs args)
    {
        progress = Mathf.Max(progress, args.Percent * SceneShareOfBar);
    }

    void OnLoadEnd(SceneLoadEndEventArgs args)
    {
        loading = false;
        hideAt = -1f;
        giveUpAt = Time.unscaledTime + giveUpSeconds;
    }

    void Update()
    {
        if (!picture.activeSelf)
            return;

        if (!loading)
        {
            float now = Time.unscaledTime;
            if (hideAt < 0f && PlayerHUD.FindLocalFrogStats() != null)
            {
                hideAt = now + holdSeconds;
                progress = 1f;
            }
            if (now >= giveUpAt || (hideAt >= 0f && now >= hideAt))
            {
                picture.SetActive(false);
                return;
            }
        }

        shownProgress = Mathf.MoveTowards(shownProgress, progress, fillSpeed * Time.unscaledDeltaTime);
        ShowPads();
    }

    // Each pad has its own slice of the bar and brightens as the bar passes through it.
    void ShowPads()
    {
        for (int i = 0; i < pads.Length; i++)
            pads[i].color = Color.Lerp(emptyPad, Color.white, shownProgress * pads.Length - i);
    }
}