using System;
using System.Runtime.CompilerServices;
using MeshDecimator;
using MeshDecimator.Algorithms;
using MeshDecimator.Math;
using Artngame.PANGAEA.RockGen.Primitive;

namespace Artngame.PANGAEA.RockGen
{
public class RockGenerator
{
    public RockGenerationSettings Settings
    {
        get => settings;
        set
        {
            if (settings.StockDensity != value.StockDensity)
                UpdateStockMesh(value);

            if (!settings.GridSettings.Equals(value.GridSettings))
                MakeGrid(value.GridSettings);

            settings = value;
        }
    }

    public Mesh LatestMesh { get; private set; }

    // For debugging
    public event Action<Vector3d, Vector3d, Vector3d> foundNearest;

    public VoronoiGrid Grid { get; private set; }

    readonly SphereCubeGenerator sphereCubeGenerator;
    RockGenerationSettings       settings;
    Mesh                         stockMesh;

        //v0.1
        public UnityEngine.Mesh unityMesh;
        public UnityEngine.MeshFilter unityMeshFilter;

        public RockGenerator()
    {
        sphereCubeGenerator = new SphereCubeGenerator {
            Radius = .5f
        };
    }

    void MakeGrid(VoronoiGridSettings gridSettings)
    {
        Grid = new VoronoiGrid(gridSettings);
    }

    public void UpdateStockMesh(RockGenerationSettings settings)
    {
        sphereCubeGenerator.NumSubDivX = (int) Math.Round(settings.StockDensity * settings.Scale.X);
        sphereCubeGenerator.NumSubDivY = (int) Math.Round(settings.StockDensity * settings.Scale.Y);
        sphereCubeGenerator.NumSubDivZ = (int) Math.Round(settings.StockDensity * settings.Scale.Z);

        stockMesh = sphereCubeGenerator.MakeSphere();
    }

    public Mesh MakeRock(UnityEngine.Transform RockTransform)
    {
        var vertices = new Vector3d[stockMesh.VertexCount];

        var distort = Settings.Distortion;

        for (var i = 0; i < vertices.Length; i++)
        {
            var worldPos    = Transform(Settings.Transform, stockMesh.Vertices[i]);
            var worldNormal = TransformDir(Settings.Transform, stockMesh.Normals[i]);

            var (nearest, nearestDS) = Grid.Nearest(worldPos * Settings.PatternSize);

            var worldResult = worldPos + worldNormal * ((nearestDS - .5) * distort);

            // Keep the scale and rotation
            vertices[i] = worldResult - new Vector3d(Settings.Transform.M14,
                                                     Settings.Transform.M24,
                                                     Settings.Transform.M34);

            OnFoundNearest(worldResult, worldNormal, nearest);
        }

        var mesh = new Mesh(
            vertices,
            stockMesh.Indices
        );

        var simplifier = new FastQuadricMeshSimplification();

           // simplifier.EnableSmartLink = true; //v0.4
           // simplifier.KeepBorders = true;//v0.4
           // simplifier.PreserveBorders = true;
           // simplifier.PreserveSeams = true;
           // simplifier.PreserveFoldovers = true;
           //// simplifier.KeepLinkedVertices = true;
           // simplifier.VertexLinkDistanceSqr = Double.Epsilon * 0.01f;

            simplifier.Initialize(mesh);
           
        simplifier.DecimateMesh(Settings.TargetTriangleCount);

        mesh = simplifier.ToMesh();

        mesh.RecalculateNormals();

        CalcUV(mesh, RockTransform);//v0.1

        LatestMesh = mesh;
        return mesh;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector3d Transform(Matrix4x4 m, Vector3d v)
    {
        return new Vector3d(
            m.M11 * v.x + m.M12 * v.y + m.M13 * v.z + m.M14,
            m.M21 * v.x + m.M22 * v.y + m.M23 * v.z + m.M24,
            m.M31 * v.x + m.M32 * v.y + m.M33 * v.z + m.M34
        );
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static Vector3d TransformDir(Matrix4x4 m, Vector3d v)
    {
        return new Vector3d(
            m.M11 * v.x + m.M12 * v.y + m.M13 * v.z,
            m.M21 * v.x + m.M22 * v.y + m.M23 * v.z,
            m.M31 * v.x + m.M32 * v.y + m.M33 * v.z
        ).Normalized;
    }

        //v0.1
        void CalcUV(Mesh mesh, UnityEngine.Transform transform)
        {
            var vertices = mesh.Vertices;// .vertices;
            var normals = mesh.Normals;// .normals;
            var uv = new Vector2[vertices.Length];

            var center = transform.position;
            var corners = new Vector3d[8];
            var cornersForward = new Vector3d[8];
            //grid.GetCellCorners(center, corners);
            Grid.GetCellCorners(new MeshDecimator.Math.Vector3(center.x, center.y,center.z), corners);

            for (var i = 0; i < corners.Length; i++)
            {
                //cornersForward[i] = (center - corners[i]).normalized;
                //cornersForward[i] = (new Vector3d(center.x,center.y,center.z) - corners[i]).normalized;
                UnityEngine.Vector3 vecUnity = (new UnityEngine.Vector3(center.x - (float)corners[i].x, center.y - (float)corners[i].y, center.z - (float)corners[i].z)).normalized;
                cornersForward[i] = new Vector3(vecUnity.x, vecUnity.y, vecUnity.z);
            }

            for (int i = 0; i < vertices.Length; i++)
            {
                // var worldPos = transform.TransformPoint(vertices[i]);
                // var worldNormal = transform.TransformDirection(normals[i]);
                var worldPos = transform.TransformPoint(new UnityEngine.Vector3((float)vertices[i].x, (float)vertices[i].y, (float)vertices[i].z) );
                var worldNormal = transform.TransformDirection(new UnityEngine.Vector3((float)normals[i].x, (float)normals[i].y, (float)normals[i].z));

                var smallestD = float.PositiveInfinity;
                var nearestIndex = 0;

                for (var j = 0; j < cornersForward.Length; j++)
                {
                    float d = UnityEngine.Vector3.Dot(new UnityEngine.Vector3((float)cornersForward[j].x, 
                        (float)cornersForward[j].y, (float)cornersForward[j].z), worldNormal); //Dot(cornersForward[j], worldNormal);

                    if (d < smallestD)
                    {
                        smallestD = d;
                        nearestIndex = j;
                    }
                }

                var normal3d = cornersForward[nearestIndex];
                var normal = new Vector3((float)normal3d.x, (float)normal3d.y, (float)normal3d.z);
                // var projected = ProjectPointOnPlane(worldPos, corners[nearestIndex], normal);
                var projected = ProjectPointOnPlane(
                    new Vector3(worldPos.x, worldPos.y, worldPos.z),
                    new Vector3((float)corners[nearestIndex].x, (float)corners[nearestIndex].y, (float)corners[nearestIndex].z), 
                    normal);

                // var tangent = UnityEngine.Vector3.Cross(normal, up);
                var tangent = UnityEngine.Vector3.Cross(new UnityEngine.Vector3(normal.x, normal.y, normal.z), transform.up);
                var binormal = UnityEngine.Vector3.Cross(tangent, new UnityEngine.Vector3(normal.x, normal.y, normal.z));
                tangent = UnityEngine.Vector3.Cross(new UnityEngine.Vector3(normal.x, normal.y, normal.z), binormal);

                // Debug.DrawRay(worldPos, tangent * .1f, Color.red);
                //Debug.DrawRay(worldPos, binormal * .1f, Color.green);

                // Debug.DrawLine(worldPos, corners[nearestIndex], Color.gray);

                uv[i] = new Vector2(
                    UnityEngine.Vector3.Dot(new UnityEngine.Vector3(projected.x, projected.y, projected.z), tangent),
                    UnityEngine.Vector3.Dot(new UnityEngine.Vector3(projected.x, projected.y, projected.z), binormal)
                );
            }


           


            //mesh.uv = uv;
            mesh.UV1 = uv;
        }

        static Vector3 ProjectPointOnPlane(Vector3 point, Vector3 planeOrig, Vector3 planeNormal)
        {
            var v = point - planeOrig;
            // var dist = UnityEngine.Vector3.Dot(v, planeNormal);
            var dist = UnityEngine.Vector3.Dot(new UnityEngine.Vector3(v.x,v.y,v.z), new UnityEngine.Vector3(planeNormal.x, planeNormal.y, planeNormal.z));
            return point - dist * planeNormal;
        }

        void OnFoundNearest(Vector3d vertex, Vector3d normal, Vector3d nearest)
    {
        foundNearest?.Invoke(vertex, normal, nearest);
    }


        ////v0.4
        ////https://stackoverflow.com/questions/39980248/uv-unwrap-on-runtime-optimization
        //public void CreateUV(Mesh mesh)
        //{
        //    int i = 0;
        //    UnityEngine.Vector3 p = UnityEngine.Vector3.up;
        //    UnityEngine.Vector3 u = UnityEngine.Vector3.Cross(p, UnityEngine.Vector3.forward);
        //    if (UnityEngine.Vector3.Dot(u, u) < 0.01f)
        //    {
        //        u = UnityEngine.Vector3.right;
        //    }
        //    else
        //    {
        //        u = UnityEngine.Vector3.Normalize(u);
        //    }

        //    UnityEngine.Vector3 v = UnityEngine.Vector3.Normalize(UnityEngine.Vector3.Cross(p, u));
        //    UnityEngine.Vector3[] vertexs = mesh.vertices;
        //    int[] tris = mesh.triangles;
        //    UnityEngine.Vector2[] uvs = new Vector2[vertexs.Length];

        //    //normals
        //    UnityEngine.Vector3[] norms = mesh.normals;

        //    for (i = 0; i < tris.Length; i += 3)
        //    {

        //        UnityEngine.Vector3 a = vertexs[tris[i]];
        //        UnityEngine.Vector3 b = vertexs[tris[i + 1]];
        //        UnityEngine.Vector3 c = vertexs[tris[i + 2]];

        //        //PLOT NORMALS of triangle
        //        UnityEngine.Vector3 aN = norms[tris[i]];
        //        UnityEngine.Vector3 bN = norms[tris[i + 1]];
        //        UnityEngine.Vector3 cN = norms[tris[i + 2]];
        //        //Debug.DrawLine(a, a + aN*0.05f, Color.red, 15);
        //        // Debug.DrawLine(b, b + bN * 0.05f, Color.red, 15);
        //        //Debug.DrawLine(c, c + cN * 0.05f, Color.red, 15);

        //        UnityEngine.Vector3 side1 = b - a;
        //        UnityEngine.Vector3 side2 = c - a;
        //        UnityEngine.Vector3 N = UnityEngine.Vector3.Cross(side1, side2);

        //        N = new UnityEngine.Vector3(UnityEngine.Mathf.Abs(N.normalized.x), UnityEngine.Mathf.Abs(N.normalized.y), UnityEngine.Mathf.Abs(N.normalized.z));
                
        //        uvs[tris[i]] = new UnityEngine.Vector2(vertexs[tris[i]].z, vertexs[tris[i]].y);
        //        uvs[tris[i + 1]] = new UnityEngine.Vector2(vertexs[tris[i + 1]].z, vertexs[tris[i + 1]].y);
        //        uvs[tris[i + 2]] = new UnityEngine.Vector2(vertexs[tris[i + 2]].z, vertexs[tris[i + 2]].y);
        //        uvs[tris[i]] += new UnityEngine.Vector2(vertexs[tris[i]].x, vertexs[tris[i]].z);
        //        uvs[tris[i + 1]] += new UnityEngine.Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].z);
        //        uvs[tris[i + 2]] += new UnityEngine.Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].z);
        //        uvs[tris[i]] += new UnityEngine.Vector2(vertexs[tris[i]].x, vertexs[tris[i]].y);
        //        uvs[tris[i + 1]] += new UnityEngine.Vector2(vertexs[tris[i + 1]].x, vertexs[tris[i + 1]].y);
        //        uvs[tris[i + 2]] += new UnityEngine.Vector2(vertexs[tris[i + 2]].x, vertexs[tris[i + 2]].y);
        //        uvs[tris[i]] = uvs[tris[i]] / 3;
        //        uvs[tris[i + 1]] = uvs[tris[i + 1]] / 3;
        //        uvs[tris[i + 2]] = uvs[tris[i + 2]] / 3;
        //    }

        //    for (i = 0; i < uvs.Length; i += 1)
        //    {
        //        UnityEngine.Debug.Log("ASSIGNED UVs in " + i + " = " + uvs[i]);
        //    }
        //    //mesh.uv = uvs;
        //    //return uvs;

        //    mesh.UV1 = uv;
        //}

        //////


    }
}
