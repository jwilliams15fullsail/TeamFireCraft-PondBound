using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

// A "How to Play" panel with one page per topic and Previous / Next buttons. Works in the main
// menu, the pause menu and the intro cutscene.
// Every panel shows the same pages, written below in DefaultPages, so a change there shows up
// everywhere. To give one panel its own pages instead, tick Use Custom Pages and fill in Pages.
// In the cutscene: hook On Finished up to IntroController.NextPage. Next then stays clickable on
// the last page and carries on into the game (its label changes to Last Page Next Label).
public class HowToPlay : MonoBehaviour
{
    [Serializable]
    public class Page
    {
        public string title;
        [TextArea(6, 20)] public string body;
    }

    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text bodyText;
    [Tooltip("Optional: shows which page this is, like 2 / 9.")]
    [SerializeField] TMP_Text pageNumberText;
    [SerializeField] Button previousButton;
    [SerializeField] Button nextButton;

    [Header("Pages")]
    [Tooltip("Off: the game's standard pages (the same on every How to Play panel). On: this panel shows the Pages below instead.")]
    [SerializeField] bool useCustomPages;
    [SerializeField] Page[] pages = DefaultPages();

    [Header("Cutscene (optional)")]
    [Tooltip("Runs when Next is clicked on the last page, e.g. IntroController.NextPage to start the game. Leave empty in the menus.")]
    [SerializeField] UnityEvent onFinished = new();
    [Tooltip("Optional: the Next button's text, so it can say something else on the last page.")]
    [SerializeField] TMP_Text nextButtonText;
    [SerializeField] string lastPageNextLabel = "Let's Go!";

    static Page[] standard;
    Page[] shown;
    string nextLabel;
    int index;

    Page[] Pages => useCustomPages && pages != null && pages.Length > 0 ? pages : standard ??= DefaultPages();
    bool HasFinish => onFinished != null && onFinished.GetPersistentEventCount() > 0;

    // Always opens on the first page.
    void OnEnable()
    {
        shown = Pages;
        if (nextButtonText != null && nextLabel == null)
            nextLabel = nextButtonText.text;
        index = 0;
        Show();
    }

    public void NextPage()
    {
        if (shown == null)
            return;
        if (index >= shown.Length - 1)
        {
            if (HasFinish)
                onFinished.Invoke();
            return;
        }
        index++;
        Show();
    }

    public void PreviousPage()
    {
        index = Mathf.Max(index - 1, 0);
        Show();
    }

    void Show()
    {
        if (shown == null || shown.Length == 0)
            return;
        Page page = shown[index];
        bool last = index == shown.Length - 1;
        if (titleText != null)
            titleText.text = page.title;
        if (bodyText != null)
            bodyText.text = page.body;
        if (pageNumberText != null)
            pageNumberText.text = $"{index + 1} / {shown.Length}";
        if (previousButton != null)
            previousButton.interactable = index > 0;
        if (nextButton != null)
            nextButton.interactable = !last || HasFinish;
        if (nextButtonText != null && nextLabel != null)
            nextButtonText.text = last && HasFinish ? lastPageNextLabel : nextLabel;
    }

    // Keyboard and mouse first, then the controller in brackets.
    static Page[] DefaultPages() => new[]
    {
        new Page
        {
            title = "Escape the Lab",
            body = "You and up to 3 friends are lab frogs on the run. Work your way through the lab and the sewers "
                 + "and get back to the pond.\n\n"
                 + "The <b>objective list</b> on the HUD says what to do next, and the <b>objective marker</b> "
                 + "points the way. Optional objectives are extras.\n\n"
                 + "A level is finished when <b>every frog</b> is standing in the exit together.",
        },
        new Page
        {
            title = "Moving",
            body = "<b>WASD</b> (Left Stick): move\n"
                 + "<b>Mouse</b> (Right Stick): look around\n"
                 + "<b>Hold Right Click</b>: look around without turning your frog\n\n"
                 + "<b>Left Shift</b> (press Left Stick): switch between <b>running</b> and <b>hopping</b>. "
                 + "Running is quick and quiet; hopping clears small bumps but tires you out faster.",
        },
        new Page
        {
            title = "Charge Jump",
            body = "<b>Hold Space</b> (A) to charge, <b>let go</b> to jump. The longer you hold, the higher and farther you go.\n\n"
                 + "While charging, a <b>landing marker</b> shows where you'll come down and the arc shows the path. "
                 + "Move the mouse to put the marker where you want it: on a floor, a desk top or a wall.\n\n"
                 + "Land on a wall and your frog clings to it. Every jump costs a little hunger; big ones cost more.",
        },
        new Page
        {
            title = "Climbing",
            body = "Jump onto a wall to cling to it, or press <b>E</b> (X) next to one.\n\n"
                 + "<b>WASD</b>: climb up, down, sideways, around corners and over the top\n"
                 + "<b>Space</b> (A): jump off the wall\n"
                 + "<b>E</b> (X): let go and drop\n\n"
                 + "Your toes can only hold so much: too much pulling (like a teammate hanging off your tongue) and you slip.",
        },
        new Page
        {
            title = "Your Tongue",
            body = "<b>Left Click</b> (Right Trigger): shoot your tongue at the crosshair. The crosshair changes when "
                 + "something is <b>in range</b> and when your tongue is <b>locked on</b>.\n\n"
                 + "<b>Food and flies</b>: reeled in and eaten.\n"
                 + "<b>Boxes, objects and frogs</b>: your tongue sticks. Walk away to drag them, "
                 + "<b>scroll</b> (hold Left Trigger) to reel them in. Click again to let go.\n"
                 + "<b>Buttons and valves</b>: hit them to switch them.\n\n"
                 + "<b>Q</b> (B): break free when someone's tongue has you.\n"
                 + "Eat a glowing <b>firefly</b> to electrify your tongue: it stuns enemies.",
        },
        new Page
        {
            title = "Staying Alive",
            body = "Keep an eye on your three bars:\n\n"
                 + "<b>Health</b>: slowly heals while hunger and hydration are both over half.\n"
                 + "<b>Hunger</b>: drops as you move and jump. Eat flies and food. A hungry frog can't jump as high.\n"
                 + "<b>Hydration</b>: drops over time. Drink from water drips. Run dry and you start losing health.\n\n"
                 + "A bar flashes when it's getting low. Reaching a <b>checkpoint</b> fills everything up.",
        },
        new Page
        {
            title = "Hazards",
            body = "<b>Lasers</b>: switch on and off in a rhythm. Watch the timing and go through when they're off.\n"
                 + "<b>Room timers</b>: when the countdown on the HUD runs out, lasers drop from the ceiling. Get out first!\n"
                 + "<b>Steam vents</b>: puff a warning before they blast. Don't stand in them.\n"
                 + "<b>Floods</b>: frogs float. Swim to the edge and jump out.\n\n"
                 + "Buttons and valves can often switch a hazard off for the whole team.",
        },
        new Page
        {
            title = "Checkpoints",
            body = "Glowing <b>checkpoints</b> sit along the way. When any frog touches one it turns green, "
                 + "and it becomes the whole team's respawn point. The game is saved there too.\n\n"
                 + "If your frog dies, wait for the countdown, then press <b>R</b> to respawn. "
                 + "The level restarts from the last checkpoint.\n\n"
                 + "<b>Continue</b> on the main menu takes you back to your last checkpoint, even after quitting. "
                 + "The pause menu's <b>Restart from Checkpoint</b> does it any time.",
        },
        new Page
        {
            title = "Teamwork",
            body = "<b>Pressure plates</b> hold doors open while there's enough weight on them. Drag boxes on with your tongue; "
                 + "heavy ones are easier to pull together.\n"
                 + "<b>Buttons</b> open doors and shut off hazards for everyone.\n"
                 + "<b>Help each other</b>: catch a falling friend with your tongue, or pull them up a ledge.\n\n"
                 + "<b>Esc</b> (Start): pause menu. Online, the game keeps going for everyone else.\n"
                 + "<b>Tab</b>: free the mouse without pausing.",
        },
    };
}
