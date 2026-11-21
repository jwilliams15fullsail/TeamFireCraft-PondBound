using UnityEngine;

public class EnemyStunAura : MonoBehaviour
{
    [Header("Stun Electric Aura")]
    [SerializeField] private EnemyStun enemyStun;
    [SerializeField] private GameObject electricAura;

    private ParticleSystem[] electricParticles;
    private bool previousStunState;
    private bool hasInitializedState;

    private void Awake()
    {
        if (enemyStun == null)
            enemyStun = GetComponentInParent<EnemyStun>();

        if (electricAura == null)
        {
            Debug.LogError("EnemyStunAura: Electric Aura is not assigned!", this);
            return;
        }

        electricAura.SetActive(true);
        electricParticles = electricAura.GetComponentsInChildren<ParticleSystem>(true);

        StopElectricParticles();
    }

    private void OnEnable()
    {
        // Refresh the visual state after this component is enabled
        hasInitializedState = false;
    }

    private void Update()
    {
        if (enemyStun == null || electricParticles == null)
            return;

        bool isStunned = enemyStun.IsStunned;

        if (hasInitializedState && isStunned == previousStunState)
            return;

        hasInitializedState = true;
        previousStunState = isStunned;

        if (isStunned)
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
            particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}