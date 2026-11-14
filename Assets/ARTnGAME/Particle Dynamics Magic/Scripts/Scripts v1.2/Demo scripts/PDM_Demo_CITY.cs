using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class PDM_Demo_CITY : MonoBehaviour
	{

#pragma warning disable 414

		void Update()
		{

			//MouseLook


			if (Input.GetButtonUp("Look"))
			{


				LOOK = this.gameObject.GetComponentsInChildren(typeof(MouseLookPDM));

				if (LOOK != null)
				{

					MouseLookPDM AA = LOOK[0] as MouseLookPDM;
					MouseLookPDM AA1 = LOOK[1] as MouseLookPDM;

					if (look_disabled)
					{
						AA.enabled = true;
						AA1.enabled = true;
						look_disabled = false;
						Debug.Log("aaa");
					}
					else
					{
						AA.enabled = false;
						AA1.enabled = false;
						look_disabled = true;

					}



				}
			}


		}

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

			SPHERE.transform.parent = null;

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

		Component[] LOOK;
		bool look_disabled = false;

		private bool Settings_mounted = true;

		private bool Camera_mounted = false;

		public GameObject Cam_follower;

		void OnGUI()
		{

			GUI.color = new Color32(255, 255, 255, 201);





			string HUD_state = "HUD ON";
			if (!HUD_ON) { HUD_state = "HUD OFF"; }
			if (GUI.Button(new Rect(10, 500, 80, 17), HUD_state)) { if (HUD_ON) { HUD_ON = false; } else { HUD_ON = true; } }

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

			if (GUI.Button(new Rect(10, 520, 120, 17), "Mount Camera"))
			{

				if (!Camera_mounted)
				{
					Camera_mounted = true;
					Camera.main.gameObject.transform.position = Cam_follower.transform.position;
					Camera.main.gameObject.transform.eulerAngles = Cam_follower.transform.eulerAngles;
					Camera.main.gameObject.transform.parent = Cam_follower.transform;

					Camera_init_transform_position = Camera.main.gameObject.transform.position;
					Camera_init_transform_rotation = Camera.main.gameObject.transform.eulerAngles;
				}
				else
				{

					Camera_mounted = false;
					Camera.main.gameObject.transform.parent = this.gameObject.transform;
					Camera.main.transform.localPosition = new Vector3(0, 0.9f, 0);

					Camera.main.transform.localEulerAngles = new Vector3(0, 0, 0);
				}

			}




			if (SplineParticlePlacer != null)
			{

				Phoenix_Spline = Phoenix.GetComponent("SplinerP") as SplinerP;
				SplineParticlePlacer = Phoenix.GetComponentInChildren(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;



				if (0 == 0)
				{



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

					if (GUI.Button(new Rect(10, 420 - 100, 120, 17), "Particle size")) { }

					ParticleSystem.MainModule main = SplineParticlePlacer.gameObject.GetComponent<ParticleSystem>().main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;

					Particle_size = GUI.HorizontalSlider(new Rect(10, 440 - 100, 160, 17), Curve.constant, 0.5f, 5f);
					//SplineParticlePlacer.gameObject.GetComponent<ParticleSystem>().startSize = Particle_size;
					Curve.constant = Particle_size;

					if (GUI.Button(new Rect(10, 460 - 100, 120, 17), "Spline Quality")) { }
					Spline_Quality = (int)GUI.HorizontalSlider(new Rect(10, 480 - 100, 160, 17), Phoenix_Spline.CurveQuality, 5, 16);
					Phoenix_Spline.CurveQuality = Spline_Quality;

				}

				if (GUI.Button(new Rect(10, 500 - 100, 120, 17), "Interpolate Steps")) { }
				Spline_Interpolate = (int)GUI.HorizontalSlider(new Rect(10, 520 - 100, 160, 17), SplineParticlePlacer.Interpolate_steps, 1, 15);
				SplineParticlePlacer.Interpolate_steps = Spline_Interpolate;

			}

			if (Phoenix_Spline != null)
			{



				string ACCEL_state = "ACCELERATE ON";
				if (!Phoenix_Spline.Accelerate) { ACCEL_state = "ACCELERATE OFF"; }

				if (GUI.Button(new Rect(10, 540 - 100, 100, 17), "Speed")) { }
				PHOENIX_SPEED = GUI.HorizontalSlider(new Rect(10, 560 - 100, 160, 17), Phoenix_Spline.Motion_Speed, 0.1f, 5);
				Phoenix_Spline.Motion_Speed = PHOENIX_SPEED;

				if (GUI.Button(new Rect(10, 580 - 100, 160, 17), ACCEL_state)) { if (Phoenix_Spline.Accelerate) { Phoenix_Spline.Accelerate = false; } else { Phoenix_Spline.Accelerate = true; } }




			}



















			///// PHOENIX ///////





			if (GUI.Button(new Rect(10, 540, 120, 21), "Settings"))
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