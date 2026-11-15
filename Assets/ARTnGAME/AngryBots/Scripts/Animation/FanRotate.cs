using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class FanRotate : MonoBehaviour {

		public Mesh thisMesh;
		public Vector2[] uvs;

		#if !UNITY_IPHONE && !UNITY_ANDROID && !UNITY_WP8 && !UNITY_BLACKBERRY

		void Start () 
		{
			thisMesh = GetComponent<MeshFilter> ().mesh; //GetComponent(MeshFilter).mesh; //v2.3
			uvs = thisMesh.uv;
		}

		void Update()
		{
			for (int i = 0; i < uvs.Length; i++) 
			{
				uvs[i].y = (uvs[i].y + 0.25f);
			}

			thisMesh.uv = uvs;
		}

		#endif
}
}