using UnityEngine;
using System.Collections;
namespace Artngame.PDM
{
	public class BlobyConnectionAuraPDM : MonoBehaviour
	{

		public Transform bodyA;
		public Transform bodyB;
		public Transform bodyC;
		public Transform bodyD;
		public Transform connectorBody;

		public bool useAttractor = true;//for dynamic effect, using attraction force evenly spread across between two particles
		public bool useShader = false;//use shader blob effect
		public bool useMultiAgent = false;
		public Material bodyA_Mat;
		public Material bodyB_Mat;

		public bool GUIon = false;

		public float effectPower = 1;
		public float effectPowerA = 1;
		public float falloffPower = 5;

		public float fadePower = 22;
		public float fadeFalloff = 2;

		public float bodyDist;
		public float bodyDist2;
		public float bodyDist3;

		public float _worldBase = 0;

		// Use this for initialization
		void Start()
		{

		}

		// Update is called once per frame
		void LateUpdate()
		{
			if (useAttractor)
			{
				connectorBody.position = bodyA.transform.position + (bodyB.transform.position - bodyA.transform.position) / 2;
			}

			if (useShader)
			{
				bodyA_Mat.SetVector("_bodyPos", new Vector4(bodyB.position.x, bodyB.position.y, bodyB.position.z, 1));
				bodyB_Mat.SetVector("_bodyPos", new Vector4(bodyA.position.x, bodyA.position.y, bodyA.position.z, 1));

				bodyDist = (bodyA.position - bodyB.position).magnitude;
				float effectPowerFINAL = effectPower * 59544.86f + (-8.046278f - 59544.86f) / (1 + Mathf.Pow((bodyDist / 8.751007f), effectPowerA * 13.14056f));

				bodyA_Mat.SetFloat("_effectPower", effectPowerFINAL);
				bodyA_Mat.SetFloat("_falloffPower", falloffPower);

				bodyB_Mat.SetFloat("_effectPower", effectPowerFINAL);
				bodyB_Mat.SetFloat("_falloffPower", falloffPower);

				bodyA_Mat.SetFloat("_fadePower", fadePower);
				bodyA_Mat.SetFloat("_fadeFalloff", fadeFalloff);

				bodyB_Mat.SetFloat("_fadePower", fadePower);
				bodyB_Mat.SetFloat("_fadeFalloff", fadeFalloff);

				bodyA_Mat.SetFloat("_bodyDist", bodyDist);
				bodyB_Mat.SetFloat("_bodyDist", bodyDist);
				bodyA_Mat.SetFloat("_worldBase", _worldBase);
				bodyB_Mat.SetFloat("_worldBase", _worldBase);

				//
				if (useMultiAgent)
				{
					bodyDist2 = (bodyA.position - bodyC.position).magnitude;
					bodyA_Mat.SetVector("_bodyPos2", new Vector4(bodyC.position.x, bodyC.position.y, bodyC.position.z, 1));
					bodyA_Mat.SetFloat("_bodyDist2", bodyDist2);

					bodyDist3 = (bodyA.position - bodyD.position).magnitude;
					bodyA_Mat.SetVector("_bodyPos3", new Vector4(bodyD.position.x, bodyD.position.y, bodyD.position.z, 1));
					bodyA_Mat.SetFloat("_bodyDist3", bodyDist3);
				}

			}

		}

		void OnGUI()
		{

			if (GUIon)
			{
				GUI.Label(new Rect(10, 10 + 32 * 0, 100, 22), "Effect Power");
				effectPower = GUI.HorizontalSlider(new Rect(10, 10 + 32 * 1, 100, 22), effectPower, -10000, 10000);

				GUI.Label(new Rect(10, 10 + 32 * 2, 100, 22), "Falloff Power");
				falloffPower = GUI.HorizontalSlider(new Rect(10, 10 + 32 * 3, 100, 22), falloffPower, 0.5f, 9);

				GUI.Label(new Rect(10, 10 + 32 * 4, 100, 22), "Fade Power");
				fadePower = GUI.HorizontalSlider(new Rect(10, 10 + 32 * 5, 100, 22), fadePower, 0, 1000);

				GUI.Label(new Rect(10, 10 + 32 * 6, 100, 22), "Fade Falloff");
				fadeFalloff = GUI.HorizontalSlider(new Rect(10, 10 + 32 * 7, 100, 22), fadeFalloff, 0.5f, 5);


			}

		}

	}
}