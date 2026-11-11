using UnityEditor;
using UnityEditor.Macros;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {
	
	[CustomEditor(typeof(SKinColoredMasked))] 

	public class SKinColoredMaskedEditorPDM : Editor {

	void Awake()
	{
			script = (SKinColoredMasked)target;

		if(script.emitter !=null){
			script.mesh = script.emitter.GetComponent<SkinnedMeshRenderer>();
			
			script.simple_mesh =  script.emitter.GetComponent<MeshFilter>();
		}
		
		script.animated_mesh = new Mesh();
			script.animated_mesh.hideFlags = HideFlags.HideAndDontSave;

			if(script.p11!=null){
		the_mesh = new SerializedObject(script.gameObject.GetComponent<ParticleSystem>());
			}else{
				Debug.Log ("Please attach the script to a particle system");
			}
		
		if(script.mesh!=null){
			script.mesh.BakeMesh(script.animated_mesh);
			
				if(the_mesh!=null){
			the_mesh.FindProperty("ShapeModule.m_Mesh").objectReferenceValue = script.animated_mesh;
			the_mesh.ApplyModifiedProperties();
				}
		}
		
		
		if(script.simple_mesh!=null){
			
				if(the_mesh!=null){
			if(Application.isPlaying){
					the_mesh.FindProperty("ShapeModule.m_Mesh").objectReferenceValue = script.simple_mesh.mesh;
			}else{
				the_mesh.FindProperty("ShapeModule.m_Mesh").objectReferenceValue = script.simple_mesh.sharedMesh;
			}
			
			
			the_mesh.ApplyModifiedProperties();
				}
		}

	}

		private SKinColoredMasked script;



	private void SceneGUI(SceneView sceneview)
	{


	}

	SerializedObject the_mesh ;

	public void OnEnable(){


	
	}

	public void  OnSceneGUI () {




	

	}

	

}
}