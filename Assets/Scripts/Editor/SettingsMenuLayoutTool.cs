using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Tidies the tabbed Settings menu: one row per setting, label on the left, control on the right.
//   Tools > Settings Menu > 1. Fix Rows: edits Settings (NEW).prefab.
//   Tools > Settings Menu > 2. Clear Old Layout Overrides: run in each scene or prefab that uses
//   the menu, so old saved positions there stop hiding the fixed layout.
// Objects are moved, not recreated, so the settings scripts keep their links. Safe to run again.
public static class SettingsMenuLayoutTool
{
    const string PrefabPath = "Assets/Prefabs/UI/Settings (NEW).prefab";
    const string UndoName = "Fix Settings Menu";

    static readonly Vector2 TabBarPosition = new Vector2(0f, -220f);
    static readonly Vector2 TabBarSize = new Vector2(1250f, 100f);
    static readonly Vector2 TabSize = new Vector2(210f, 100f);
    static readonly Vector2 PagesPosition = new Vector2(0f, -70f);
    static readonly Vector2 PagesSize = new Vector2(1100f, 600f);

    const float RowHeight = 64f;
    const float LabelWidth = 420f;
    const float ValueWidth = 110f;
    const float SliderHeight = 30f;
    const float DropdownHeight = 56f;
    const float ToggleSize = 56f;
    const float DropdownItemHeight = 44f;
    const float LabelFontSize = 40f;
    const float DropdownFontSize = 28f;

    // Label text by row name (lower case, letters only, without "row"). Other rows use their name.
    static readonly Dictionary<string, string> RowTitles = new Dictionary<string, string>
    {
        { "resolution", "Resolution" },
        { "displayraw", "Display Mode" },
        { "displaymode", "Display Mode" },
        { "brightness", "Brightness" },
        { "quality", "Quality" },
        { "shadows", "Shadows" },
        { "antialiasing", "Anti-Aliasing" },
        { "postprocessing", "Post-Processing" },
        { "ambientocclusion", "Ambient Occlusion" },
        { "renderscale", "Render Scale" },
        { "autoresolution", "Auto Resolution" },
        { "framerate", "Frame Rate" },
        { "showfps", "Show FPS" },
        { "mastervol", "Master Volume" },
        { "mastervolume", "Master Volume" },
        { "musicvol", "Music Volume" },
        { "musicvolume", "Music Volume" },
        { "sfxvol", "SFX Volume" },
        { "sfxvolume", "SFX Volume" },
        { "sensitivity", "Mouse Sensitivity" },
        { "mousesensitivity", "Mouse Sensitivity" },
    };

    [MenuItem("Tools/Settings Menu/1. Fix Rows")]
    static void FixRowsMenu()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Settings Menu: stop Play mode first.");
            return;
        }

        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage == null || stage.assetPath != PrefabPath)
            stage = PrefabStageUtility.OpenPrefab(PrefabPath);
        if (stage == null)
        {
            Debug.LogError($"Settings Menu: couldn't open {PrefabPath}.");
            return;
        }

        Undo.IncrementCurrentGroup();
        Undo.SetCurrentGroupName(UndoName);
        int group = Undo.GetCurrentGroup();
        if (FixRows(stage.prefabContentsRoot))
            EditorSceneManager.MarkSceneDirty(stage.scene);
        Undo.CollapseUndoOperations(group);
    }

    [MenuItem("Tools/Settings Menu/2. Clear Old Layout Overrides")]
    static void ClearOverridesMenu()
    {
        if (EditorApplication.isPlaying)
        {
            Debug.LogError("Settings Menu: stop Play mode first.");
            return;
        }

        var roots = new List<GameObject>();
        var stage = PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
            roots.Add(stage.prefabContentsRoot);
        else
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isLoaded)
                    roots.AddRange(SceneManager.GetSceneAt(i).GetRootGameObjects());

        ClearOverrides(roots);
    }

    public static bool FixRows(GameObject root)
    {
        var pages = root.transform.Find("Pages") as RectTransform;
        if (pages == null)
        {
            Debug.LogError("Settings Menu: there is no 'Pages' object directly under the Settings panel.");
            return false;
        }

        Place(pages, new Vector2(0.5f, 0.5f), PagesPosition, PagesSize);

        // The game's own heading and slider are the style everything else copies.
        var labelStyle = pages.GetComponentsInChildren<TMP_Text>(true)
            .FirstOrDefault(t => t.name.StartsWith("Settings Text"));
        var sliderStyle = pages.GetComponentsInChildren<Slider>(true)
            .FirstOrDefault(s => s.name.StartsWith("Settings Slider"));

        var problems = new List<string>();
        int rows = 0;
        foreach (var page in Children(pages))
        {
            SetUpPage(page);
            WrapLooseItems(page);
            foreach (var row in Children(page).Where(IsRow))
            {
                FixRow(row, labelStyle, sliderStyle, problems);
                rows++;
            }

            // Display page order: Resolution, Display Mode, Brightness.
            foreach (var row in Children(page).Where(r => RowKey(r) == "brightness"))
                Undo.SetSiblingIndex(row, page.childCount - 1, UndoName);
        }

        var tabBar = root.transform.Find("Tab Bar") as RectTransform;
        if (tabBar != null)
            FixTabBar(tabBar);

        Debug.Log($"Settings Menu: fixed {rows} rows. Save the prefab if Auto Save is off.");
        foreach (string problem in problems)
            Debug.LogWarning("Settings Menu: " + problem);
        return true;
    }

    public static int ClearOverrides(IEnumerable<GameObject> roots)
    {
        // Only layout values. Button events, script links and the panel's own placement are kept.
        string[] layoutValues = { "m_AnchorMin", "m_AnchorMax", "m_AnchoredPosition", "m_SizeDelta", "m_Pivot", "m_LocalScale" };
        int menus = 0, cleared = 0;

        var candidates = roots.SelectMany(r => r.GetComponentsInChildren<Transform>(true)).ToList();
        foreach (var candidate in candidates)
        {
            var go = candidate.gameObject;
            if (!PrefabUtility.IsAnyPrefabInstanceRoot(go) ||
                PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(go) != PrefabPath)
                continue;

            menus++;
            foreach (var rect in go.GetComponentsInChildren<RectTransform>(true))
            {
                if (rect == candidate)
                    continue;
                foreach (string value in layoutValues)
                {
                    var property = new SerializedObject(rect).FindProperty(value);
                    if (property == null || !property.prefabOverride)
                        continue;
                    PrefabUtility.RevertPropertyOverride(property, InteractionMode.UserAction);
                    cleared++;
                }
            }
            EditorSceneManager.MarkSceneDirty(go.scene);
        }

        if (menus == 0)
            Debug.LogWarning("Settings Menu: no Settings (NEW) menu is open here. Open the scene or prefab that uses it.");
        else
            Debug.Log($"Settings Menu: cleared {cleared} old layout values on {menus} menu(s). Save to keep it.");
        return cleared;
    }

    static void SetUpPage(Transform page)
    {
        Stretch((RectTransform)page, 0f);

        var layout = GetOrAdd<VerticalLayoutGroup>(page.gameObject);
        Change(layout, () =>
        {
            layout.padding = new RectOffset(60, 60, 40, 40);
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        });
    }

    // A label or control sitting straight on a page gets a row of its own.
    static void WrapLooseItems(Transform page)
    {
        foreach (var item in Children(page))
        {
            if (IsRow(item) || (!IsLabel(item) && !IsControl(item)))
                continue;

            string name = item.name.Substring(item.name.LastIndexOf('-') + 1).Trim();
            int index = item.GetSiblingIndex();
            var row = CreateChild(page, name + " Row");
            Undo.AddComponent<HorizontalLayoutGroup>(row.gameObject);
            Undo.SetSiblingIndex(row, index, UndoName);
            Undo.SetTransformParent(item, row, false, UndoName);
        }
    }

    static void FixRow(Transform row, TMP_Text labelStyle, Slider sliderStyle, List<string> problems)
    {
        var layout = GetOrAdd<HorizontalLayoutGroup>(row.gameObject);
        Change(layout, () =>
        {
            layout.padding = new RectOffset();
            layout.spacing = 40f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            layout.childScaleWidth = false;
            layout.childScaleHeight = false;
        });
        SetSize(row.gameObject, -1f, RowHeight, -1f);

        // A control inside its label can't be laid out, so bring it up into the row.
        foreach (var label in Children(row).Where(IsLabel))
            foreach (var inner in Children(label).Where(IsControl))
            {
                if (CanMove(inner.gameObject))
                    Undo.SetTransformParent(inner, row, false, UndoName);
                else
                    problems.Add($"couldn't move '{inner.name}' out of '{label.name}' because it belongs to that nested prefab.");
            }

        var texts = Children(row).Where(IsLabel).ToList();
        var valueText = texts.FirstOrDefault(IsValueText);
        var mainLabel = texts.FirstOrDefault(t => t != valueText);
        var controls = Children(row).Where(IsControl).ToList();

        bool knownTitle = RowTitles.TryGetValue(RowKey(row), out string title);
        if (!knownTitle)
            title = row.name.Replace("Row", "").Trim();

        bool createdLabel = mainLabel == null;
        if (createdLabel)
        {
            mainLabel = CreateChild(row, "Label");
            Undo.AddComponent<TextMeshProUGUI>(mainLabel.gameObject);
        }

        var mainText = mainLabel.GetComponent<TMP_Text>();
        if (knownTitle || createdLabel)
            Change(mainText, () => mainText.text = title);
        StyleText(mainText, labelStyle);
        SetSize(mainLabel.gameObject, LabelWidth, RowHeight, 0f);

        // Order: label, control, value.
        int next = 0;
        Undo.SetSiblingIndex(mainLabel, next++, UndoName);
        foreach (var control in controls)
        {
            Undo.SetSiblingIndex(control, next++, UndoName);
            FixControl(control, labelStyle, sliderStyle);
        }
        if (valueText != null)
        {
            Undo.SetSiblingIndex(valueText, next, UndoName);
            StyleText(valueText.GetComponent<TMP_Text>(), labelStyle);
            SetSize(valueText.gameObject, ValueWidth, RowHeight, 0f);
        }

        // Old hand-set scales (the Resolution dropdown was at 2x) break the sizes.
        foreach (var child in Children(row))
            if (child.localScale != Vector3.one)
                Change(child, () => child.localScale = Vector3.one);

        if (controls.Count == 0)
            problems.Add($"'{row.parent.name}/{row.name}' has no slider, dropdown or toggle.");
    }

    static void FixControl(Transform control, TMP_Text labelStyle, Slider sliderStyle)
    {
        if (control.TryGetComponent(out Slider slider))
        {
            SetSize(control.gameObject, 0f, SliderHeight, 1f);
            if (sliderStyle != null && slider != sliderStyle && !slider.name.StartsWith("Settings Slider"))
                CopySliderLook(sliderStyle, slider);
            if (slider.handleRect != null)
            {
                var handle = slider.handleRect;
                Change(handle, () => handle.sizeDelta = new Vector2(SliderHeight, handle.sizeDelta.y));
            }
        }
        else if (control.TryGetComponent(out TMP_Dropdown dropdown))
        {
            SetSize(control.gameObject, 0f, DropdownHeight, 1f);
            FixDropdown(dropdown, labelStyle);
        }
        else if (control.TryGetComponent(out Toggle toggle))
        {
            SetSize(control.gameObject, ToggleSize, ToggleSize, 0f);
            FixToggle(toggle);
        }
    }

    static void CopySliderLook(Slider from, Slider to)
    {
        foreach (string path in new[] { "Background", "Fill Area/Fill", "Handle Slide Area/Handle" })
        {
            var source = from.transform.Find(path);
            var target = to.transform.Find(path);
            if (source == null || target == null ||
                !source.TryGetComponent(out Image look) || !target.TryGetComponent(out Image image))
                continue;

            Change(image, () =>
            {
                image.sprite = look.sprite;
                image.color = look.color;
                image.type = look.type;
                image.preserveAspect = look.preserveAspect;
                image.pixelsPerUnitMultiplier = look.pixelsPerUnitMultiplier;
            });
        }
        Change(to, () => to.colors = from.colors);
    }

    static void FixDropdown(TMP_Dropdown dropdown, TMP_Text labelStyle)
    {
        foreach (var text in new[] { dropdown.captionText, dropdown.itemText })
        {
            if (text == null)
                continue;
            Change(text, () =>
            {
                // Font only: white outlined text would vanish on the white box.
                if (labelStyle != null && text.font != labelStyle.font)
                    text.font = labelStyle.font;
                text.enableAutoSizing = true;
                text.fontSizeMin = 14f;
                text.fontSizeMax = DropdownFontSize;
                text.textWrappingMode = TextWrappingModes.NoWrap;
            });
        }

        if (dropdown.template != null)
        {
            var template = dropdown.template;
            Change(template, () => template.sizeDelta = new Vector2(template.sizeDelta.x, DropdownItemHeight * 6f));
        }

        if (dropdown.itemText != null && dropdown.itemText.transform.parent is RectTransform item)
        {
            Change(item, () => item.sizeDelta = new Vector2(item.sizeDelta.x, DropdownItemHeight));
            if (item.parent is RectTransform content)
                Change(content, () => content.sizeDelta = new Vector2(content.sizeDelta.x, DropdownItemHeight));
        }
    }

    static void FixToggle(Toggle toggle)
    {
        // The row has its own label, so hide the little "Toggle" text.
        foreach (var child in Children(toggle.transform))
            if (child.GetComponent<Text>() != null || child.GetComponent<TMP_Text>() != null)
            {
                var caption = child.gameObject;
                Change(caption, () => caption.SetActive(false));
            }

        if (toggle.targetGraphic != null && toggle.targetGraphic.transform != toggle.transform)
            Stretch(toggle.targetGraphic.rectTransform, 0f);
        if (toggle.graphic != null)
            Stretch(toggle.graphic.rectTransform, 8f);
    }

    static void FixTabBar(RectTransform bar)
    {
        Place(bar, new Vector2(0.5f, 1f), TabBarPosition, TabBarSize);

        var layout = GetOrAdd<HorizontalLayoutGroup>(bar.gameObject);
        Change(layout, () =>
        {
            layout.spacing = 30f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
        });

        foreach (var tab in Children(bar))
        {
            SetSize(tab.gameObject, TabSize.x, TabSize.y, 0f);
            if (tab.TryGetComponent(out Image image))
                Change(image, () => image.preserveAspect = true);

            foreach (var text in tab.GetComponentsInChildren<TMP_Text>(true))
                Change(text, () =>
                {
                    text.enableAutoSizing = true;
                    text.fontSizeMin = 18f;
                    text.fontSizeMax = 34f;
                    text.margin = Vector4.zero;
                    text.alignment = TextAlignmentOptions.Center;
                    text.textWrappingMode = TextWrappingModes.NoWrap;
                });
        }
    }

    static void StyleText(TMP_Text text, TMP_Text style)
    {
        Change(text, () =>
        {
            if (style != null && text != style)
            {
                text.font = style.font;
                text.fontSharedMaterial = style.fontSharedMaterial;
                text.color = style.color;
                text.fontStyle = style.fontStyle;
                text.enableVertexGradient = style.enableVertexGradient;
                text.colorGradient = style.colorGradient;
                text.colorGradientPreset = style.colorGradientPreset;
            }
            text.enableAutoSizing = true;
            text.fontSizeMin = 18f;
            text.fontSizeMax = LabelFontSize;
            text.margin = Vector4.zero;
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.textWrappingMode = TextWrappingModes.NoWrap;
        });
    }

    static void SetSize(GameObject go, float width, float height, float flexibleWidth)
    {
        var size = GetOrAdd<LayoutElement>(go);
        Change(size, () =>
        {
            size.ignoreLayout = false;
            size.minWidth = width;
            size.preferredWidth = width;
            size.minHeight = height;
            size.preferredHeight = height;
            size.flexibleWidth = flexibleWidth;
        });
    }

    static void Place(RectTransform rect, Vector2 anchor, Vector2 position, Vector2 size)
    {
        Change(rect, () =>
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        });
    }

    static void Stretch(RectTransform rect, float padding)
    {
        Change(rect, () =>
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
        });
    }

    static RectTransform CreateChild(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform)) { layer = parent.gameObject.layer };
        SceneManager.MoveGameObjectToScene(go, parent.gameObject.scene);
        Undo.RegisterCreatedObjectUndo(go, UndoName);
        Undo.SetTransformParent(go.transform, parent, false, UndoName);
        return (RectTransform)go.transform;
    }

    static T GetOrAdd<T>(GameObject go) where T : Component
    {
        return go.TryGetComponent(out T component) ? component : Undo.AddComponent<T>(go);
    }

    // Every edit goes through here, so Ctrl+Z works and changes to nested prefabs get saved.
    static void Change(Object target, System.Action edit)
    {
        Undo.RecordObject(target, UndoName);
        edit();
        EditorUtility.SetDirty(target);
        if (PrefabUtility.IsPartOfPrefabInstance(target))
            PrefabUtility.RecordPrefabInstancePropertyModifications(target);
    }

    static bool CanMove(GameObject go)
    {
        return !PrefabUtility.IsPartOfPrefabInstance(go)
            || PrefabUtility.IsAnyPrefabInstanceRoot(go)
            || PrefabUtility.IsAddedGameObjectOverride(go);
    }

    // A copy, because the hierarchy changes while we go through it.
    static List<Transform> Children(Transform parent) => parent.Cast<Transform>().ToList();

    static bool IsRow(Transform t) => t.GetComponent<HorizontalLayoutGroup>() != null;

    static bool IsControl(Transform t) => t.GetComponent<Selectable>() != null;

    static bool IsLabel(Transform t) => t.GetComponent<TMP_Text>() != null && !IsControl(t);

    static bool IsValueText(Transform t) => t.name.Contains("%") || t.GetComponent<TMP_Text>().text.Trim().EndsWith("%");

    static string RowKey(Transform row)
    {
        string key = new string(row.name.Where(char.IsLetter).ToArray()).ToLowerInvariant();
        return key.EndsWith("row") ? key.Substring(0, key.Length - 3) : key;
    }
}