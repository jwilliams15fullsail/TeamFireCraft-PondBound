using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class SmoothLookAtPDM : MonoBehaviour
	{


		public Transform target;
		public float damping = 6.0f;
		public bool smooth = true;

		//@script AddComponentMenu("Camera-Control/Smooth Look At")

		void LateUpdate()
		{
			if (target)
			{
				if (smooth)
				{
					// Look at and dampen the rotation
					Quaternion rotation = Quaternion.LookRotation(target.position - transform.position);
					transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * damping);
				}
				else
				{
					// Just lookat
					transform.LookAt(target);
				}
			}
		}

		void Start()
		{
			// Make the rigid body not change rotation
			if (GetComponent<Rigidbody>())
				GetComponent<Rigidbody>().freezeRotation = true;
		}
	}
}