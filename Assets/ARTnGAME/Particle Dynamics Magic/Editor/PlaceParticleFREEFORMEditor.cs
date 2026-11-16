using UnityEditor;
using UnityEditor.Macros;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {
	
[CustomEditor(typeof(PlaceParticleFREEFORM))] 

public class PlaceParticleFREEFORMEditor : Editor {

	void Awake()
	{
		script = (PlaceParticleFREEFORM)target;

	}

	private PlaceParticleFREEFORM script;



	private void SceneGUI(SceneView sceneview)
	{

	}

	public void OnEnable(){

	
	}

	public void  OnSceneGUI () {

		Handles.color = Color.red;
		Event cur = Event.current;
		
		if(cur.type == EventType.MouseDown && cur.button == 1)
		{

			Ray ray = HandleUtility.GUIPointToWorldRay(cur.mousePosition);
			
			RaycastHit hit = new RaycastHit();
			if (Physics.Raycast(ray, out hit, Mathf.Infinity))
			
			{
				if(hit.collider.gameObject.tag == "PPaint"){
					Undo.RecordObject(script,"undo paint");

					if(script.Emitter_objects!=null){

						if(!script.Erase_mode){
								if(script.Emitter_objects.Count > (script.p11.main.maxParticles/2)){//v2.1
								//do nothing
							}else{
								script.Emitter_objects.Add(hit.collider.gameObject);
								script.Registered_paint_positions.Add(hit.point);
								script.Registered_initial_positions.Add(hit.collider.gameObject.transform.position);
								script.Registered_initial_scale.Add (hit.collider.gameObject.transform.localScale);
								script.Registered_initial_rotation.Add(hit.collider.gameObject.transform.eulerAngles);
							}
						}else if(script.Erase_mode){


							for (int i=0;i< script.Registered_paint_positions.Count;i++){

								if( Vector3.Distance(hit.point,script.Registered_paint_positions[i]) < (0.5f* script.brush_size))
								{
									script.Emitter_objects.RemoveAt(i);
									script.Registered_paint_positions.RemoveAt(i);
									script.Registered_initial_positions.RemoveAt(i);
									script.Registered_initial_rotation.RemoveAt(i);
									script.Registered_initial_scale.RemoveAt(i);
									break;
								}

							}

						}

					}
				}
			}

				script.maxemitter_count = ((int)script.p11.main.maxParticles/2)+1;//v2.1
			if(script.Emitter_objects!=null){
				script.current_emitters_count = script.Emitter_objects.Count;
			}

		}


		if (script.Registered_paint_positions !=null){
			if (script.Registered_paint_positions.Count >0){
				for (int i =0;i<script.Registered_paint_positions.Count;i++){


					Vector3 FIND_moved_toZERO = (script.Registered_paint_positions[i]-script.Emitter_objects[i].gameObject.transform.position) -(script.Registered_initial_positions[i] - script.Emitter_objects[i].gameObject.transform.position);;
					
					Vector3 FIND_rotated = Quaternion.Euler( -script.Registered_initial_rotation[i]+script.Emitter_objects[i].gameObject.transform.eulerAngles)*(FIND_moved_toZERO);

					Vector3 FIND_scaled = new Vector3(  FIND_rotated.x*(script.Emitter_objects[i].gameObject.transform.localScale.x / script.Registered_initial_scale[i].x),FIND_rotated.y*(script.Emitter_objects[i].gameObject.transform.localScale.y / script.Registered_initial_scale[i].y),FIND_rotated.z*(script.Emitter_objects[i].gameObject.transform.localScale.z / script.Registered_initial_scale[i].z)  );


					Vector3 FIND_re_translated = FIND_scaled+script.Emitter_objects[i].gameObject.transform.position;

					Vector3 FIND_moved_pos = FIND_re_translated;

					//Handles.SphereCap(i,FIND_moved_pos,Quaternion.identity,script.Marker_size);
					Handles.SphereHandleCap(i,FIND_moved_pos,Quaternion.identity,script.Marker_size,EventType.ContextClick); //v2.3


					

					script.Registered_paint_positions[i] = FIND_moved_pos;

					script.Registered_initial_positions[i]=(script.Emitter_objects[i].gameObject.transform.position);
					
					script.Registered_initial_rotation[i]=(script.Emitter_objects[i].gameObject.transform.eulerAngles);

					script.Registered_initial_scale[i]=(script.Emitter_objects[i].gameObject.transform.localScale);

				}
			}
		}

	}
	
}

}