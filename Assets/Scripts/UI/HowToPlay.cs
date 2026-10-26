using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A "How to Play" panel with one page per topic and Previous / Next buttons.
// The pages start filled in with the game's controls and rules: edit them in the
// Inspector whenever a mechanic changes. Works in the pause menu or the main menu.
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
    [Tooltip("Optional: shows which page this is, like 2 / 7.")]
    [SerializeField] TMP_Text pageNumberText;
    [SerializeField] Button previousButton;
    [SerializeField] Button nextButton;
    [SerializeField] Page[] pages = DefaultPages();

    int index;

    // Always opens on the first page.
    void OnEnable()
    {
        index = 0;
        Show();
    }

    public void NextPage()
    {
        index = Mathf.Min(index + 1, pages.Length - 1);
        Show();
    }

    public void PreviousPage()
    {
        index = Mathf.Max(index - 1, 0);
        Show();
    }

    void Show()
    {
        if (pages == null || pages.Length == 0)
            return;
        Page page = pages[index];
        if (titleText != null)
            titleText.text = page.title;
        if (bodyText != null)
            bodyText.text = page.body;
        if (pageNumberText != null)
            pageNumberText.text = $"{index + 1} / {pages.Length}";
        if (previousButton != null)
            previousButton.interactable = index > 0;
        if (nextButton != null)
            nextButton.interactable = index < pages.Length - 1;
    }

    static Page[] DefaultPages() => new[]
    {
        new Page
        {
            title = "The Goal",
            body = "Work together to get every frog through the level.\n\n"
                 + "When all the frogs in the game are standing in the level exit, a short countdown starts and "
                 + "everyone moves on to the next level. The HUD shows how many frogs are at the exit.\n\n"
                 + "Play online with up to 4 players, or offline on your own.",
        },
        new Page
        {
            title = "Moving",
            body = "<b>WASD</b> / <b>Left Stick</b>: hop around\n"
                 + "<b>Mouse</b> / <b>Right Stick</b>: look around\n"
                 + "<b>Mouse Wheel</b>: zoom the camera in and out\n\n"
                 + "Your frog hops the way you push. Small pushes make small, slow hops.",
        },
        new Page
        {
            title = "Charge Jump",
            body = "<b>Hold Space</b> / <b>A</b> to charge, <b>let go</b> to jump. The longer you hold, the higher and farther you go.\n\n"
                 + "Hold a direction while charging to aim: your frog turns to face it and leaps that way. "
                 + "Charge without a direction to jump straight up.\n\n"
                 + "Every jump uses up some hunger, and big jumps use more.",
        },
        new Page
        {
            title = "Climbing",
            body = "Jump into a climbable wall and your frog grabs on by itself. "
                 + "You can also press <b>E</b> / <b>X</b> next to one.\n\n"
                 + "<b>WASD</b>: climb up, down and sideways, around corners and over the top\n"
                 + "<b>Space</b> / <b>A</b>: jump off the wall\n"
                 + "<b>E</b> / <b>X</b>: let go and drop\n\n"
                 + "Your toes can only hold so much. Too much pulling on you (like a teammate hanging on your tongue) and you slip off.",
        },
        new Page
        {
            title = "Tongue",
            body = "<b>Left Click</b> / <b>Right Trigger</b>: shoot your tongue at whatever is under the crosshair\n\n"
                 + "<b>Flies and food</b>: your tongue reels them in and you eat them, which refills hunger.\n"
                 + "<b>Boxes, objects and frogs</b>: your tongue sticks. Walk away to drag them, and hold "
                 + "<b>Right Click</b> / <b>Left Trigger</b> to reel them closer. Click again to let go.\n"
                 + "<b>Wall buttons</b>: hit them with your tongue to press them.\n\n"
                 + "<b>Q</b> / <b>B</b>: break free when someone's tongue is holding you.",
        },
        new Page
        {
            title = "Staying Alive",
            body = "<b>Health</b>: slowly heals while hunger and hydration are both over half full.\n"
                 + "<b>Hunger</b>: goes down when you jump. Eat flies and food to fill it. "
                 + "A hungry frog can't jump as high.\n"
                 + "<b>Hydration</b>: goes down over time. Catch water drops to fill it. "
                 + "When it runs out you start losing health.\n\n"
                 + "Some spots in a level refill everything. If your health runs out, you start again with full bars.",
        },
        new Page
        {
            title = "Teamwork and Puzzles",
            body = "<b>Pressure plates</b> open doors while enough weight is on them. Drag boxes onto them with your tongue. "
                 + "Heavy boxes are easier to move together.\n"
                 + "<b>Doors and buttons</b> open for everyone at once.\n"
                 + "<b>Help each other</b>: catch a falling teammate with your tongue, or pull them up a ledge.\n\n"
                 + "<b>Esc</b> / <b>Start</b>: pause menu. Online, the game keeps going for everyone else while you're paused.\n"
                 + "<b>Tab</b>: free the mouse without pausing.",
        },
    };
}