using UnityEngine;

public class FrogElectricAura : MonoBehaviour
{
    [Header("Electric Aura")]
    [SerializeField] private FrogElectricCharge electricCharge;
    [SerializeField] private GameObject electricAura;

    private ParticleSystem[] electricParticles;
    private bool previousChargeState;

    private void Awake()
    {
        if (electricCharge == null)
            electricCharge = GetComponentInParent<FrogElectricCharge>();

        if (electricAura == null)
        {
            Debug.LogError("FrogElectricAura: Electric Aura is not assigned!");
            return;
        }

        // Keep the GameObject active so its particles can run.
        electricAura.SetActive(true);

        electricParticles =
            electricAura.GetComponentsInChildren<ParticleSystem>(true);

        // Start with no electricity.
        StopElectricParticles();

        previousChargeState = false;
    }

    private void Update()
    {
        if (electricCharge == null || electricParticles == null)
            return;

        bool isCharged = electricCharge.HasCharge.Value;

        // Only react when the charge changes.
        if (isCharged == previousChargeState)
            return;

        previousChargeState = isCharged;

        if (isCharged)
        {
            PlayElectricParticles();
        }
        else
        {
            StopElectricParticles();
        }

        Debug.Log($"Electric Aura active: {isCharged}");
    }

    private void PlayElectricParticles()
    {

        Debug.Log(
    $"AURA REFERENCE: {electricAura.name}, " +
    $"Parent={electricAura.transform.parent?.name ?? "NONE"}, " +
    $"Scene={electricAura.scene.name}, " +
    $"EntityID={electricAura.GetEntityId()}"
);
        Debug.Log(
    $"Frog ROOT: activeSelf={gameObject.activeSelf}, " +
    $"activeInHierarchy={gameObject.activeInHierarchy}"
);

        foreach (ParticleSystem particles in electricParticles)
        {
            Debug.Log(
    $"{particles.name}: " +
    $"activeSelf={particles.gameObject.activeSelf}, " +
    $"activeInHierarchy={particles.gameObject.activeInHierarchy}"
);

            particles.Clear(true);
            particles.Play();
            particles.Clear(true);
            particles.Play();

            Debug.Log(
                $"{particles.name}: IsPlaying={particles.isPlaying}"
            );
        }
    }

    private void StopElectricParticles()
    {
        if (electricParticles == null)
            return;

        foreach (ParticleSystem particles in electricParticles)
        {
            particles.Stop(
                true,
                ParticleSystemStopBehavior.StopEmittingAndClear
            );
        }
    }
}
