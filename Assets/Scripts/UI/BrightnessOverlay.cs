using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// The Brightness setting. A see-through layer over the whole screen that darkens or brightens
// everything, menus included. It makes itself, so it needs no scene setup, and it works with
// post-processing off. At 50 (normal) it's switched off and costs nothing.
// Needs Assets/Resources/BrightnessOverlay.shader.
public class BrightnessOverlay : MonoBehaviour
{
    const string Key = "BrightnessPercent"; // a new key: the old one saved 100 as its normal value
    const float Normal = 50f;
    const float Darkest = 0.4f;
    const float Brightest = 1.8f;

    static readonly int AmountId = Shader.PropertyToID("_Amount");
    static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
    static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");

    static BrightnessOverlay instance;

    Canvas canvas;
    Material material;

    public static float Percent
    {
        get => Mathf.Clamp(PlayerPrefs.GetFloat(Key, Normal), 0f, 100f);
        set
        {
            PlayerPrefs.SetFloat(Key, Mathf.Clamp(value, 0f, 100f));
            if (instance != null)
                instance.Apply();
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (instance != null)
            return;
        var shader = Resources.Load<Shader>("BrightnessOverlay");
        if (shader == null || !shader.isSupported)
        {
            Debug.LogWarning("[Display] Brightness needs Assets/Resources/BrightnessOverlay.shader.");
            return;
        }
        var go = new GameObject("Brightness Overlay", typeof(RectTransform));
        DontDestroyOnLoad(go);
        instance = go.AddComponent<BrightnessOverlay>();
        instance.Build(shader);
    }

    void Build(Shader shader)
    {
        canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue; // above the menus and their dropdown lists

        material = new Material(shader);
        var layer = new GameObject("Layer", typeof(RectTransform), typeof(Image));
        var rect = (RectTransform)layer.transform;
        rect.SetParent(transform, false);
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = rect.offsetMax = Vector2.zero;

        var image = layer.GetComponent<Image>();
        image.material = material;
        image.raycastTarget = false; // clicks go through
        Apply();
    }

    void Apply()
    {
        float percent = Percent;
        canvas.enabled = !Mathf.Approximately(percent, Normal);

        if (percent < Normal)
        {
            // screen x amount
            material.SetFloat(SrcBlendId, (float)BlendMode.Zero);
            material.SetFloat(DstBlendId, (float)BlendMode.SrcColor);
            material.SetFloat(AmountId, Mathf.Lerp(Darkest, 1f, percent / Normal));
        }
        else
        {
            // screen + screen x amount
            material.SetFloat(SrcBlendId, (float)BlendMode.DstColor);
            material.SetFloat(DstBlendId, (float)BlendMode.One);
            material.SetFloat(AmountId, (Brightest - 1f) * (percent - Normal) / Normal);
        }
    }

    void OnDestroy()
    {
        if (material != null)
            Destroy(material);
        if (instance == this)
            instance = null;
    }
}
