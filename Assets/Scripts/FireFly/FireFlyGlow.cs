using UnityEngine;

public class FireflyGlow : MonoBehaviour
{
    [Header("Firefly Light")]
    [SerializeField] private Light fireflyLight;

    [Header("Glow Settings")]
    [SerializeField] private float minimumIntensity = 0.5f;
    [SerializeField] private float maximumIntensity = 1.5f;
    [SerializeField] private float pulseSpeed = 2f;

    private void Awake()
    {
        if (fireflyLight == null)
        {
            fireflyLight = GetComponentInChildren<Light>();
        }
    }

    private void Update()
    {
        if (fireflyLight == null)
            return;

        // Smoothly pulse between the minimum and maximum brightness.
        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;

        fireflyLight.intensity = Mathf.Lerp(
            minimumIntensity,
            maximumIntensity,
            pulse
        );
    }
}
