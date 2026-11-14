using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class PDM_Demo_GRASS : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{
			timer = Time.fixedTime;
		}

		//v2.0
		//grab and disable the grass casters and pool particle
		public GameobjectProjection Projection;
		public ControlCombineChildrenPDM Combiner;
		public Transform Particles;
		public MeshCollider Colliders;
		public Light lighty;

		bool init = false;

		// Update is called once per frame
		void Update()
		{
			if (Time.fixedTime - timer > 0.1f & !init)
			{
				if (Projection != null)
				{
					Projection.enabled = false;
					Particles.gameObject.SetActive(false);
					Colliders.enabled = true;
				}
				init = true;
			}
		}

		bool Toggle = false;
		float timer;


		void OnGUI()
		{

			string HUD_state = "Batch grass";
			if (!Toggle)
			{
				HUD_state = "Move grass";
			}
			if (GUI.Button(new Rect(5, 10, 80, 25), HUD_state))
			{

				if (Toggle)
				{
					Toggle = false;
					if (Projection != null)
					{
						Projection.enabled = false;
						Projection.fix_initial = true;
					}
					Particles.gameObject.SetActive(false);
					if (Combiner != null)
					{
						Combiner.MakeActive = true;
					}
					Colliders.enabled = true;
					lighty.enabled = true;
				}
				else
				{
					Toggle = true;
					Particles.gameObject.SetActive(true);
					if (Projection != null)
					{
						Projection.enabled = true;
						Projection.fix_initial = false;
					}
					if (Combiner != null)
					{
						Combiner.Decombine = true;
					}
					Colliders.enabled = false;
					lighty.enabled = false;
				}
			}

			if (GUI.Button(new Rect(5, 10 + 25, 80, 25), "Toggle light"))
			{
				if (!lighty.enabled)
				{
					lighty.enabled = true;
				}
				else
				{
					lighty.enabled = false;
				}
			}


		}
	}
}