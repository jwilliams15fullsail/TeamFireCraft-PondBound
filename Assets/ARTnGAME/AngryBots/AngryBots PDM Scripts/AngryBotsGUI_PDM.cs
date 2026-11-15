using UnityEngine;
using System.Collections;
using System.Collections.Generic;
namespace Artngame.PDM
{
	public class AngryBotsGUI_PDM : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{

			for (int i = 0; i < POOLS.Count; i++)
			{
				POOLS[i].SetActive(false);
			}
			POOLS[0].SetActive(true);

			for (int i = 0; i < WEAPONS.Count; i++)
			{
				WEAPONS[i].SetActive(false);
			}
			WEAPONS[0].SetActive(true);

		}

		public List<GameObject> POOLS;
		public List<GameObject> WEAPONS;

		// Update is called once per frame
		void Update()
		{



		}

		void OnGUI()
		{

			float Y_DIST = 35;
			float Y_START = 25;
			float X_WIDTH = 100;

			if (GUI.Button(new Rect(10, Y_START + 0 * Y_DIST, 50 + X_WIDTH, 17), "Flamethrower"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[0].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[0].SetActive(true);
			}

			if (GUI.Button(new Rect(10, Y_START + 1 * Y_DIST, 50 + X_WIDTH, 17), "Icethrower"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[1].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[1].SetActive(true);
			}

			if (GUI.Button(new Rect(10, Y_START + 2 * Y_DIST, 50 + X_WIDTH, 17), "Ice bomb"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[2].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[2].SetActive(true);
			}

			if (GUI.Button(new Rect(10, Y_START + 3 * Y_DIST, 50 + X_WIDTH, 17), "Flame-Ice Duel"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[3].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[3].SetActive(true);
			}

			if (GUI.Button(new Rect(10, Y_START + 4 * Y_DIST, 50 + X_WIDTH, 17), "Lightning"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[4].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[4].SetActive(true);
			}

			if (GUI.Button(new Rect(10, Y_START + 5 * Y_DIST, 50 + X_WIDTH, 17), "Tentacles"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[5].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[5].SetActive(true);
			}

			if (GUI.Button(new Rect(10, Y_START + 6 * Y_DIST, 50 + X_WIDTH, 17), "Melting Ice"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[6].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[6].SetActive(true);
			}

			if (GUI.Button(new Rect(10, Y_START + 7 * Y_DIST, 50 + X_WIDTH, 17), "Butterflies"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[7].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[7].SetActive(true);
			}

			if (GUI.Button(new Rect(10, Y_START + 8 * Y_DIST, 50 + X_WIDTH, 17), "Burn wood"))
			{
				for (int i = 0; i < POOLS.Count; i++)
				{
					POOLS[i].SetActive(false);
				}
				POOLS[8].SetActive(true);
				for (int i = 0; i < WEAPONS.Count; i++)
				{
					WEAPONS[i].SetActive(false);
				}
				WEAPONS[8].SetActive(true);
			}

		}
	}
}