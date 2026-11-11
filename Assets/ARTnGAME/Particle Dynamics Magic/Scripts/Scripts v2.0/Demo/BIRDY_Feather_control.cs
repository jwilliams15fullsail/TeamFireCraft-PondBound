using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class BIRDY_Feather_control : MonoBehaviour
	{

		SKinColoredMaskedCol SkinnedController;
		public AttractParticles Attractor;

		public SKinColoredMaskedCol SkinnedControllerICE1;
		public SKinColoredMaskedCol SkinnedControllerICE2;
		// Use this for initialization
		void Start()
		{
			SkinnedController = Feathers_particles.GetComponent(typeof(SKinColoredMaskedCol)) as SKinColoredMaskedCol;
			//Attractor = 
			//Looker = Camera.main.GetComponent(typeof(SmoothLookAtPDM)) as SmoothLookAtPDM;
		}

		// Update is called once per frame
		void Update()
		{
			if (grow_ice)
			{
				if (SkinnedControllerICE1.Start_size < 5.5f)
				{
					SkinnedControllerICE1.Start_size = SkinnedControllerICE1.Start_size + 0.15f;
				}
				if (SkinnedControllerICE1.Start_size > 4.5f & SkinnedControllerICE2.Start_size < 5.5f)
				{
					SkinnedControllerICE2.Start_size = SkinnedControllerICE2.Start_size + 0.15f;
				}

				if (SkinnedControllerICE2.Start_size >= 5.5f)
				{
					grow_ice = false;
				}
			}
		}

		public GameObject Feathers_Gameobj_pool;
		public GameObject Feathers_particles;
		public GameObject Flying_Feathers_particles;
		public GameObject AnimatorB;
		public GameObject AnimatorD;
		GameObject Animator;
		public ParticleSystem Fire_particles;

		public Transform Bird_camera_target;
		public Transform Dog_camera_target;
		//SmoothLookAtPDM Looker;

		float Gravity = 0.005f;

		float Left_scale = 0.1f;
		float Right_scale = 1;
		float Wind_scale = 1;
		float Wind_dir = -1;
		float Wind_dir1 = -1;
		float Wind_dir2 = 0;
		float Lerpy = 0;

		int cur_target = 0;
		bool grow_particles = false;
		bool toggle_growth = false;
		int fire_mode = 0;

		bool grow_ice = false;

		void OnGUI()
		{
			string Feathers_on = "On";

			if (Feathers_Gameobj_pool.activeInHierarchy)
			{
				Feathers_on = "Off";
			}

			if (GUI.Button(new Rect(10, 10 - 10, 100, 20), "Feathers " + Feathers_on))
			{

				if (Feathers_Gameobj_pool.activeInHierarchy)
				{
					Feathers_Gameobj_pool.SetActive(false);
					Feathers_particles.SetActive(false);
					Flying_Feathers_particles.SetActive(false);
				}
				else
				{
					Feathers_Gameobj_pool.SetActive(true);
					Feathers_particles.SetActive(true);
					Flying_Feathers_particles.SetActive(true);
				}
			}

			//		string Fire_on = "On";
			//		if(Fire_particles.activeInHierarchy){
			//			Fire_on = "Off";
			//		}
			//		
			//		if (GUI.Button(new Rect(10+(1*100),10,100,20), "Fire "+Fire_on)){
			//			
			//			if(Fire_particles.activeInHierarchy){
			//				Fire_particles.SetActive(false);
			//
			//			}else{
			//				Fire_particles.SetActive(true);
			//
			//			}
			//		}



			//		if (GUI.Button(new Rect(10+(1*100),10-10,150,20), "Toggle focus")){
			//
			//			if(Looker.target != Dog_camera_target){
			//				Looker.target = Dog_camera_target;
			//				cur_target=1;
			//			}else{
			//				Looker.target = Bird_camera_target;
			//				cur_target=0;
			//			}
			//		}

			if (GUI.Button(new Rect(10 + (1 * 100), 10 + 1 * 20 - 10, 150, 20), "Toggle fire"))
			{
				if (fire_mode == 0)
				{
					Attractor.MultiThread = true;
					Attractor.Calc_per_frame = true;
					Attractor.Emit_in_transforms = false;
					Attractor.Emit_specific_transf = false;
				}
				if (fire_mode == 1)
				{
					Attractor.MultiThread = true;
					Attractor.Calc_per_frame = false;
					Attractor.Emit_in_transforms = true;
					Attractor.Emit_specific_transf = true;
				}
				if (fire_mode == 2)
				{
					Attractor.MultiThread = true;
					Attractor.Calc_per_frame = false;
					Attractor.Emit_in_transforms = true;
					Attractor.Emit_specific_transf = false;
				}
				if (fire_mode == 3)
				{
					Attractor.MultiThread = false;
					Attractor.Calc_per_frame = false;
					Attractor.Emit_in_transforms = false;
					Attractor.Emit_specific_transf = false;
				}
				if (fire_mode == 4)
				{
					Attractor.MultiThread = false;
					Attractor.Calc_per_frame = false;
					Attractor.Emit_in_transforms = true;
					Attractor.Emit_specific_transf = false;
				}
				if (fire_mode == 5)
				{
					Attractor.MultiThread = false;
					Attractor.Calc_per_frame = false;
					Attractor.Emit_in_transforms = true;
					Attractor.Emit_specific_transf = true;
				}
				if (fire_mode == 6)
				{
					Attractor.MultiThread = true;
					Attractor.Calc_per_frame = true;
					Attractor.Emit_in_transforms = false;
					Attractor.Emit_specific_transf = false;
					fire_mode = 0;
				}
				fire_mode++;
			}

			if (GUI.Button(new Rect(10 + (1 * 100), 10 + 2 * 20 - 10, 150, 20), "Grow ice"))
			{
				if (!SkinnedControllerICE1.gameObject.activeInHierarchy)
				{
					SkinnedControllerICE1.gameObject.SetActive(true);
					SkinnedControllerICE2.gameObject.SetActive(true);
					grow_ice = true;
					SkinnedControllerICE1.Start_size = 0;
				}
				else
				{
					SkinnedControllerICE1.gameObject.SetActive(false);
					SkinnedControllerICE2.gameObject.SetActive(false);
					grow_ice = false;
					SkinnedControllerICE1.Start_size = 0;
					SkinnedControllerICE2.Start_size = 0;
				}


			}


			Animator = AnimatorB;
			if (cur_target == 1)
			{
				Animator = AnimatorD;
			}

			if (GUI.Button(new Rect(10, 10 + (1 * 20) - 10, 100, 20), "Walk"))
			{
				if (cur_target == 0)
				{
					Animator.GetComponent<Animation>().Play("walkWeapon");
				}
				else
				{
					Animator.GetComponent<Animation>().Play("walk");
				}
				grow_particles = false;

				//Fire_particles.maxParticles = 0;
				ParticleSystem.MainModule main = Fire_particles.main;//v2.3
				main.maxParticles = 0;

				toggle_growth = false;
				//Fire_particles.startSize=0;
				//ParticleSystem.MainModule main = Fire_particles.main;//v2.3
				ParticleSystem.MinMaxCurve Curve = main.startSize;
				Curve.constant = 0;
			}
			if (GUI.Button(new Rect(10, 10 + (2 * 20) - 10, 100, 20), "Run"))
			{
				if (cur_target == 0)
				{
					Animator.GetComponent<Animation>().Play("runWeapon");
				}
				else
				{
					Animator.GetComponent<Animation>().Play("run");
				}
				grow_particles = false;
				//Fire_particles.maxParticles = 0;
				ParticleSystem.MainModule main = Fire_particles.main;//v2.3
				main.maxParticles = 0;
				toggle_growth = false;
				//Fire_particles.startSize=0;
				ParticleSystem.MinMaxCurve Curve = main.startSize;
				Curve.constant = 0;
			}
			int disp_x = 45;
			int disp_y = 20;
			if (GUI.Button(new Rect(disp_x + 10, disp_y + 10 + (3 * 20), 100, 20), "Fall"))
			{
				if (cur_target == 0)
				{
					Animator.GetComponent<Animation>().Play("deathWeapon");
				}
				else
				{
					Animator.GetComponent<Animation>().Play("agressiveDeath");
				}
				grow_particles = false;
				//Fire_particles.maxParticles = 0;
				ParticleSystem.MainModule main = Fire_particles.main;//v2.3
				main.maxParticles = 0;
				toggle_growth = false;
				//Fire_particles.startSize=0;
				ParticleSystem.MinMaxCurve Curve = main.startSize;
				Curve.constant = 0;
			}
			if (GUI.Button(new Rect(disp_x + 10, disp_y + 10 + (4 * 20), 100, 20), "Get hit"))
			{
				if (cur_target == 0)
				{
					Animator.GetComponent<Animation>().Play("getHitWeapon");
				}
				else
				{
					Animator.GetComponent<Animation>().Play("getHit");
				}
				grow_particles = false;
				//Fire_particles.maxParticles = 0;
				ParticleSystem.MainModule main = Fire_particles.main;//v2.3
				main.maxParticles = 0;
				toggle_growth = false;
				//Fire_particles.startSize=0;
				ParticleSystem.MinMaxCurve Curve = main.startSize;
				Curve.constant = 0;
			}
			if (GUI.Button(new Rect(disp_x + 10, disp_y + 10 + (5 * 20), 100, 20), "Idle"))
			{
				if (cur_target == 0)
				{
					Animator.GetComponent<Animation>().Play("idleBreatheWeapon");
				}
				else
				{
					Animator.GetComponent<Animation>().Play("idleLookAround");
				}
				grow_particles = false;
				//Fire_particles.maxParticles = 0;
				ParticleSystem.MainModule main = Fire_particles.main;//v2.3
				main.maxParticles = 0;
				toggle_growth = false;
				//Fire_particles.startSize=0;
				ParticleSystem.MinMaxCurve Curve = main.startSize;
				Curve.constant = 0;
			}
			if (GUI.Button(new Rect(disp_x + 10, disp_y + 10 + (6 * 20), 100, 20), "Attack"))
			{
				if (cur_target == 0)
				{
					Animator.GetComponent<Animation>().Play("attack1Weapon");
					grow_particles = false;
					//Fire_particles.maxParticles = 0;
					ParticleSystem.MainModule main = Fire_particles.main;//v2.3
					main.maxParticles = 0;
					toggle_growth = false;
					//Fire_particles.startSize=0;
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant = 0;
				}
				else
				{
					Animator.GetComponent<Animation>()["standBite"].speed = 0.11f;
					Animator.GetComponent<Animation>().Play("standBite");
					//Fire_particles.maxParticles = 0;
					ParticleSystem.MainModule main = Fire_particles.main;//v2.3
					main.maxParticles = 0;
					grow_particles = true;
					toggle_growth = false;
					//Fire_particles.startSize = 0; 
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant = 0;
					//				if(Fire_particles.maxParticles > 0){
					//
					//				}
					//
					//				if(Fire_particles.maxParticles < 1000){
					//					Fire_particles.maxParticles++;
					//				}
					Attractor.MultiThread = true;
					Attractor.Calc_per_frame = true;
					Attractor.Emit_in_transforms = false;
					Attractor.Emit_specific_transf = false;
				}
			}

			if (grow_particles)
			{
				if (Fire_particles.main.maxParticles < 1000 & !toggle_growth)
				{
					//Fire_particles.maxParticles=Fire_particles.maxParticles+5;
					ParticleSystem.MainModule main = Fire_particles.main;//v2.3
					main.maxParticles = Fire_particles.main.maxParticles + 5;

					if (Fire_particles.main.maxParticles > 350)
					{
						if (Fire_particles.main.startSize.constant < 3)
						{
							//Fire_particles.startSize=Fire_particles.main.startSize.constant+0.1f;
							ParticleSystem.MinMaxCurve Curve = main.startSize;
							Curve.constant = Fire_particles.main.startSize.constant + 0.1f;
						}
					}

					if (Fire_particles.main.maxParticles >= 999)
					{
						toggle_growth = true;
					}
				}
				else if (toggle_growth)
				{
					//grow_particles = false;
					//Fire_particles.maxParticles = Fire_particles.main.maxParticles-10;
					ParticleSystem.MainModule main = Fire_particles.main;//v2.3
					main.maxParticles = Fire_particles.main.maxParticles - 10;
					//Fire_particles.startSize=Fire_particles.startSize-0.1f;

					//ParticleSystem.MainModule main = Fire_particles.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant = Fire_particles.main.startSize.constant - 0.1f;

					if (Fire_particles.main.maxParticles <= 10)
					{
						toggle_growth = false;
						//Fire_particles.startSize=0;
						//ParticleSystem.MinMaxCurve Curve = main.startSize;
						Curve.constant = 0;
					}
				}
			}

			if (GUI.Button(new Rect(disp_x + 10, disp_y + 10 + (7 * 20), 100, 20), "Fly away"))
			{
				if (!SkinnedController.Let_loose)
				{
					SkinnedController.Let_loose = true;
					//SkinnedController.Angled = false;
					//SkinnedController.Asign_rot = false;
					SkinnedController.Return_speed = 0.005f;
					Gravity = 0.005f;
				}
				else if (SkinnedController.Let_loose)
				{
					SkinnedController.Let_loose = false;
					//SkinnedController.Angled = true;
					//SkinnedController.Asign_rot = true;
				}
				//Animator.animation.Play("attack1Weapon");
			}
			Gravity = GUI.HorizontalSlider(new Rect(disp_x + 10 + 100 + 5, disp_y + 10 + (8 * 20), 100 - 5, 20), Gravity, 0.005f, 2);
			SkinnedController.Return_speed = Gravity;
			GUI.TextArea(new Rect(disp_x + 10 + 100, disp_y + 10 + (7 * 20), 100, 20), "Gravitate back");

			GUI.TextArea(new Rect(10, 10 + (9 * 20), 150, 20), "Small feather scale");
			Left_scale = GUI.HorizontalSlider(new Rect(10, 10 + (10 * 20), 150, 20), SkinnedController.Scale_low_high.x, 0, 3);
			SkinnedController.Scale_low_high.x = Left_scale;

			GUI.TextArea(new Rect(10, 10 + (11 * 20), 150, 20), "Big feather scale");
			Right_scale = GUI.HorizontalSlider(new Rect(10, 10 + (12 * 20), 150, 20), SkinnedController.Scale_low_high.y, 0, 3);
			SkinnedController.Scale_low_high.y = Right_scale;

			GUI.TextArea(new Rect(10, 10 + (13 * 20), 150, 20), "Wind strength");
			Wind_scale = GUI.HorizontalSlider(new Rect(10, 10 + (14 * 20), 150, 20), SkinnedController.Wind_speed, 0, 6);
			SkinnedController.Wind_speed = Wind_scale;

			GUI.TextArea(new Rect(10, 10 + (15 * 20), 150, 20), "Wind direction A");
			Wind_dir = GUI.HorizontalSlider(new Rect(10, 10 + (16 * 20), 150, 20), SkinnedController.Local_rot.z, -12.55f, 12.55f);
			SkinnedController.Local_rot.z = Wind_dir;

			GUI.TextArea(new Rect(10, 10 + (17 * 20), 150, 20), "Wind direction B");
			Wind_dir1 = GUI.HorizontalSlider(new Rect(10, 10 + (18 * 20), 150, 20), SkinnedController.Local_rot.x, -3f, 3f);
			SkinnedController.Local_rot.x = Wind_dir1;

			GUI.TextArea(new Rect(10, 10 + (19 * 20), 150, 20), "Wind direction C");
			Wind_dir2 = GUI.HorizontalSlider(new Rect(10, 10 + (20 * 20), 150, 20), SkinnedController.Wind_Y_offset, -3f, 3f);
			SkinnedController.Wind_Y_offset = Wind_dir2;

			GUI.TextArea(new Rect(10, 10 + (21 * 20), 150, 20), "Lerp to normal");
			Lerpy = GUI.HorizontalSlider(new Rect(10, 10 + (22 * 20), 150, 20), SkinnedController.Lerp_ammount, -0.09f, 0.09f);
			SkinnedController.Lerp_ammount = Lerpy;

		}
	}
}