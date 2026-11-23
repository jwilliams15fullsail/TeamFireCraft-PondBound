using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Shows the level's active objectives: a marker over each one's target, with its distance, and
// a list of what each one says. The main objective uses Marker Image, optional ones use
// Optional Marker Image (or a tinted copy of Marker Image if that's empty). An objective can
// mark several places (Target plus More Targets, like two pressure plates); the marker is
// copied for each one. With Hide Done Targets, a plate that's pressed or a button that's on
// loses its marker, so only the places still to do are marked.
// The list is either one text (Objective Text) or, with a Row Template, one row per objective
// with an icon in front: Main Icon for the main objective, Optional Icon for optional ones, or
// the objective's own Icon when it has one.
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
    [Tooltip("Optional: a text that lists what the active objectives say. Not used when Row Template is set.")]
    public TMP_Text objectiveText;
    public Vector3 offset;
    [Tooltip("Subtract this amount from the camera-to-objective distance.")]
    public float distanceAdjustment = 3f;
    [Tooltip("A target that's a pressure plate already pressed, or a button already on, loses its marker until it's let go again.")]
    public bool hideDoneTargets = true;

    [Header("Objective list with icons (optional)")]
    [Tooltip("One line of the list: an object with a child Image (the icon) and a TMP text, inside a Vertical Layout Group. It's copied for each active objective. Empty: Objective Text is used instead.")]
    public RectTransform rowTemplate;
    [Tooltip("Icon in front of the main objective.")]
    public Sprite mainIcon;
    [Tooltip("Icon in front of optional objectives.")]
    public Sprite optionalIcon;
    [Tooltip("Tint for the optional icon. White keeps the picture's own colors.")]
    public Color optionalIconColor = Color.white;
    [Tooltip("Written before an optional objective's text in a row. Leave empty when the icon says it already.")]
    public string optionalPrefix = "Optional: ";

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

    // One line of the icon list.
    class Row
    {
        public GameObject root;
        public Image icon;
        public TMP_Text text;
    }

    Camera mainCamera;
    Pool mainPool, optionalPool;
    readonly List<ObjectiveSequence.Objective> active = new();
    readonly List<string> lines = new();
    readonly List<Row> rows = new();
    Color mainTextColor = Color.white, mainIconColor = Color.white;

    void Awake()
    {
        mainCamera = Camera.main;
        mainPool = NewPool(markerImage, meter, true, null);
        // No optional marker of its own: it uses tinted copies of the main one.
        optionalPool = optionalMarkerImage != null
            ? NewPool(optionalMarkerImage, optionalMeter, true, null)
            : NewPool(markerImage, meter, false, optionalColor);

        if (rowTemplate != null)
        {
            Row first = NewRow(rowTemplate.gameObject);
            if (first.text != null)
                mainTextColor = first.text.color;
            if (first.icon != null)
                mainIconColor = first.icon.color;
            rows.Add(first);
            rowTemplate.gameObject.SetActive(false);
            if (objectiveText != null)
                objectiveText.text = ""; // the rows replace the old one-text list
        }
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

    // The icon is the first Image below the row (so the row itself can have a background image).
    static Row NewRow(GameObject root)
    {
        var row = new Row { root = root, text = root.GetComponentInChildren<TMP_Text>(true) };
        foreach (Image image in root.GetComponentsInChildren<Image>(true))
        {
            if (image.gameObject != root)
            {
                row.icon = image;
                break;
            }
        }
        return row;
    }

    void Update()
    {
        active.Clear();
        if (sequence != null)
            sequence.GetActive(active);

        if (rowTemplate != null)
        {
            ShowRows();
        }
        else if (objectiveText != null)
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
                Pool pool = objective.optional ? optionalPool : mainPool;
                MarkIfNeeded(pool, objective.target);
                if (objective.moreTargets != null)
                {
                    foreach (Transform extra in objective.moreTargets)
                        MarkIfNeeded(pool, extra);
                }
            }
        }
        HideUnused(mainPool);
        HideUnused(optionalPool);
    }

    // One row per active objective (main first), copying the template when more are needed.
    void ShowRows()
    {
        Color optionalText = optionalColor;
        optionalText.a = 1f; // the marker tint may be see-through; the text shouldn't be

        int used = 0;
        foreach (ObjectiveSequence.Objective objective in active)
        {
            if (used >= rows.Count)
            {
                RectTransform copy = Instantiate(rowTemplate, rowTemplate.parent);
                copy.name = rowTemplate.name + " " + (rows.Count + 1);
                rows.Add(NewRow(copy.gameObject));
            }

            Row row = rows[used++];
            row.root.SetActive(true);
            // Keep rows in list order even after copies were added.
            if (row.root.transform.GetSiblingIndex() != rowTemplate.GetSiblingIndex() + used - 1)
                row.root.transform.SetSiblingIndex(rowTemplate.GetSiblingIndex() + used - 1);

            if (row.icon != null)
            {
                Sprite sprite = objective.icon != null ? objective.icon : objective.optional ? optionalIcon : mainIcon;
                row.icon.sprite = sprite;
                row.icon.enabled = sprite != null;
                row.icon.color = objective.optional && objective.icon == null ? optionalIconColor : mainIconColor;
            }
            if (row.text != null)
            {
                row.text.text = objective.optional ? optionalPrefix + objective.text : objective.text;
                row.text.color = objective.optional ? optionalText : mainTextColor;
            }
        }
        for (int i = used; i < rows.Count; i++)
        {
            if (rows[i].root.activeSelf)
                rows[i].root.SetActive(false);
        }
    }

    // A marker over this target, unless it's empty or (with Hide Done Targets) already done.
    void MarkIfNeeded(Pool pool, Transform target)
    {
        if (target == null || (hideDoneTargets && IsTargetDone(target)))
            return;
        Place(pool, target);
    }

    // A pressure plate that's pressed, or a wall button that's on, on the target or above it.
    static bool IsTargetDone(Transform target)
    {
        PressurePlate plate = target.GetComponentInParent<PressurePlate>();
        if (plate != null && plate.IsPressed)
            return true;
        WallButton button = target.GetComponentInParent<WallButton>();
        return button != null && button.IsActive;
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

        // The rect is in canvas units; lossyScale turns it into screen pixels at this resolution.
        Vector3 toPixels = marker.rectTransform.lossyScale;
        float minX = marker.GetPixelAdjustedRect().width * toPixels.x / 2f;
        float maxX = Screen.width - minX;
        float minY = marker.GetPixelAdjustedRect().height * toPixels.y / 2f;
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
