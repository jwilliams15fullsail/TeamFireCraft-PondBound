using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {
public class GlowPlane : MonoBehaviour {

		public Transform playerTransform;
		private Vector3 pos;
		private Vector3 scale;
		public float minGlow = 0.2f;
		public float maxGlow = 0.5f;
		Color glowColor = Color.white;

		private Material mat;

		void Start () {
			if (!playerTransform)
				playerTransform = GameObject.FindWithTag ("Player").transform;	
			pos = transform.position;
			scale = transform.localScale;
			mat = GetComponent<Renderer>().material;
			enabled = false;
		}

		void OnDrawGizmos () {
			Gizmos.color = glowColor;
			//Gizmos.color.a = maxGlow * 0.25f;	
			Gizmos.color = new Color(Gizmos.color.r,Gizmos.color.g,Gizmos.color.b,maxGlow * 0.25f);	//v2.3
			Gizmos.matrix = transform.localToWorldMatrix;
			Vector3 scale = 5.0f * Vector3.Scale (Vector3.one, new Vector3(1,0,1));
			Gizmos.DrawCube (Vector3.zero, scale);
			Gizmos.matrix = Matrix4x4.identity;
		}

		void OnDrawGizmosSelected () {
			Gizmos.color = glowColor;
			//Gizmos.color.a = maxGlow;
			Gizmos.color = new Color(Gizmos.color.r,Gizmos.color.g,Gizmos.color.b,maxGlow);	//v2.3
			Gizmos.matrix = transform.localToWorldMatrix;
			Vector3 scale = 5.0f * Vector3.Scale (Vector3.one, new Vector3(1,0,1));
			Gizmos.DrawCube (Vector3.zero, scale);
			Gizmos.matrix = Matrix4x4.identity;
		}

		void OnBecameVisible () {
			enabled = true;	
		}

		void OnBecameInvisible () {
			enabled = false;
		}

		void Update () {
			Vector3 vec = (pos - playerTransform.position);
			vec.y = 0.0f;
			float distance = vec.magnitude;	
			transform.localScale = Vector3.Lerp (Vector3.one * minGlow, scale, Mathf.Clamp01 (distance * 0.35f));
			mat.SetColor ("_TintColor",  glowColor * Mathf.Clamp (distance * 0.1f, minGlow, maxGlow));	
		}
}
}