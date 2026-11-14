using UnityEngine;
namespace Artngame.RiverStudio.FastFluids
{
    [RequireComponent(typeof(Terrain))]
    public class UnityTerrainAdapterInfiniRIVER : MonoBehaviour
    {
        public SimulationInfiniRIVER Simulation;

        private TerrainData _terrainData;
        private Terrain _terrain;

        //v0.1
        public bool varyVariablesForFlow = false;//create flow by varying parameters
        public float varyVariablesFreq = 1;
        public float varyVariablesOffset = 0.5f;

        private void Start()
        {
            _terrain = GetComponent<Terrain>();
            _terrainData = _terrain.terrainData;
        }
        public bool useUnityTerrain = true;
        public bool useUnityRenderTexture = false;
        public bool appendObstacles = false;
        public float depthFactor = 1;  public float depthOffset = 0; public float depthCamFactor = 1; public float depthScale = 1;
        public RenderTexture terrainTex;
        public RenderTexture osbtaclesTex;
        public RenderTexture DebugTerrainTex;
        void Update()
        {

            if (varyVariablesForFlow)
            {
                Simulation.Settings.TimeScale = varyVariablesOffset * (Mathf.Cos(Time.fixedTime*varyVariablesFreq) + 2) + varyVariablesOffset;
            }

            if (useUnityTerrain && !appendObstacles && Simulation != null && _terrainData != null)
            {
                Simulation.UpdateSurfaceFromTerrainData(_terrainData);               
            }
            if (useUnityTerrain && appendObstacles && Simulation != null && _terrainData != null)
            {               
                Simulation.UpdateSurfaceFromTerrainDataAPPEND(_terrainData, osbtaclesTex, depthFactor, depthOffset, depthCamFactor, depthScale);
            }
            if (useUnityRenderTexture && terrainTex!=null && !appendObstacles && Simulation != null)
            {
                Simulation.UpdateSurfaceFromRenderTexture(terrainTex);
            }
            if (useUnityRenderTexture && terrainTex != null && appendObstacles && Simulation != null)
            {
                Simulation.UpdateSurfaceFromRenderTextureAPPEND(terrainTex, osbtaclesTex, depthFactor, depthOffset, depthCamFactor, depthScale);
            }
            DebugTerrainTex = _terrainData.heightmapTexture;
        }
    }
}