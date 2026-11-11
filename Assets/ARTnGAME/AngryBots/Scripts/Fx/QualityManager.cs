using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
	[RequireComponent(typeof(Camera))]
	[RequireComponent(typeof(ShaderDatabase))]
public class QualityManager : MonoBehaviour {


		// Quality enum values will be used directly for shader LOD settings

		public enum Quality {
			Lowest = 100,
			Poor = 190,
			Low = 200,
			Medium = 210,
			High = 300,
			Highest = 500,
		}

		public bool autoChoseQualityOnStart = true;
		public Quality currentQuality = Quality.Highest;

		public MobileBloom bloom ;
		public HeightDepthOfField depthOfField ;
		public ColoredNoise noise ;
		public RenderFogPlane heightFog ;
		public MonoBehaviour reflection ;
		public ShaderDatabase shaders ;
		public GameObject heightFogBeforeTransparentGO ;

		public static Quality quality = Quality.Highest;

		void Start () {
			if (heightFogBeforeTransparentGO != null)
				heightFogBeforeTransparentGO.SetActive(true);

			if (!bloom)
				bloom = GetComponent<MobileBloom> ();
			if (!noise)
				noise = GetComponent<ColoredNoise> ();
			if (!depthOfField)
				depthOfField = GetComponent<HeightDepthOfField> ();
			if (!heightFog)
				heightFog = gameObject.GetComponentInChildren<RenderFogPlane> ();
			if (!shaders)
				shaders = GetComponent<ShaderDatabase> ();
			if (!reflection)
				reflection = GetComponent ("ReflectionFx") as MonoBehaviour;

			if (autoChoseQualityOnStart)
				AutoDetectQuality ();

			ApplyAndSetQuality (currentQuality);
		}

		// we support dynamic quality adjustments if in edit mode

		#if UNITY_EDITOR

		void Update () {
			Quality newQuality = currentQuality;
			if (newQuality != quality)
				ApplyAndSetQuality (newQuality);
		}

		#endif

		private void AutoDetectQuality ()
		// Some special quality settings cases for various platforms
		{
			#if UNITY_IPHONE

			switch (iPhone.generation)
			{
			case iPhoneGeneration.iPad1Gen:
			currentQuality = Quality.Low;
			break;
			case iPhoneGeneration.iPad2Gen:
			currentQuality = Quality.High;
			break;
			case iPhoneGeneration.iPhone3GS:
			case iPhoneGeneration.iPodTouch3Gen:
			currentQuality = Quality.Low;
			break;
			default:
			currentQuality = Quality.Medium;
			break;
			}

			#elif UNITY_ANDROID || UNITY_WP8

			currentQuality = Quality.Low;

			#elif UNITY_BLACKBERRY

			currentQuality = Quality.Medium;

			#else
			// Desktops/consoles

			switch (Application.platform)
			{
			//v2.1
			//			case RuntimePlatform.NaCl:
			//				currentQuality = Quality.Highest;
			//			break;
			//			case RuntimePlatform.FlashPlayer:
			//				currentQuality = Quality.Low;
			//			break;
			default:
				//currentQuality = SystemInfo.graphicsPixelFillrate < 2800 ? Quality.High : Quality.Highest;
				currentQuality = Quality.Highest; //v2.1
				break;
			}

			#endif

			//Debug.Log (String.Format (
			Debug.Log (string.Format (
				"AngryBots: Quality set to '{0}'{1}",
				currentQuality,
				#if UNITY_IPHONE
				" (" + iPhone.generation + " class iOS)"
				#elif UNITY_ANDROID
				" (Android)"
				#elif UNITY_WP8
				" (Windows Phone)"
				#elif UNITY_BLACKBERRY
				" (Blackberry)"
				#else
				" (" + Application.platform + ")"
				#endif
			));
		}

		private void ApplyAndSetQuality (Quality newQuality) {
			quality = newQuality;

			// default states

			GetComponent<Camera>().cullingMask = -1 & ~(1 << LayerMask.NameToLayer ("Adventure"));
			GameObject textAdventure = GameObject.Find ("TextAdventure");
			if (textAdventure)
				textAdventure.GetComponent<TextAdventureManager> ().enabled = false;

			// check for quality specific states

			if (quality == Quality.Lowest) {
				DisableAllFx ();
				if (textAdventure)
					textAdventure.GetComponent<TextAdventureManager> ().enabled = true;
				GetComponent<Camera>().cullingMask = 1 << LayerMask.NameToLayer ("Adventure");
				EnableFx (depthOfField, false);
				EnableFx (heightFog, false);
				EnableFx (bloom, false);
				EnableFx (noise, false);
				GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
			}
			else if (quality == Quality.Poor) {
				EnableFx (depthOfField, false);
				EnableFx (heightFog, false);
				EnableFx (bloom, false);
				EnableFx (noise, false);
				EnableFx (reflection, false);
				GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
			}
			else if (quality == Quality.Low) {
				EnableFx (depthOfField, false);
				EnableFx (heightFog, false);
				EnableFx (bloom, false);
				EnableFx (noise, false);
				EnableFx (reflection, true);
				GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
			}
			else if (quality == Quality.Medium) {
				EnableFx (depthOfField, false);
				EnableFx (heightFog, false);
				EnableFx (bloom, true);
				EnableFx (noise, false);
				EnableFx (reflection, true);
				GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
			}
			else if (quality == Quality.High) {
				EnableFx (depthOfField, false);
				EnableFx (heightFog, false);
				EnableFx (bloom, true);
				EnableFx (noise, true);
				EnableFx (reflection, true);
				GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
			}
			else { // Highest
				EnableFx (depthOfField, true);
				EnableFx (heightFog, true);
				EnableFx (bloom, true);
				EnableFx (reflection,true);
				EnableFx (noise, true);
				if ((heightFog && heightFog.enabled) || (depthOfField && depthOfField.enabled))
					GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
			}

			Debug.Log ("AngryBots: setting shader LOD to " + quality);

			Shader.globalMaximumLOD = (int)quality; //IEnumerable.getc // quality; //v2.3
			foreach (Shader s in shaders.shaders) {//for (var s : Shader in shaders.shaders) {
				s.maximumLOD = (int)quality;//v2.3
			}
		}

		private void DisableAllFx () {
			GetComponent<Camera>().depthTextureMode = DepthTextureMode.None;
			EnableFx (reflection, false);
			EnableFx (depthOfField, false);
			EnableFx (heightFog, false);
			EnableFx (bloom, false);
			EnableFx (noise, false);
		}

		private void EnableFx (MonoBehaviour fx, bool enable) {
			if (fx)
				fx.enabled = enable;
		}

}
}