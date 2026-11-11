using System.IO;
using System.Text;
using MeshDecimator;
//using UnityEngine;

namespace Artngame.PANGAEA.RockGen.Unity
{
public static class WaveFrontObjExporter
{
    public static string ToObjString(Mesh mesh)
    {
        StringBuilder sb = new StringBuilder();

        // Unity use left handed coord systems
        // Obj use right handed
        for (var i = 0; i < mesh.Vertices.Length; i++)
        {
            var vertex = mesh.Vertices[i];
            sb.Append("v ");
            sb.Append(-vertex.x);
            sb.Append(" ");
            sb.Append(vertex.y);
            sb.Append(" ");
            sb.Append(vertex.z);
            sb.AppendLine();
        }
            sb.Append("\n");
           
            for (var i = 0; i < mesh.Normals.Length; i++)
        {
            var normal = mesh.Normals[i];
            sb.Append("vn ");
            sb.Append(-normal.x);
            sb.Append(" ");
            sb.Append(normal.y);
            sb.Append(" ");
            sb.Append(normal.z);
            sb.AppendLine();
        }

            sb.Append("\n");
            //v0.1
            for (var i = 0; i < mesh.UV1.Length; i++)
            {
                MeshDecimator.Math.Vector2 uv = mesh.UV1[i];
                sb.Append("vt ");
                sb.Append(uv.x);
                sb.Append(" ");
                sb.Append(uv.y);
                // sb.Append(" ");
                // sb.Append("0.000000");
                sb.AppendLine();
                //UnityEngine.Debug.Log("SAVED UVs in " + i + " = " + uv);
            }
            sb.Append("\n");

            var indices = mesh.GetIndices(0);
        for (var i = 0; i < indices.Length; i += 3)
        {
            // indices is 1-indexed
            var (i0, i1, i2) = (indices[i + 0] + 1,
                                indices[i + 1] + 1,
                                indices[i + 2] + 1);
            sb.Append("f ");
            sb.Append(i1);
            sb.Append("//");
            sb.Append(i1);
            sb.Append(" ");
            sb.Append(i0);
            sb.Append("//");
            sb.Append(i0);
            sb.Append(" ");
            sb.Append(i2);
            sb.Append("//");
            sb.Append(i2);
            sb.AppendLine();
        }

        return sb.ToString();
    }
}
}
