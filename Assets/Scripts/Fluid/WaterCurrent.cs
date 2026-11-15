using System.Collections.Generic;
using UnityEngine;

// A current in the flood water, such as the stream pouring out of a sewer tube. Floating frogs and
// floating boxes drift with it. Put it on an empty at the tube's mouth with its blue (Z) arrow
// pointing into the room. It is strongest there and fades out by Reach, spreading out as it goes.
public class WaterCurrent : MonoBehaviour
{
    [Tooltip("How fast (m/s) the water flows right at the tube's mouth.")]
    [Min(0f)] public float speed = 0.8f;
    [Tooltip("How far (m) into the room the current still pushes.")]
    [Min(0.1f)] public float reach = 10f;
    [Tooltip("How wide (m) the stream is at the mouth. It gets wider as it spreads into the room.")]
    [Min(0.1f)] public float width = 3f;

    static readonly List<WaterCurrent> all = new List<WaterCurrent>();

    void OnEnable() => all.Add(this);
    void OnDisable() => all.Remove(this);

    // How fast and which way (m/s, flat) the water moves at a point, from every current together.
    public static Vector3 At(Vector3 point)
    {
        Vector3 total = Vector3.zero;
        foreach (WaterCurrent current in all)
            total += current.Flow(point);
        return total;
    }

    Vector3 Flow(Vector3 point)
    {
        Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 offset = Vector3.ProjectOnPlane(point - transform.position, Vector3.up);
        float along = Vector3.Dot(offset, direction);
        if (along < 0f || along > reach)
            return Vector3.zero; // behind the mouth or past the end
        float across = (offset - direction * along).magnitude;
        float halfWidth = width * 0.5f + along * 0.4f; // spreads out as it goes
        float strength = (1f - along / reach) * Mathf.Clamp01(1f - across / halfWidth);
        return direction * (speed * strength);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 side = Vector3.Cross(Vector3.up, direction);
        Vector3 end = transform.position + direction * reach;
        float endHalf = width * 0.5f + reach * 0.4f;
        Gizmos.color = new Color(0.3f, 0.7f, 1f);
        Gizmos.DrawLine(transform.position - side * width * 0.5f, end - side * endHalf);
        Gizmos.DrawLine(transform.position + side * width * 0.5f, end + side * endHalf);
        Gizmos.DrawLine(transform.position, end);
    }
}