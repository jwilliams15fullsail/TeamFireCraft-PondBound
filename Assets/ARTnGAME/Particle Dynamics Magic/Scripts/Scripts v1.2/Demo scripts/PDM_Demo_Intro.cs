using UnityEngine;
using System.Collections;
namespace Artngame.PDM
{
	public class PDM_Demo_Intro : MonoBehaviour
	{

		void Start()
		{

			GET_TIME = Time.fixedTime;

		}
		float GET_TIME;
		public GameObject HERO;
		public GameObject LETTERS;

		void Update()
		{

			if (Time.fixedTime - GET_TIME > 15)
			{
				LETTERS.gameObject.SetActive(false);

				PDM_Demo_Scripts DEMO = this.gameObject.GetComponent("PDM_Demo_Scripts") as PDM_Demo_Scripts;
				DEMO.enabled = true;
				HERO.gameObject.SetActive(true);
			}

		}

	}


}









