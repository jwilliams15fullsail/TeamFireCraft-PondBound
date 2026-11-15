using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM{
public class ExplosionLine : MonoBehaviour {

		public int frames = 2;
		private int _frames = 0;


		void OnEnable ()
		{
			_frames = 0;
		}


		void Update ()
		{
			_frames++;

			if (_frames>frames)
			{
				gameObject.SetActive (false);
			}
		}
}
}