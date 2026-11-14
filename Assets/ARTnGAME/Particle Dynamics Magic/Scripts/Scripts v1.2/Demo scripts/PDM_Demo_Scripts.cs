using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class PDM_Demo_Scripts : MonoBehaviour
	{

#pragma warning disable 414

		void Start()
		{

			HERO_CAM = HERO.GetComponent("ThirdPersonCameraPDM") as ThirdPersonCameraPDM;

		}

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


		void OnGUI()
		{

			GUI.color = new Color32(255, 255, 255, 201);

			CAMERA_UP = GUI.VerticalSlider(new Rect(10, 260, 30, 100), CAMERA_UP, -1, 20);
			CAMERA_DIST = GUI.VerticalSlider(new Rect(40, 260, 30, 100), CAMERA_DIST, 1, 20);

			if (GUI.Button(new Rect(10, 235, 60, 17), "Camera")) { }

			string HUD_state = "HUD ON";
			if (!HUD_ON) { HUD_state = "HUD OFF"; }
			if (GUI.Button(new Rect(10, 400, 60, 17), HUD_state)) { if (HUD_ON) { HUD_ON = false; } else { HUD_ON = true; } }

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

			int PIXELS_DOWN = 18;

			int BOX_WIDTH = 70;
			int BOX_HEIGHT = 70;

			BOX_WIDTH = (int)Slide_left + BOX_WIDTH;

			if (HUD_ON)
			{

				if (GUI.Button(new Rect(10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), AURAS_Texture))
				{
					if (MODE == 1)
					{
						MODE = 0;
						for (int i = 0; i < AURAS.Length; i++)
						{
							AURAS[i].gameObject.SetActive(false);
						}
					}
					else { MODE = 1; }

				}
				if (GUI.Button(new Rect(BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), SWORD_Texture))
				{

					if (MODE == 2)
					{
						MODE = 0;
						for (int i = 0; i < SWORDS.Length; i++)
						{
							SWORDS[i].gameObject.SetActive(false);
						}
					}
					else
					{
						MODE = 2;
					}


				}
				if (GUI.Button(new Rect(2 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), PROJECTILES_Texture))
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
				if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), SPLINES_Texture))
				{
					if (MODE == 4)
					{
						MODE = 0;
						for (int i = 0; i < SPLINES.Length; i++)
						{
							SPLINES[i].gameObject.SetActive(false);
						}
					}
					else
					{
						MODE = 4;
					}

				}
				if (GUI.Button(new Rect(4 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), FIELDS_Texture))
				{
					if (MODE == 5)
					{
						MODE = 0;
						for (int i = 0; i < FIELDS.Length; i++)
						{
							FIELDS[i].gameObject.SetActive(false);
						}

					}
					else
					{
						MODE = 5;
					}

				}
				if (GUI.Button(new Rect(5 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), FLOWS_Texture))
				{
					if (MODE == 6)
					{
						MODE = 0;
						for (int i = 0; i < FLOWS.Length; i++)
						{
							FLOWS[i].gameObject.SetActive(false);
						}
					}
					else
					{
						MODE = 6;
					}
				}
				if (GUI.Button(new Rect(6 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), SHIELDS_Texture))
				{

					if (MODE == 7)
					{
						MODE = 0;
						for (int i = 0; i < SHIELDS.Length; i++)
						{
							SHIELDS[i].gameObject.SetActive(false);
						}
						FlameThrowers.gameObject.SetActive(false);
					}
					else
					{

						MODE = 7;
					}
				}
				if (GUI.Button(new Rect(7 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), SKINNED_MESH_Texture))
				{

					if (MODE == 8)
					{
						MODE = 0;
						for (int i = 0; i < SKINNED_MESH.Length; i++)
						{
							SKINNED_MESH[i].gameObject.SetActive(false);
						}
					}
					else
					{

						MODE = 8;
					}
				}
				if (GUI.Button(new Rect(8 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), WEATHER_Texture))
				{
					if (MODE == 9)
					{
						MODE = 0;
						for (int i = 0; i < WEATHER.Length; i++)
						{
							WEATHER[i].gameObject.SetActive(false);
						}

					}
					else
					{
						MODE = 9;
					}
				}
				if (GUI.Button(new Rect(9 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), PARTICLE_PAINT_Texture))
				{
					if (MODE == 10)
					{
						MODE = 0;
						for (int i = 0; i < PARTICLE_PAINT.Length; i++)
						{
							PARTICLE_PAINT[i].gameObject.SetActive(false);
						}
					}
					else
					{
						MODE = 10;
					}
				}
				if (GUI.Button(new Rect(10 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), IMAGE_Texture))
				{
					if (MODE == 11)
					{
						MODE = 0;
						for (int i = 0; i < IMAGE.Length; i++)
						{
							IMAGE[i].gameObject.SetActive(false);
						}
					}
					else
					{
						MODE = 11;
					}
				}
				if (GUI.Button(new Rect(11 * BOX_WIDTH + 10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), VORTEXES_Texture))
				{
					if (MODE == 12)
					{
						MODE = 0;
						for (int i = 0; i < VORTEXES.Length; i++)
						{
							VORTEXES[i].gameObject.SetActive(false);
						}
					}
					else
					{
						MODE = 12;
					}
				}


				if (GUI.Button(new Rect(10, 1, BOX_WIDTH, 17), "Auras")) { }
				if (GUI.Button(new Rect(BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Swords")) { }
				if (GUI.Button(new Rect(2 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Projectiles")) { }
				if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Splines")) { }
				if (GUI.Button(new Rect(4 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Fields")) { }
				if (GUI.Button(new Rect(5 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Flows")) { }
				if (GUI.Button(new Rect(6 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Shields")) { }
				if (GUI.Button(new Rect(7 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Skinned")) { }
				if (GUI.Button(new Rect(8 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Weather")) { }
				if (GUI.Button(new Rect(9 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Paint")) { }
				if (GUI.Button(new Rect(10 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Image")) { }
				if (GUI.Button(new Rect(11 * BOX_WIDTH + 10, 1, BOX_WIDTH, 17), "Vortex")) { }

				if (1 == 1)
				{

					Nebula.gameObject.SetActive(false);
					Nebula_Active = false;

					//12
					if (MODE != 12)
					{
						for (int i = 0; i < VORTEXES.Length; i++)
						{
							VORTEXES[i].gameObject.SetActive(false);
						}
					}

					//11
					if (MODE != 11)
					{
						for (int i = 0; i < IMAGE.Length; i++)
						{
							IMAGE[i].gameObject.SetActive(false);
						}
					}

					//10
					if (MODE != 10)
					{
						for (int i = 0; i < PARTICLE_PAINT.Length; i++)
						{
							PARTICLE_PAINT[i].gameObject.SetActive(false);
						}
					}

					//9
					if (MODE != 9)
					{
						for (int i = 0; i < WEATHER.Length; i++)
						{
							WEATHER[i].gameObject.SetActive(false);
						}
					}

					//8
					if (MODE != 8)
					{
						for (int i = 0; i < SKINNED_MESH.Length; i++)
						{
							SKINNED_MESH[i].gameObject.SetActive(false);
						}
					}

					//7
					if (MODE != 7)
					{
						for (int i = 0; i < SHIELDS.Length; i++)
						{
							SHIELDS[i].gameObject.SetActive(false);
						}
						FlameThrowers.gameObject.SetActive(false);
					}
					//6
					if (MODE != 6)
					{
						for (int i = 0; i < FLOWS.Length; i++)
						{
							FLOWS[i].gameObject.SetActive(false);
						}
					}
					//5
					if (MODE != 5 & MODE != 3)
					{
						for (int i = 0; i < FIELDS.Length; i++)
						{
							FIELDS[i].gameObject.SetActive(false);
						}
					}

					//4
					if (MODE != 4)
					{
						for (int i = 0; i < SPLINES.Length; i++)
						{
							SPLINES[i].gameObject.SetActive(false);
						}
					}
					//3

					if (MODE == 0)
					{
						for (int i = 0; i < PROJECTILES.Length; i++)
						{
							PROJECTILES[i].gameObject.SetActive(false);
							Leaf_floor.SetActive(false);
							GlobalPropagate.SetActive(false);
							Flammables.SetActive(false);
							Conductors.SetActive(false);
						}
						Destroy(SPHERE);
					}
					//2
					if (MODE != 2)
					{
						for (int i = 0; i < SWORDS.Length; i++)
						{
							SWORDS[i].gameObject.SetActive(false);
						}
					}
					//1
					if (MODE != 1)
					{
						for (int i = 0; i < AURAS.Length; i++)
						{
							AURAS[i].gameObject.SetActive(false);
						}
					}

				}


				if (MODE == 1)
				{ //draw MODE 1  - AURAS

					if (GUI.Button(new Rect(10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(AURA_COUNTER);

						for (int i = 0; i < AURAS.Length; i++)
						{
							AURAS[i].gameObject.SetActive(false);
						}
						AURAS[AURA_COUNTER].gameObject.SetActive(true);


						if (AURA_COUNTER != 0)
						{
							AURA = AURAS[AURA_COUNTER].gameObject.GetComponent("PlaceParticleOnGround") as PlaceParticleOnGround;

							if (AURA != null)
							{
								CURRENT_AURA_RADIUS = AURA.Circle_radius;

								CURRENT_AURA_HEIGHT = AURA.Dist_Above_Terrain;

								CURRENT_AURA_SPREAD = AURA.spread;
								CURRENT_AURA_LOOSEN = AURA.loosen_circle;

							}

						}

						if (AURA_COUNTER > AURAS.Length - 2)
						{
							AURA_COUNTER = 0;
						}
						else
						{
							AURA_COUNTER = AURA_COUNTER + 1;
						}
					}

					if (AURA != null)
					{
						if (AURAS.Length != 0)
						{
							GUI.TextArea(new Rect(5, 150 - 20, 100, 20), "Aura radius");
							AURA_RADIUS = GUI.HorizontalSlider(new Rect(10, 150, 100, 30), CURRENT_AURA_RADIUS, 0.1f, 20);
							CURRENT_AURA_RADIUS = AURA_RADIUS;
							AURA.Circle_radius = AURA_RADIUS;

							GUI.TextArea(new Rect(5, 190 - 20, 100, 20), "Height above ground");
							AURA_HEIGHT = GUI.HorizontalSlider(new Rect(10, 190, 100, 30), CURRENT_AURA_HEIGHT, 0f, 20);
							CURRENT_AURA_HEIGHT = AURA_HEIGHT;
							AURA.Dist_Above_Terrain = AURA_HEIGHT;

							string FIX_STATE = "Fix Circle";
							if (AURA.is_target == true) { FIX_STATE = "Loosen Circle"; }
							if (GUI.Button(new Rect(105, 230, 100, 20), FIX_STATE))
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

							GUI.TextArea(new Rect(100 + 5, 150 - 20, 100, 20), "Aura spread");
							AURA_SPREAD = GUI.HorizontalSlider(new Rect(100 + 10, 150, 100, 30), CURRENT_AURA_SPREAD, 0, 20);
							CURRENT_AURA_SPREAD = AURA_SPREAD;
							AURA.spread = AURA_SPREAD;

							if (AURA.is_target == false)
							{
								GUI.TextArea(new Rect(100 + 5, 190 - 20, 100, 20), "Loosen form");
								AURA_LOOSEN = (int)GUI.HorizontalSlider(new Rect(100 + 10, 190, 150, 30), CURRENT_AURA_LOOSEN, 1, 200);
								CURRENT_AURA_LOOSEN = AURA_LOOSEN;
								AURA.loosen_circle = AURA_LOOSEN;
							}

						}
					}
				}


				if (MODE == 2)
				{ //draw MODE 2  - SWORDS

					if (GUI.Button(new Rect(BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(SWORD_COUNTER);

						for (int i = 0; i < SWORDS.Length; i++)
						{
							SWORDS[i].gameObject.SetActive(false);
						}
						SWORDS[SWORD_COUNTER].gameObject.SetActive(true);

						//// ONLY FOR WOODEN WAND
						if (SWORD_COUNTER == SWORDS.Length - 1)
						{
							Orbs_around_hero.gameObject.SetActive(true);
							Particle_floor.gameObject.SetActive(true);
						}
						else
						{
							Orbs_around_hero.gameObject.SetActive(false);
							Particle_floor.gameObject.SetActive(false);
						}

						if (SWORD_COUNTER > SWORDS.Length - 2)
						{
							SWORD_COUNTER = 0;
						}
						else
						{
							SWORD_COUNTER = SWORD_COUNTER + 1;
						}
					}

				}

				if (MODE == 3)
				{ //draw MODE 3  - PROJECTILES

					if (GUI.Button(new Rect(2 * BOX_WIDTH + 10, BOX_HEIGHT + 50, BOX_WIDTH, 30), "Recast"))
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

					}

					if (GUI.Button(new Rect(2 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(PROJECTILES_COUNTER);

						for (int i = 0; i < PROJECTILES.Length; i++)
						{
							PROJECTILES[i].gameObject.SetActive(false);
						}
						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);

						Destroy(SPHERE);

						SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, HERO.transform.position, Quaternion.identity);

						SPHERE.transform.parent = null;

						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);

						Leaf_floor.SetActive(false);
						GlobalPropagate.SetActive(false);
						Conductors.SetActive(false);
						Flammables.SetActive(false);

						if (PROJECTILES_COUNTER == 6)
						{
							Leaf_floor.SetActive(true);
						}
						else if (PROJECTILES_COUNTER == 11 | PROJECTILES_COUNTER == 9)
						{
							GlobalPropagate.SetActive(true);
							Flammables.SetActive(true);
						}
						else if (PROJECTILES_COUNTER == 10)
						{
							Conductors.SetActive(true);
						}
						else
						{
							Conductors.SetActive(false);
							GlobalPropagate.SetActive(false);
							Flammables.SetActive(false);
							Leaf_floor.SetActive(false);
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

				}


				if (MODE == 4)
				{ //draw MODE 4  - SPLINE AURAS

					if (GUI.Button(new Rect(3 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(SPLINES.Length);

						for (int i = 0; i < SPLINES.Length; i++)
						{
							SPLINES[i].gameObject.SetActive(false);
						}
						SPLINES[SPLINES_COUNTER].gameObject.SetActive(true);

						if (SPLINES.Length != 0)
						{
							Debug.Log("INININ");
							SPLINE_SPLINER = SPLINES[SPLINES_COUNTER].gameObject.GetComponent("SplinerP") as SplinerP;

							CURRENT_AURA_RADIUS = SPLINE_SPLINER.gameObject.transform.localScale.x;
						}

						if (SPLINES_COUNTER > SPLINES.Length - 2)
						{
							SPLINES_COUNTER = 0;
						}
						else
						{
							SPLINES_COUNTER = SPLINES_COUNTER + 1;
						}
					}

					if (SPLINE_SPLINER != null)
					{

						if (SPLINES.Length != 0)
						{

							AURA_RADIUS = GUI.HorizontalSlider(new Rect(10, 160, 100, 30), CURRENT_AURA_RADIUS, 0.05f, 0.4f);
							CURRENT_AURA_RADIUS = AURA_RADIUS;
							SPLINE_SPLINER.transform.localScale = new Vector3(AURA_RADIUS, AURA_RADIUS, AURA_RADIUS);
						}
					}
				}

				if (MODE == 5)
				{ //draw MODE 5  - GRASS FIELDS

					if (GUI.Button(new Rect(4 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(FIELDS_COUNTER);

						for (int i = 0; i < FIELDS.Length; i++)
						{
							FIELDS[i].gameObject.SetActive(false);
						}
						FIELDS[FIELDS_COUNTER].gameObject.SetActive(true);

						if (FIELDS_COUNTER == 4 | FIELDS_COUNTER == 6)
						{
							SUN.gameObject.SetActive(false);
						}

						if (FIELDS_COUNTER > FIELDS.Length - 2)
						{
							FIELDS_COUNTER = 0;
						}
						else
						{
							FIELDS_COUNTER = FIELDS_COUNTER + 1;
						}
					}

				}

				if (MODE == 6)
				{ //draw MODE 6  - FLOWS

					if (GUI.Button(new Rect(5 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(FLOWS_COUNTER);

						for (int i = 0; i < FLOWS.Length; i++)
						{
							FLOWS[i].gameObject.SetActive(false);
						}
						FLOWS[FLOWS_COUNTER].gameObject.SetActive(true);

						if (HERO_CAM != null)
						{
							HERO_CAM.distance = 16;
							HERO_CAM.height = 3.76f;
						}

						CAMERA_UP = 3.76f;
						CAMERA_DIST = 16;

						if (FLOWS_COUNTER > FLOWS.Length - 2)
						{
							FLOWS_COUNTER = 0;
						}
						else
						{
							FLOWS_COUNTER = FLOWS_COUNTER + 1;
						}

					}
				}

				if (MODE == 7)
				{ //draw MODE 7  - SHIELDS

					if (GUI.Button(new Rect(6 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(SHIELDS_COUNTER);

						for (int i = 0; i < SHIELDS.Length; i++)
						{
							SHIELDS[i].gameObject.SetActive(false);
						}
						SHIELDS[SHIELDS_COUNTER].gameObject.SetActive(true);

						if (SUN.gameObject.activeInHierarchy)
						{
							FlameThrowers.gameObject.SetActive(true);
						}

						if (HERO_CAM != null)
						{
							HERO_CAM.distance = 16;
							HERO_CAM.height = 3.76f;
						}

						CAMERA_UP = 3.76f;
						CAMERA_DIST = 16;

						if (SHIELDS_COUNTER > SHIELDS.Length - 2)
						{
							SHIELDS_COUNTER = 0;
						}
						else
						{
							SHIELDS_COUNTER = SHIELDS_COUNTER + 1;
						}

					}
				}

				if (MODE == 8)
				{ //draw MODE 8  - SKINNED_MESH EMISSION

					if (GUI.Button(new Rect(7 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(SKINNED_MESH_COUNTER);

						for (int i = 0; i < SKINNED_MESH.Length; i++)
						{
							SKINNED_MESH[i].gameObject.SetActive(false);
						}
						SKINNED_MESH[SKINNED_MESH_COUNTER].gameObject.SetActive(true);

						if (HERO_CAM != null)
						{
							HERO_CAM.distance = 6;
							HERO_CAM.height = 1f;
						}

						CAMERA_UP = 1f;
						CAMERA_DIST = 6;

						if (SKINNED_MESH_COUNTER > SKINNED_MESH.Length - 2)
						{
							SKINNED_MESH_COUNTER = 0;
						}
						else
						{
							SKINNED_MESH_COUNTER = SKINNED_MESH_COUNTER + 1;
						}
					}
				}

				//9 - WEATHER
				if (MODE == 9)
				{ //draw MODE 9  - WEATHER

					if (GUI.Button(new Rect(8 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						Debug.Log(WEATHER_COUNTER);

						for (int i = 0; i < WEATHER.Length; i++)
						{
							WEATHER[i].gameObject.SetActive(false);
						}
						WEATHER[WEATHER_COUNTER].gameObject.SetActive(true);

						if (HERO_CAM != null)
						{
							HERO_CAM.distance = 2.4f;
							HERO_CAM.height = -1;
						}

						CAMERA_UP = -1;
						CAMERA_DIST = 2.4f;

						if (WEATHER_COUNTER > WEATHER.Length - 2)
						{
							WEATHER_COUNTER = 0;
						}
						else
						{
							WEATHER_COUNTER = WEATHER_COUNTER + 1;
						}

					}
				}

				//10 - PARTICLE_PAINT
				if (HERO != null)
				{
					if (MODE == 10)
					{ //draw MODE 10 - PARTICLE_PAINT 

						if (GUI.Button(new Rect(9 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
						{


							for (int i = 0; i < PARTICLE_PAINT.Length; i++)
							{
								PARTICLE_PAINT[i].gameObject.SetActive(false);
							}
							PARTICLE_PAINT[PARTICLE_PAINT_COUNTER].gameObject.SetActive(true);

							Destroy(HERO.GetComponent("AttractParticles"));
							HERO.AddComponent<AttractParticles>();
							AttractParticles PAINT_SCRIPT1 = HERO.GetComponent("AttractParticles") as AttractParticles;
							PAINT_SCRIPT1.enabled = true;

							PAINT_SCRIPT1.enable_paint = true;

							if (HERO_CAM != null)
							{
								HERO_CAM.distance = 4;
								HERO_CAM.height = 2.76f;
							}

							CAMERA_UP = 2.76f;
							CAMERA_DIST = 4;

							if (PARTICLE_PAINT_COUNTER > PARTICLE_PAINT.Length - 2)
							{
								PARTICLE_PAINT_COUNTER = 0;
							}
							else
							{
								PARTICLE_PAINT_COUNTER = PARTICLE_PAINT_COUNTER + 1;
							}

						}
						if (PARTICLE_PAINT_COUNTER < 3)
						{
							AttractParticles PAINT_SCRIPT2 = HERO.GetComponent("AttractParticles") as AttractParticles;
							if (PAINT_SCRIPT2 != null)
							{
								PAINT_SCRIPT2.enable_paint = true;
							}
						}
						else
						{
							AttractParticles PAINT_SCRIPT1 = HERO.GetComponent("AttractParticles") as AttractParticles;
							if (PAINT_SCRIPT1 != null)
							{
								PAINT_SCRIPT1.enabled = false;
							}
						}

					}
					else
					{
						AttractParticles PAINT_SCRIPT1 = HERO.GetComponent("AttractParticles") as AttractParticles;
						if (PAINT_SCRIPT1 != null)
						{
							PAINT_SCRIPT1.enabled = false;
						}
					}
				}

				//11 - IMAGE EMISSION
				if (MODE == 11)
				{ //draw MODE 11 - PARTICLE_PAINT

					if (GUI.Button(new Rect(10 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						for (int i = 0; i < IMAGE.Length; i++)
						{
							IMAGE[i].gameObject.SetActive(false);
						}
						IMAGE[IMAGE_COUNTER].gameObject.SetActive(true);

						IMAGE_PARTICLE = IMAGE[IMAGE_COUNTER].gameObject.GetComponentInChildren(typeof(ImageToParticles)) as ImageToParticles;

						IMAGE_PARTICLE_DYNAMIC = IMAGE[IMAGE_COUNTER].gameObject.GetComponentInChildren(typeof(ImageToParticlesDYNAMIC)) as ImageToParticlesDYNAMIC;

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
						if (HERO_CAM != null)
						{
							HERO_CAM.distance = 16;
							HERO_CAM.height = 3.76f;
						}

						CAMERA_UP = 3.76f;
						CAMERA_DIST = 16;

						if (IMAGE_COUNTER > IMAGE.Length - 2)
						{
							IMAGE_COUNTER = 0;
						}
						else
						{
							IMAGE_COUNTER = IMAGE_COUNTER + 1;
						}

					}

					if (IMAGE_COUNTER >= 0)
					{
						if (IMAGE.Length != 0)
						{

							if (IMAGE_PARTICLE != null)
							{
								GUI.TextArea(new Rect(5, 120 - 20, 100, 20), "Particle size");
								IMAGE_PARTICLE_RADIUS = GUI.HorizontalSlider(new Rect(10, 120, 100, 30), CURRENT_IMAGE_PARTICLE_RADIUS, 0.05f, 1.5f);
								CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE_RADIUS;
								IMAGE_PARTICLE.PARTICLE_SIZE = IMAGE_PARTICLE_RADIUS;

								GUI.TextArea(new Rect(5, 160 - 20, 100, 20), "Depth");
								IMAGE_PARTICLE_DEPTH = GUI.HorizontalSlider(new Rect(10, 160, 100, 30), CURRENT_IMAGE_PARTICLE_DEPTH, -70, 70);
								CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE_DEPTH;
								IMAGE_PARTICLE.DEPTH_FACTOR = IMAGE_PARTICLE_DEPTH;

								GUI.TextArea(new Rect(5, 200 - 20, 100, 20), "Scale");
								IMAGE_PARTICLE_SCALE = GUI.HorizontalSlider(new Rect(10, 200, 100, 30), CURRENT_IMAGE_PARTICLE_SCALE, 1, 20);
								CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE_SCALE;
								IMAGE_PARTICLE.SCALE_FACTOR = IMAGE_PARTICLE_SCALE;
							}
							if (IMAGE_PARTICLE_DYNAMIC != null)
							{

								GUI.TextArea(new Rect(5, 120 - 20, 100, 20), "Particle size");
								IMAGE_PARTICLE_RADIUS = GUI.HorizontalSlider(new Rect(10, 120, 100, 30), CURRENT_IMAGE_PARTICLE_RADIUS, 0.05f, 1.5f);
								CURRENT_IMAGE_PARTICLE_RADIUS = IMAGE_PARTICLE_RADIUS;
								IMAGE_PARTICLE_DYNAMIC.PARTICLE_SIZE = IMAGE_PARTICLE_RADIUS;

								GUI.TextArea(new Rect(5, 160 - 20, 100, 20), "Depth");
								IMAGE_PARTICLE_DEPTH = GUI.HorizontalSlider(new Rect(10, 160, 100, 30), CURRENT_IMAGE_PARTICLE_DEPTH, -70, 70);
								CURRENT_IMAGE_PARTICLE_DEPTH = IMAGE_PARTICLE_DEPTH;
								IMAGE_PARTICLE_DYNAMIC.DEPTH_FACTOR = IMAGE_PARTICLE_DEPTH;

								GUI.TextArea(new Rect(5, 200 - 20, 100, 20), "Scale");
								IMAGE_PARTICLE_SCALE = GUI.HorizontalSlider(new Rect(10, 200, 100, 30), CURRENT_IMAGE_PARTICLE_SCALE, 1, 20);
								CURRENT_IMAGE_PARTICLE_SCALE = IMAGE_PARTICLE_SCALE;
								IMAGE_PARTICLE_DYNAMIC.SCALE_FACTOR = IMAGE_PARTICLE_SCALE;

								string TAG_ME = "Explode";
								if (IMAGE_PARTICLE_DYNAMIC.Changing_texture == false)
								{
									TAG_ME = "Reform";
								}

								if (GUI.Button(new Rect(105, 230, 100, 20), TAG_ME))
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
					}

				}

				//////////////////////////// VORTEXES //////////////////////////////////

				//12 - VORTEXES
				if (MODE == 12)
				{

					if (GUI.Button(new Rect(11 * BOX_WIDTH + 10, BOX_HEIGHT + 20, BOX_WIDTH, 30), "Next"))
					{

						if (HERO_CAM != null)
						{
							HERO_CAM.enabled = false;
						}

						for (int i = 0; i < VORTEXES.Length; i++)
						{
							VORTEXES[i].gameObject.SetActive(false);
						}
						VORTEXES[VORTEXES_COUNTER].gameObject.SetActive(true);

						ATTRACTOR = VORTEXES[VORTEXES_COUNTER].gameObject.GetComponentInChildren(typeof(AttractParticles)) as AttractParticles;

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

						Camera.main.transform.position = new Vector3(460, 33, -30);
						Camera.main.transform.eulerAngles = new Vector3(4, 0, 0);

						if (HERO_CAM != null)
						{
							HERO_CAM.distance = 16;
							HERO_CAM.height = 3.76f;
						}

						CAMERA_UP = 3.76f;
						CAMERA_DIST = 16;

						if (VORTEXES_COUNTER > VORTEXES.Length - 2)
						{
							VORTEXES_COUNTER = 0;
						}
						else
						{
							VORTEXES_COUNTER = VORTEXES_COUNTER + 1;
						}

					}

					if (VORTEXES_COUNTER > 0)
					{
						if (VORTEXES.Length != 0)
						{

							if (ATTRACTOR != null)
							{

								int BASE1 = 140;//160
								int BASE_X = 25;//60

								if (ATTRACTOR.make_moving_star == false)
								{

									#region AAA

									string TAG_ME = "Remove Turbulance";
									if (ATTRACTOR.Turbulance == false) { TAG_ME = "Add Turbulance"; }
									if (GUI.Button(new Rect(BASE_X, BASE1 - 40, 100, 20), TAG_ME))
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
									if (GUI.Button(new Rect(BASE_X + 45, BASE1 + (4f * 40) + 15, 100, 20), TAG_ME))
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
									if (GUI.Button(new Rect(BASE_X + 45, BASE1 + (5f * 40), 100, 20), TAG_ME))
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

									BASE_X = 215;
									BASE1 = 140;

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
									Vortex_scale = GUI.HorizontalSlider(new Rect(BASE_X - 5, BASE1 + (2f * 40), 100, 30), CURRENT_Vortex_scale, 0, 20);
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

									BASE_X = 390;
									BASE1 = 140;

									TAG_ME = "Attract";
									if (ATTRACTOR.repel == false) { TAG_ME = "Repel"; }
									if (GUI.Button(new Rect(BASE_X, BASE1 - 40, 100, 20), TAG_ME))
									{
										if (ATTRACTOR.repel == false)
										{
											ATTRACTOR.repel = true;
											CURRENT_repel = true;
										}
										else
										{
											ATTRACTOR.repel = false;
											CURRENT_repel = false;
										}
									}

									TAG_ME = "Fast";
									if (ATTRACTOR.smoothattraction == false) { TAG_ME = "Smooth"; }
									if (GUI.Button(new Rect(BASE_X, BASE1 - 20, 100, 20), TAG_ME))
									{
										if (ATTRACTOR.smoothattraction == false)
										{
											ATTRACTOR.smoothattraction = true;
											CURRENT_smoothattraction = true;
										}
										else
										{
											ATTRACTOR.smoothattraction = false;
											CURRENT_smoothattraction = false;
										}
									}

									TAG_ME = "Linear";
									if (ATTRACTOR.use_exponent == false) { TAG_ME = "Exponential"; }
									if (GUI.Button(new Rect(BASE_X, BASE1, 100, 20), TAG_ME))
									{
										if (ATTRACTOR.use_exponent == false)
										{
											ATTRACTOR.use_exponent = true;
											CURRENT_use_exponent = true;
										}
										else
										{
											ATTRACTOR.use_exponent = false;
											CURRENT_use_exponent = false;
										}
									}

									GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (1f * 40) - 20, 180, 20), "Affect dist.: " + CURRENT_affectDistance);
									affectDistance = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (1f * 40), 100, 30), CURRENT_affectDistance, 0, 400);
									CURRENT_affectDistance = affectDistance;
									ATTRACTOR.affectDistance = affectDistance;

									GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (2f * 40) - 20, 180, 20), "Dumpen: " + CURRENT_dumpen);
									dumpen = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (2f * 40), 800, 30), CURRENT_dumpen, 0, 1600);
									CURRENT_dumpen = dumpen;
									ATTRACTOR.dumpen = dumpen;

									TAG_ME = "Remove Color";
									if (ATTRACTOR.Color_force == false) { TAG_ME = "Color affected"; }
									if (GUI.Button(new Rect(BASE_X, BASE1 + (2f * 40) + 20, 100, 20), TAG_ME))
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

									GUI.TextArea(new Rect(BASE_X - 0, BASE1 + (3.5f * 40) - 20, 180, 20), "Vortex color: " + CURRENT_Force_color);
									Force_color.x = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (3.5f * 40), 100, 30), CURRENT_Force_color.x, 0, 1);
									CURRENT_Force_color.x = Force_color.x;
									Force_color.y = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (4.3f * 40), 100, 30), CURRENT_Force_color.y, 0, 1);
									CURRENT_Force_color.y = Force_color.y;
									Force_color.z = GUI.HorizontalSlider(new Rect(BASE_X - 0, BASE1 + (5.1f * 40), 100, 30), CURRENT_Force_color.z, 0, 1);
									CURRENT_Force_color.z = Force_color.z;

									ATTRACTOR.Force_color = new Color(Force_color.x, Force_color.y, Force_color.z);

									#endregion
								}
								else
								{

									BASE_X = 80;
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
						}
					}

				}
				else
				{
					if (HERO_CAM != null)
					{
						HERO_CAM.enabled = true;
					}
				}

			}

		}


	}

}