using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class conveyorBelt : MonoBehaviour {


		public float scrollSpeed = 0.1f;
		public Material mat;

		void Start () {
			enabled = false;
		}

		void OnBecameVisible () {
			enabled = true;	
		}

		void OnBecameInvisible () {
			enabled = false;	
		}

		void Update () {
			float offset = (Time.time * scrollSpeed) % 1.0f;

			mat.SetTextureOffset ("_MainTex", new Vector2(0, -offset));
			mat.SetTextureOffset ("_BumpMap", new Vector2(0, -offset));
		}
}
}