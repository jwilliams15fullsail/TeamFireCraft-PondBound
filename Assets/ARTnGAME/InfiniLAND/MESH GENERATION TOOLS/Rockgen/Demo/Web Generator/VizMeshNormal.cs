using UnityEngine;

namespace Artngame.PANGAEA.RockGen.Unity.Demo
{
public class VizMeshNormal : MonoBehaviour
{
    MeshFilter mf;

    void OnEnable()
    {
        mf = GetComponent<MeshFilter>();
    }
        public bool readUVs = false;
        private void Update()
        {
            //read uvs
            if (readUVs)
            {
                readUVs = false;
                for (var i = 0; i < mf.mesh.uv.Length; i++)
                {
                    Vector2 uv = mf.mesh.uv[i];

                    UnityEngine.Debug.Log("UVs in " + i + " = " + uv.x + ", " + uv.y);
                }
                for (var i = 0; i < mf.mesh.uv2.Length; i++)
                {
                    Vector2 uv = mf.mesh.uv2[i];

                    UnityEngine.Debug.Log("UVs 2 in " + i + " = " + uv.x + ", " + uv.y);
                }
            }
        }

        void OnDrawGizmosSelected()
    {
        if (!enabled) return;
        if (!mf) return;

        var m = mf.mesh;
        var v = m.vertices;
        var n = m.normals;

        for (var i = 0; i < v.Length; i++)
        {
            Gizmos.DrawRay(transform.TransformPoint(v[i]),
                           transform.TransformDirection(n[i]) * .2f);
        }
    }
}
}
