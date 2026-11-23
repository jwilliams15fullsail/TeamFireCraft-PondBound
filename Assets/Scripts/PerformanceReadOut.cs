using UnityEngine;

// The F3 / Show FPS readout. PerformanceManager switches this on only while it's showing:
// any enabled OnGUI costs time and memory every frame, even when it draws nothing.
public class PerformanceReadout : MonoBehaviour
{
    const float RefreshSeconds = 0.25f;

    GUIStyle style;
    string text = "";
    float nextRefresh;

    void Awake() => useGUILayout = false;

    void Update()
    {
        if (Time.unscaledTime < nextRefresh || PerformanceManager.Instance == null)
            return;
        nextRefresh = Time.unscaledTime + RefreshSeconds;
        text = PerformanceManager.Instance.ReadoutText();
    }

    void OnGUI()
    {
        if (Event.current.type != EventType.Repaint)
            return;
        style ??= new GUIStyle(GUI.skin.label);
        Matrix4x4 before = GUI.matrix;
        float size = Mathf.Max(1f, Screen.height / 1080f); // readable at 1440p and 4K
        GUI.matrix = Matrix4x4.Scale(new Vector3(size, size, 1f));
        GUI.Label(new Rect(10, 10, 420, 24), text, style);
        GUI.matrix = before;
    }
}
