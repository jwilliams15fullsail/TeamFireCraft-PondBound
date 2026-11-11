using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Artngame.PDM {

	[ExecuteInEditMode]
public class MoodBoxManager : MonoBehaviour {


		public MoodBox current = null; //public static MoodBox current = null; //v2.3
		public MoodBoxData currentData;

		public MobileBloom bloom ;
		public ColoredNoise noise;
		public RenderFogPlane fog;

		public MoodBox startMoodBox ;
		public Cubemap defaultPlayerReflection;
		public Material[] playerReflectionMaterials;
		public bool applyNearestMoodBox = false;

		public MoodBox currentMoodBox = null;

		//@HideInInspector
		public GameObject[] splashManagers;
		//@HideInInspector
		public GameObject[] rainManagers;

		public void Awake () {
			// mood boxes have the power to disable expensive effects when it makes sense

			splashManagers = GameObject.FindGameObjectsWithTag ("RainSplashManager");
			rainManagers = GameObject.FindGameObjectsWithTag ("RainBoxManager");
		}

		public void Start () {
			if (!bloom)
				bloom = Camera.main.gameObject.GetComponent<MobileBloom> ();	
			if (!noise)
				noise = Camera.main.gameObject.GetComponent<ColoredNoise> ();	
			if (!fog)
				fog = Camera.main.gameObject.GetComponentInChildren<RenderFogPlane> ();

			current = startMoodBox;	
			UpdateFromMoodBox ();
		}

		public void Update () {
			UpdateFromMoodBox ();
		}

		public MoodBoxData GetData () {
			return currentData;
		}

		void UpdateFromMoodBox () {

			#if UNITY_EDITOR	
			ApplyNearestMoodBoxIfDesired ();
			#endif

			// we want to see what the current mood box is in the editor
			currentMoodBox = current;

			if (current) {	
				if (!Application.isPlaying) {	
					currentData.noiseAmount = current.data.noiseAmount;
					currentData.colorMixBlend = current.data.colorMixBlend;
					currentData.colorMix = current.data.colorMix;
					currentData.fogY = current.data.fogY;
					currentData.fogColor = current.data.fogColor;
					currentData.outside = current.data.outside;		
				} 
				else { 
					// play mode, interpolate nicely
					currentData.noiseAmount = Mathf.Lerp (currentData.noiseAmount, current.data.noiseAmount, Time.deltaTime);		
					currentData.colorMixBlend = Mathf.Lerp (currentData.colorMixBlend, current.data.colorMixBlend, Time.deltaTime);
					currentData.colorMix = Color.Lerp (currentData.colorMix, current.data.colorMix, Time.deltaTime);
					currentData.fogY = Mathf.Lerp (currentData.fogY, current.data.fogY, Time.deltaTime * 1.5f);	
					currentData.fogColor = Color.Lerp (currentData.fogColor, current.data.fogColor, Time.deltaTime * 0.25f);
					currentData.outside = current.data.outside;			
				}	
			} 	

			// apply new mood and effect values to actual effects (if in use)

			if (bloom && bloom.enabled) {
				bloom.colorMix = currentData.colorMix;
				bloom.colorMixBlend = currentData.colorMixBlend;		
			}
			if (noise && noise.enabled) {
				noise.localNoiseAmount = currentData.noiseAmount;
			}
			if (fog && fog.enabled) {
				fog.GetComponent<Renderer>().sharedMaterial.SetFloat ("_Y", currentData.fogY);
				fog.GetComponent<Renderer>().sharedMaterial.SetColor ("_FogColor", currentData.fogColor);			
			}
		}

		void ApplyNearestMoodBoxIfDesired () {
			if (applyNearestMoodBox) { 
				Component[] boxes  = null;
				//boxes = GetComponentsInChildren (MoodBox); // as MoodBox[];
				boxes = GetComponentsInChildren<MoodBox>(); //v2.3
				if (boxes != null) {
					Vector3 cameraPos = Camera.main.transform.position;
					var minMoodBox = boxes[0] as MoodBox;
					var minDistance = Mathf.Infinity;
					foreach (Component b in boxes) {//for (var b : Component in boxes) {
						if ( ((b as MoodBox).transform.position - cameraPos).sqrMagnitude < minDistance) {
							minDistance = ((b as MoodBox).transform.position - cameraPos).sqrMagnitude;
							minMoodBox = b as MoodBox;
						}
					}
					current = minMoodBox;
				}
				else
					Debug.Log ("no MoodBox components found ...");

				applyNearestMoodBox = false;	
			}	
		}

}
}