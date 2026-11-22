using UnityEngine;
using UnityEngine.UI;

// Category tabs for the Settings panel (Display, Graphics, Performance, Audio, Controls...).
// Each tab is a button and a page. Clicking a button shows its page and hides the others.
// Put it on the Settings panel and fill in the Tabs list: the button, the page, and optionally
// an underline / highlight object that shows only on the selected tab.
// The settings scripts (settingsController, GraphicsSettingsMenu, VolumeSettings) stay on the
// Settings panel itself, not on a page, so they work whichever page is showing.
public class SettingsTabs : MonoBehaviour
{
    [System.Serializable]
    public class Tab
    {
        [Tooltip("Only for your own reference in the Inspector.")]
        public string name;
        public Button button;
        [Tooltip("The page of settings this tab shows.")]
        public GameObject page;
        [Tooltip("Optional: an underline or glow shown only while this tab is selected.")]
        public GameObject selectedMark;
    }

    [SerializeField] Tab[] tabs;
    [Tooltip("Tint the tab buttons so the selected one stands out. Turn off if your buttons already have their own look.")]
    [SerializeField] bool tintButtons = true;
    [SerializeField] Color selectedColor = new Color(1f, 1f, 1f, 1f);
    [SerializeField] Color normalColor = new Color(1f, 1f, 1f, 0.45f);
    [Tooltip("Open on the tab the player used last. Off: always open on the first tab.")]
    [SerializeField] bool rememberLastTab = true;

    const string LastTabKey = "Settings.LastTab";

    int current = -1;

    void Awake()
    {
        for (int i = 0; i < tabs.Length; i++)
        {
            int index = i; // each button needs its own copy
            if (tabs[i].button != null)
                tabs[i].button.onClick.AddListener(() => Show(index));
        }
    }

    void OnEnable()
    {
        Show(rememberLastTab ? PlayerPrefs.GetInt(LastTabKey, 0) : 0);
    }

    // Also callable from a button's OnClick, e.g. Show(0) to jump to the first tab.
    public void Show(int index)
    {
        if (tabs == null || tabs.Length == 0)
            return;
        current = Mathf.Clamp(index, 0, tabs.Length - 1);

        for (int i = 0; i < tabs.Length; i++)
        {
            bool selected = i == current;
            if (tabs[i].page != null)
                tabs[i].page.SetActive(selected);
            if (tabs[i].selectedMark != null)
                tabs[i].selectedMark.SetActive(selected);
            if (tintButtons && tabs[i].button != null && tabs[i].button.targetGraphic != null)
                tabs[i].button.targetGraphic.color = selected ? selectedColor : normalColor;
        }

        if (rememberLastTab)
            PlayerPrefs.SetInt(LastTabKey, current);
    }

    // For keyboard / gamepad shortcuts (e.g. Q and E, or the shoulder buttons).
    public void ShowNext() => Show((current + 1) % tabs.Length);
    public void ShowPrevious() => Show((current - 1 + tabs.Length) % tabs.Length);
}