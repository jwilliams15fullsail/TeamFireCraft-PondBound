
using UnityEngine;
using System.Collections;

public class GroundSpike : MonoBehaviour
{
    public float riseHeight = 2f;
    public float speed = 5f;
    public float waitTime = 2f;

    private Vector3 startPosition;
    private Vector3 raisedPosition;

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
}
