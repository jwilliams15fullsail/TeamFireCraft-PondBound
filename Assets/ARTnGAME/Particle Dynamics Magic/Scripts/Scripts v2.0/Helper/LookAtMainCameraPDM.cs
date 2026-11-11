using UnityEngine;
using System.Collections;
namespace Artngame.PDM
{
	public class LookAtMainCameraPDM : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{
			ThisTransform = transform;
		}

		public bool enable_rot = false;
		public float rot_speed = 1f;

		Transform ThisTransform;
		// Update is called once per frame
		void Update()
		{
			ThisTransform.LookAt(Camera.main.transform.position);
			if (enable_rot)
			{

				ThisTransform.RotateAround(ThisTransform.position, ThisTransform.forward, Time.deltaTime * rot_speed * Random.Range(0f, 1f));
			}
		}
	}
}