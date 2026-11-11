using MeshDecimator.Math;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Vector2 = UnityEngine.Vector2; //v0.4

namespace Artngame.PANGAEA.RockGen.Unity.Demo
{
// Mostly for debugging.
public class RockBehavior : MonoBehaviour
{
    [Header("Debug")]
    public bool debug;

    public Color debugColor = Color.white;

    internal RockGenerator generator;
    MeshFilter             meshFilter;

    void OnEnable()
    {
        meshFilter = GetComponent<MeshFilter>();
        generator  = new RockGenerator();

        generator.foundNearest += OnSettingsOnfoundNearest;
    }

    void OnSettingsOnfoundNearest(Vector3d vertex, Vector3d normal, Vector3d nearest)
    {
        if (debug)
        {
            var vert = new Vector3((float) vertex.x,  (float) vertex.y,  (float) vertex.z);
            var norm = new Vector3((float) normal.x,  (float) normal.y,  (float) normal.z);
            var near = new Vector3((float) nearest.x, (float) nearest.y, (float) nearest.z);
            Debug.DrawLine(vert, near, debugColor);
            Debug.DrawRay(vert, norm * .2f, Color.green);
        }
    }



        //v0.4
        //https://stackoverflow.com/questions/39980248/uv-unwrap-on-runtime-optimization
        public static Vector2[] CreateUV(Mesh mesh)
        {
            int i = 0;
            Vector3 p = Vector3.up;
            Vector3 u = Vector3.Cross(p, Vector3.forward);
            if (Vector3.Dot(u, u) < 0.01f)
            {
                u = Vector3.right;
            }
            else
            {
                u = Vector3.Normalize(u);
            }

            Vector3 v = Vector3.Normalize(Vector3.Cross(p, u));
            Vector3[] vertexs = mesh.vertices;
            int[] tris = mesh.triangles;
            Vector2[] uvs = new Vector2[vertexs.Length];

            //normals
            Vector3[] norms = mesh.normals;

            for (i = 0; i < tris.Length; i += 3)
            {

                Vector3 a = vertexs[tris[i]];
                Vector3 b = vertexs[tris[i + 1]];
                Vector3 c = vertexs[tris[i + 2]];

                //PLOT NORMALS of triangle
                Vector3 aN = norms[tris[i]];
                Vector3 bN = norms[tris[i + 1]];
                Vector3 cN = norms[tris[i + 2]];
                //Debug.DrawLine(a, a + aN*0.05f, Color.red, 15);
                // Debug.DrawLine(b, b + bN * 0.05f, Color.red, 15);
                //Debug.DrawLine(c, c + cN * 0.05f, Color.red, 15);

                Vector3 side1 = b - a;
                Vector3 side2 = c - a;
                Vector3 N = Vector3.Cross(side1, side2);

                N = new Vector3(Mathf.Abs(N.normalized.x), Mathf.Abs(N.normalized.y), Mathf.Abs(N.normalized.z));
                //N = new Vector3((N.normalized.x), (N.normalized.y), (N.normalized.z));


                //if (N.x > N.y && N.x > N.z)
                //{
                //    uvs[tris[i]] = new Vector2(vertexs[tris[i]].z, vertexs[tris[i]].y);
                //    uvs[tris[i + 1]] = new Vector2(vertexs[tris[i + 1]].z, vertexs[tris[i + 1]].y);
                //    uvs[tris[i + 2]] = new Vector2(vertexs[tris[i + 2]].z, vertexs[tris[i + 2]].y);
                //}
                //else if (N.y > N.x && N.y > N.z)
                //{
                //    uvs[tris[i]] = new Vector2(vertexs[tris[i]].x, vertexs[tris[i]].z);
                //    uvs[tris[i + 1]] = new Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].z);
                //    uvs[tris[i + 2]] = new Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].z);
                //}
                //else if (N.z > N.x && N.z > N.y)
                //{
                //    uvs[tris[i]] = new Vector2(vertexs[tris[i]].x, vertexs[tris[i]].y);
                //    uvs[tris[i + 1]] = new Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].y);
                //    uvs[tris[i + 2]] = new Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].y);
                //}
                //else
                //{
                //    //uvs[tris[i]] = new Vector2(vertexs[tris[i]].x, vertexs[tris[i]].y);
                //    //uvs[tris[i + 1]] = new Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].y);
                //    //uvs[tris[i + 2]] = new Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].y);
                //}
                uvs[tris[i]] = new Vector2(vertexs[tris[i]].z, vertexs[tris[i]].y);
                uvs[tris[i + 1]] = new Vector2(vertexs[tris[i + 1]].z, vertexs[tris[i + 1]].y);
                uvs[tris[i + 2]] = new Vector2(vertexs[tris[i + 2]].z, vertexs[tris[i + 2]].y);
                uvs[tris[i]] += new Vector2(vertexs[tris[i]].x, vertexs[tris[i]].z);
                uvs[tris[i + 1]] += new Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].z);
                uvs[tris[i + 2]] += new Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].z);
                uvs[tris[i]] += new Vector2(vertexs[tris[i]].x, vertexs[tris[i]].y);
                uvs[tris[i + 1]] += new Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].y);
                uvs[tris[i + 2]] += new Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].y);
                uvs[tris[i]] = uvs[tris[i]] / 3;
                uvs[tris[i + 1]] = uvs[tris[i + 1]] / 3;
                uvs[tris[i + 2]] = uvs[tris[i + 2]] / 3;
            }

            //for (i = 0; i < uvs.Length; i += 1)
            //{
            //    UnityEngine.Debug.Log("ASSIGNED UVs in " + i + " = " + uvs[i]);
            //}
            //mesh.uv = uvs;
            return uvs;
        }


        internal void UpdateMesh()
        {
            FrameTime.Instance.StartWork("Gen");
            meshFilter.mesh = Convert.ToUnityMesh(generator.MakeRock(meshFilter.transform));//v0.1

            //v0.4 - 1.0.5
            bool methodUV3 = true;
            if (methodUV3)
            {
                Vector2[] uvsA = CreateUV(meshFilter.mesh);
                meshFilter.mesh.uv = uvsA;
                meshFilter.mesh.RecalculateTangents();

                generator.unityMesh = meshFilter.mesh;
                generator.unityMeshFilter = meshFilter;

                //update latest mesh
                var uvDECIM = new MeshDecimator.Math.Vector2[uvsA.Length];
                for (int i = 0; i < uvsA.Length; i += 1)
                {
                    uvDECIM[i] = new MeshDecimator.Math.Vector2(uvsA[i].x, uvsA[i].y);
                }
                generator.LatestMesh.UV1 = new MeshDecimator.Math.Vector2[uvsA.Length];
                generator.LatestMesh.UV1 = uvDECIM;
            }
            /*
            bool methodUV2 = false;
            if (methodUV2)
            {
                meshFilter.mesh.RecalculateTangents();
                meshFilter.mesh.RecalculateNormals();
                //https://answers.unity.com/questions/1189522/how-do-i-use-the-output-of-unwrappinggeneratepertr.html
                //SkinnedMeshRenderer meshRenderer = GetComponent<SkinnedMeshRenderer>();
                Vector2[] uvs = new Vector2[meshFilter.mesh.vertexCount];
                UnityEditor.UnwrapParam paramS = new UnityEditor.UnwrapParam();
                UnityEditor.UnwrapParam.SetDefaults(out paramS);
                //Debug.Log(paramS.angleError +","+ paramS.areaError + "," + paramS.hardAngle + "," + paramS.packMargin);
                //0.08,  0.15,  88,  0.00390625
                paramS.angleError= 0.08f;
                paramS.areaError = 0.15f;
                paramS.hardAngle = 89.9f;
                paramS.packMargin = 0.00390625f;
               //angleError	Maximum allowed angle distortion (0..1).
               //areaError     Maximum allowed area distortion(0..1).
               //hardAngle     This angle(in degrees) or greater between triangles will cause seam to be created.
               //packMargin    How much uv-islands will be padded.

                //paramS.packMargin = 0.0001f;
                //paramS.angleError = 0.1f;
                Vector2[] all = UnityEditor.Unwrapping.GeneratePerTriangleUV(meshFilter.mesh, paramS);
                int[] triangles = meshFilter.mesh.triangles;

                int count = 0;
                while (count < uvs.Length)
                {
                    for (int i = 0; i < triangles.Length; i++)
                    {
                        if (triangles[i] == count)
                        {
                            uvs[count] = all[i];
                            count++;
                        }
                    }
                }
                Debug.Log("Count = " + count + " , Out of " + meshFilter.mesh.vertexCount);
                //for (int i = 0; i < meshFilter.mesh.triangles.Length; i++)
                //{
                //    uvs[meshFilter.mesh.triangles[i]] = uvs[i];
                //}

                meshFilter.mesh.uv = uvs;
                meshFilter.mesh.RecalculateTangents();
                //meshFilter.ap
                //               Mesh m = new Mesh();
                //...
                //Vector2[] uvPerTriangle = Unwrapping.GeneratePerTriangleUV(m, param);

                //               Vector2[] uvs = new Vector2[m.vertices.Length];
                //               for (int i = 0; i < m.triangles.Length; i++)
                //                   // Triangle contents reference to vertex # 
                //                   uvs[m.triangles[i]] = uvPerTriangle[i];

                //               m.uv = uvs;
                //               m.RecalculateTangents();
                Debug.Log("CREATING UVs METHOD 2");
            }
            */

            FrameTime.Instance.EndWork("Gen");
    }

    void OnDrawGizmos()
    {
        if (!debug) return;
        if (generator == null) return;

        foreach (var point in generator.Grid.points)
        {
            Gizmos.DrawSphere(ToUV3(point), .03f);
        }

        for (var z = 1; z < generator.Settings.GridSettings.Size; z++)
        for (var y = 1; y < generator.Settings.GridSettings.Size; y++)
        for (var x = 1; x < generator.Settings.GridSettings.Size; x++)
        {
            Gizmos.DrawLine(ToUV3(generator.Grid.points[x - 1, y, z]), ToUV3(generator.Grid.points[x, y, z]));
            Gizmos.DrawLine(ToUV3(generator.Grid.points[x, y - 1, z]), ToUV3(generator.Grid.points[x, y, z]));
            Gizmos.DrawLine(ToUV3(generator.Grid.points[x, y, z - 1]), ToUV3(generator.Grid.points[x, y, z]));
        }
    }

    Vector3 ToUV3(Vector3d v)
    {
        return new Vector3((float) v.x, (float) v.y, (float) v.z);
    }
}
}
