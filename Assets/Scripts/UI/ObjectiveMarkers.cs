using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shows the level's active objectives: a marker over each one's target, with its distance, and
// a list of what each one says. The main objective uses Marker Image, optional ones use
// Optional Marker Image (or a tinted copy of Marker Image if that's empty). If several optional
// objectives are active at once, the optional marker is copied for each extra one.
public class ObjectiveMarkers : MonoBehaviour
{
    [Tooltip("The main objective's marker.")]
    public Image markerImage;
    public ObjectiveSequence sequence;
    [Tooltip("The main marker's distance text: a child of Marker Image.")]
    public TMP_Text meter;

    [Header("Optional objectives")]
    [Tooltip("Optional: the optional objectives' marker, with its own picture. Empty: a tinted copy of Marker Image.")]
    public Image optionalMarkerImage;
    [Tooltip("Its distance text: a child of Optional Marker Image.")]
    public TMP_Text optionalMeter;
    [Tooltip("The color of optional lines in the text, and of the marker when it's a copy of the main one.")]
    public Color optionalColor = new Color(0.55f, 0.85f, 1f);

    [Header("Text and placement")]
    [Tooltip("Optional: a text that lists what the active objectives say.")]
    public TMP_Text objectiveText;
    public Vector3 offset;
    [Tooltip("Subtract this amount from the camera-to-objective distance.")]
    public float distanceAdjustment = 3f;

    // One kind of marker: the one set up in the Inspector, plus copies made when more are needed.
    class Pool
    {
        public Image template;
        public int meterIndex = -1; // which text under the image is the distance
        public Color? tint;         // set when the markers are recolored copies of the main one
        public readonly List<Image> images = new();
        public readonly List<TMP_Text> meters = new();
        public int used;
    }

    Camera mainCamera;
    Pool mainPool, optionalPool;
    readonly List<ObjectiveSequence.Objective> active = new();
    readonly List<string> lines = new();

    void Awake()
    {
        mainCamera = Camera.main;
        mainPool = NewPool(markerImage, meter, true, null);
        // No optional marker of its own: it uses tinted copies of the main one.
        optionalPool = optionalMarkerImage != null
            ? NewPool(optionalMarkerImage, optionalMeter, true, null)
            : NewPool(markerImage, meter, false, optionalColor);
    }

    // useTemplate: the template itself is the first marker (otherwise only copies of it are used).
    static Pool NewPool(Image template, TMP_Text distance, bool useTemplate, Color? tint)
    {
        var pool = new Pool { template = template, tint = tint };
        if (template == null)
            return pool;
        if (distance != null)
            pool.meterIndex = System.Array.IndexOf(template.GetComponentsInChildren<TMP_Text>(true), distance);
        if (useTemplate)
        {
            pool.images.Add(template);
            pool.meters.Add(distance);
        }
        return pool;
    }

    void Update()
    {
        active.Clear();
        if (sequence != null)
            sequence.GetActive(active);

        if (objectiveText != null)
        {
            lines.Clear();
            string optionalHex = ColorUtility.ToHtmlStringRGB(optionalColor);
            foreach (ObjectiveSequence.Objective objective in active)
                lines.Add(objective.optional ? $"<color=#{optionalHex}>Optional: {objective.text}</color>" : objective.text);
            objectiveText.text = string.Join("\n", lines);
        }

        if (mainCamera == null)
            mainCamera = Camera.main;

        mainPool.used = optionalPool.used = 0;
        if (mainCamera != null)
        {
            foreach (ObjectiveSequence.Objective objective in active)
            {
                if (objective.target != null)
                    Place(objective.optional ? optionalPool : mainPool, objective.target);
            }
        }
        HideUnused(mainPool);
        HideUnused(optionalPool);
    }

    static void HideUnused(Pool pool)
    {
        for (int i = pool.used; i < pool.images.Count; i++)
            pool.images[i].gameObject.SetActive(false);
    }

    // Puts the pool's next marker over a target, making another copy if they're all in use.
    void Place(Pool pool, Transform target)
    {
        if (pool.template == null)
            return;
        if (pool.used >= pool.images.Count)
        {
            Image copy = Instantiate(pool.template, pool.template.transform.parent);
            copy.name = pool.template.name + " " + (pool.images.Count + 1);
            if (pool.tint.HasValue)
                copy.color = pool.tint.Value;
            TMP_Text[] texts = copy.GetComponentsInChildren<TMP_Text>(true);
            pool.images.Add(copy);
            pool.meters.Add(pool.meterIndex >= 0 && pool.meterIndex < texts.Length ? texts[pool.meterIndex] : null);
        }

        Image marker = pool.images[pool.used];
        TMP_Text distance = pool.meters[pool.used];
        pool.used++;
        marker.gameObject.SetActive(true);

        float minX = marker.GetPixelAdjustedRect().width / 2f;
        float maxX = Screen.width - minX;
        float minY = marker.GetPixelAdjustedRect().height / 2f;
        float maxY = Screen.height - minY;

        Vector3 targetPosition = target.position + offset;
        Vector3 screenPosition = mainCamera.WorldToScreenPoint(targetPosition);

        if (Vector3.Dot(targetPosition - mainCamera.transform.position, mainCamera.transform.forward) < 0)
        {
            // Target is behind the camera.
            screenPosition.x = screenPosition.x < Screen.width / 2f ? maxX : minX;
        }

        screenPosition.x = Mathf.Clamp(screenPosition.x, minX, maxX);
        screenPosition.y = Mathf.Clamp(screenPosition.y, minY, maxY);
        marker.transform.position = screenPosition;

        if (distance != null)
        {
            float actualDistance = Vector3.Distance(targetPosition, mainCamera.transform.position);
            int displayedDistance = Mathf.Max(0, Mathf.RoundToInt(actualDistance - distanceAdjustment));
            distance.text = displayedDistance + "m";
        }
    }
}