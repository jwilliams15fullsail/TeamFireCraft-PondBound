using UnityEngine;

// An invisible round wall that keeps a frog inside a capsule. The glass mesh's triangles
// only face outward, so it can't stop anything from the inside. These are solid boxes,
// which block from both sides. Turn this object off to let the frog out.
public class CapsuleBarrier : MonoBehaviour
{
    [SerializeField] float radius = 1.3f;
    [SerializeField] float height = 2.4f;
    [SerializeField] float thickness = 0.2f;
    [SerializeField] int boxCount = 16;

    void Awake()
    {
        for (int i = 0; i < boxCount; i++)
        {
            GameObject box = new GameObject("Barrier Box");
            box.layer = gameObject.layer;
            box.transform.SetParent(transform, false);
            box.transform.SetLocalPositionAndRotation(BoxPosition(i), BoxRotation(i));
            box.AddComponent<BoxCollider>().size = BoxSize();
        }
    }

    // Shows the ring in the Scene view while this object is selected.
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        for (int i = 0; i < boxCount; i++)
        {
            Gizmos.matrix = transform.localToWorldMatrix * Matrix4x4.TRS(BoxPosition(i), BoxRotation(i), Vector3.one);
            Gizmos.DrawWireCube(Vector3.zero, BoxSize());
        }
    }

    Quaternion BoxRotation(int i) => Quaternion.Euler(0f, i * 360f / boxCount, 0f);

    Vector3 BoxPosition(int i) => BoxRotation(i) * Vector3.forward * (radius + thickness * 0.5f) + Vector3.up * height * 0.5f;

    // Wide enough to overlap at the outer edge, so there are no gaps between boxes.
    Vector3 BoxSize() => new Vector3(2f * (radius + thickness) * Mathf.Tan(180f / boxCount * Mathf.Deg2Rad), height, thickness);
}