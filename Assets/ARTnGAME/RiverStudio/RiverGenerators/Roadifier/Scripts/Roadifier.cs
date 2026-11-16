using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using System.Collections.Generic;

namespace Artngame.RiverStudio.RiverCreator
{
    public class Roadifier : MonoBehaviour
    {
        //
        public float roadWidth = 5.0f;
        public float smoothingFactor = 0.2f;
        public int smoothingIterations = 3;
        public Material material;
        public float terrainClearance = 0.05f;
        private Mesh mesh;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        //ROAD GEN
        public void GenerateRoad(List<Vector3> points)
        {
            GenerateRoad(points, null);
        }

        public void GenerateRoad(List<Vector3> points, Terrain terrain)
        {
            // parameters validation
            CheckParams(points, smoothingFactor);

            if (smoothingFactor > 0.0f)
            {
                for (int smoothingPass = 0; smoothingPass < smoothingIterations; smoothingPass++)
                {
                    AddSmoothingPoints(points);
                }
            }

            // if a terrain parameter was specified, replace the y-coordinate
            // of every point with the height of the terrain (+ an offset)
            if (terrain)
            {
                AdaptPointsToTerrainHeight(points, terrain);
            }

            Vector3 perpendicularDirection;
            Vector3 nextPoint;
            Vector3 nextNextPoint;
            Vector3 point1;
            Vector3 point2;
            Vector3 cornerPoint1;
            Vector3 cornerPoint2;
            Vector3 tangent;
            Vector3 cornerNormal;

            mesh = new Mesh();
            mesh.name = "Roadifier Road Mesh";

            var vertices = new List<Vector3>();
            var triangles = new List<int>();

            var idx = 0;
            foreach (Vector3 currentPoint in points)
            {
                if (idx == points.Count - 1)
                {
                    // no need to do anything in the last point, all triangles
                    // have been created in previous iterations
                    break;
                }
                else if (idx == points.Count - 2)
                {
                    // second to last point, we need to make up a "next next point"
                    nextPoint = points[idx + 1];
                    // assuming the 'next next' imaginary segment has the same
                    // direction as the real last one
                    nextNextPoint = nextPoint + (nextPoint - currentPoint);
                }
                else
                {
                    nextPoint = points[idx + 1];
                    nextNextPoint = points[idx + 2];
                }

                Vector3 terrainNormal1 = Vector3.up; // default normal: straight up
                Vector3 terrainNormal2 = Vector3.up; // default normal: straight up
                if (terrain)
                {
                    // if there's a terrain, calculate the actual normals
                    RaycastHit hit;
                    Ray ray;

                    ray = new Ray(currentPoint + Vector3.up, Vector3.down);
                    terrain.GetComponent<Collider>().Raycast(ray, out hit, 100.0f);
                    terrainNormal1 = hit.normal;

                    ray = new Ray(nextPoint + Vector3.up, Vector3.down);
                    terrain.GetComponent<Collider>().Raycast(ray, out hit, 100.0f);
                    terrainNormal2 = hit.normal;
                }

                // calculate the normal to the segment, so we can displace 'left' and 'right' of
                // the point by half the road width and create our first vertices there
                perpendicularDirection = (Vector3.Cross(terrainNormal1, nextPoint - currentPoint)).normalized;
                point1 = currentPoint + perpendicularDirection * roadWidth * 0.5f;
                point2 = currentPoint - perpendicularDirection * roadWidth * 0.5f;

                // here comes the tricky part...
                // we calculate the tangent to the corner between the current segment and the next
                tangent = ((nextNextPoint - nextPoint).normalized + (nextPoint - currentPoint).normalized).normalized;
                cornerNormal = (Vector3.Cross(terrainNormal2, tangent)).normalized;
                // project the normal line to the corner to obtain the correct length
                var cornerWidth = (roadWidth * 0.5f) / Vector3.Dot(cornerNormal, perpendicularDirection);
                cornerPoint1 = nextPoint + cornerWidth * cornerNormal;
                cornerPoint2 = nextPoint - cornerWidth * cornerNormal;

                // first point has no previous vertices set by past iterations
                if (idx == 0)
                {
                    vertices.Add(point1);
                    vertices.Add(point2);
                }
                vertices.Add(cornerPoint1);
                vertices.Add(cornerPoint2);

                int doubleIdx = idx * 2;

                // add first triangle
                triangles.Add(doubleIdx);
                triangles.Add(doubleIdx + 1);
                triangles.Add(doubleIdx + 2);

                // add second triangle
                triangles.Add(doubleIdx + 3);
                triangles.Add(doubleIdx + 2);
                triangles.Add(doubleIdx + 1);

                idx++;
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, GenerateUVs(vertices));
            mesh.triangles = triangles.ToArray();
            mesh.RecalculateNormals();

            CreateGameObject(mesh);
        }

        private void AddSmoothingPoints(List<Vector3> points)
        {
            for (int i = 0; i < points.Count - 2; i++)
            {
                Vector3 currentPoint = points[i];
                Vector3 nextPoint = points[i + 1];
                Vector3 nextNextPoint = points[i + 2];

                float distance1 = Vector3.Distance(currentPoint, nextPoint);
                float distance2 = Vector3.Distance(nextPoint, nextNextPoint);

                Vector3 dir1 = (nextPoint - currentPoint).normalized;
                Vector3 dir2 = (nextNextPoint - nextPoint).normalized;

                points.RemoveAt(i + 1);
                points.Insert(i + 1, currentPoint + dir1 * distance1 * (1.0f - smoothingFactor));
                points.Insert(i + 2, nextPoint + dir2 * distance2 * (smoothingFactor));
                i++;
            }
        }

        private void AdaptPointsToTerrainHeight(List<Vector3> points, Terrain terrain)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Vector3 point = points[i];
                points[i] = new Vector3(point.x, terrain.transform.position.y + terrainClearance + terrain.SampleHeight(new Vector3(point.x, 0, point.z)), point.z);
            }
        }

        private void CreateGameObject(Mesh mesh)
        {
            GameObject obj = new GameObject("Roadifier Road");//, MeshRenderer, MeshFilter, MeshCollider);
                                                              //obj.add
            obj.AddComponent<MeshRenderer>();
            obj.AddComponent<MeshFilter>();
            obj.AddComponent<MeshCollider>();

            obj.GetComponent<MeshFilter>().mesh = mesh;
            obj.transform.SetParent(transform);

            MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
            var materials = renderer.materials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }
            renderer.materials = materials;
        }

        private List<Vector2> GenerateUVs(List<Vector3> vertices)
        {
            List<Vector2> uvs = new List<Vector2>();

            for (int vertIdx = 0; vertIdx < vertices.Count; vertIdx++)
            {
                if (vertIdx % 4 == 0)
                {
                    uvs.Add(new Vector2(0, 0));
                }
                else if (vertIdx % 4 == 1)
                {
                    uvs.Add(new Vector2(0, 1));
                }
                else if (vertIdx % 4 == 2)
                {
                    uvs.Add(new Vector2(1, 0));
                }
                else
                {
                    uvs.Add(new Vector2(1, 1));
                }
            }
            return uvs;
        }

        private void CheckParams(List<Vector3> points, float smoothingFactor)
        {
            if (points.Count < 2)
            {
                //throw "At least two points are required to make a road";
                Debug.Log("At least two points are required to make a road");
            }

            if (smoothingFactor < 0.0f || smoothingFactor > 0.5f)
            {
                //throw "Smoothing factor should be between 0 and 0.5";
                Debug.Log("Smoothing factor should be between 0 and 0.5");
            }
        }
        //END  ROAD GEN
    }
}
