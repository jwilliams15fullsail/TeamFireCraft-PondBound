using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class PDM_Demo_PHOENIX : MonoBehaviour
	{

#pragma warning disable 414

		void Start()
		{

			HERO_CAM = HERO.GetComponent("ThirdPersonCameraPDM") as ThirdPersonCameraPDM;

			Phoenix_Spline = Phoenix.GetComponent("SplinerP") as SplinerP;
			SplineParticlePlacer = Phoenix.GetComponentInChildren(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;

			Camera_init_transform_position = Camera.main.transform.position;
			Camera_init_transform_rotation = Camera.main.transform.eulerAngles;

			AttractorTurbulant = new Component[2];

			PROJECTILES_COUNTER = 0;
			for (int i = 0; i < PROJECTILES.Length; i++)
			{
				PROJECTILES[i].gameObject.SetActive(false);
			}
			PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);

			Destroy(SPHERE);

			SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, HERO.transform.position, Quaternion.identity);

			SPHERE.transform.parent = null;

			PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);


			Gameobject_SCATTER = SPHERE.GetComponentInChildren(typeof(GameobjectProjection)) as GameobjectProjection;

			AttractorTurbulant = SPHERE.GetComponentsInChildren(typeof(AttractParticles));

			if (AttractorTurbulant != null & AttractorTurbulant.Length > 1)
			{
				AttractorTurbulant1 = AttractorTurbulant[0] as AttractParticles;
				AttractorTurbulant2 = AttractorTurbulant[1] as AttractParticles;
			}

			FLAMER_Particle = SPHERE.GetComponentInChildren(typeof(PlaceParticleFREEFORM)) as PlaceParticleFREEFORM;

			if (FLAMER_Particle != null)
			{
				Projectile5_FLAMMABLES = FLAMER_Particle.gameObject;

				Debug.Log("PROJECTILES_COUNTER=" + PROJECTILES_COUNTER);
			}

			if (PROJECTILES_COUNTER > PROJECTILES.Length - 2)
			{
				PROJECTILES_COUNTER = 0;
			}
			else
			{
				PROJECTILES_COUNTER = PROJECTILES_COUNTER + 1;
			}



		}

		public GameObject Phoenix;
		public GameObject Phoenix_BODY;
		private Vector3 Camera_init_transform_position;
		private Vector3 Camera_init_transform_rotation;

		public float Slide_left = 0f;

		public GameObject Leaf_floor;
		public GameObject GlobalPropagate;
		public GameObject Flammables;
		public GameObject Conductors;

		public GameObject SUN;

		public GameObject[] AURAS;
		public int AURA_COUNTER;
		public Texture AURAS_Texture;

		public GameObject[] SWORDS;
		public int SWORD_COUNTER;
		public Texture SWORD_Texture;

		public GameObject[] PROJECTILES;
		public int PROJECTILES_COUNTER;
		public Texture PROJECTILES_Texture;

		public GameObject[] SPLINES;
		public int SPLINES_COUNTER;
		public Texture SPLINES_Texture;

		public GameObject[] FIELDS;
		public int FIELDS_COUNTER;
		public Texture FIELDS_Texture;

		public GameObject[] FLOWS;
		public int FLOWS_COUNTER;
		public Texture FLOWS_Texture;

		public GameObject[] SHIELDS;
		public int SHIELDS_COUNTER;
		public Texture SHIELDS_Texture;

		public GameObject[] SKINNED_MESH;
		public int SKINNED_MESH_COUNTER;
		public Texture SKINNED_MESH_Texture;

		public GameObject[] VORTEXES;
		public int VORTEXES_COUNTER;
		public Texture VORTEXES_Texture;

		public GameObject[] WEATHER;
		public int WEATHER_COUNTER;
		public Texture WEATHER_Texture;

		public GameObject[] PARTICLE_PAINT;
		public int PARTICLE_PAINT_COUNTER;
		public Texture PARTICLE_PAINT_Texture;

		public GameObject[] IMAGE;
		public int IMAGE_COUNTER;
		public Texture IMAGE_Texture;

		GameObject SPHERE;

		SplinerP SPLINE_SPLINER;

		public GameObject FlameThrowers;
		public GameObject Orbs_around_hero;
		public GameObject Particle_floor;
		public GameObject Nebula;
		bool Nebula_Active;

		public Texture btnTexture;

		private int MODE = 0;

		float CAMERA_UP = 0.55f;
		float CAMERA_DIST = 8;

		public GameObject HERO;

		float AURA_RADIUS;
		float CURRENT_AURA_RADIUS;
		float AURA_HEIGHT;
		float CURRENT_AURA_HEIGHT;

		float AURA_SPREAD;
		int AURA_LOOSEN;
		bool AURA_TARGET;
		float CURRENT_AURA_SPREAD;
		int CURRENT_AURA_LOOSEN;

		PlaceParticleOnGround AURA;
		ThirdPersonCameraPDM HERO_CAM;

		public ImageToParticles IMAGE_PARTICLE;
		public ImageToParticlesDYNAMIC IMAGE_PARTICLE_DYNAMIC;

		float IMAGE_PARTICLE_RADIUS;
		float IMAGE_PARTICLE_DEPTH;
		float IMAGE_PARTICLE_SCALE;

		bool IMAGE_PARTICLE_CHANGE;

		float CURRENT_IMAGE_PARTICLE_RADIUS;
		float CURRENT_IMAGE_PARTICLE_DEPTH;
		float CURRENT_IMAGE_PARTICLE_SCALE;

		bool CURRENT_IMAGE_PARTICLE_CHANGE = false;

		public AttractParticles ATTRACTOR;

		bool Turbulance;
		float Turbulance_strength;
		float Turbulance_frequency;
		Vector3 Axis_affected;
		bool splash_effect;
		bool vortex_motion;
		int Vortex_count;
		float Vortex_life;
		float Vortex_angularvelocity;
		float Vortex_scale;
		float Vortex_center_size;
		Vector3 Vortex_center_color;
		bool Show_vortex;
		bool Color_force;
		Vector3 Force_color;
		bool use_exponent;
		float affectDistance;
		float dumpen;
		bool smoothattraction;
		bool repel;

		bool make_moving_star;
		float star_trail_dist;
		int star_trails;
		float trail_distance;
		float speed_of_trail;
		float distance_of_trail;
		float trail_length_out;
		float size_of_trail_out;
		float distance_between_trail;
		float vertical_trail_separation;
		float smooth_trail;

		private bool CURRENT_Turbulance;
		float CURRENT_Turbulance_strength;
		float CURRENT_Turbulance_frequency;
		Vector3 CURRENT_Axis_affected;
		bool CURRENT_splash_effect;
		bool CURRENT_vortex_motion;
		int CURRENT_Vortex_count;
		float CURRENT_Vortex_life;
		float CURRENT_Vortex_angularvelocity;
		float CURRENT_Vortex_scale;
		float CURRENT_Vortex_center_size;
		Vector3 CURRENT_Vortex_center_color;
		bool CURRENT_Show_vortex;
		bool CURRENT_Color_force;
		Vector3 CURRENT_Force_color;
		bool CURRENT_use_exponent;
		float CURRENT_affectDistance;
		float CURRENT_dumpen;
		bool CURRENT_smoothattraction;
		bool CURRENT_repel;

		bool CURRENT_make_moving_star;
		float CURRENT_star_trail_dist;
		int CURRENT_star_trails;
		float CURRENT_trail_distance;
		float CURRENT_speed_of_trail;
		float CURRENT_distance_of_trail;
		float CURRENT_trail_length_out;
		float CURRENT_size_of_trail_out;
		float CURRENT_distance_between_trail;
		float CURRENT_vertical_trail_separation;
		float CURRENT_smooth_trail;

		public bool HUD_ON = true;


		//PHOENIX

		float PHOENIX_SPEED;
		int Spline_Interpolate;

		SplinerP Phoenix_Spline;
		PlaceParticleOnSpline SplineParticlePlacer;

		private bool Camera_mounted = false;

		void OnGUI()
		{

			GUI.color = new Color32(255, 255, 255, 201);

			string HUD_state = "HUD ON";
			if (!HUD_ON) { HUD_state = "HUD OFF"; }
			if (GUI.Button(new Rect(10, 260, 80, 17), HUD_state)) { if (HUD_ON) { HUD_ON = false; } else { HUD_ON = true; } }

			if (HERO_CAM != null)
			{
				HERO_CAM.distance = CAMERA_DIST;
				HERO_CAM.height = CAMERA_UP;
			}

			if (!btnTexture)
			{
				Debug.LogError("Please assign a texture on the inspector");
				return;
			}


			///// PHOENIX //////


			if (GUI.Button(new Rect(10, 280, 120, 17), "Mount Camera"))
			{

				if (!Camera_mounted)
				{
					Camera_mounted = true;
					Camera.main.gameObject.transform.position = Phoenix_BODY.transform.position;
					Camera.main.gameObject.transform.eulerAngles = Phoenix_BODY.transform.eulerAngles;
					Camera.main.gameObject.transform.parent = Phoenix_BODY.transform;
				}
				else
				{
					Camera.main.transform.position = Camera_init_transform_position;
					Camera.main.transform.eulerAngles = Camera_init_transform_rotation;
					Camera_mounted = false;
					Camera.main.gameObject.transform.parent = null;
				}

			}

			if (SplineParticlePlacer != null)
			{

				if (GUI.Button(new Rect(10, 300, 120, 17), "Interpolate Steps")) { }
				Spline_Interpolate = (int)GUI.HorizontalSlider(new Rect(10, 320, 160, 17), SplineParticlePlacer.Interpolate_steps, 1, 15);
				SplineParticlePlacer.Interpolate_steps = Spline_Interpolate;
			}

			if (Phoenix_Spline != null)
			{

				string ACCEL_state = "ACCELERATE ON";
				if (!Phoenix_Spline.Accelerate) { ACCEL_state = "ACCELERATE OFF"; }

				if (GUI.Button(new Rect(10, 340, 100, 17), "Speed")) { }
				PHOENIX_SPEED = GUI.HorizontalSlider(new Rect(10, 360, 160, 17), Phoenix_Spline.Motion_Speed, 0.1f, 5);
				Phoenix_Spline.Motion_Speed = PHOENIX_SPEED;

				if (GUI.Button(new Rect(10, 380, 160, 17), ACCEL_state)) { if (Phoenix_Spline.Accelerate) { Phoenix_Spline.Accelerate = false; } else { Phoenix_Spline.Accelerate = true; } }

			}


			int PIXELS_DOWN = 18;

			int BOX_WIDTH = 70;
			int BOX_HEIGHT = 70;

			BOX_WIDTH = (int)Slide_left + BOX_WIDTH;

			if (HUD_ON)
			{

				if (GUI.Button(new Rect(0 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), PROJECTILES_Texture))
				{
					if (MODE == 3)
					{
						MODE = 0;
						for (int i = 0; i < PROJECTILES.Length; i++)
						{
							PROJECTILES[i].gameObject.SetActive(false);
						}
						Destroy(SPHERE);
					}
					else
					{
						MODE = 3;
					}
				}


				if (GUI.Button(new Rect(0 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Effects")) { }

				MODE = 3;

				if (MODE == 0 & !Nebula_Active)
				{

					Nebula.gameObject.SetActive(true);
					Nebula_Active = true;

					Leaf_floor.SetActive(false);
					GlobalPropagate.SetActive(false);
					Flammables.SetActive(false);
					Conductors.SetActive(false);

					//12
					for (int i = 0; i < VORTEXES.Length; i++)
					{
						VORTEXES[i].gameObject.SetActive(false);
					}

					//11
					for (int i = 0; i < IMAGE.Length; i++)
					{
						IMAGE[i].gameObject.SetActive(false);
					}

					//10
					for (int i = 0; i < PARTICLE_PAINT.Length; i++)
					{
						PARTICLE_PAINT[i].gameObject.SetActive(false);
					}

					//9
					for (int i = 0; i < WEATHER.Length; i++)
					{
						WEATHER[i].gameObject.SetActive(false);
					}

					//8
					for (int i = 0; i < SKINNED_MESH.Length; i++)
					{
						SKINNED_MESH[i].gameObject.SetActive(false);
					}

					//7
					for (int i = 0; i < SHIELDS.Length; i++)
					{
						SHIELDS[i].gameObject.SetActive(false);
					}
					FlameThrowers.gameObject.SetActive(false);

					//6
					for (int i = 0; i < FLOWS.Length; i++)
					{
						FLOWS[i].gameObject.SetActive(false);
					}

					//5
					for (int i = 0; i < FIELDS.Length; i++)
					{
						FIELDS[i].gameObject.SetActive(false);
					}

					//4
					for (int i = 0; i < SPLINES.Length; i++)
					{
						SPLINES[i].gameObject.SetActive(false);
					}
					//3
					for (int i = 0; i < PROJECTILES.Length; i++)
					{
						PROJECTILES[i].gameObject.SetActive(false);
					}
					Destroy(SPHERE);
					//2
					for (int i = 0; i < SWORDS.Length; i++)
					{
						SWORDS[i].gameObject.SetActive(false);
					}
					//1
					for (int i = 0; i < AURAS.Length; i++)
					{
						AURAS[i].gameObject.SetActive(false);
					}

				}
				else if (MODE != 0 & Nebula_Active) { Nebula.gameObject.SetActive(false); Nebula_Active = false; }


				if (MODE == 3)
				{ //draw MODE 3  - PROJECTILES

					if (GUI.Button(new Rect(0 * BOX_WIDTH + 10, BOX_HEIGHT + 50, BOX_WIDTH, 30), "Recast"))
					{

						Debug.Log(PROJECTILES_COUNTER);

						int COUNT_ME = PROJECTILES_COUNTER - 1;
						if (PROJECTILES_COUNTER == 0)
						{

							COUNT_ME = PROJECTILES.Length - 1;
						}
						PROJECTILES[COUNT_ME].gameObject.SetActive(true);

						Destroy(SPHERE);

						SPHERE = (GameObject)Instantiate(PROJECTILES[COUNT_ME].gameObject, HERO.transform.position, Quaternion.identity);

						SPHERE.transform.parent = null;

						PROJECTILES[COUNT_ME].gameObject.SetActive(false);

						#region Get Effect scripts
						Gameobject_SCATTER = SPHERE.GetComponentInChildren(typeof(GameobjectProjection)) as GameobjectProjection;

						AttractorTurbulant = SPHERE.GetComponentsInChildren(typeof(AttractParticles));

						if (AttractorTurbulant != null & AttractorTurbulant.Length > 1)
						{
							AttractorTurbulant1 = AttractorTurbulant[0] as AttractParticles;
							AttractorTurbulant2 = AttractorTurbulant[1] as AttractParticles;
						}

						FLAMER_Particle = SPHERE.GetComponentInChildren(typeof(PlaceParticleFREEFORM)) as PlaceParticleFREEFORM;

						if (FLAMER_Particle != null)
						{
							Projectile5_FLAMMABLES = FLAMER_Particle.gameObject;

							Debug.Log("PROJECTILES_COUNTER=" + PROJECTILES_COUNTER);
						}


						#endregion
					}


					if (GUI.Button(new Rect(0 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log("Projectile " + PROJECTILES_COUNTER);

						for (int i = 0; i < PROJECTILES.Length; i++)
						{
							PROJECTILES[i].gameObject.SetActive(false);
						}
						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);

						Destroy(SPHERE);

						SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, HERO.transform.position, Quaternion.identity);

						SPHERE.transform.parent = null;

						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);


						Gameobject_SCATTER = SPHERE.GetComponentInChildren(typeof(GameobjectProjection)) as GameobjectProjection;

						AttractorTurbulant = SPHERE.GetComponentsInChildren(typeof(AttractParticles));

						if (AttractorTurbulant != null & AttractorTurbulant.Length > 1)
						{
							AttractorTurbulant1 = AttractorTurbulant[0] as AttractParticles;
							AttractorTurbulant2 = AttractorTurbulant[1] as AttractParticles;
						}

						FLAMER_Particle = SPHERE.GetComponentInChildren(typeof(PlaceParticleFREEFORM)) as PlaceParticleFREEFORM;

						if (FLAMER_Particle != null)
						{
							Projectile5_FLAMMABLES = FLAMER_Particle.gameObject;

							Debug.Log("PROJECTILES_COUNTER=" + PROJECTILES_COUNTER);
						}

						if (PROJECTILES_COUNTER > PROJECTILES.Length - 2)
						{
							PROJECTILES_COUNTER = 0;
						}
						else
						{
							PROJECTILES_COUNTER = PROJECTILES_COUNTER + 1;
						}
					}

					if (PROJECTILES_COUNTER == 6 | PROJECTILES_COUNTER == 7 | PROJECTILES_COUNTER == 8 | PROJECTILES_COUNTER == 0)
					{
						if (Phoenix.activeInHierarchy)
						{
							Phoenix.SetActive(false);
						}

					}
					else
					{

						if (!Phoenix.activeInHierarchy)
						{
							Phoenix.SetActive(true);
						}
					}

					if ((PROJECTILES_COUNTER == 6 | PROJECTILES_COUNTER == 8 | PROJECTILES_COUNTER == 0) & (Projectile5_FLAMMABLES != null | PROJECTILES_COUNTER == 8))
					{ //gameobject, on fire


						string TITLE = "Flame on";
						if (PROJECTILES_COUNTER != 8)
						{ //if lighning only

							if (Projectile5_FLAMMABLES.gameObject.activeInHierarchy)
							{
								TITLE = "Flame off";
							}
							else { TITLE = "Flame on"; }

							if (GUI.Button(new Rect(2 * BOX_WIDTH + 35, BOX_HEIGHT + 120, BOX_WIDTH, 30), TITLE))
							{


								if (FLAMER_Particle != null)
								{
									if (FLAMER_Particle.gameObject != null)
									{
										Projectile5_FLAMMABLES = FLAMER_Particle.gameObject;
									}
								}

								if (FLAMER_Particle.gameObject != null)
								{
									if (Projectile5_FLAMMABLES.gameObject.activeInHierarchy)
									{
										Projectile5_FLAMMABLES.gameObject.SetActive(false);
									}
									else { Projectile5_FLAMMABLES.gameObject.SetActive(true); }
								}

							}
						}

						TITLE = "Gravity Forces on";
						int X_DIST = 35;

						//if(AttractorTurbulant1!=null & AttractorTurbulant2!=null){
						if (AttractorTurbulant1 != null)
						{
							if (AttractorTurbulant1.enabled)
							{
								TITLE = "Gravity on";
							}
							else { TITLE = "Gravity off"; }

							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 160, BOX_WIDTH, 30), TITLE))
							{
								if (AttractorTurbulant1.enabled)
								{
									AttractorTurbulant1.enabled = false;
								}
								else
								{
									AttractorTurbulant1.enabled = true;
									if (AttractorTurbulant2 != null)
									{
										AttractorTurbulant2.enabled = false;
									}
								}
							}
						}

						if (AttractorTurbulant2 != null)
						{
							if (AttractorTurbulant2.enabled)
							{
								TITLE = "Vortex on";
							}
							else { TITLE = "Vortex off"; }

							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST + 80, BOX_HEIGHT + 160, BOX_WIDTH, 30), TITLE))
							{
								if (AttractorTurbulant2.enabled)
								{
									AttractorTurbulant2.enabled = false;
								}
								else
								{
									AttractorTurbulant2.enabled = true;
									if (AttractorTurbulant1 != null)
									{
										AttractorTurbulant1.enabled = false;
									}
								}
							}

						}


						if (Gameobject_SCATTER != null)
						{
							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 200, 150, 17), "Extend Scatter range")) { }
							Scatter_extend = GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 220, 150, 17), Gameobject_SCATTER.extend, 1, 100);
							Gameobject_SCATTER.extend = Scatter_extend;

							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 240, 150, 17), "Settle speed")) { }
							Scatter_settle = GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 260, 150, 17), Gameobject_SCATTER.Return_speed, 0.005f, 0.1f);
							Gameobject_SCATTER.Return_speed = Scatter_settle;

							string NAME_IT = "Position free";
							if (Gameobject_SCATTER.fix_initial) { NAME_IT = "Position Fixed"; }

							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 280, 150, 17), NAME_IT))
							{
								if (Gameobject_SCATTER.fix_initial) { Gameobject_SCATTER.fix_initial = false; } else { Gameobject_SCATTER.fix_initial = true; }
							}
							NAME_IT = "Projection conformed";
							if (Gameobject_SCATTER.letloose) { NAME_IT = "Particles freed"; }
							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 300, 150, 17), NAME_IT))
							{
								if (Gameobject_SCATTER.letloose) { Gameobject_SCATTER.letloose = false; } else { Gameobject_SCATTER.letloose = true; }
							}
							NAME_IT = "Gavity off";
							if (Gameobject_SCATTER.Gravity_Mode) { NAME_IT = "Gavity on"; }
							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 320, 150, 17), NAME_IT))
							{
								if (Gameobject_SCATTER.Gravity_Mode) { Gameobject_SCATTER.Gravity_Mode = false; } else { Gameobject_SCATTER.Gravity_Mode = true; }
							}
							NAME_IT = "Not Following";
							if (Gameobject_SCATTER.follow_particles) { NAME_IT = "Following particles"; }
							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 340, 150, 17), NAME_IT))
							{
								if (Gameobject_SCATTER.follow_particles) { Gameobject_SCATTER.follow_particles = false; } else { Gameobject_SCATTER.follow_particles = true; }
							}
							NAME_IT = "Colliders on";
							if (Gameobject_SCATTER.Remove_colliders) { NAME_IT = "Colliders off"; }
							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 360, 150, 17), NAME_IT))
							{
								if (Gameobject_SCATTER.Remove_colliders) { Gameobject_SCATTER.Remove_colliders = false; } else { Gameobject_SCATTER.Remove_colliders = true; }
							}

							NAME_IT = "Particles hidden";
							if (Gameobject_SCATTER.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled) { NAME_IT = "Particles shown"; }
							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 380, 150, 17), NAME_IT))
							{
								if (Gameobject_SCATTER.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled) { Gameobject_SCATTER.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = false; } else { Gameobject_SCATTER.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = true; }
							}


							if (PROJECTILES_COUNTER == 6)
							{
								if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 400, 150, 17), "Gravity")) { }

								ParticleSystem.MainModule main = Gameobject_SCATTER.gameObject.GetComponent<ParticleSystem>().main;//v2.3
								ParticleSystem.MinMaxCurve Curve = main.gravityModifier;

								Gravity = GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 420, 150, 17), Curve.constant, 0, 5);
								//Gameobject_SCATTER.gameObject.GetComponent<ParticleSystem>().gravityModifier = Gravity;
								Curve.constant = Gravity;
							}
							if (PROJECTILES_COUNTER == 0 & FLAMER_Particle != null)
							{
								if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 400, 180, 17), "Flame performance factor")) { }
								DelayFLAME = GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 420, 150, 17), FLAMER_Particle.Delay, 4, 40);
								FLAMER_Particle.Delay = DelayFLAME;
							}

						}

					}

					//NAME THEM
					if (PROJECTILES_COUNTER == 1)
					{
						if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 10, 280, 27), "Lightning")) { }
					}
					if (PROJECTILES_COUNTER == 2)
					{
						if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 10, 280, 27), "Flame propagation")) { }
					}
					if (PROJECTILES_COUNTER == 3)
					{
						if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 10, 280, 27), "Turbulence")) { }
					}
					if (PROJECTILES_COUNTER == 4)
					{
						if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 10, 280, 27), "Stair to Heaven")) { }
					}
					if (PROJECTILES_COUNTER == 5)
					{
						if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 10, 280, 27), "Volcano")) { }
					}
					if (PROJECTILES_COUNTER == 6)
					{
						if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 10, 340, 27), "Gameobject Scattering, Forces and Dynamic Fire")) { }
					}
					if (PROJECTILES_COUNTER == 7)
					{
						if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 10, 280, 27), "Phoenix")) { }
					}
					if (PROJECTILES_COUNTER == 8)
					{
						if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 10, 340, 27), "Gameobject Scattering, Forces and Dynamic Lightning")) { }
					}

				}

			}

		} //END ON_GUI

		private GameObject Projectile5_FLAMMABLES;
		GameobjectProjection Gameobject_SCATTER;
		Component[] AttractorTurbulant;

		AttractParticles AttractorTurbulant1;
		AttractParticles AttractorTurbulant2;

		PlaceParticleFREEFORM FLAMER_Particle;
		float Scatter_extend;
		float Scatter_settle;

		float Gravity;
		float DelayFLAME;


	}

}