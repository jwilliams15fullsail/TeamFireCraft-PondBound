using System;
using UnityEngine;

// Draws a script's on-screen text (Unity's old OnGUI) only while it's switched on. Any enabled
// script with an OnGUI makes Unity run its whole IMGUI pass every frame, about 17 KB of garbage
// and a few calls a frame, even when nothing gets drawn. So scripts keep their drawing here and
// switch it on only while there is something to show; the rest of the time it costs nothing.
// Added by code (FrogCamera, FrogNetworkDriver, NetworkSessionUI); nothing to set up.
[AddComponentMenu("")]
public class GuiOverlay : MonoBehaviour
{
    Action draw;

    // A switched-off overlay on the owner's GameObject. Layout: on only for GUILayout code.
    public static GuiOverlay Create(MonoBehaviour owner, Action draw, bool layout = false)
    {
        GuiOverlay overlay = owner.gameObject.AddComponent<GuiOverlay>();
        overlay.hideFlags = HideFlags.HideInInspector;
        overlay.draw = draw;
        overlay.useGUILayout = layout; // no layout pass for plain labels
        overlay.enabled = false;
        return overlay;
    }

    public void Show(bool show)
    {
        if (enabled != show)
            enabled = show;
    }

    void OnGUI() => draw?.Invoke();
}
