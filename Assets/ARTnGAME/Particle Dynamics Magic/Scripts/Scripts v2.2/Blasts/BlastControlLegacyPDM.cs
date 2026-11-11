using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Artngame.PDM;
namespace Artngame.PDM
{
	public class BlastControlLegacyPDM : MonoBehaviour
	{

		public ParticleSystem Psystem;
		public ParticleSystemRenderer Prenderer;
		public SKinColoredMasked SkinnedController;
		public SkinnedMeshRenderer Hero_mesh;
		public Material ParticleMat; //use to fade out particles with material, can also be done (more expensive) with per particle control
		public float maxParticleSize = 4f;
		public float Vanishdelay = 2;
		public float GrowSpeed = 2;
		float StartTime = 0;

		//float disolvedTime = 0;
		public float desolveTime = 10;
		public bool triggerDisolve = false;

		public bool disable_player_mesh = false;

		IEnumerator VanishParticlesTimed()
		{

			//wait two secs
			if (ParticleMat != null)
			{

			}
			yield return new WaitForSeconds(2);

			while (Time.fixedTime - (StartTime + 2) < Vanishdelay)
			{
				//fade 
				yield return null;
			}
		}

		// Use this for initialization
		void Start()
		{
			StartTime = Time.fixedTime; //v2.3
		}

		// Update is called once per frame
		void Update()
		{
			if (SkinnedController.Start_size < maxParticleSize)
			{
				SkinnedController.Start_size += Time.deltaTime * GrowSpeed;
			}
			else
			{
				if (disable_player_mesh)
				{
					if (Hero_mesh.enabled)
					{
						Hero_mesh.enabled = false;
					}
				}
			}

			if ((Time.fixedTime - StartTime > desolveTime) || triggerDisolve)
			{ //v2.3
				SkinnedController.Let_loose = true;
				SkinnedController.Gravity_Mode = false;
				SkinnedController.Start_size = SkinnedController.Start_size - Time.deltaTime * 2;
				//Debug.Log (SkinnedController.Start_size);
				if (SkinnedController.Start_size <= 0)
				{
					Destroy(SkinnedController.emitter);
					Destroy(Psystem.gameObject);
				}
			}

		}

	}
}