using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class PDM_Demo_CASTING : MonoBehaviour
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


			MODE = 3;
			PROJECTILES_COUNTER = 0;

			PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);


			SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);

			SPHERE.transform.parent = Holder;

			PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);

			for (int j = 0; j < PROJECTILE_NAMES.Length; j++)
			{

				if (j == PROJECTILES_COUNTER)
				{
					PROJECTILE_NAMES[j].gameObject.SetActive(true);
				}
				else { PROJECTILE_NAMES[j].gameObject.SetActive(false); }
			}
			PROJECTILES_COUNTER = 1;


		}

		//v2.0 - Adapted turbulence demo v1.3.5
		public Transform Holder; //parent instances to holder, to disable easilly

		public GameObject Colliders_to_hide;
		public GameObject Setting_to_hide;

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

		public GameObject[] PROJECTILE_NAMES;

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


		SKinnedGAmeobjEmit SKINNED;

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
		int Spline_Quality;
		float Spline_grow;
		float Particle_size;

		SplinerP Phoenix_Spline;
		PlaceParticleOnSpline SplineParticlePlacer;

		private bool Settings_mounted = true;

		void OnGUI()
		{

			GUI.color = new Color32(255, 255, 255, 201);



			string HUD_state = "HUD ON";
			if (!HUD_ON) { HUD_state = "HUD OFF"; }
			if (GUI.Button(new Rect(10, 320 + 115, 80, 17), HUD_state)) { if (HUD_ON) { HUD_ON = false; } else { HUD_ON = true; } }

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


			///// PHOENIX ///////



			//parent camera
			if (Setting_to_hide != null)
			{
				if (GUI.Button(new Rect(10, 340 + 115, 120, 21), "Toggle Background"))
				{

					if (Setting_to_hide.activeInHierarchy)
					{
						Camera.main.clearFlags = CameraClearFlags.SolidColor;
						Setting_to_hide.SetActive(false);
					}
					else
					{
						Camera.main.clearFlags = CameraClearFlags.Skybox;
						Setting_to_hide.SetActive(true);
					}
					//GameObject go = Instantiate(Resources.Load("AAA"),new Vector3(451.9189f,36.71621f,-35.49669f),Quaternion.identity) as GameObject; 

					//GameObject go = Instantiate(Resources.Load("DDD"),new Vector3(451.9189f,36.71621f,-35.49669f),Quaternion.identity) as GameObject; 

				}
			}

			if (GUI.Button(new Rect(10, 360 + 120, 120, 21), "Settings"))
			{

				if (!Settings_mounted)
				{
					Settings_mounted = true;

				}
				else
				{

					Settings_mounted = false;
				}

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


				if (GUI.Button(new Rect(0 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Effect")) { }


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

					if (GUI.Button(new Rect(0 * BOX_WIDTH + 10, BOX_HEIGHT + 50, BOX_WIDTH, 30), "Respawn"))
					{

						Debug.Log(PROJECTILES_COUNTER);

						int COUNT_ME = PROJECTILES_COUNTER - 1;
						if (PROJECTILES_COUNTER == 0)
						{

							COUNT_ME = PROJECTILES.Length - 1;
						}
						PROJECTILES[COUNT_ME].gameObject.SetActive(true);

						Destroy(SPHERE);

						SPHERE = (GameObject)Instantiate(PROJECTILES[COUNT_ME].gameObject, PROJECTILES[COUNT_ME].gameObject.transform.position, PROJECTILES[COUNT_ME].gameObject.transform.rotation);

						SPHERE.transform.parent = Holder;

						PROJECTILES[COUNT_ME].gameObject.SetActive(false);

						#region EXTRA

						for (int j = 0; j < PROJECTILE_NAMES.Length; j++)
						{

							if (j == PROJECTILES_COUNTER)
							{
								PROJECTILE_NAMES[j].gameObject.SetActive(true);
							}
							else { PROJECTILE_NAMES[j].gameObject.SetActive(false); }
						}


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

						#region ATTRACTOR

						ATTRACTOR = SPHERE.GetComponentInChildren(typeof(AttractParticles)) as AttractParticles;

						if (ATTRACTOR != null)
						{
							CURRENT_Turbulance = ATTRACTOR.Turbulance;//bool--
							CURRENT_Turbulance_strength = ATTRACTOR.Turbulance_strength;//float--
							CURRENT_Turbulance_frequency = ATTRACTOR.Turbulance_frequency;//float--
							CURRENT_Axis_affected = ATTRACTOR.Axis_affected;//Vector3--
							CURRENT_splash_effect = ATTRACTOR.splash_effect;//bool--
							CURRENT_vortex_motion = ATTRACTOR.vortex_motion;//bool--
							CURRENT_Vortex_count = ATTRACTOR.Vortex_count;//int--
							CURRENT_Vortex_life = ATTRACTOR.Vortex_life;//float--
							CURRENT_Vortex_angularvelocity = ATTRACTOR.Vortex_angularvelocity;//float--
							CURRENT_Vortex_scale = ATTRACTOR.Vortex_scale;//float--
							CURRENT_Vortex_center_size = ATTRACTOR.Vortex_center_size;//float--
							CURRENT_Vortex_center_color = new Vector3(ATTRACTOR.Vortex_center_color.r, ATTRACTOR.Vortex_center_color.g, ATTRACTOR.Vortex_center_color.b);//color
							CURRENT_Show_vortex = ATTRACTOR.Show_vortex;//bool--
							CURRENT_Color_force = ATTRACTOR.Color_force;//bool--
							CURRENT_Force_color = new Vector3(ATTRACTOR.Force_color.r, ATTRACTOR.Force_color.g, ATTRACTOR.Force_color.b);//color
							CURRENT_use_exponent = ATTRACTOR.use_exponent;//bool--
							CURRENT_affectDistance = ATTRACTOR.affectDistance;//float--
							CURRENT_dumpen = ATTRACTOR.dumpen;//float--
							CURRENT_smoothattraction = ATTRACTOR.smoothattraction;//bool--
							CURRENT_repel = ATTRACTOR.repel;//bool--

							CURRENT_make_moving_star = ATTRACTOR.make_moving_star;//bool

							if (ATTRACTOR.make_moving_star == true)
							{
								CURRENT_star_trail_dist = ATTRACTOR.star_trail_dist;//float--
								CURRENT_star_trails = ATTRACTOR.star_trails;//int--
								CURRENT_trail_distance = ATTRACTOR.trail_distance;//float--
								CURRENT_speed_of_trail = ATTRACTOR.speed_of_trail;//float--
								CURRENT_distance_of_trail = ATTRACTOR.distance_of_trail;//float--
								CURRENT_trail_length_out = ATTRACTOR.trail_length_out; //float--
								CURRENT_size_of_trail_out = ATTRACTOR.size_of_trail_out;//float--
								CURRENT_distance_between_trail = ATTRACTOR.distance_between_trail;//float--
								CURRENT_vertical_trail_separation = ATTRACTOR.vertical_trail_separation;//float
								CURRENT_smooth_trail = ATTRACTOR.smooth_trail;//float
							}
						}

						#endregion

						#region IMAGE
						IMAGE_PARTICLE = SPHERE.GetComponentInChildren(typeof(ImageToParticles)) as ImageToParticles;

						IMAGE_PARTICLE_DYNAMIC = SPHERE.GetComponentInChildren(typeof(ImageToParticlesDYNAMIC)) as ImageToParticlesDYNAMIC;

						if (IMAGE_PARTICLE_DYNAMIC != null)
						{
							CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE_DYNAMIC.PARTICLE_SIZE;
							CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE_DYNAMIC.DEPTH_FACTOR;
							CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE_DYNAMIC.SCALE_FACTOR;

							CURRENT_IMAGE_PARTICLE_CHANGE = IMAGE_PARTICLE_DYNAMIC.Changing_texture;
						}

						if (IMAGE_PARTICLE != null)
						{
							CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE.PARTICLE_SIZE;
							CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE.DEPTH_FACTOR;
							CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE.SCALE_FACTOR;
						}
						#endregion

						#region AURA

						AURA = SPHERE.GetComponentInChildren(typeof(PlaceParticleOnGround)) as PlaceParticleOnGround;

						if (AURA != null)
						{
							CURRENT_AURA_RADIUS = AURA.Circle_radius;

							CURRENT_AURA_HEIGHT = AURA.Dist_Above_Terrain;

							CURRENT_AURA_SPREAD = AURA.spread;
							CURRENT_AURA_LOOSEN = AURA.loosen_circle;

						}
						#endregion

						#region SKINNED

						SKINNED = SPHERE.GetComponentInChildren(typeof(SKinnedGAmeobjEmit)) as SKinnedGAmeobjEmit;


						#endregion

						#region PHOENIX
						ProceduralNoisePDM PN = SPHERE.GetComponentInChildren(typeof(ProceduralNoisePDM)) as ProceduralNoisePDM;


						if (PN != null)
						{
							SplineParticlePlacer = PN.GetComponent(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
							//Phoenix_Spline = PN.transform.parent.GetComponent(typeof(SplinerP)) as SplinerP;

							if (SplineParticlePlacer != null)
							{
								Debug.Log("aass1");
							}
							if (Phoenix_Spline != null)
							{
								Debug.Log("aass2");
							}

						}

						if (PROJECTILES_COUNTER == 50)
						{

							SplineParticlePlacer = SPHERE.GetComponentInChildren(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
							Phoenix_Spline = SPHERE.GetComponentInChildren(typeof(SplinerP)) as SplinerP;

						}

						#endregion
					}




					#region BACK BUTTON

					if (GUI.Button(new Rect(0 * BOX_WIDTH + 10, BOX_HEIGHT + 80, BOX_WIDTH, 30), "Back"))
					{

						Debug.Log("Projectile " + PROJECTILES_COUNTER);


						if (PROJECTILES_COUNTER == 0) { PROJECTILES_COUNTER = PROJECTILES.Length - 2; }
						else if (PROJECTILES_COUNTER >= 2)
						{
							PROJECTILES_COUNTER = PROJECTILES_COUNTER - 2;
						}
						else
						{
							PROJECTILES_COUNTER = PROJECTILES.Length - 1;
						}



						for (int i = 0; i < PROJECTILES.Length; i++)
						{
							PROJECTILES[i].gameObject.SetActive(false);
						}
						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);

						Destroy(SPHERE);

						SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);

						SPHERE.transform.parent = Holder;

						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);

						for (int j = 0; j < PROJECTILE_NAMES.Length; j++)
						{

							if (j == PROJECTILES_COUNTER)
							{
								PROJECTILE_NAMES[j].gameObject.SetActive(true);
							}
							else { PROJECTILE_NAMES[j].gameObject.SetActive(false); }
						}


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


						#region ATTRACTOR

						ATTRACTOR = SPHERE.GetComponentInChildren(typeof(AttractParticles)) as AttractParticles;

						if (ATTRACTOR != null)
						{
							CURRENT_Turbulance = ATTRACTOR.Turbulance;//bool--
							CURRENT_Turbulance_strength = ATTRACTOR.Turbulance_strength;//float--
							CURRENT_Turbulance_frequency = ATTRACTOR.Turbulance_frequency;//float--
							CURRENT_Axis_affected = ATTRACTOR.Axis_affected;//Vector3--
							CURRENT_splash_effect = ATTRACTOR.splash_effect;//bool--
							CURRENT_vortex_motion = ATTRACTOR.vortex_motion;//bool--
							CURRENT_Vortex_count = ATTRACTOR.Vortex_count;//int--
							CURRENT_Vortex_life = ATTRACTOR.Vortex_life;//float--
							CURRENT_Vortex_angularvelocity = ATTRACTOR.Vortex_angularvelocity;//float--
							CURRENT_Vortex_scale = ATTRACTOR.Vortex_scale;//float--
							CURRENT_Vortex_center_size = ATTRACTOR.Vortex_center_size;//float--
							CURRENT_Vortex_center_color = new Vector3(ATTRACTOR.Vortex_center_color.r, ATTRACTOR.Vortex_center_color.g, ATTRACTOR.Vortex_center_color.b);//color
							CURRENT_Show_vortex = ATTRACTOR.Show_vortex;//bool--
							CURRENT_Color_force = ATTRACTOR.Color_force;//bool--
							CURRENT_Force_color = new Vector3(ATTRACTOR.Force_color.r, ATTRACTOR.Force_color.g, ATTRACTOR.Force_color.b);//color
							CURRENT_use_exponent = ATTRACTOR.use_exponent;//bool--
							CURRENT_affectDistance = ATTRACTOR.affectDistance;//float--
							CURRENT_dumpen = ATTRACTOR.dumpen;//float--
							CURRENT_smoothattraction = ATTRACTOR.smoothattraction;//bool--
							CURRENT_repel = ATTRACTOR.repel;//bool--

							CURRENT_make_moving_star = ATTRACTOR.make_moving_star;//bool

							if (ATTRACTOR.make_moving_star == true)
							{
								CURRENT_star_trail_dist = ATTRACTOR.star_trail_dist;//float--
								CURRENT_star_trails = ATTRACTOR.star_trails;//int--
								CURRENT_trail_distance = ATTRACTOR.trail_distance;//float--
								CURRENT_speed_of_trail = ATTRACTOR.speed_of_trail;//float--
								CURRENT_distance_of_trail = ATTRACTOR.distance_of_trail;//float--
								CURRENT_trail_length_out = ATTRACTOR.trail_length_out; //float--
								CURRENT_size_of_trail_out = ATTRACTOR.size_of_trail_out;//float--
								CURRENT_distance_between_trail = ATTRACTOR.distance_between_trail;//float--
								CURRENT_vertical_trail_separation = ATTRACTOR.vertical_trail_separation;//float
								CURRENT_smooth_trail = ATTRACTOR.smooth_trail;//float
							}
						}

						#endregion

						#region IMAGE
						IMAGE_PARTICLE = SPHERE.GetComponentInChildren(typeof(ImageToParticles)) as ImageToParticles;

						IMAGE_PARTICLE_DYNAMIC = SPHERE.GetComponentInChildren(typeof(ImageToParticlesDYNAMIC)) as ImageToParticlesDYNAMIC;

						if (IMAGE_PARTICLE_DYNAMIC != null)
						{
							CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE_DYNAMIC.PARTICLE_SIZE;
							CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE_DYNAMIC.DEPTH_FACTOR;
							CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE_DYNAMIC.SCALE_FACTOR;

							CURRENT_IMAGE_PARTICLE_CHANGE = IMAGE_PARTICLE_DYNAMIC.Changing_texture;
						}

						if (IMAGE_PARTICLE != null)
						{
							CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE.PARTICLE_SIZE;
							CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE.DEPTH_FACTOR;
							CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE.SCALE_FACTOR;
						}
						#endregion

						#region AURA

						AURA = SPHERE.GetComponentInChildren(typeof(PlaceParticleOnGround)) as PlaceParticleOnGround;

						if (AURA != null)
						{
							CURRENT_AURA_RADIUS = AURA.Circle_radius;

							CURRENT_AURA_HEIGHT = AURA.Dist_Above_Terrain;

							CURRENT_AURA_SPREAD = AURA.spread;
							CURRENT_AURA_LOOSEN = AURA.loosen_circle;

						}
						#endregion

						#region SKINNED

						SKINNED = SPHERE.GetComponentInChildren(typeof(SKinnedGAmeobjEmit)) as SKinnedGAmeobjEmit;


						#endregion

						#region PHOENIX
						ProceduralNoisePDM PN = SPHERE.GetComponentInChildren(typeof(ProceduralNoisePDM)) as ProceduralNoisePDM;
						if (PN != null)
						{
							SplineParticlePlacer = PN.GetComponent(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
							//Phoenix_Spline = PN.transform.parent.GetComponent(typeof(SplinerP)) as SplinerP;
						}

						if (PROJECTILES_COUNTER == 49)
						{

							SplineParticlePlacer = SPHERE.GetComponentInChildren(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
							Phoenix_Spline = SPHERE.GetComponentInChildren(typeof(SplinerP)) as SplinerP;

						}

						#endregion




						if (PROJECTILES_COUNTER > PROJECTILES.Length - 2)
						{
							PROJECTILES_COUNTER = 0;
						}
						else
						{
							PROJECTILES_COUNTER = PROJECTILES_COUNTER + 1;
						}
					}

					#endregion


					if (GUI.Button(new Rect(0 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log("Projectile " + PROJECTILES_COUNTER);

						for (int i = 0; i < PROJECTILES.Length; i++)
						{
							PROJECTILES[i].gameObject.SetActive(false);
						}
						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);

						Destroy(SPHERE);

						SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);

						SPHERE.transform.parent = Holder;

						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);

						for (int j = 0; j < PROJECTILE_NAMES.Length; j++)
						{

							if (j == PROJECTILES_COUNTER)
							{
								PROJECTILE_NAMES[j].gameObject.SetActive(true);
							}
							else { PROJECTILE_NAMES[j].gameObject.SetActive(false); }
						}


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


						#region ATTRACTOR

						ATTRACTOR = SPHERE.GetComponentInChildren(typeof(AttractParticles)) as AttractParticles;

						if (ATTRACTOR != null)
						{
							CURRENT_Turbulance = ATTRACTOR.Turbulance;//bool--
							CURRENT_Turbulance_strength = ATTRACTOR.Turbulance_strength;//float--
							CURRENT_Turbulance_frequency = ATTRACTOR.Turbulance_frequency;//float--
							CURRENT_Axis_affected = ATTRACTOR.Axis_affected;//Vector3--
							CURRENT_splash_effect = ATTRACTOR.splash_effect;//bool--
							CURRENT_vortex_motion = ATTRACTOR.vortex_motion;//bool--
							CURRENT_Vortex_count = ATTRACTOR.Vortex_count;//int--
							CURRENT_Vortex_life = ATTRACTOR.Vortex_life;//float--
							CURRENT_Vortex_angularvelocity = ATTRACTOR.Vortex_angularvelocity;//float--
							CURRENT_Vortex_scale = ATTRACTOR.Vortex_scale;//float--
							CURRENT_Vortex_center_size = ATTRACTOR.Vortex_center_size;//float--
							CURRENT_Vortex_center_color = new Vector3(ATTRACTOR.Vortex_center_color.r, ATTRACTOR.Vortex_center_color.g, ATTRACTOR.Vortex_center_color.b);//color
							CURRENT_Show_vortex = ATTRACTOR.Show_vortex;//bool--
							CURRENT_Color_force = ATTRACTOR.Color_force;//bool--
							CURRENT_Force_color = new Vector3(ATTRACTOR.Force_color.r, ATTRACTOR.Force_color.g, ATTRACTOR.Force_color.b);//color
							CURRENT_use_exponent = ATTRACTOR.use_exponent;//bool--
							CURRENT_affectDistance = ATTRACTOR.affectDistance;//float--
							CURRENT_dumpen = ATTRACTOR.dumpen;//float--
							CURRENT_smoothattraction = ATTRACTOR.smoothattraction;//bool--
							CURRENT_repel = ATTRACTOR.repel;//bool--

							CURRENT_make_moving_star = ATTRACTOR.make_moving_star;//bool

							if (ATTRACTOR.make_moving_star == true)
							{
								CURRENT_star_trail_dist = ATTRACTOR.star_trail_dist;//float--
								CURRENT_star_trails = ATTRACTOR.star_trails;//int--
								CURRENT_trail_distance = ATTRACTOR.trail_distance;//float--
								CURRENT_speed_of_trail = ATTRACTOR.speed_of_trail;//float--
								CURRENT_distance_of_trail = ATTRACTOR.distance_of_trail;//float--
								CURRENT_trail_length_out = ATTRACTOR.trail_length_out; //float--
								CURRENT_size_of_trail_out = ATTRACTOR.size_of_trail_out;//float--
								CURRENT_distance_between_trail = ATTRACTOR.distance_between_trail;//float--
								CURRENT_vertical_trail_separation = ATTRACTOR.vertical_trail_separation;//float
								CURRENT_smooth_trail = ATTRACTOR.smooth_trail;//float
							}
						}

						#endregion

						#region IMAGE
						IMAGE_PARTICLE = SPHERE.GetComponentInChildren(typeof(ImageToParticles)) as ImageToParticles;

						IMAGE_PARTICLE_DYNAMIC = SPHERE.GetComponentInChildren(typeof(ImageToParticlesDYNAMIC)) as ImageToParticlesDYNAMIC;

						if (IMAGE_PARTICLE_DYNAMIC != null)
						{
							CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE_DYNAMIC.PARTICLE_SIZE;
							CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE_DYNAMIC.DEPTH_FACTOR;
							CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE_DYNAMIC.SCALE_FACTOR;

							CURRENT_IMAGE_PARTICLE_CHANGE = IMAGE_PARTICLE_DYNAMIC.Changing_texture;
						}

						if (IMAGE_PARTICLE != null)
						{
							CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE.PARTICLE_SIZE;
							CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE.DEPTH_FACTOR;
							CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE.SCALE_FACTOR;
						}
						#endregion

						#region AURA

						AURA = SPHERE.GetComponentInChildren(typeof(PlaceParticleOnGround)) as PlaceParticleOnGround;

						if (AURA != null)
						{
							CURRENT_AURA_RADIUS = AURA.Circle_radius;

							CURRENT_AURA_HEIGHT = AURA.Dist_Above_Terrain;

							CURRENT_AURA_SPREAD = AURA.spread;
							CURRENT_AURA_LOOSEN = AURA.loosen_circle;

						}
						#endregion

						#region SKINNED

						SKINNED = SPHERE.GetComponentInChildren(typeof(SKinnedGAmeobjEmit)) as SKinnedGAmeobjEmit;


						#endregion

						#region PHOENIX
						ProceduralNoisePDM PN = SPHERE.GetComponentInChildren(typeof(ProceduralNoisePDM)) as ProceduralNoisePDM;
						if (PN != null)
						{
							SplineParticlePlacer = PN.GetComponent(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
							//Phoenix_Spline = PN.transform.parent.GetComponent(typeof(SplinerP)) as SplinerP;
						}

						if (PROJECTILES_COUNTER == 49)
						{

							SplineParticlePlacer = SPHERE.GetComponentInChildren(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
							Phoenix_Spline = SPHERE.GetComponentInChildren(typeof(SplinerP)) as SplinerP;

						}
						#endregion



						if (PROJECTILES_COUNTER > PROJECTILES.Length - 2)
						{
							PROJECTILES_COUNTER = 0;
						}
						else
						{
							PROJECTILES_COUNTER = PROJECTILES_COUNTER + 1;
						}
					}


					if (Settings_mounted)
					{

						BOX_HEIGHT = -150 + 70;
						BOX_WIDTH = 30;

						int LIGHTNING_SETTING = 10;
						int GAMEOBJECT_SETTING = 11;
						int TURBULENCE_SETTING = 16;
						int IMAGE_SETTING = 20;
						int GROUND_SETTING = 23;

						if (PROJECTILES_COUNTER == 50) { Colliders_to_hide.SetActive(false); }
						else
						{
							if (!Colliders_to_hide.activeInHierarchy) { Colliders_to_hide.SetActive(true); }
						}

						if (PROJECTILES_COUNTER == 15) { GAMEOBJECT_SETTING = 15; }

						if (PROJECTILES_COUNTER == 18) { TURBULENCE_SETTING = 18; }

						if (PROJECTILES_COUNTER == 21) { IMAGE_SETTING = 21; }

						#region ATTRACTOR
						if (ATTRACTOR != null)
						{



							int BASE1 = 140 - 80 + 70;//160
							int BASE_X = 25 + 70;//60

							if (ATTRACTOR.make_moving_star == false)
							{

								#region AAA
								if (PROJECTILES_COUNTER == TURBULENCE_SETTING)
								{
									string TAG_ME = "Remove Turbulance";
									if (ATTRACTOR.Turbulance == false) { TAG_ME = "Add Turbulance"; }
									if (GUI.Button(new Rect(BASE_X, BASE1 - 40, 140, 20), TAG_ME))
									{
										if (ATTRACTOR.Turbulance == false)
										{
											ATTRACTOR.Turbulance = true;
											CURRENT_Turbulance = true;
										}
										else
										{
											ATTRACTOR.Turbulance = false;
											CURRENT_Turbulance = false;
										}
									}



									GUI.TextArea(new Rect(BASE_X - 5, BASE1 - 20, 180, 20), "Turb. strength: " + CURRENT_Turbulance_strength);
									Turbulance_strength = GUI.HorizontalSlider(new Rect(BASE_X, BASE1, 100, 30), CURRENT_Turbulance_strength, 0.1f, 70f);
									CURRENT_Turbulance_strength = Turbulance_strength;
									ATTRACTOR.Turbulance_strength = Turbulance_strength;

									GUI.TextArea(new Rect(BASE_X - 5, BASE1 + (1 * 40) - 20, 180, 20), "Turb. frequency: " + CURRENT_Turbulance_frequency);
									Turbulance_frequency = GUI.HorizontalSlider(new Rect(BASE_X, BASE1 + 40, 100, 30), CURRENT_Turbulance_frequency, 0, 50);
									CURRENT_Turbulance_frequency = Turbulance_frequency;
									ATTRACTOR.Turbulance_frequency = Turbulance_frequency;

									GUI.TextArea(new Rect(BASE_X - 5, BASE1 + (2 * 40) - 20, 180, 20), "Vortex Axis:" + CURRENT_Axis_affected);
									Axis_affected.x = GUI.HorizontalSlider(new Rect(BASE_X + 60, BASE1 + (2 * 40), 100, 30), CURRENT_Axis_affected.x, -10, 30);
									CURRENT_Axis_affected.x = Axis_affected.x; ATTRACTOR.Axis_affected.x = Axis_affected.x;
									Axis_affected.y = GUI.HorizontalSlider(new Rect(BASE_X + 60, BASE1 + (2.8f * 40), 100, 30), CURRENT_Axis_affected.y, -10, 30);
									CURRENT_Axis_affected.y = Axis_affected.y; ATTRACTOR.Axis_affected.y = Axis_affected.y;
									Axis_affected.z = GUI.HorizontalSlider(new Rect(BASE_X + 60, BASE1 + (3.6f * 40), 100, 30), CURRENT_Axis_affected.z, -10, 30);
									CURRENT_Axis_affected.z = Axis_affected.z; ATTRACTOR.Axis_affected.z = Axis_affected.z;

									TAG_ME = "Remove splash";
									if (ATTRACTOR.splash_effect == false) { TAG_ME = "Add splash"; }
									if (GUI.Button(new Rect(BASE_X + 45, BASE1 + (4f * 40) + 15, 120, 20), TAG_ME))
									{
										if (ATTRACTOR.splash_effect == false)
										{
											ATTRACTOR.splash_effect = true;
											CURRENT_splash_effect = true;
										}
										else
										{
											ATTRACTOR.splash_effect = false;
											CURRENT_splash_effect = false;
										}
									}

									TAG_ME = "Remove Vortexes";
									if (ATTRACTOR.vortex_motion == false) { TAG_ME = "Add Vortexes"; }
									if (GUI.Button(new Rect(BASE_X + 45, BASE1 + (5f * 40), 120, 20), TAG_ME))
									{
										if (ATTRACTOR.vortex_motion == false)
										{
											ATTRACTOR.vortex_motion = true;
											CURRENT_vortex_motion = true;
										}
										else
										{
											ATTRACTOR.vortex_motion = false;
											CURRENT_vortex_motion = false;
										}
									}

									GUI.TextArea(new Rect(BASE_X - 10, BASE1 + (6.2f * 40) - 20, 180, 20), "Vortex count: " + CURRENT_Vortex_count);
									Vortex_count = (int)GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (6.2f * 40), 100, 30), CURRENT_Vortex_count, 0, 50);
									CURRENT_Vortex_count = Vortex_count;
									ATTRACTOR.Vortex_count = Vortex_count;

									BASE_X = 215 + 70;
									BASE1 = 140 - 80 + 70;

									TAG_ME = "Hide Centers";
									if (ATTRACTOR.Show_vortex == false) { TAG_ME = "Show Centers"; }
									if (GUI.Button(new Rect(BASE_X, BASE1 - 40, 100, 20), TAG_ME))
									{
										if (ATTRACTOR.Show_vortex == false)
										{
											ATTRACTOR.Show_vortex = true;
											CURRENT_Show_vortex = true;
										}
										else
										{
											ATTRACTOR.Show_vortex = false;
											CURRENT_Show_vortex = false;
										}
									}

									GUI.TextArea(new Rect(BASE_X - 10, BASE1 + (0f * 40) - 20, 180, 20), "Vortex life: " + CURRENT_Vortex_life);
									Vortex_life = GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (0f * 40), 100, 30), CURRENT_Vortex_life, 0, 5);
									CURRENT_Vortex_life = Vortex_life;
									ATTRACTOR.Vortex_life = Vortex_life;

									GUI.TextArea(new Rect(BASE_X - 10, BASE1 + (1f * 40) - 20, 180, 20), "Vortex angular vel.: " + CURRENT_Vortex_angularvelocity);
									Vortex_angularvelocity = GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (1f * 40), 100, 30), CURRENT_Vortex_angularvelocity, 0, 160);
									CURRENT_Vortex_angularvelocity = Vortex_angularvelocity;
									ATTRACTOR.Vortex_angularvelocity = Vortex_angularvelocity;

									GUI.TextArea(new Rect(BASE_X - 10, BASE1 + (2f * 40) - 20, 180, 20), "Vortex scale: " + CURRENT_Vortex_scale);
									Vortex_scale = GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (2f * 40), 100, 30), CURRENT_Vortex_scale, 0, 0.08f);
									CURRENT_Vortex_scale = Vortex_scale;
									ATTRACTOR.Vortex_scale = Vortex_scale;

									GUI.TextArea(new Rect(BASE_X - 10, BASE1 + (3f * 40) - 20, 180, 20), "Vortex size: " + CURRENT_Vortex_center_size);
									Vortex_center_size = GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (3f * 40), 100, 30), CURRENT_Vortex_center_size, 0, 25);
									CURRENT_Vortex_center_size = Vortex_center_size;
									ATTRACTOR.Vortex_center_size = Vortex_center_size;

									GUI.TextArea(new Rect(BASE_X - 10, BASE1 + (4f * 40) - 20, 180, 20), "Vortex color: " + CURRENT_Vortex_center_color);
									Vortex_center_color.x = GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (4f * 40), 100, 30), CURRENT_Vortex_center_color.x, 0, 1);
									CURRENT_Vortex_center_color.x = Vortex_center_color.x;
									Vortex_center_color.y = GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (4.8f * 40), 100, 30), CURRENT_Vortex_center_color.y, 0, 1);
									CURRENT_Vortex_center_color.y = Vortex_center_color.y;
									Vortex_center_color.z = GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (5.6f * 40), 100, 30), CURRENT_Vortex_center_color.z, 0, 1);
									CURRENT_Vortex_center_color.z = Vortex_center_color.z;

									ATTRACTOR.Vortex_center_color = new Color(Vortex_center_color.x, Vortex_center_color.y, Vortex_center_color.z);

									BASE_X = 390 - 250;
									BASE1 = 140 + 80;



									TAG_ME = "Remove Color";
									if (ATTRACTOR.Color_force == false) { TAG_ME = "Color affected"; }
									if (GUI.Button(new Rect(BASE_X + 130, BASE1 + (2f * 40) + 20 + 70, 100, 20), TAG_ME))
									{
										if (ATTRACTOR.Color_force == false)
										{
											ATTRACTOR.Color_force = true;
											CURRENT_Color_force = true;
										}
										else
										{
											ATTRACTOR.Color_force = false;
											CURRENT_Color_force = false;
										}
									}

									GUI.TextArea(new Rect(BASE_X + 130, BASE1 + (3.5f * 40) - 20 + 70, 180, 20), "Vortex color: " + CURRENT_Force_color);
									Force_color.x = GUI.HorizontalSlider(new Rect(BASE_X + 130, BASE1 + (3.5f * 40) + 70, 100, 30), CURRENT_Force_color.x, 0, 1);
									CURRENT_Force_color.x = Force_color.x;
									Force_color.y = GUI.HorizontalSlider(new Rect(BASE_X + 130, BASE1 + (4.3f * 40) + 70, 100, 30), CURRENT_Force_color.y, 0, 1);
									CURRENT_Force_color.y = Force_color.y;
									Force_color.z = GUI.HorizontalSlider(new Rect(BASE_X + 130, BASE1 + (5.1f * 40) + 70, 100, 30), CURRENT_Force_color.z, 0, 1);
									CURRENT_Force_color.z = Force_color.z;

									ATTRACTOR.Force_color = new Color(Force_color.x, Force_color.y, Force_color.z);
								}
								#endregion
							}
							else
							{

								BASE_X = 55;
								BASE1 = 140;

								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (0f * 40) - 20, 180, 20), "Affect dist.: " + CURRENT_affectDistance);
								affectDistance = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (0f * 40), 100, 30), CURRENT_affectDistance, 0, 400);
								CURRENT_affectDistance = affectDistance;
								ATTRACTOR.affectDistance = affectDistance;

								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (1f * 40) - 20, 180, 20), "Trail dist.: " + CURRENT_star_trail_dist);
								star_trail_dist = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (1f * 40), 100, 30), CURRENT_star_trail_dist, 0, 15);
								CURRENT_star_trail_dist = star_trail_dist;
								ATTRACTOR.star_trail_dist = star_trail_dist;
								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (10f * 40) - 20, 180, 20), "Star trails: " + CURRENT_star_trails);
								star_trails = (int)GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (10f * 40), 100, 30), CURRENT_star_trails, 0, 50);
								CURRENT_star_trails = star_trails;
								ATTRACTOR.star_trails = star_trails; //
								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (2f * 40) - 20, 180, 20), "Star Trail dist.: " + CURRENT_trail_distance);
								trail_distance = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (2f * 40), 100, 30), CURRENT_trail_distance, 0, 10);
								CURRENT_trail_distance = trail_distance;
								ATTRACTOR.trail_distance = trail_distance;
								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (3f * 40) - 20, 180, 20), "Trail speed: " + CURRENT_speed_of_trail);
								speed_of_trail = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (3f * 40), 100, 30), CURRENT_speed_of_trail, 0, 100);
								CURRENT_speed_of_trail = speed_of_trail;
								ATTRACTOR.speed_of_trail = speed_of_trail;
								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (4f * 40) - 20, 180, 20), "Dist. of trail: " + CURRENT_distance_of_trail);
								distance_of_trail = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (4f * 40), 100, 30), CURRENT_distance_of_trail, 0, 2);
								CURRENT_distance_of_trail = distance_of_trail;
								ATTRACTOR.distance_of_trail = distance_of_trail;

								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (5f * 40) - 20, 180, 20), "Trail length: " + CURRENT_trail_length_out);
								trail_length_out = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (5f * 40), 100, 30), CURRENT_trail_length_out, 0, 10);
								CURRENT_trail_length_out = trail_length_out;
								ATTRACTOR.trail_length_out = trail_length_out;
								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (6f * 40) - 20, 180, 20), "Trail size: " + CURRENT_size_of_trail_out);
								size_of_trail_out = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (6f * 40), 100, 30), CURRENT_size_of_trail_out, 0, 0.01f);
								CURRENT_size_of_trail_out = size_of_trail_out;
								ATTRACTOR.size_of_trail_out = size_of_trail_out;
								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (7f * 40) - 20, 180, 20), "Trail between dist.: " + CURRENT_distance_between_trail);
								distance_between_trail = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (7f * 40), 100, 30), CURRENT_distance_between_trail, 0, 3);
								CURRENT_distance_between_trail = distance_between_trail;
								ATTRACTOR.distance_between_trail = distance_between_trail;
								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (8f * 40) - 20, 180, 20), "Vertical sepration: " + CURRENT_vertical_trail_separation);
								vertical_trail_separation = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (8f * 40), 100, 30), CURRENT_vertical_trail_separation, 0, 4);
								CURRENT_vertical_trail_separation = vertical_trail_separation;
								ATTRACTOR.vertical_trail_separation = vertical_trail_separation;
								GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (9f * 40) - 20, 180, 20), "Trail smoothness: " + CURRENT_smooth_trail);
								smooth_trail = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (9f * 40), 100, 30), CURRENT_smooth_trail, 0, 10);
								CURRENT_smooth_trail = smooth_trail;
								ATTRACTOR.smooth_trail = smooth_trail;


							}



						}
						#endregion

						#region IMAGE
						if (PROJECTILES_COUNTER == IMAGE_SETTING)
						{
							if (IMAGE_PARTICLE != null)
							{
								GUI.TextArea(new Rect(5, 120 + 70, 100, 20), "Particle size");
								IMAGE_PARTICLE_RADIUS = GUI.HorizontalSlider(new Rect(10, 120 + 90, 100, 30), CURRENT_IMAGE_PARTICLE_RADIUS, 0.05f, 1.5f);
								CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE_RADIUS;
								IMAGE_PARTICLE.PARTICLE_SIZE = IMAGE_PARTICLE_RADIUS;

								GUI.TextArea(new Rect(5, 160 + 70, 100, 20), "Depth");
								IMAGE_PARTICLE_DEPTH = GUI.HorizontalSlider(new Rect(10, 160 + 90, 100, 30), CURRENT_IMAGE_PARTICLE_DEPTH, -70, 70);
								CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE_DEPTH;
								IMAGE_PARTICLE.DEPTH_FACTOR = IMAGE_PARTICLE_DEPTH;

								GUI.TextArea(new Rect(5, 200 + 70, 100, 20), "Scale");
								IMAGE_PARTICLE_SCALE = GUI.HorizontalSlider(new Rect(10, 200 + 90, 100, 30), CURRENT_IMAGE_PARTICLE_SCALE, 1, 30);
								CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE_SCALE;
								IMAGE_PARTICLE.SCALE_FACTOR = IMAGE_PARTICLE_SCALE;
							}
							if (IMAGE_PARTICLE_DYNAMIC != null)
							{
								GUI.TextArea(new Rect(5, 120 + 70, 100, 20), "Particle size");
								IMAGE_PARTICLE_RADIUS = GUI.HorizontalSlider(new Rect(10, 120 + 90, 100, 30), CURRENT_IMAGE_PARTICLE_RADIUS, 0.05f, 1.5f);
								CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE_RADIUS;
								IMAGE_PARTICLE_DYNAMIC.PARTICLE_SIZE = IMAGE_PARTICLE_RADIUS;

								GUI.TextArea(new Rect(5, 160 + 70, 100, 20), "Depth");
								IMAGE_PARTICLE_DEPTH = GUI.HorizontalSlider(new Rect(10, 160 + 90, 100, 30), CURRENT_IMAGE_PARTICLE_DEPTH, -70, 70);
								CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE_DEPTH;
								IMAGE_PARTICLE_DYNAMIC.DEPTH_FACTOR = IMAGE_PARTICLE_DEPTH;

								GUI.TextArea(new Rect(5, 200 + 70, 100, 20), "Scale");
								IMAGE_PARTICLE_SCALE = GUI.HorizontalSlider(new Rect(10, 200 + 90, 100, 30), CURRENT_IMAGE_PARTICLE_SCALE, 1, 30);
								CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE_SCALE;
								IMAGE_PARTICLE_DYNAMIC.SCALE_FACTOR = IMAGE_PARTICLE_SCALE;

								string TAG_ME = "Explode";
								if (IMAGE_PARTICLE_DYNAMIC.Changing_texture == false)
								{
									TAG_ME = "Reform";
								}

								if (GUI.Button(new Rect(5, 230 + 50 + 30, 100, 20), TAG_ME))
								{
									if (IMAGE_PARTICLE_DYNAMIC.Changing_texture == false)
									{
										IMAGE_PARTICLE_DYNAMIC.Changing_texture = true;
										CURRENT_IMAGE_PARTICLE_CHANGE = true;
									}
									else
									if (IMAGE_PARTICLE_DYNAMIC.Changing_texture == true)
									{
										IMAGE_PARTICLE_DYNAMIC.Changing_texture = false;
										CURRENT_IMAGE_PARTICLE_CHANGE = false;
									}
								}

							}
						}
						#endregion

						#region AURA
						if (PROJECTILES_COUNTER == GROUND_SETTING)
						{
							if (AURA != null)
							{




								GUI.TextArea(new Rect(5, 150 + 50, 100, 20), "Aura radius");
								AURA_RADIUS = GUI.HorizontalSlider(new Rect(10, 150 + 40 + 30, 100, 30), CURRENT_AURA_RADIUS, 0.1f, 20);
								CURRENT_AURA_RADIUS = AURA_RADIUS;
								AURA.Circle_radius = AURA_RADIUS;

								GUI.TextArea(new Rect(5, 190 + 50, 100, 20), "Height above ground");
								AURA_HEIGHT = GUI.HorizontalSlider(new Rect(10, 190 + 40 + 30, 100, 30), CURRENT_AURA_HEIGHT, 0f, 20);
								CURRENT_AURA_HEIGHT = AURA_HEIGHT;
								AURA.Dist_Above_Terrain = AURA_HEIGHT;

								string FIX_STATE = "Fix Circle";
								if (AURA.is_target == true) { FIX_STATE = "Loosen Circle"; }
								if (GUI.Button(new Rect(105, 250 + 30, 100, 20), FIX_STATE))
								{
									if (AURA.is_target == false)
									{
										AURA.is_target = true;
									}
									else
									if (AURA.is_target == true)
									{
										AURA.is_target = false;
									}
								}

								GUI.TextArea(new Rect(100 + 5, 150 + 50, 100, 20), "Aura spread");
								AURA_SPREAD = GUI.HorizontalSlider(new Rect(100 + 10, 150 + 40 + 30, 100, 30), CURRENT_AURA_SPREAD, 0, 20);
								CURRENT_AURA_SPREAD = AURA_SPREAD;
								AURA.spread = AURA_SPREAD;

								if (AURA.is_target == false)
								{
									GUI.TextArea(new Rect(100 + 5, 190 + 50, 100, 20), "Loosen form");
									AURA_LOOSEN = (int)GUI.HorizontalSlider(new Rect(100 + 10, 190 + 40 + 30, 150, 30), CURRENT_AURA_LOOSEN, 1, 200);
									CURRENT_AURA_LOOSEN = AURA_LOOSEN;
									AURA.loosen_circle = AURA_LOOSEN;
								}


							}
						}
						#endregion

						#region SKINNED
						if (SKINNED != null & PROJECTILES_COUNTER != 33)
						{

							int X_DIST = 35;


							string TITLE1 = "Revive particles";
							if (SKINNED.extend_life)
							{
								TITLE1 = "Particles Persistent";
							}
							else { TITLE1 = "Particles Renewed"; }

							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST + 6, BOX_HEIGHT + 280, BOX_WIDTH + 100, 30), TITLE1))
							{
								if (SKINNED.extend_life)
								{
									SKINNED.extend_life = false;
								}
								else { SKINNED.extend_life = true; }
							}

							TITLE1 = "Particles hidden";
							if (SKINNED.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled) { TITLE1 = "Particles shown"; }
							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 320, 150, 17), TITLE1))
							{
								if (SKINNED.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled) { SKINNED.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = false; } else { SKINNED.gameObject.GetComponent<ParticleSystem>().GetComponent<Renderer>().enabled = true; }
							}

							if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 240, 150, 17), "Settle speed")) { }
							SKinned_return_speed = GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 260, 150, 17), SKINNED.Return_speed, 0f, 1f);
							SKINNED.Return_speed = SKinned_return_speed;

						}
						#endregion


						#region PHOENIX
						if (SplineParticlePlacer != null)
						{



							if (GUI.Button(new Rect(10, 300 - 100, 120, 17), "Interpolate Steps")) { }
							Spline_Interpolate = (int)GUI.HorizontalSlider(new Rect(10, 320 - 100, 160, 17), SplineParticlePlacer.Interpolate_steps, 1, 15);
							SplineParticlePlacer.Interpolate_steps = Spline_Interpolate;

							if (PROJECTILES_COUNTER == 50)
							{

								//Settings_mounted=true;

								if (GUI.Button(new Rect(10, 340 - 100, 120, 17), "Add point"))
								{
									if (Phoenix_Spline != null)
									{
										Phoenix_Spline.With_manip = true;
										Phoenix_Spline.Add_point = true;
									}
								}

								if (GUI.Button(new Rect(10, 360 - 100, 120, 17), "Grow size")) { }
								Spline_grow = GUI.HorizontalSlider(new Rect(10, 380 - 100, 160, 17), SplineParticlePlacer.Reduce_factor, 0.005f, 0.05f);
								SplineParticlePlacer.Reduce_factor = Spline_grow;

								if (GUI.Button(new Rect(10, 400 - 100, 120, 17), "Reset grow"))
								{
									SplineParticlePlacer.Reduce_factor = 1;
								}

								ParticleSystem.MainModule main = SplineParticlePlacer.gameObject.GetComponent<ParticleSystem>().main;//v2.3
								ParticleSystem.MinMaxCurve Curve = main.startSize;

								if (GUI.Button(new Rect(10, 420 - 100, 120, 17), "Particle size")) { }
								Particle_size = GUI.HorizontalSlider(new Rect(10, 440 - 100, 160, 17), Curve.constant, 0.5f, 5f);
								//SplineParticlePlacer.gameObject.GetComponent<ParticleSystem>().startSize = Particle_size;
								Curve.constant = Particle_size;

								if (GUI.Button(new Rect(10, 460 - 100, 120, 17), "Spline Quality")) { }
								Spline_Quality = (int)GUI.HorizontalSlider(new Rect(10, 480 - 100, 160, 17), Phoenix_Spline.CurveQuality, 5, 16);
								Phoenix_Spline.CurveQuality = Spline_Quality;

							}

						}

						if (Phoenix_Spline != null)
						{

							if (PROJECTILES_COUNTER != 50)
							{

								string ACCEL_state = "ACCELERATE ON";
								if (!Phoenix_Spline.Accelerate) { ACCEL_state = "ACCELERATE OFF"; }

								if (GUI.Button(new Rect(10, 340 - 100, 100, 17), "Speed")) { }
								PHOENIX_SPEED = GUI.HorizontalSlider(new Rect(10, 360 - 100, 160, 17), Phoenix_Spline.Motion_Speed, 0.1f, 5);
								Phoenix_Spline.Motion_Speed = PHOENIX_SPEED;

								if (GUI.Button(new Rect(10, 380 - 100, 160, 17), ACCEL_state)) { if (Phoenix_Spline.Accelerate) { Phoenix_Spline.Accelerate = false; } else { Phoenix_Spline.Accelerate = true; } }

							}


						}
						#endregion

						#region GRASS
						if (PROJECTILES_COUNTER == 8)
						{

							int X_DIST = 35;

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

								if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 400, 150, 17), "Wind speed")) { }
								Grass_speed = GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 420, 150, 17), Gameobject_SCATTER.Wind_speed, 0.05f, 3.1f);
								Gameobject_SCATTER.Wind_speed = Grass_speed;



							}

						}
						#endregion


						if (PROJECTILES_COUNTER == GAMEOBJECT_SETTING | PROJECTILES_COUNTER == 7 | PROJECTILES_COUNTER == LIGHTNING_SETTING | PROJECTILES_COUNTER == 0)
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

						if ((PROJECTILES_COUNTER == GAMEOBJECT_SETTING | PROJECTILES_COUNTER == LIGHTNING_SETTING | PROJECTILES_COUNTER == 0) & (Projectile5_FLAMMABLES != null | PROJECTILES_COUNTER == LIGHTNING_SETTING))
						{ //gameobject, on fire


							string TITLE = "Flame on";
							if (PROJECTILES_COUNTER != LIGHTNING_SETTING)
							{ //if lighning only

								if (Projectile5_FLAMMABLES.gameObject.activeInHierarchy)
								{
									TITLE = "Flame off";
								}
								else { TITLE = "Flame on"; }

								if (GUI.Button(new Rect(2 * BOX_WIDTH + 188, BOX_HEIGHT + 160, BOX_WIDTH + 50, 30), TITLE))
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

							if (AttractorTurbulant1 != null & AttractorTurbulant2 != null)
							{

								if (AttractorTurbulant1.enabled)
								{
									TITLE = "Gravity on";
								}
								else { TITLE = "Gravity off"; }

								if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 160, BOX_WIDTH + 45, 30), TITLE))
								{
									if (AttractorTurbulant1.enabled)
									{
										AttractorTurbulant1.enabled = false;
									}
									else { AttractorTurbulant1.enabled = true; AttractorTurbulant2.enabled = false; }
								}

								if (AttractorTurbulant2.enabled)
								{
									TITLE = "Vortex on";
								}
								else { TITLE = "Vortex off"; }

								if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST + 76, BOX_HEIGHT + 160, BOX_WIDTH + 45, 30), TITLE))
								{
									if (AttractorTurbulant2.enabled)
									{
										AttractorTurbulant2.enabled = false;
									}
									else { AttractorTurbulant2.enabled = true; AttractorTurbulant1.enabled = false; }
								}




							}

							//

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




								if (AttractorTurbulant1 != null)
								{
									if (AttractorTurbulant1.Lerp_velocity)
									{
										NAME_IT = "Lerp Velocity on";
									}
									else
									{
										NAME_IT = "Lerp Velocity off";
									}

									if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 400, 150, 19), NAME_IT))
									{
										if (AttractorTurbulant1.Lerp_velocity)
										{
											AttractorTurbulant1.Lerp_velocity = false;
										}
										else
										{
											AttractorTurbulant1.Lerp_velocity = true;
										}
									}


									NAME_IT = "Update every frame";
									if (AttractorTurbulant1.Time_to_update == 1) { NAME_IT = "Update every other frame"; }
									if (AttractorTurbulant1.Time_to_update == 2) { NAME_IT = "Update every " + AttractorTurbulant1.Time_to_update + "ond frame"; }
									if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST - 10, BOX_HEIGHT + 420, 160, 17), NAME_IT)) { }
									Update_interval = (int)GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 440, 150, 17), AttractorTurbulant1.Time_to_update, 0, 2);
									AttractorTurbulant1.Time_to_update = Update_interval;



								}

								if (PROJECTILES_COUNTER == GAMEOBJECT_SETTING)
								{

									ParticleSystem.MainModule main = Gameobject_SCATTER.gameObject.GetComponent<ParticleSystem>().main;//v2.3
									ParticleSystem.MinMaxCurve Curve = main.gravityModifier;

									if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 460, 150, 17), "Gravity")) { }
									Gravity = GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 480, 150, 17), Curve.constant, 0, 5);
									//Gameobject_SCATTER.gameObject.GetComponent<ParticleSystem>().gravityModifier = Gravity;
									Curve.constant = Gravity;
								}

								if (PROJECTILES_COUNTER == GAMEOBJECT_SETTING & FLAMER_Particle != null)
								{
									if (GUI.Button(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 500, 180, 17), "Flame performance factor")) { }
									DelayFLAME = GUI.HorizontalSlider(new Rect(2 * BOX_WIDTH + X_DIST, BOX_HEIGHT + 520, 150, 17), FLAMER_Particle.Delay, 4, 40);
									FLAMER_Particle.Delay = DelayFLAME;
								}

							}





						}



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

		float SKinned_scale;
		float SKinned_return_speed;

		float Grass_speed;
		int Update_interval;
	}
}
