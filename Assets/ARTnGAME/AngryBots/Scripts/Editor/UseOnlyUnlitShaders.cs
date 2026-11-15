using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Artngame.PDM{
	//[AddComponentMenu
	//@MenuItem ("Tools/Use Only Unlit Shaders")

public class UseOnlyUnlitShaders : MonoBehaviour {

		[MenuItem("Tools/Use Only Unlit Shader(s)")]
		static void SampleAnimation () {
			Renderer[] renderers = FindObjectsOfType (typeof(Renderer)) as Renderer[];
			foreach (Renderer renderer in renderers) {//for (var renderer : Renderer in renderers) {
				renderer.sharedMaterial.shader = Shader.Find( "Unlit/Texture" );
			}
		}
}
}