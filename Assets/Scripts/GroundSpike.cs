using Unity.Netcode;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GroundSpike : MonoBehaviour
{
    [Header("Movement")]
    public float riseHeight = 2f;
    public float speed = 5f;
    public float waitTime = 2f;

    [Header("Damage")]
    public float damage = 20f;
    public float damageCooldown = 1f;

    private Vector3 startPosition;
    private Vector3 raisedPosition;

    private readonly Dictionary<FrogStats, float> lastHitTimes = new();

    void Start()
    {
        startPosition = transform.position;
        raisedPosition = startPosition + Vector3.up * riseHeight;

        StartCoroutine(SpikeCycle());
    }

    IEnumerator SpikeCycle()
    {
        while (true)
        {
            while (Vector3.Distance(transform.position, raisedPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    raisedPosition,
                    speed * Time.deltaTime
                );

                yield return null;
            }

            yield return new WaitForSeconds(waitTime);

            while (Vector3.Distance(transform.position, startPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    startPosition,
                    speed * Time.deltaTime
                );

                yield return null;
            }

            yield return new WaitForSeconds(waitTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay(Collider other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider other)
    {
        // Only the server can change frog health.
        if (NetworkManager.Singleton == null ||
            !NetworkManager.Singleton.IsServer)
            return;

        FrogStats frog = other.GetComponentInParent<FrogStats>();

        if (frog == null || !frog.IsSpawned)
            return;

        if (lastHitTimes.TryGetValue(frog, out float lastHit))
        {
            if (Time.time - lastHit < damageCooldown)
                return;
        }

        lastHitTimes[frog] = Time.time;

        frog.Damage(damage);

        Debug.Log("GroundSpike hit frog! Damage: " + damage);
    }
}
