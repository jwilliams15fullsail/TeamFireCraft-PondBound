using UnityEngine;
using System.Collections;
//using FluidSim2DProject;

//namespace Artngame. {
namespace Artngame.CommonTools.FluidSim2D
{
    public class RippleCausticsGeneratorSMDemo : MonoBehaviour
    {

        public Transform moveObstacle;
        public Transform moveFish;
        public FluidSim fluidSimulator;
        public float Yoffset = 0;
        public bool showGUI = false;

        void Start()
        {


        }

        void OnGUI()
        {
            if (showGUI)
            {
                float obstacleX = GUI.HorizontalSlider(new Rect(0, 0 * 22 + Yoffset, 60, 20), fluidSimulator.m_obstaclePos.x, -0.2f, 1.2f);
                fluidSimulator.m_obstaclePos.x = obstacleX;

                float obstacleRadius = GUI.HorizontalSlider(new Rect(0, 1 * 22 + Yoffset, 60, 20), fluidSimulator.m_obstacleRadius, 0.01f, 0.4f);
                fluidSimulator.m_obstacleRadius = obstacleRadius;

                float fluidDensity = GUI.HorizontalSlider(new Rect(70, 0 * 22 + Yoffset, 60, 20), fluidSimulator.m_impulseDensity, -0.1f, 4.9f);
                fluidSimulator.m_impulseDensity = fluidDensity;

                randomizefreq = GUI.HorizontalSlider(new Rect(70, 1 * 22 + Yoffset, 60, 20), randomizefreq, 0.5f, 10f);

                if (GUI.Button(new Rect(70 * 2, 0 * 22 + Yoffset, 120, 20), "Auto Density"))
                {
                    if (randomizeDensity)
                    {
                        randomizeDensity = false;
                    }
                    else
                    {
                        randomizeDensity = true;
                    }
                }

                //if (GUI.Button (new Rect (70 * 6, 0 * 22 + Yoffset, 120, 20), "Toggle fish")) {
                if (GUI.Button(new Rect(70 * 2, 1 * 22 + Yoffset, 120, 20), "Toggle fish"))
                {
                    if (moveFish.gameObject.activeInHierarchy)
                    {
                        moveFish.gameObject.SetActive(false);
                    }
                    else
                    {
                        moveFish.gameObject.SetActive(true);
                    }
                }

                //if (GUI.Button (new Rect (70 * 4, 0 * 22 + Yoffset, 120, 20), "Toggle Flow")) {
                if (GUI.Button(new Rect(70 * 2, 2 * 22 + Yoffset, 120, 20), "Toggle Flow"))
                {
                    if (toggleFlow)
                    {
                        toggleFlow = false;

                        fluidSimulator.m_impulseTemperature = 0.034f;
                        fluidSimulator.m_impulseDensity = 1.15f;
                        fluidSimulator.m_temperatureDissipation = 0.978f;
                        fluidSimulator.m_velocityDissipation = 0.99f;
                        fluidSimulator.m_densityDissipation = 0.9988f;
                        fluidSimulator.m_numJacobiIterations = 50;
                        fluidSimulator.m_impluseRadius = 1.08f;

                    }
                    else
                    {
                        toggleFlow = true;

                        fluidSimulator.m_impulseTemperature = 0.03f;
                        fluidSimulator.m_impulseDensity = 0.994f;
                        fluidSimulator.m_temperatureDissipation = 0.985f;
                        fluidSimulator.m_velocityDissipation = 0.99999f;
                        fluidSimulator.m_densityDissipation = 0.9999f;
                        fluidSimulator.m_numJacobiIterations = 70;
                        fluidSimulator.m_impluseRadius = 1.02f;

                    }
                }

                //show timer - counter
                if (GUI.Button(new Rect(70 * 2, 3 * 22 + Yoffset, 120, 20), "Time:" + Mathf.RoundToInt(Time.fixedTime)))
                { }
            }
        }

        bool randomizeDensity = false;
        bool toggleFlow = false;
        float randomizefreq = 1;

        void Update()
        {

            if (randomizeDensity)
            {
                fluidSimulator.m_impulseDensity = (Mathf.Cos(Time.fixedTime * randomizefreq) + 1) + (Mathf.Sin(Time.fixedTime * randomizefreq / 2 + 2) + 1)
                    + (Mathf.Sin(Time.fixedTime * randomizefreq / 4.2f + 3) + 0.5f);
                fluidSimulator.m_impulseDensity = fluidSimulator.m_impulseDensity / 2;
            }
        }

    }
    //
}