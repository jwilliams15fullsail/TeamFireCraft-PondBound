using UnityEngine;
using UnityEngine.UI;

// Added by ScreenFitter to full-screen panels. On screens that aren't 16:9 it fills the space
// beside (or above and below) the panel by carrying the picture's outermost pixels outward.
// The fill blocks clicks, like the panel does.
public class ScreenEdgeFill : MonoBehaviour
{
    const float Inset = 2f;   // pixels in from the picture's edge, past any soft border
    const float Overlap = 1f; // hides the hairline seam

    Image picture;
    RectTransform panel, screen;
    readonly RawImage[] bars = new RawImage[4]; // left, right, bottom, top
    Vector2 lastScreen;
    Sprite lastSprite;

    public static void AddTo(Image picture, RectTransform screen)
    {
        var fill = picture.gameObject.AddComponent<ScreenEdgeFill>();
        fill.Build(picture, screen);
    }

    void Build(Image source, RectTransform screenRect)
    {
        picture = source;
        panel = (RectTransform)source.transform;
        screen = screenRect;

        for (int i = 0; i < bars.Length; i++)
        {
            var go = new GameObject("Screen Edge", typeof(RectTransform), typeof(RawImage), typeof(LayoutElement));
            go.layer = gameObject.layer;
            go.GetComponent<LayoutElement>().ignoreLayout = true;
            var rect = (RectTransform)go.transform;
            rect.SetParent(panel, false);
            rect.SetAsFirstSibling(); // under everything else on the panel
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0.5f, 0.5f);
            bars[i] = go.GetComponent<RawImage>();
        }
        Refresh();
    }

    void OnEnable() => Refresh();

    void LateUpdate() => Refresh();

    public void Refresh()
    {
        if (picture == null || screen == null)
            return;

        Sprite sprite = picture.overrideSprite;
        Color color = picture.color;
        bool show = picture.enabled && sprite != null;
        foreach (RawImage bar in bars)
        {
            if (bar == null)
                return;
            if (bar.color != color)
                bar.color = color; // follows fades
        }

        Vector2 screenSize = screen.rect.size;
        Vector2 size = panel.rect.size;
        Vector2 extra = (screenSize - size) * 0.5f;
        SetShown(0, show && extra.x > 0.5f);
        SetShown(1, show && extra.x > 0.5f);
        SetShown(2, show && extra.y > 0.5f);
        SetShown(3, show && extra.y > 0.5f);

        if (!show || (screenSize == lastScreen && sprite == lastSprite))
            return;
        lastScreen = screenSize;
        lastSprite = sprite;

        Texture texture = sprite.texture;
        Rect art = sprite.rect;
        float tw = texture.width, th = texture.height;
        float left = (art.xMin + Inset) / tw;
        float right = (art.xMax - Inset) / tw;
        float bottom = (art.yMin + Inset) / th;
        float top = (art.yMax - Inset) / th;

        float barWidth = Mathf.Max(0f, extra.x) + Overlap;
        float barHeight = Mathf.Max(0f, extra.y) + Overlap;
        float x = size.x * 0.5f + barWidth * 0.5f - Overlap;
        float y = size.y * 0.5f + barHeight * 0.5f - Overlap;

        Place(0, texture, new Vector2(-x, 0f), new Vector2(barWidth, size.y), new Rect(left, bottom, 0f, top - bottom));
        Place(1, texture, new Vector2(x, 0f), new Vector2(barWidth, size.y), new Rect(right, bottom, 0f, top - bottom));
        Place(2, texture, new Vector2(0f, -y), new Vector2(size.x, barHeight), new Rect(left, bottom, right - left, 0f));
        Place(3, texture, new Vector2(0f, y), new Vector2(size.x, barHeight), new Rect(left, top, right - left, 0f));
    }

    void SetShown(int index, bool shown)
    {
        if (bars[index].enabled != shown)
            bars[index].enabled = shown;
    }

    void Place(int index, Texture texture, Vector2 position, Vector2 size, Rect uv)
    {
        RawImage bar = bars[index];
        bar.texture = texture;
        bar.uvRect = uv;
        bar.rectTransform.anchoredPosition = position;
        bar.rectTransform.sizeDelta = size;
    }
}
