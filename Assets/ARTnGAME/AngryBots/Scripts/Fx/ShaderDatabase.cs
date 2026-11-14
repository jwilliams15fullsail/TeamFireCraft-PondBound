using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {
[RequireComponent(typeof(Camera))]
public class ShaderDatabase : MonoBehaviour {

		// ShaderDatabase
		// knows and eventually "cooks" shaders in the beginning of the game (see CookShaders),
		// also knows some tricks to hide the frame buffer with white and/or black planes
		// to hide loading artefacts or shader cooking process

		//#pragma strict 

		//@script RequireComponent(Camera)

		public Shader[] shaders;
		public bool cookShadersOnMobiles = true;
		public Material cookShadersCover;
		private GameObject cookShadersObject;

		void Awake () {	
			#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
			Screen.sleepTimeout = 0;

			if (!cookShadersOnMobiles)
			return;

			if (!cookShadersCover.HasProperty ("_TintColor"))
			Debug.LogWarning ("Dualstick: the CookShadersCover material needs a _TintColor property to properly hide the cooking process", transform);

			CreateCameraCoverPlane ();
			cookShadersCover.SetColor ("_TintColor",new Color (0.0f,0.0f,0.0f,1.0f));
			#endif
		}

		GameObject CreateCameraCoverPlane () {
			cookShadersObject = GameObject.CreatePrimitive (PrimitiveType.Cube);
			cookShadersObject.GetComponent<Renderer>().material = cookShadersCover;	
			cookShadersObject.transform.parent = transform;
			cookShadersObject.transform.localPosition = new Vector3 (0,0,1.55f);//Vector3.zero; //v2.3
			//cookShadersObject.transform.localPosition.z += 1.55f;

			cookShadersObject.transform.localRotation = Quaternion.identity;

			//cookShadersObject.transform.localEulerAngles.z += 180;
			cookShadersObject.transform.localEulerAngles = 
				new Vector3 (cookShadersObject.transform.localEulerAngles.x, cookShadersObject.transform.localEulerAngles.y, 180+cookShadersObject.transform.localEulerAngles.z); //v2.3

			//cookShadersObject.transform.localScale = Vector3.one *1.5f;	
			//cookShadersObject.transform.localScale.x *= 1.6f;	
			cookShadersObject.transform.localScale = new Vector3(1.5f * 1.6f,1.5f,1.5f); //v2.3

			return cookShadersObject;		
		}

		IEnumerator WhiteOut () {
			CreateCameraCoverPlane ();
			Material mat  = cookShadersObject.GetComponent<Renderer>().sharedMaterial;
			mat.SetColor ("_TintColor", new Color (1.0f, 1.0f, 1.0f, 0.0f));	

			yield return true;

			Color c = new Color (1.0f, 1.0f, 1.0f, 0.0f);
			while (c.a < 1.0f) {
				c.a += Time.deltaTime * 0.25f;
				mat.SetColor ("_TintColor", c);
				yield return true;
			}

			DestroyCameraCoverPlane ();
		}

		public IEnumerator WhiteIn () {	
			CreateCameraCoverPlane ();
			Material mat  = cookShadersObject.GetComponent<Renderer>().sharedMaterial;
			mat.SetColor ("_TintColor", new Color (1.0f, 1.0f, 1.0f, 1.0f));	

			yield return true;

			Color c = new Color (1.0f, 1.0f, 1.0f, 1.0f);
			while (c.a > 0.0) {
				c.a -= Time.deltaTime * 0.25f;
				mat.SetColor ("_TintColor", c);
				yield return true;
			}

			DestroyCameraCoverPlane ();
		}

		void DestroyCameraCoverPlane () {
			if (cookShadersObject)
				DestroyImmediate (cookShadersObject);	
			cookShadersObject = null;
		}

        IEnumerator Start(){ //void Start() { //v2.5
#if UNITY_IPHONE || UNITY_ANDROID || UNITY_WP8 || UNITY_BLACKBERRY
            if (cookShadersOnMobiles) {
                yield return CookShaders();     //yield CookShaders ();	 //v2.5
            }
#else
            yield return 0;//v2.5
#endif
        }

		// this function is cooking all shaders to be used in the game. 
		// it's good practice to draw all of them in order to avoid
		// triggering in game shader compilations which might cause evil
		// frame time spikes

		// currently only enabled for mobile (iOS, Android, Windows Phone and BlackBerry) platforms

		IEnumerator CookShaders () {
			if (shaders.Length > 0) { //v2.3
				Material m = new Material (shaders[0]);
				GameObject cube = GameObject.CreatePrimitive (PrimitiveType.Cube);

				cube.transform.parent = transform;
				cube.transform.localPosition = new Vector3 (0,0, 4.0f); //Vector3.zero;
				//cube.transform.localPosition.z += 4.0f;

				yield return true;

				foreach (Shader s in shaders) {//for (var s : Shader in shaders) {
					if (s) {
						m.shader = s;
						cube.GetComponent<Renderer>().material = m;
					}
					yield return true;
				}

				Destroy (m);
				Destroy (cube);

				yield return true;
				Color c = Color.black;
				c.a = 1.0f;
				while (c.a>0.0f) {
					c.a -= Time.deltaTime*0.5f;
					cookShadersCover.SetColor ("_TintColor", c);
					yield return true;
				}
			}

			DestroyCameraCoverPlane ();
		}
}
}