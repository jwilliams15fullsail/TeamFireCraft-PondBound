using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Collections.Generic;

namespace Artngame.BrunetonsOcean
{
    [ExecuteInEditMode]
	public class ProjectedGrid : MonoBehaviour
    {
        //v1.2
        float prevEuler = 0;
        public float minAngleSpeed = 15; //if previosu to nxt frame has 15 degrees difference, do update

        public bool createMeshes = false;

        public Transform SUN;

        public Material m_oceanMaterial;

        public GameObject m_grid;

        private Projection m_projection;

        public int pixelsPerQuad = 8;

        public float forwardOffset = 6;//100;//200
        public float heightOffset = 12;//50;//100
        public float heightScale = 0.3f;

        public float maxheight = 10;

        void Start()
		{
            m_projection = new Projection();
            m_projection.OceanLevel = transform.position.y;
            m_projection.MaxHeight = Camera.main.transform.position.y; //maxheight;// 10.0f;


            MeshFilter meshes = this.gameObject.GetComponentInChildren<MeshFilter>(true);

            if (meshes == null && m_grid == null)
            {
                CreateGrid(pixelsPerQuad); //CreateGrid(8);
                UpdateA();
            }

            //v1.2
            prevEuler = Camera.main.transform.eulerAngles.y;
        }

        public int updateEvery = 1;
        public int updateEveryRot = 2;
        int currentUpdate = 0;
        int currentRotUpdate = 0;

        void Update()
        {
            if (!Application.isPlaying)
            {
                //if meshes found and empty, auto recreate
                MeshFilter meshes = this.gameObject.GetComponentInChildren<MeshFilter>(true);
                if(meshes != null && meshes.sharedMesh == null)
                {
                    RecreateMesh(pixelsPerQuad);
                    UpdateA();
                }
            }

            if (createMeshes)
            {
                RecreateMesh(pixelsPerQuad);
                createMeshes = false;
            }

            if(m_projection == null)
            {
                Start();
            }

            //if (Application.isPlaying)
            if (updateEvery > 0) //v1.2
            {
                // if (!Application.isPlaying || currentUpdate > updateEvery || Camera.main.transform.eulerAngles.y != prevEuler)//v1.2
                if (!Application.isPlaying || currentUpdate > updateEvery || (Camera.main.transform.eulerAngles.y != prevEuler 
                    && (currentRotUpdate > updateEveryRot || Mathf.Abs(Camera.main.transform.eulerAngles.y - prevEuler) > minAngleSpeed)))//v1.2
                {
                    UpdateA();
                    currentUpdate = 0;
                    currentRotUpdate = 0;//v1.2

                    prevEuler = Camera.main.transform.eulerAngles.y;//v1.2
                }
            }
            else
            {
                UpdateA(); //v1.2
            }
            currentUpdate++;

            if (Camera.main.transform.eulerAngles.y != prevEuler)//v1.2
            {
                currentRotUpdate++;
            }
        }

        //v1.1
        public float eulerX = 0;
        public float heightScaler = 200;
        public bool autoAdjustGrid = false;

            void UpdateA()
        {
          
            Camera cam = Camera.main;
            if (cam == null || m_oceanMaterial == null) return;

            m_projection.OceanLevel = transform.position.y;
            m_projection.MaxHeight = 10;

            //v1.1
            if (autoAdjustGrid)
            {
                m_projection.MaxHeight = maxheight + cam.transform.position.y - 100;// maxheight;// 10.0f;
                if (cam.transform.position.y > 320)
                {
                    //m_projection.MaxHeight = maxheight + 320 - 100;
                }

                if (cam.transform.position.y < 4)
                {
                    //maxheight = 322;
                    m_projection.MaxHeight = 322 + cam.transform.position.y - 100;
                }
                //v1.2a
                if (cam.transform.position.y < -3)
                {
                    maxheight = 372;
                    m_projection.MaxHeight = 372;
                    //m_projection.MaxHeight = 222 + cam.transform.position.y - 100;
                }
                if (cam.transform.position.y < -7)
                {
                    //maxheight = 322;
                    m_projection.MaxHeight = 222 + cam.transform.position.y - 100;
                }
                if (cam.transform.position.y < -20)
                {
                    //maxheight = 322;
                    //m_projection.MaxHeight = 222 + cam.transform.position.y - 100;
                }

                
            }

            //Camera modifiedCam = new Camera(); 
            // modifiedCam.CopyFrom(cam);
            Vector3 keepEuler = cam.transform.eulerAngles;//v1.1
            Vector3 keepPos = cam.transform.position;
            cam.transform.position += new Vector3(0, heightOffset, 0) - cam.transform.forward * forwardOffset; //emulate a camera more upwards than water level, to reduce close detail
            //Debug.Log("cam.transform.eulerAngles.x" + cam.transform.eulerAngles.x);

            //v1.1
            if (autoAdjustGrid)
            {
                float eulerXA = Mathf.Lerp(cam.transform.eulerAngles.x, eulerX, m_projection.MaxHeight / heightScaler);
                if (cam.transform.eulerAngles.x > 180)
                {
                    eulerXA = Mathf.Lerp(cam.transform.eulerAngles.x - 360, eulerX, m_projection.MaxHeight / heightScaler);
                }
                cam.transform.eulerAngles = new Vector3(eulerXA, cam.transform.eulerAngles.y, cam.transform.eulerAngles.z);
            }

            //cam.transform.position +=  new Vector3(cam.transform.forward.x,0, cam.transform.forward.z)* forwardOffset;

            m_projection.UpdateProjection(cam);//m_projection.UpdateProjection(cam);

            //v1.1
            cam.transform.eulerAngles = keepEuler;
            cam.transform.position = keepPos;// new Vector3(0, -heightOffset, 0) + cam.transform.forward * forwardOffset;
           

            m_oceanMaterial.SetMatrix("_Interpolation", m_projection.Interpolation);

            //Once the camera goes below the projection plane (the ocean level) the projected
            //grid will flip the triangle winding order. 
            //Need to flip culling so the top remains the top.
            bool isFlipped = m_projection.IsFlipped;
            m_oceanMaterial.SetInt("_CullFace", (isFlipped) ? (int)CullMode.Front : (int)CullMode.Back);

            //SUN LIGHT
            if (SUN != null)
            {
                m_oceanMaterial.SetVector("SUN_DIR", SUN.transform.forward * -1.0f);
            }

            //scale it 
            this.transform.localScale = new Vector3(transform.localScale.x, heightScale, transform.localScale.z);
        }

        /// <summary>
        /// Creates the ocean mesh gameobject.
        /// The resolutions is how many pixels per quad in mesh.
        /// The higher the number the less verts in mesh.
        /// </summary>
        void CreateGrid(int resolution)
        {

            int width = 1320;// Screen.width;
            int height = 720;// Screen.height;
            int numVertsX = width / resolution;
            int numVertsY = height / resolution;

            Mesh mesh = CreateQuad(numVertsX, numVertsY);

            if (mesh == null) return;

            //The position of the mesh is not known until its projected in the shader. 
            //Make the bounds large enough so the camera will draw it.
            float bigNumber = 1e6f;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(bigNumber, 20.0f, bigNumber));

            m_grid = new GameObject("Ocean mesh");
            m_grid.transform.parent = transform;

            MeshFilter filter = m_grid.AddComponent<MeshFilter>();
            MeshRenderer renderer = m_grid.AddComponent<MeshRenderer>();

            filter.sharedMesh = mesh;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
            renderer.sharedMaterial = m_oceanMaterial;

        }

        //v0.5
        void RecreateMesh(int resolution)
        {

            if(resolution < 3)
            {
                resolution = 3;
            }

            int width = 1320;// Screen.width;
            int height = 720;// Screen.height;
            int numVertsX = width / resolution;
            int numVertsY = height / resolution;

            Mesh mesh = CreateQuad(numVertsX, numVertsY);
           // Debug.Log("Count =" + mesh.vertexCount);
            if (mesh == null) return;

            //The position of the mesh is not known until its projected in the shader. 
            //Make the bounds large enough so the camera will draw it.
            float bigNumber = 1e6f;
            mesh.bounds = new Bounds(Vector3.zero, new Vector3(bigNumber, 20.0f, bigNumber));

            MeshFilter[] meshes = this.gameObject.GetComponentsInChildren<MeshFilter>(true);
          //  Debug.Log("Count =" + meshes.Length);
            for (int i = 0; i < meshes.Length; i++)
            {
                //m_grid = new GameObject("Ocean mesh");
                //m_grid.transform.parent = transform;

                //MeshFilter filter = m_grid.AddComponent<MeshFilter>();
                //MeshRenderer renderer = m_grid.AddComponent<MeshRenderer>();
                if (meshes[i].sharedMesh == null)
                {
                    meshes[i].sharedMesh = mesh;
                    meshes[i].mesh = mesh;
                }
                //renderer.shadowCastingMode = ShadowCastingMode.Off;
                //renderer.receiveShadows = false;
                //renderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                //renderer.sharedMaterial = m_oceanMaterial;
            }

        }

        public Mesh CreateQuad(int numVertsX, int numVertsY)
		{
			
			Vector3[] vertices = new Vector3[numVertsX * numVertsY];
			Vector2[] texcoords = new Vector2[numVertsX * numVertsY];
			int[] indices = new int[numVertsX * numVertsY * 6];
			
			for (int x = 0; x < numVertsX; x++)
			{
				for (int y = 0; y < numVertsY; y++)
				{
                    Vector2 uv = new Vector3(x / (numVertsX - 1.0f), y / (numVertsY - 1.0f));

                    texcoords[x + y * numVertsX] = uv;
				    vertices[x + y * numVertsX] = new Vector3(uv.x, uv.y, 0.0f);
				}
			}
			
			int num = 0;
			for (int x = 0; x < numVertsX - 1; x++)
			{
				for (int y = 0; y < numVertsY - 1; y++)
				{
					indices[num++] = x + y * numVertsX;
					indices[num++] = x + (y + 1) * numVertsX;
					indices[num++] = (x + 1) + y * numVertsX;
					
					indices[num++] = x + (y + 1) * numVertsX;
					indices[num++] = (x + 1) + (y + 1) * numVertsX;
					indices[num++] = (x + 1) + y * numVertsX;
				}
			}

            if (vertices.Length > 65000)
            {
                //Too many verts to make a mesh. 
                //You will need to split the mesh.
                return null;
            }
            else
            {
                Mesh mesh = new Mesh();
                mesh.vertices = vertices;
                mesh.uv = texcoords;
                mesh.triangles = indices;

                return mesh;
            }
		}

    }


}







