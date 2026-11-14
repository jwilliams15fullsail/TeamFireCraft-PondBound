
using UnityEngine;

public class FrogElectricAura : MonoBehaviour
{
    [Header("Electric Aura")]
    [SerializeField] private FrogElectricCharge electricCharge;
    [SerializeField] private GameObject electricAura;

    private ParticleSystem[] electricParticles;
    private bool previousChargeState;
    private bool hasInitializedState;

    private void Awake()
    {
        if (electricCharge == null)
            electricCharge = GetComponentInParent<FrogElectricCharge>();

        if (electricAura == null)
        {
            Debug.LogError(
                "FrogElectricAura: Electric Aura is not assigned!"
            );
            return;
        }

        electricAura.SetActive(true);

        electricParticles =
            electricAura.GetComponentsInChildren<ParticleSystem>(true);

        StopElectricParticles();
    }

    private void OnEnable()
    {
        // Refresh the visual state after this component is enabled.
        hasInitializedState = false;
    }

    private void Update()
    {
        if (electricCharge == null || electricParticles == null)
            return;

        bool isCharged = electricCharge.HasCharge.Value;

        if (hasInitializedState &&
            isCharged == previousChargeState)
            return;

        hasInitializedState = true;
        previousChargeState = isCharged;

        if (isCharged)
            PlayElectricParticles();
        else
            StopElectricParticles();
    }

    private void PlayElectricParticles()
    {
        foreach (ParticleSystem particles in electricParticles)
        {
            if (!particles.gameObject.activeInHierarchy)
                continue;

            particles.Clear(true);
            particles.Play(true);
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
