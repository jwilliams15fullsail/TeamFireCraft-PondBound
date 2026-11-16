using UnityEngine;
using System.Collections;
using UnityEngine.UI;//v0.1

namespace FluidSim2DProject
{

    public class FluidSimLight : MonoBehaviour
    {

		//v0.1
		//public RenderTexture outputFlow;
		public RenderTexture m_guiTex;

        public Vector4 m_windDirection = new Vector4(1, 1,0,0);

        public Color m_fluidColor = Color.red;

        public Color m_obstacleColor = Color.white;
      
        public Material m_guiMat, m_advectMat, m_buoyancyMat, m_divergenceMat, m_jacobiMat, m_impluseMat, m_gradientMat, m_obstaclesMat;

        //RenderTexture m_guiTex, 
		RenderTexture m_divergenceTex, m_obstaclesTex;
        RenderTexture[] m_velocityTex, m_densityTex, m_pressureTex, m_temperatureTex;

		public float m_timeStep = 0.125f;
		public float m_impulseTemperature = 10.0f;
		public float m_impulseDensity = 1.0f;
		public float m_temperatureDissipation = 0.99f;
		public float m_velocityDissipation = 0.99f;
		public float m_densityDissipation = 0.9999f;
		public float m_ambientTemperature = 0.0f;
		public  float m_smokeBuoyancy = 1.0f;
		public  float m_smokeWeight = 0.05f;

		public float m_cellSize = 1.0f;
		public float m_gradientScale = 1.0f;

        Vector2 m_inverseSize;
		public int m_numJacobiIterations = 50;

		public Vector2 m_implusePos = new Vector2(0.5f, 0.0f);
		public float m_impluseRadius = 0.1f;
		public float m_mouseImpluseRadius = 0.05f;

		public Vector2 m_obstaclePos = new Vector2(0.5f, 0.5f);
		public  float m_obstacleRadius = 0.1f;

        Image m_gui; //GUITexture m_gui; //v0.1

        public int m_width, m_height;
		public Vector2 m_offset;

        void Start()
        {

            m_gui = GetComponent<Image>();//GetComponent<GUITexture>(); //v0.1

            //m_width = (int)m_gui.pixelInset.width;
            //m_height = (int)m_gui.pixelInset.height;
            //m_offset = new Vector2(m_gui.pixelInset.x, m_gui.pixelInset.y);
            m_width = 512;// (int)m_gui.minWidth;
            m_height = 512;// (int)m_gui.minHeight;
            m_offset = Vector2.zero;// new Vector2(m_gui.pixelsPerUnitMultiplier, m_gui.pixelsPerUnitMultiplier);

            m_inverseSize = new Vector2(1.0f / m_width, 1.0f / m_height);

            m_velocityTex = new RenderTexture[2];
            m_densityTex = new RenderTexture[2];
            m_temperatureTex = new RenderTexture[2];
            m_pressureTex = new RenderTexture[2];

            //CreateSurface(m_velocityTex, RenderTextureFormat.RGFloat, FilterMode.Bilinear);
            //CreateSurface(m_densityTex, RenderTextureFormat.RFloat, FilterMode.Bilinear);
            //CreateSurface(m_temperatureTex, RenderTextureFormat.RFloat, FilterMode.Bilinear);
            //CreateSurface(m_pressureTex, RenderTextureFormat.RFloat, FilterMode.Point);

            //for mobile, billinear is not supported for Full 32 bit float !!! use halfs !!!
            CreateSurface(m_velocityTex, RenderTextureFormat.RGHalf, FilterMode.Bilinear);
            CreateSurface(m_densityTex, RenderTextureFormat.RHalf, FilterMode.Bilinear);
            CreateSurface(m_temperatureTex, RenderTextureFormat.RHalf, FilterMode.Bilinear);
            CreateSurface(m_pressureTex, RenderTextureFormat.RHalf, FilterMode.Point);



            //v0.1 - make guitexture as public output
            //          m_guiTex = new RenderTexture(m_width, m_height, 0, RenderTextureFormat.ARGB32);
            //          m_guiTex.filterMode = FilterMode.Bilinear;
            //          m_guiTex.wrapMode = TextureWrapMode.Clamp;
            //          m_guiTex.Create();
            RenderTexture rt = UnityEngine.RenderTexture.active;
			UnityEngine.RenderTexture.active = m_guiTex;
			GL.Clear(true, true, Color.red);
			UnityEngine.RenderTexture.active = rt;


            m_divergenceTex = new RenderTexture(m_width, m_height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            m_divergenceTex.filterMode = FilterMode.Point;
            m_divergenceTex.wrapMode = TextureWrapMode.Clamp;
            m_divergenceTex.Create();

            m_obstaclesTex = new RenderTexture(m_width, m_height, 0, RenderTextureFormat.RHalf, RenderTextureReadWrite.Linear);
            m_obstaclesTex.filterMode = FilterMode.Point;
            m_obstaclesTex.wrapMode = TextureWrapMode.Clamp;
            m_obstaclesTex.Create();

            //GetComponent<Image>().material.mainTexture = m_guiTex; //GetComponent<GUITexture>().texture = m_guiTex; //v0.1
            m_guiMat.SetTexture("_Obstacles", m_obstaclesTex);

        }

        void CreateSurface(RenderTexture[] surface, RenderTextureFormat format, FilterMode filter)
        {
            surface[0] = new RenderTexture(m_width, m_height, 0, format, RenderTextureReadWrite.Linear);
            surface[0].filterMode = filter;
            surface[0].wrapMode = TextureWrapMode.Clamp;
            surface[0].Create();

            surface[1] = new RenderTexture(m_width, m_height, 0, format, RenderTextureReadWrite.Linear);
            surface[1].filterMode = filter;
            surface[1].wrapMode = TextureWrapMode.Clamp;
            surface[1].Create();
        }

        void Advect(RenderTexture velocity, RenderTexture source, RenderTexture dest, float dissipation)
        {
            m_advectMat.SetVector("_InverseSize", m_inverseSize);
            m_advectMat.SetFloat("_TimeStep", m_timeStep);
            m_advectMat.SetFloat("_Dissipation", dissipation);
            m_advectMat.SetTexture("_Velocity", velocity);
            m_advectMat.SetTexture("_Source", source);
            m_advectMat.SetTexture("_Obstacles", m_obstaclesTex);

            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_advectMat);
        }

        void ApplyBuoyancy(RenderTexture velocity, RenderTexture temperature, RenderTexture density, RenderTexture dest)
        {

            m_buoyancyMat.SetTexture("_Velocity", velocity);
            m_buoyancyMat.SetTexture("_Temperature", temperature);
            m_buoyancyMat.SetTexture("_Density", density);
            m_buoyancyMat.SetFloat("_AmbientTemperature", m_ambientTemperature);
            m_buoyancyMat.SetFloat("_TimeStep", m_timeStep);
            m_buoyancyMat.SetFloat("_Sigma", m_smokeBuoyancy);
            m_buoyancyMat.SetFloat("_Kappa", m_smokeWeight);

            //v0.1
            m_buoyancyMat.SetVector("_Wind", m_windDirection);

            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_buoyancyMat);
        }

        void ApplyImpulse(RenderTexture source, RenderTexture dest, Vector2 pos, float radius, float val)
        {

            m_impluseMat.SetVector("_Point", pos);
            m_impluseMat.SetFloat("_Radius", radius);
            m_impluseMat.SetFloat("_Fill", val);
            m_impluseMat.SetTexture("_Source", source);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_impluseMat);

        }

        void ComputeDivergence(RenderTexture velocity, RenderTexture dest)
        {

            m_divergenceMat.SetFloat("_HalfInverseCellSize", 0.5f / m_cellSize);
            m_divergenceMat.SetTexture("_Velocity", velocity);
            m_divergenceMat.SetVector("_InverseSize", m_inverseSize);
            m_divergenceMat.SetTexture("_Obstacles", m_obstaclesTex);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_divergenceMat);
        }

        void Jacobi(RenderTexture pressure, RenderTexture divergence, RenderTexture dest)
        {

            m_jacobiMat.SetTexture("_Pressure", pressure);
            m_jacobiMat.SetTexture("_Divergence", divergence);
            m_jacobiMat.SetVector("_InverseSize", m_inverseSize);
            m_jacobiMat.SetFloat("_Alpha", -m_cellSize * m_cellSize);
            m_jacobiMat.SetFloat("_InverseBeta", 0.25f);
            m_jacobiMat.SetTexture("_Obstacles", m_obstaclesTex);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_jacobiMat);
        }

        void SubtractGradient(RenderTexture velocity, RenderTexture pressure, RenderTexture dest)
        {
            m_gradientMat.SetTexture("_Velocity", velocity);
            m_gradientMat.SetTexture("_Pressure", pressure);
            m_gradientMat.SetFloat("_GradientScale", m_gradientScale);
            m_gradientMat.SetVector("_InverseSize", m_inverseSize);
            m_gradientMat.SetTexture("_Obstacles", m_obstaclesTex);
            dest.DiscardContents();
            ClearSurface(dest);
            Graphics.Blit(null, dest, m_gradientMat);
        }

        void AddObstacles()
        {
            m_obstaclesMat.SetVector("_InverseSize", m_inverseSize);
            m_obstaclesMat.SetVector("_Point", m_obstaclePos);
            m_obstaclesMat.SetFloat("_Radius", m_obstacleRadius);
            m_obstaclesTex.DiscardContents();
            ClearSurface(m_obstaclesTex);
            Graphics.Blit(null, m_obstaclesTex, m_obstaclesMat);

//			m_obstaclesMat.SetVector("_InverseSize", m_inverseSize);
//			m_obstaclesMat.SetVector("_Point", m_obstaclePos + new Vector2(0.1f,0.1f));
//			m_obstaclesMat.SetFloat("_Radius", m_obstacleRadius * 1.2f);
//
//			Graphics.Blit(m_obstaclesTex, m_obstaclesTex, m_obstaclesMat);
        }

        void ClearSurface(RenderTexture surface)
        {
            Graphics.SetRenderTarget(surface);
            GL.Clear(false, true, new Color(0, 0, 0, 0));
            Graphics.SetRenderTarget(null);
        }

        void Swap(RenderTexture[] texs)
        {
            RenderTexture temp = texs[0];
            texs[0].DiscardContents();
            texs[0] = texs[1];
            texs[1] = temp;
        }

        //v0.2
        public int updateEvery = 1;
        public int currentUpdate = 0;

        void Update()
        {
            //v0.2 - optimize fps
            if (currentUpdate == updateEvery)
            {
                currentUpdate = 0;
                
            }
            else
            {
                currentUpdate++;
                return;
            }

           

            //m_velocityTex[0].filterMode = FilterMode.Bilinear;
            //m_velocityTex[1].filterMode = FilterMode.Bilinear;
            //m_densityTex[0].filterMode = FilterMode.Bilinear;
            //m_densityTex[1].filterMode = FilterMode.Bilinear;
            //m_temperatureTex[0].filterMode = FilterMode.Bilinear;
            //m_temperatureTex[1].filterMode = FilterMode.Bilinear;

            //Obstacles only need to be added once unless changed.
            AddObstacles();

            //Set the density field and obstacle color.
            m_guiMat.SetColor("_FluidColor", m_fluidColor);
            m_guiMat.SetColor("_ObstacleColor", m_obstacleColor);

            int READ = 0;
            int WRITE = 1;

            //Advect velocity against its self
            Advect(m_velocityTex[READ], m_velocityTex[READ], m_velocityTex[WRITE], m_velocityDissipation);
            //Advect temperature against velocity
            Advect(m_velocityTex[READ], m_temperatureTex[READ], m_temperatureTex[WRITE], m_temperatureDissipation);
            //Advect density against velocity
            Advect(m_velocityTex[READ], m_densityTex[READ], m_densityTex[WRITE], m_densityDissipation);

            Swap(m_velocityTex);
            Swap(m_temperatureTex);
            Swap(m_densityTex);

            //Determine how the flow of the fluid changes the velocity
            ApplyBuoyancy(m_velocityTex[READ], m_temperatureTex[READ], m_densityTex[READ], m_velocityTex[WRITE]);

            Swap(m_velocityTex);

            //Refresh the impluse of density and temperature
            ApplyImpulse(m_temperatureTex[READ], m_temperatureTex[WRITE], m_implusePos, m_impluseRadius, m_impulseTemperature);
            ApplyImpulse(m_densityTex[READ], m_densityTex[WRITE], m_implusePos, m_impluseRadius, m_impulseDensity);

            Swap(m_temperatureTex);
            Swap(m_densityTex);

            //If left click down add impluse, if right click down remove impulse from mouse pos.
            if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
            {
                Vector2 pos = Input.mousePosition;

                pos.x -= Screen.width * 0.5f;
                pos.y -= Screen.height * 0.5f;

                pos -= m_offset;

                pos.x /= m_width - 1.0f;
                pos.y /= m_height - 1.0f;

                float sign = (Input.GetMouseButton(0)) ? 1.0f : -1.0f;

                ApplyImpulse(m_temperatureTex[READ], m_temperatureTex[WRITE], pos, m_mouseImpluseRadius, m_impulseTemperature);
                ApplyImpulse(m_densityTex[READ], m_densityTex[WRITE], pos, m_mouseImpluseRadius, m_impulseDensity * sign);

                Swap(m_temperatureTex);
                Swap(m_densityTex);
            }

            //Calculates how divergent the velocity is
           
            ComputeDivergence(m_velocityTex[READ], m_divergenceTex);

            ClearSurface(m_pressureTex[READ]);

            

            int i = 0;
            for (i = 0; i < m_numJacobiIterations; ++i)
            {
                Jacobi(m_pressureTex[READ], m_divergenceTex, m_pressureTex[WRITE]);
                Swap(m_pressureTex);
            }

            //Use the pressure tex that was last rendered into. This computes divergence free velocity
            SubtractGradient(m_velocityTex[READ], m_pressureTex[READ], m_velocityTex[WRITE]);

            Swap(m_velocityTex);
            m_guiTex.DiscardContents();
            ClearSurface(m_guiTex);
            //Render the rex you want to see into gui tex. Will only use the red channel
            Graphics.Blit(m_densityTex[READ], m_guiTex, m_guiMat);
        }
    }

}
