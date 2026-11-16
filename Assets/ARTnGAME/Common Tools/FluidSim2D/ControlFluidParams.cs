using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using FluidSim2DProject;

namespace Artngame.CommonTools.FluidSim2D
{

    public class ControlFluidParams : MonoBehaviour
    {
        public FluidSim fluidSimulation;
        public float impulseModInit;
        public float powImpulseMod = 1;
        public float freqImpulseMod = 1;

        public Vector2 windDirYminMax = new Vector2(-25, 25);
        public float freqWindDitYMod = 1;

        // Start is called before the first frame update
        void Start()
        {
            if (fluidSimulation != null)
            {
                impulseModInit = fluidSimulation.m_impluseRadius;
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (fluidSimulation != null)
            {
                fluidSimulation.m_impluseRadius = impulseModInit + powImpulseMod * Mathf.Cos(Time.fixedTime * freqImpulseMod);


                // fluidSimulation.m_windDirection.z = windDirYminMax.y * Mathf.Cos(Time.fixedTime * freqWindDitYMod);
            }
        }
    }
}