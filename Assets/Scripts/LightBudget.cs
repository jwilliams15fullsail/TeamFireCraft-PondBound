using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

// Keeps realtime lighting affordable on PC. It makes itself, so it needs no scene setup.
// The lab's realtime point lights each need six shadow maps, and they were the biggest cost on PC.
// - Only the lights nearest the camera cast shadows, following the Shadows setting:
//   High 4, Medium 2, Low 1, Off 0. Shadows fade in and out as that changes.
// - Faraway realtime lights smoothly shrink away (they barely reach the camera from there):
//   by 40 m on High, 32 m on Medium, 26 m on Low and Off. Short-range lights (the red alarms,
//   small accents) never fade: they colour what's around them and are seen from across a room.
// Baked and mixed lights are never touched. It does nothing on the Web pipeline, which has no
// point-light shadows and picks 4 lights per object, so changing lights there would show.
public class LightBudget : MonoBehaviour
{
    const float RescanSeconds = 3f;    // finds lights that spawn later
    const float PickSeconds = 0.2f;
    const float NeverFadeRange = 16f;  // lights this short never fade
    const float FadeBand = 8f;         // metres over which a faraway light shrinks away
    const float StickMetres = 3f;      // a light keeps its shadow until another is this much nearer
    const float ShadowFadeSpeed = 3f;  // full shadow strength in a third of a second
    const float DimLight = 0.5f;       // lights dimmer than this never take a shadow slot

    class Managed
    {
        public Light light;
        public LightShadows shadows;
        public float range, strength, intensity;
        public float distance, level;
        public bool wanted;
    }

    static LightBudget instance;

    readonly List<Managed> managed = new();
    readonly HashSet<Light> known = new();
    Camera view;
    float nextScan, nextPick;
    bool active;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (instance != null)
            return;
        var go = new GameObject("Light Budget");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<LightBudget>();
    }

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Restore();
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        nextScan = 0f;
        view = null;
    }

    void LateUpdate()
    {
        UniversalRenderPipelineAsset pipeline = UniversalRenderPipeline.asset;
        bool run = pipeline != null && pipeline.supportsAdditionalLightShadows;
        if (!run)
        {
            if (active)
                Restore();
            return;
        }
        active = true;

        float now = Time.unscaledTime;
        if (now >= nextScan)
        {
            nextScan = now + RescanSeconds;
            Scan();
        }
        if (view == null || !view.isActiveAndEnabled)
            view = Camera.main;
        if (view == null)
            return;

        bool pick = now >= nextPick;
        if (pick)
            nextPick = now + PickSeconds;
        Apply(view.transform.position, GraphicsOptions.ShadowQuality, Time.unscaledDeltaTime, pick);
    }

    public void Scan()
    {
        for (int i = managed.Count - 1; i >= 0; i--)
            if (managed[i].light == null)
                managed.RemoveAt(i);
        known.RemoveWhere(l => l == null);

        foreach (Light light in FindObjectsByType<Light>(FindObjectsInactive.Include))
        {
            if (light.type != LightType.Point && light.type != LightType.Spot)
                continue;
            LightBakingOutput baked = light.bakingOutput;
            if (baked.isBaked && baked.lightmapBakeType != LightmapBakeType.Realtime)
                continue; // baked and mixed lights keep exactly what the bake expects
            if (!known.Add(light))
                continue;
            managed.Add(new Managed
            {
                light = light,
                shadows = light.shadows,
                range = light.range,
                strength = light.shadowStrength,
                intensity = light.intensity,
            });
        }
    }

    static int ShadowSlots(GraphicsOptions.Shadows quality) => quality switch
    {
        GraphicsOptions.Shadows.High => 4,
        GraphicsOptions.Shadows.Medium => 2,
        GraphicsOptions.Shadows.Low => 1,
        _ => 0,
    };

    static float FadeEnd(GraphicsOptions.Shadows quality) => quality switch
    {
        GraphicsOptions.Shadows.High => 40f,
        GraphicsOptions.Shadows.Medium => 32f,
        _ => 26f,
    };

    public void Apply(Vector3 camera, GraphicsOptions.Shadows quality, float deltaTime, bool pick)
    {
        foreach (Managed m in managed)
        {
            if (m.light != null)
                m.distance = Vector3.Distance(m.light.transform.position, camera);
        }

        float fadeEnd = FadeEnd(quality);
        if (pick)
            PickShadows(ShadowSlots(quality), fadeEnd);

        foreach (Managed m in managed)
        {
            if (m.light == null)
                continue;

            // Shrinking the range (not the intensity) leaves AlarmLight and FlickeringLight alone.
            float near = m.range <= NeverFadeRange ? 1f : Mathf.Clamp01((fadeEnd - m.distance) / FadeBand);
            float range = Mathf.Max(0.01f, m.range * near);
            if (!Mathf.Approximately(m.light.range, range))
                m.light.range = range;

            if (m.shadows == LightShadows.None)
                continue;
            m.level = Mathf.MoveTowards(m.level, m.wanted ? 1f : 0f, deltaTime * ShadowFadeSpeed);
            LightShadows mode = m.level > 0f ? m.shadows : LightShadows.None;
            if (m.light.shadows != mode)
                m.light.shadows = mode;
            float strength = m.strength * m.level;
            if (m.level > 0f && !Mathf.Approximately(m.light.shadowStrength, strength))
                m.light.shadowStrength = strength;
        }
    }

    void PickShadows(int slots, float fadeEnd)
    {
        // Lower is better. A light that already has a shadow gets a small head start.
        static float Score(Managed m, bool had) => m.distance - (had ? StickMetres : 0f);

        foreach (Managed m in managed)
        {
            bool canCast = m.light != null && m.light.isActiveAndEnabled && m.shadows != LightShadows.None
                && m.intensity >= DimLight && m.distance < fadeEnd;
            int better = 0;
            if (canCast)
            {
                float mine = Score(m, m.level > 0f);
                foreach (Managed other in managed)
                {
                    if (other == m || other.light == null || !other.light.isActiveAndEnabled || other.shadows == LightShadows.None)
                        continue;
                    if (other.intensity < DimLight || other.distance >= fadeEnd)
                        continue;
                    if (Score(other, other.level > 0f) < mine && ++better >= slots)
                        break;
                }
            }
            m.wanted = canCast && better < slots;
        }
    }

    public void Restore()
    {
        active = false;
        foreach (Managed m in managed)
        {
            if (m.light == null)
                continue;
            m.light.range = m.range;
            m.light.shadows = m.shadows;
            m.light.shadowStrength = m.strength;
        }
        managed.Clear();
        known.Clear();
        nextScan = 0f;
    }
}
