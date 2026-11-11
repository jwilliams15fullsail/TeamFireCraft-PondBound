using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class PDM_Demo_V20 : MonoBehaviour
	{

		//	public bool Ai_system = false;
		//	bool Ai_enabled = true;
		//	public FlockCollisionsPDM flock;

#pragma warning disable 414
		public bool left_side = false;
		public bool no_instance = false;

		void Start()
		{

			MODE = 3;
			PROJECTILES_COUNTER = 0;
			PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);

			if (!no_instance)
			{
				SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);
				SPHERE.transform.parent = Holder;
				PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);
			}
			else
			{
				for (int i = 0; i < PROJECTILES.Length; i++)
				{
					PROJECTILES[i].gameObject.SetActive(false);
				}
				PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);
			}

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

		public Transform Holder;

		public GameObject[] PROJECTILES;
		public GameObject[] PROJECTILE_NAMES;
		public int PROJECTILES_COUNTER;
		GameObject SPHERE;

		private int MODE = 0;
		public GameObject HERO;

		public bool HUD_ON = true;

		void OnGUI()
		{

			GUI.color = new Color32(255, 255, 255, 201);

			if (HUD_ON)
			{

				if (MODE == 3)
				{ //draw MODE 3  - PROJECTILES

					int coutn = PROJECTILES_COUNTER - 1;
					if (PROJECTILES_COUNTER - 1 < 0)
					{
						coutn = PROJECTILES.Length - 1;
					}
					string texty = PROJECTILES[coutn].name;

					int left = 350;
					int down = 5;
					int wide = 150;
					int Y_offset = 60;
					if (left_side)
					{
						left = 5;
						down = 5 + 30;
						wide = 150;
					}

					if (left_side)
					{
						GUI.TextArea(new Rect(5, 5 + Y_offset - 5, wide, 30), "");
						GUI.Label(new Rect(7, 0 + Y_offset - 5, wide, 36), "<size=11>" + texty + "</size>");
					}
					else
					{
						GUI.TextArea(new Rect(300 + 150 + 100, 5 + 30 + Y_offset - 5, wide, 30), texty);
					}

					left = 350;
					down = 5;
					wide = 50;
					if (left_side)
					{
						left = 5;
						down = 5 + 25 + 25;
						wide = 50;
					}

					if (left_side)
					{
						if (GUI.Button(new Rect(left, down + Y_offset, wide, 25), "Reset"))
						{

							Debug.Log(PROJECTILES_COUNTER);

							int COUNT_ME = PROJECTILES_COUNTER - 1;
							if (PROJECTILES_COUNTER == 0)
							{

								COUNT_ME = PROJECTILES.Length - 1;
							}
							PROJECTILES[COUNT_ME].gameObject.SetActive(true);

							if (!no_instance)
							{
								Destroy(SPHERE);
								SPHERE = (GameObject)Instantiate(PROJECTILES[COUNT_ME].gameObject, PROJECTILES[COUNT_ME].gameObject.transform.position, PROJECTILES[COUNT_ME].gameObject.transform.rotation);
								SPHERE.transform.parent = Holder;
								PROJECTILES[COUNT_ME].gameObject.SetActive(false);
							}
							else
							{
								for (int i = 0; i < PROJECTILES.Length; i++)
								{
									PROJECTILES[i].gameObject.SetActive(false);
								}
								PROJECTILES[COUNT_ME].gameObject.SetActive(true);
							}

							#region EXTRA
							for (int j = 0; j < PROJECTILE_NAMES.Length; j++)
							{
								if (j == PROJECTILES_COUNTER)
								{
									PROJECTILE_NAMES[j].gameObject.SetActive(true);
								}
								else { PROJECTILE_NAMES[j].gameObject.SetActive(false); }
							}
							#endregion
						}
					}

					#region BACK BUTTON
					left = 300 + 150 + 100;
					down = 5;
					wide = 50;
					if (left_side)
					{
						left = 5;
						down = 5 + 25 + 25 + 25;
						wide = 50;
					}
					if (GUI.Button(new Rect(left, down + Y_offset, wide, 25), "Back"))
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

						if (!no_instance)
						{
							Destroy(SPHERE);
							SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);
							SPHERE.transform.parent = Holder;
							PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);
						}
						else
						{
							for (int i = 0; i < PROJECTILES.Length; i++)
							{
								PROJECTILES[i].gameObject.SetActive(false);
							}
							PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);
						}

						for (int j = 0; j < PROJECTILE_NAMES.Length; j++)
						{
							if (j == PROJECTILES_COUNTER)
							{
								PROJECTILE_NAMES[j].gameObject.SetActive(true);
							}
							else { PROJECTILE_NAMES[j].gameObject.SetActive(false); }
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
					#endregion
					left = 400 + 100 + 100;
					down = 5;
					wide = 50;
					if (left_side)
					{
						left = 5;
						down = 5 + 25 - 25 + 25;
						wide = 50;
					}
					if (GUI.Button(new Rect(left, down + Y_offset, wide, 25), "Next"))
					{

						Debug.Log("Projectile " + PROJECTILES_COUNTER);

						for (int i = 0; i < PROJECTILES.Length; i++)
						{
							PROJECTILES[i].gameObject.SetActive(false);
						}
						PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);

						if (!no_instance)
						{
							Destroy(SPHERE);
							SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position, PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);
							SPHERE.transform.parent = Holder;
							PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);
						}
						else
						{
							for (int i = 0; i < PROJECTILES.Length; i++)
							{
								PROJECTILES[i].gameObject.SetActive(false);
							}
							PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true);
						}

						for (int j = 0; j < PROJECTILE_NAMES.Length; j++)
						{
							if (j == PROJECTILES_COUNTER)
							{
								PROJECTILE_NAMES[j].gameObject.SetActive(true);
							}
							else { PROJECTILE_NAMES[j].gameObject.SetActive(false); }
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

				///////////////////////// SPECIFIC SYSTEMS ///////////////

				// AI
				//			string aionoff = "on";
				//			if(!Ai_enabled){
				//				aionoff = "off";
				//			}
				//			if(Ai_system & flock != null){
				//				if (GUI.Button(new Rect(5, 5, 50, 25), "AI is "+aionoff)){
				//					if(!Ai_enabled){
				//						Ai_enabled = true;
				//						flock.enabled = true;
				//					}else{
				//						Ai_enabled = false;
				//						flock.enabled = false;
				//					}
				//				}
				//			}

			}   //end if HUD on
		} //END ON_GUI
	}
}