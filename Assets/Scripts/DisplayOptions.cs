using System.Collections.Generic;
using UnityEngine;

// Resolution and display mode for the Settings menu.
// - Fullscreen: the window covers the monitor. A lower resolution is scaled up to fill it, with
//   bars if its shape is different from the monitor's.
// - Exclusive Fullscreen (Windows only): the monitor itself switches to that resolution.
// - Windowed: only sizes that fit on the desktop, so the title bar can't end up off screen.
// - Web: the browser owns the size, so there is only Fullscreen and Windowed.
// The Editor's Game view ignores all of this. Use the Game view's size dropdown to test shapes there.
public static class DisplayOptions
{
    public enum Mode { Fullscreen, Exclusive, Windowed }

    const string FullWidthKey = "Display.FullWidth";
    const string FullHeightKey = "Display.FullHeight";
    const string WindowWidthKey = "Display.WindowWidth";
    const string WindowHeightKey = "Display.WindowHeight";

    const int MinWidth = 1024;
    const int MinHeight = 720;
    const int WindowBorder = 16;
    const int WindowTitleBar = 40;

    static readonly Vector2Int[] WindowSizes =
    {
        new(3840, 2160), new(3440, 1440), new(2560, 1600), new(2560, 1440), new(2560, 1080), new(1920, 1200),
        new(1920, 1080), new(1680, 1050), new(1600, 900), new(1440, 900), new(1366, 768), new(1280, 800),
        new(1280, 720), new(1024, 768),
    };

    // Screen changes land a frame or more after they're asked for, so report the request until then.
    static Vector2Int requestedSize;
    static Mode requestedMode;
    static float requestedUntil;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetForNewRun()
    {
        requestedSize = default;
        requestedUntil = 0f;
    }

    public static bool IsWeb => Application.platform == RuntimePlatform.WebGLPlayer;
    public static bool CanChangeResolution => !IsWeb;

    static bool HasRequest => requestedSize.x > 0 && (Application.isEditor || Time.unscaledTime < requestedUntil);

    public static Mode CurrentMode
    {
        get
        {
            if (HasRequest)
                return requestedMode;
            if (IsWeb)
                return Screen.fullScreen ? Mode.Fullscreen : Mode.Windowed;
            return Screen.fullScreenMode switch
            {
                FullScreenMode.Windowed => Mode.Windowed,
                FullScreenMode.ExclusiveFullScreen => Mode.Exclusive,
                _ => Mode.Fullscreen,
            };
        }
    }

    public static Vector2Int CurrentSize => HasRequest ? requestedSize : new Vector2Int(Screen.width, Screen.height);

    public static List<Mode> Modes()
    {
        var modes = new List<Mode> { Mode.Fullscreen };
        if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            modes.Add(Mode.Exclusive);
        modes.Add(Mode.Windowed);
        return modes;
    }

    public static string NameOf(Mode mode) => mode == Mode.Exclusive ? "Exclusive Fullscreen" : mode.ToString();

    // The monitor the window is on.
    public static Vector2Int MonitorSize
    {
        get
        {
            if (!IsWeb && !Application.isEditor)
            {
                DisplayInfo info = Screen.mainWindowDisplayInfo;
                if (info.width > 0 && info.height > 0)
                    return new Vector2Int(info.width, info.height);
            }
            Resolution current = Screen.currentResolution;
            return new Vector2Int(current.width, current.height);
        }
    }

    static Vector2Int LargestWindow
    {
        get
        {
            Vector2Int area = MonitorSize;
            if (!IsWeb && !Application.isEditor)
            {
                RectInt work = Screen.mainWindowDisplayInfo.workArea; // the desktop without the taskbar
                if (work.width > 0 && work.height > 0)
                    area = new Vector2Int(work.width, work.height);
            }
            return new Vector2Int(area.x - WindowBorder, area.y - WindowTitleBar);
        }
    }

    // The sizes that work in this mode, biggest first.
    public static List<Vector2Int> Sizes(Mode mode)
    {
        var found = new HashSet<Vector2Int>();
        Vector2Int limit = mode == Mode.Windowed ? LargestWindow : MonitorSize;

        void Add(Vector2Int size)
        {
            if (size.x >= MinWidth && size.y >= MinHeight && size.x <= limit.x && size.y <= limit.y)
                found.Add(size);
        }

        if (mode == Mode.Windowed)
        {
            foreach (Vector2Int size in WindowSizes)
                Add(size);
        }
        else
        {
            Add(limit);
            foreach (Resolution resolution in Screen.resolutions)
                Add(new Vector2Int(resolution.width, resolution.height));
        }

        if (found.Count == 0)
            found.Add(limit); // a very small screen

        var sizes = new List<Vector2Int>(found);
        sizes.Sort((a, b) => b.x != a.x ? b.x.CompareTo(a.x) : b.y.CompareTo(a.y));
        return sizes;
    }

    public static void SetSize(Vector2Int size)
    {
        if (CanChangeResolution)
            Apply(size, CurrentMode);
    }

    public static void SetMode(Mode mode)
    {
        if (IsWeb)
        {
            Screen.fullScreen = mode != Mode.Windowed;
            return;
        }
        Apply(SizeFor(mode), mode);
    }

    // When switching mode: the size last picked in that mode, if it still fits.
    static Vector2Int SizeFor(Mode mode)
    {
        bool window = mode == Mode.Windowed;
        var saved = new Vector2Int(
            PlayerPrefs.GetInt(window ? WindowWidthKey : FullWidthKey, 0),
            PlayerPrefs.GetInt(window ? WindowHeightKey : FullHeightKey, 0));

        List<Vector2Int> sizes = Sizes(mode);
        if (sizes.Contains(saved))
            return saved;

        Vector2Int monitor = MonitorSize;
        if (!window)
            return sizes.Contains(monitor) ? monitor : sizes[0];

        // First time windowed: clearly a window, not nearly the whole screen.
        foreach (Vector2Int size in sizes)
            if (size.x <= monitor.x * 0.85f && size.y <= monitor.y * 0.85f)
                return size;
        return sizes[sizes.Count - 1];
    }

    static void Apply(Vector2Int size, Mode mode)
    {
        requestedSize = size;
        requestedMode = mode;
        requestedUntil = Time.unscaledTime + 1f;

        bool window = mode == Mode.Windowed;
        PlayerPrefs.SetInt(window ? WindowWidthKey : FullWidthKey, size.x);
        PlayerPrefs.SetInt(window ? WindowHeightKey : FullHeightKey, size.y);
        PlayerPrefs.Save();

        FullScreenMode screenMode = mode switch
        {
            Mode.Windowed => FullScreenMode.Windowed,
            Mode.Exclusive => FullScreenMode.ExclusiveFullScreen,
            _ => FullScreenMode.FullScreenWindow,
        };
        Screen.SetResolution(size.x, size.y, screenMode);
        Debug.Log($"[Display] {size.x} x {size.y} {screenMode}" + (Application.isEditor ? " (takes effect in a build, not in the Editor)" : ""));
    }

    // Unity reopens at the last size. If that window no longer fits on the desktop, shrink it.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void KeepWindowOnScreen()
    {
        if (IsWeb || Application.isEditor || Application.isBatchMode || Screen.fullScreenMode != FullScreenMode.Windowed)
            return;
        Vector2Int limit = LargestWindow;
        if (limit.x >= MinWidth && (Screen.width > limit.x || Screen.height > limit.y))
            Apply(Sizes(Mode.Windowed)[0], Mode.Windowed);
    }
}
