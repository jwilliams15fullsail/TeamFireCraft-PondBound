using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Artngame.PDM;
namespace Artngame.PDM
{
	public class BlastControlPDM : MonoBehaviour
	{

		public ParticleSystem Psystem;
		public ParticleSystemRenderer Prenderer;
		public float VelocityScale = 2;
		public SKinColoredMaskedCol SkinnedController;
		public bool Blast_on = false;
		bool grabbed_lights = false;
		bool Blast_in_progress = false;
		List<Light> Lights = new List<Light>();

		public float LightGrowSpeed = 2;
		public float LightFadeSpeed = 1;
		public float Max_vel_scale = 0.06f;
		public float max_light_intesity = 0.22f;

		public bool ScaleDownInit = false;//scale object down as it is sucked in a black hole and scale back before blast
		public float grow_back_speed = 9;
		public float grow_down_speed = 1;

		public SkinnedMeshRenderer Hero_mesh;
		public bool ControlBloom = false;
		public float BloomSpeed = 2f;

		public Material ParticleMat; //use to fade out particles with material, can also be done (more expensive) with per particle control

		public float BlastDelay = 1;
		public bool UseDelay = false; //instead of waiting for light intensity to grow, start blast after timer
		float StartTime;
		public bool GrowSizeOnDelay = false;//let partcile size grow on initial delay before blast
		public float GrowOnDelaySpeed = 0.05f;
		public bool Use_exponent = false; //use exponential growth
		public AnimationCurve ExpCurve;//define exponential increase with curve
		public float maxParticleSize = 0.1f;
		public float IncreaseParticleRate = 2; //decrease every other vertex count to Increase particles and make them gradually appear on the body
		public float Vanishdelay = 2;
		public float GameObjScaleSpeed = 2;
		public bool DestroyOnEnd = true;

		IEnumerator VanishParticles()
		{

			//wait two secs
			yield return new WaitForSeconds(2);

			while (Lights[0].intensity > 0)
			{
				//fade lights
				for (int i = 0; i < Lights.Count; i++)
				{
					Lights[i].intensity = Lights[i].intensity - Time.deltaTime * LightFadeSpeed;
				}
				if (Lights[0].intensity <= 0)
				{
					//end effect
					SkinnedController.Start_size = 0;
					//Psystem.startSize = 0;

					ParticleSystem.MainModule main = Psystem.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant = 0;

					Prenderer.enabled = false;
				}
				else
				{
					if (SkinnedController.Start_size > 0)
					{
						SkinnedController.Start_size -= 0.1f * Time.deltaTime * LightFadeSpeed;
					}
					if (Psystem.main.startSize.constant > 0)
					{
						//Psystem.startSize  -= 0.1f*Time.deltaTime * LightFadeSpeed;
						ParticleSystem.MainModule main = Psystem.main;//v2.3
						ParticleSystem.MinMaxCurve Curve = main.startSize;
						Curve.constant -= 0.1f * Time.deltaTime * LightFadeSpeed;//v2.3
					}
				}
				yield return null;
			}
		}

		IEnumerator VanishParticlesTimed()
		{

			//wait two secs
			if (ParticleMat != null)
			{
				ParticleMat.color = Color.Lerp(ParticleMat.color, new Color(ParticleMat.color.r, ParticleMat.color.g, ParticleMat.color.b, 0), Time.deltaTime * LightFadeSpeed * 0.45f);
			}
			yield return new WaitForSeconds(2);

			while (Time.fixedTime - (StartTime + 2) < Vanishdelay)
			{
				//fade lights
				for (int i = 0; i < Lights.Count; i++)
				{
					Lights[i].intensity = Lights[i].intensity - Time.deltaTime * LightFadeSpeed;
				}
				if (ParticleMat != null)
				{
					ParticleMat.color = Color.Lerp(ParticleMat.color, new Color(ParticleMat.color.r, ParticleMat.color.g, ParticleMat.color.b, 0), Time.deltaTime * LightFadeSpeed * 0.45f);
				}
				if (Time.fixedTime - (StartTime + 2) >= Vanishdelay - 0.05f)
				{
					//end effect
					SkinnedController.Start_size = 0;
					//Psystem.startSize = 0;
					ParticleSystem.MainModule main = Psystem.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant = 0;
					Prenderer.enabled = false;
					if (DestroyOnEnd)
					{
						if (SkinnedController.gameobject_mode && SkinnedController.Parent_OBJ != null)
						{
							Destroy(SkinnedController.Parent_OBJ);
						}
						Destroy(this.gameObject);
					}
				}
				else
				{
					if (SkinnedController.Start_size > 0)
					{
						SkinnedController.Start_size -= 0.1f * Time.deltaTime * LightFadeSpeed;
					}
					if (Psystem.main.startSize.constant > 0)
					{
						//Psystem.startSize  -= 0.1f*Time.deltaTime * LightFadeSpeed;
						ParticleSystem.MainModule main = Psystem.main;//v2.3
						ParticleSystem.MinMaxCurve Curve = main.startSize;
						Curve.constant -= 0.1f * Time.deltaTime * LightFadeSpeed;//v2.3
					}
				}
				yield return null;
			}
		}

		// Use this for initialization
		void Start()
		{
			Prenderer.velocityScale = VelocityScale;
			StartTime = Time.fixedTime;
			//reset color transparency
			if (ParticleMat != null)
			{
				ParticleMat.color = new Color(ParticleMat.color.r, ParticleMat.color.g, ParticleMat.color.b, 1);
			}
		}

		// Update is called once per frame
		void Update()
		{

			//if gameobject particles (lights) are erased
			for (int i = Lights.Count - 1; i >= 0; i--)
			{
				if (Lights[i] == null)
				{
					Lights.RemoveAt(i);
				}
			}

			if (Blast_in_progress)
			{

				//grab lights from gameobject particles
				if (!grabbed_lights)
				{
					Lights.Clear();
					//increase light instensity
					if (SkinnedController.Gameobj_instances.Count == SkinnedController.particle_count)
					{ //check if all objects are in
						grabbed_lights = true;
						for (int i = 0; i < SkinnedController.Gameobj_instances.Count; i++)
						{
							Lights.Add(SkinnedController.Gameobj_instances[i].GetComponent<Light>());
							Lights[i].intensity = 0;
							//scale sun beams
							SkinnedController.Gameobj_instances[i].localScale = SkinnedController.Gameobj_instances[i].localScale / 10;
						}
					}
				}
				else
				{
					for (int i = 0; i < Lights.Count; i++)
					{
						if (Lights[i].intensity < 0.1f)
						{
							Lights[i].intensity = 1.5f * Lights[i].intensity + Time.deltaTime * LightGrowSpeed;
						}
					}

					if (Lights[0].intensity > 0.12f)
					{
						if (Prenderer.velocityScale > Max_vel_scale)
						{
							Prenderer.velocityScale -= 0.5f * Time.deltaTime;
						}
						if (ScaleDownInit)
						{
							SkinnedController.Scale_factor += grow_back_speed * Time.deltaTime;
						}
					}
					else
					{
						if (Prenderer.velocityScale < 1)
						{
							Prenderer.velocityScale += 0.03f * Time.deltaTime;
						}
						if (ScaleDownInit)
						{
							SkinnedController.Scale_factor -= grow_down_speed * Time.deltaTime;
						}
					}

					//grow particle size gradually (also use Shuriken between curves size to make them change size to emulate blast behavior)
					if (GrowSizeOnDelay)
					{

						//two methods to know when to stop, either check light max intensity or define a specific delay (UseDelay option)
						if (UseDelay)
						{
							if (SkinnedController.Start_size < maxParticleSize)
							{
								if (Use_exponent)
								{
									float Exp = ExpCurve.Evaluate(Time.fixedTime - StartTime) * 2;
									SkinnedController.Start_size += Exp * GrowOnDelaySpeed * Time.deltaTime * 0.01f;
								}
								else
								{
									SkinnedController.Start_size += GrowOnDelaySpeed * Time.deltaTime * 0.01f;
								}
							}
						}
						else
						{
							if (SkinnedController.Start_size < maxParticleSize & Lights[0].intensity > (max_light_intesity - (max_light_intesity / 10)))
							{
								SkinnedController.Start_size += 0.01f * Time.deltaTime;
							}
						}
						for (int i = 0; i < SkinnedController.Gameobj_instances.Count; i++)
						{
							//scale sun beams
							SkinnedController.Gameobj_instances[i].localScale += new Vector3(1, 1, 1) * Time.deltaTime * GameObjScaleSpeed;
						}
						if (ControlBloom)
						{
							//Camera.main.GetComponent<UnityStandardAssets.ImageEffects.Bloom>().lensflareIntensity += 0.1f*Time.deltaTime*BloomSpeed;
						}
					}
					if ((Lights[0].intensity > max_light_intesity & !UseDelay) | (UseDelay & Time.fixedTime - StartTime > BlastDelay))
					{
						Blast_in_progress = false; // DELETE this - debug only

						if (UseDelay & Time.fixedTime - StartTime > BlastDelay)
						{
							StartCoroutine(VanishParticlesTimed());
							StartTime = Time.fixedTime;
						}
						else
						{
							StartCoroutine(VanishParticles());
						}
					}

					//control mesh particle emission per mask and vertex
					if (SkinnedController.Every_other_vertex > 1)
					{
						SkinnedController.Every_other_vertex -= 0.2f * Time.deltaTime * IncreaseParticleRate;
					}
					if (SkinnedController.low_mask_thres > 1)
					{
						SkinnedController.low_mask_thres -= (int)(0.2f * Time.deltaTime * IncreaseParticleRate);
					}
					if (ParticleMat != null)
					{
						ParticleMat.color = Color.Lerp(ParticleMat.color, new Color(ParticleMat.color.r, ParticleMat.color.g, ParticleMat.color.b, 0), Time.deltaTime * LightFadeSpeed * 0.04f);
					}
				}

				if (!Blast_in_progress)
				{
					//Release particles from mesh
					if (!SkinnedController.Let_loose)
					{
						SkinnedController.Let_loose = true;

						if (Hero_mesh != null)
						{
							Hero_mesh.enabled = false;
						}

						SkinnedController.Return_speed = 0.005f;
						float Gravity = 0.005f;
						SkinnedController.Return_speed = Gravity;
					}
					else if (SkinnedController.Let_loose)
					{
						SkinnedController.Let_loose = false;
					}
				}
			}

			//Start and reset blast
			if (Blast_on & !Blast_in_progress)
			{

				Blast_in_progress = true;

				StopCoroutine(VanishParticles());

				if (Hero_mesh != null)
				{
					//Hero_mesh.enabled = true;
				}
				if (ParticleMat != null)
				{
					ParticleMat.color = new Color(ParticleMat.color.r, ParticleMat.color.g, ParticleMat.color.b, 1);
				}

				StartTime = Time.fixedTime;
				//Psystem.startSize = 0.04f;
				ParticleSystem.MainModule main = Psystem.main;//v2.3
				ParticleSystem.MinMaxCurve Curve = main.startSize;
				Curve.constant = 0.04f; ;//v2.3
				SkinnedController.Start_size = 0.01f;
				SkinnedController.Scale_factor = 1;
				Prenderer.enabled = true;

				grabbed_lights = false;
				Blast_on = false;
			}
		}
	}
}