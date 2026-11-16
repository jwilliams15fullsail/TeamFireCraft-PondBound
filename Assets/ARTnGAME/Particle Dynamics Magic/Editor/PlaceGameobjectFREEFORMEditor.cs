using UnityEditor;
using UnityEditor.Macros;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {
	
[CustomEditor(typeof(PlaceGameobjectFREEFORM))] 

	public class PlaceGameobjectFREEFORMEditor : Editor {

	void Awake()
	{
			script = (PlaceGameobjectFREEFORM)target;

			if(script.Registered_paint_positions==null){
				script.Registered_paint_positions = new List<Vector3>();
			}
			if(script.Registered_paint_rotations==null){
				script.Registered_paint_rotations = new List<Vector3>();
			}
			if(script.Updated_Registered_paint_positions==null){
				script.Updated_Registered_paint_positions = new List<Vector3>();
			}

	}

		private PlaceGameobjectFREEFORM script;



	private void SceneGUI(SceneView sceneview)
	{

	}

	public void OnEnable(){

	
	}

		Vector3 Keep_last_mouse_pos;

	public void  OnSceneGUI () {

		Handles.color = Color.red;
		Event cur = Event.current;
		
	  if(!script.Use_stencil){
		if( (cur.type == EventType.MouseDrag && cur.button == 1  &  Vector3.Distance(Keep_last_mouse_pos,cur.mousePosition)>8)  | (cur.type == EventType.MouseDown && cur.button == 1)  ) 
		{

			Ray ray = HandleUtility.GUIPointToWorldRay(cur.mousePosition);

					Keep_last_mouse_pos = cur.mousePosition;
			
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
									script.Updated_Registered_paint_positions.Add(hit.point);

									script.Registered_paint_rotations.Add(hit.normal);
									script.Registered_initial_normal_rotation.Add(hit.normal);

								script.Registered_initial_positions.Add(hit.collider.gameObject.transform.position);
								script.Registered_initial_scale.Add (hit.collider.gameObject.transform.localScale);
//								script.Registered_initial_rotation.Add(hit.collider.gameObject.transform.eulerAngles);
									script.Registered_initial_rotation.Add(hit.collider.gameObject.transform.rotation);

									Debug.Log ("Editor add point");


										if(script.Gameobj_instances.Count < (script.particle_count)){
											
											
											GameObject TEMP = Instantiate(script.Gameobj,script.Registered_paint_positions[script.Registered_paint_positions.Count-1],Quaternion.identity)as GameObject;
											
											Undo.RegisterCreatedObjectUndo(TEMP,"destroy spheres");
											
											if(TEMP.GetComponent<Collider>()!=null){
												if(script.Remove_colliders ){
													TEMP.GetComponent<Collider>().enabled=false;
												}else{TEMP.GetComponent<Collider>().enabled=true;}
											}
											
											script.Gameobj_instances.Add(TEMP);
											TEMP.transform.position = script.Registered_paint_positions[script.Registered_paint_positions.Count-1];
											
											if(script.Angled){
												
												TEMP.transform.localEulerAngles = script.Registered_paint_rotations[script.Registered_paint_positions.Count-1];
												
											}
											
											TEMP.transform.parent = script.Parent_OBJ.transform;
										}

									}
								}else if(script.Erase_mode){


								for (int i=0;i< script.Updated_Registered_paint_positions.Count;i++){

									if( Vector3.Distance(hit.point,script.Updated_Registered_paint_positions[i]) < (0.5f* script.brush_size))
								{
									script.Emitter_objects.RemoveAt(i);
									script.Registered_paint_positions.RemoveAt(i);
										script.Updated_Registered_paint_positions.RemoveAt(i);

										script.Registered_paint_rotations.RemoveAt(i);

										
										DestroyImmediate(script.Gameobj_instances[i]);
										script.Gameobj_instances.RemoveAt(i);
										
										


									script.Registered_initial_positions.RemoveAt(i);
										script.Registered_initial_rotation.RemoveAt(i); script.Registered_initial_normal_rotation.RemoveAt(i);
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
		}

		if(script.Use_stencil){
				//if(cur.type == EventType.MouseDown && cur.button == 1)
				{
					if(cur.type == EventType.MouseDown && cur.button == 1)
					{
						Undo.RegisterCompleteObjectUndo(script,"undo paint");
					}


					int counter=0;

				  for( int j=0; j<script.Stencil.width ;j=j+8){
						for( int k=0; k<script.Stencil.height ;k=k+8){


							Color tex_col = script.Stencil.GetPixel(j,k);

						if(1==0 | tex_col.a > 0.9f){



								Ray ray = HandleUtility.GUIPointToWorldRay(
									cur.mousePosition 
									- new Vector2(script.tex_scale.x*script.Stencil.width/2,script.tex_scale.y*script.Stencil.height/2) 
									+ new Vector2(script.tex_scale.x*j,script.tex_scale.y*k) 
									);

						
						
						RaycastHit hit = new RaycastHit();
						if (Physics.Raycast(ray, out hit, Mathf.Infinity))
							
						{
							if(hit.collider.gameObject.tag == "PPaint"){
								
								
								if(script.Emitter_objects!=null){
									
									if(!script.Erase_mode){

												if(script.Emitter_objects.Count > (script.p11.main.maxParticles/2)){//v2.1
											//do nothing
										}else{

													Handles.color = Color.red;
													//Handles.SphereCap(counter,hit.point,Quaternion.identity,script.Marker_size*5);
													Handles.SphereHandleCap(counter,hit.point,Quaternion.identity,script.Marker_size*5,EventType.ContextClick); //v2.3

											if(cur.type == EventType.MouseDown && cur.button == 1)
											{
												

											script.Emitter_objects.Add(hit.collider.gameObject);
											script.Registered_paint_positions.Add(hit.point);
											script.Updated_Registered_paint_positions.Add(hit.point);
											
											script.Registered_paint_rotations.Add(hit.normal);
											script.Registered_initial_normal_rotation.Add(hit.normal);
											
											script.Registered_initial_positions.Add(hit.collider.gameObject.transform.position);
											script.Registered_initial_scale.Add (hit.collider.gameObject.transform.localScale);
											script.Registered_initial_rotation.Add(hit.collider.gameObject.transform.rotation);
													
											


													
													if(script.Gameobj_instances.Count < (script.particle_count)){


														GameObject TEMP = Instantiate(script.Gameobj,script.Registered_paint_positions[script.Registered_paint_positions.Count-1],Quaternion.identity)as GameObject;

														Undo.RegisterCreatedObjectUndo(TEMP,"destroy spheres");

														if(TEMP.GetComponent<Collider>()!=null){
															if(script.Remove_colliders ){
																TEMP.GetComponent<Collider>().enabled=false;
															}else{TEMP.GetComponent<Collider>().enabled=true;}
														}
														
														script.Gameobj_instances.Add(TEMP);
														TEMP.transform.position = script.Registered_paint_positions[script.Registered_paint_positions.Count-1];
														
														if(script.Angled){
															
															TEMP.transform.localEulerAngles = script.Registered_paint_rotations[script.Registered_paint_positions.Count-1];
															
														}

														if(script.Scale_by_texture){

															
															TEMP.transform.localScale = new Vector3(600*tex_col.b/255,600*tex_col.b/255,600*tex_col.b/255);


														}
														if(script.Color_by_texture){
															
															Renderer[] renderer = TEMP.GetComponentsInChildren< Renderer >();

															if(!Application.isPlaying){
															renderer[0].sharedMaterial.color = Color.Lerp(renderer[0].sharedMaterial.color, tex_col,0.5f);
															renderer[1].sharedMaterial.color = Color.Lerp(renderer[0].sharedMaterial.color, tex_col,0.5f);
															}else{
																	renderer[0].material.color = Color.Lerp(renderer[0].material.color, tex_col,script.Coloration_ammount);
																	renderer[1].material.color = Color.Lerp(renderer[0].material.color, tex_col,script.Coloration_ammount);

															}
														}
														
														TEMP.transform.parent = script.Parent_OBJ.transform;
													}
													
												}
											}
									}
									else if(script.Erase_mode){
										
										
										for (int i=0;i< script.Updated_Registered_paint_positions.Count;i++){
											
											if( Vector3.Distance(hit.point,script.Updated_Registered_paint_positions[i]) < (0.5f* script.brush_size))
											{
												script.Emitter_objects.RemoveAt(i);
												script.Registered_paint_positions.RemoveAt(i);
												script.Updated_Registered_paint_positions.RemoveAt(i);
												
												script.Registered_paint_rotations.RemoveAt(i);
												
												DestroyImmediate(script.Gameobj_instances[i]);
												script.Gameobj_instances.RemoveAt(i);
																									
												
												script.Registered_initial_positions.RemoveAt(i);
												script.Registered_initial_rotation.RemoveAt(i); script.Registered_initial_normal_rotation.RemoveAt(i);
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
							counter=counter+1;
					}
					
				}
			}
		}


		if (script.Registered_paint_positions !=null & 1==1){
			if (script.Registered_paint_positions.Count >0){
				for (int i =0;i<script.Registered_paint_positions.Count;i++){
										
						Vector3 FIND_moved_toZERO = script.Registered_paint_positions[i] +script.Emitter_objects[i].gameObject.transform.position - script.Registered_initial_positions[i]
						-script.Emitter_objects[i].gameObject.transform.position ;
						Vector3 FIXED_ROT = script.Emitter_objects[i].gameObject.transform.eulerAngles;
						Vector3 FIND_rotated = Quaternion.Euler( -script.Registered_initial_rotation[i].eulerAngles+FIXED_ROT)*(FIND_moved_toZERO);
						Vector3 FIND_scaled = new Vector3(FIND_rotated.x*(script.Emitter_objects[i].gameObject.transform.localScale.x / script.Registered_initial_scale[i].x),
						                                  FIND_rotated.y*(script.Emitter_objects[i].gameObject.transform.localScale.y / script.Registered_initial_scale[i].y),
						                                  FIND_rotated.z*(script.Emitter_objects[i].gameObject.transform.localScale.z / script.Registered_initial_scale[i].z)  );
						
						Vector3 FIND_re_translated = FIND_scaled+script.Emitter_objects[i].gameObject.transform.position;
						//Vector3 FIND_moved_pos = FIND_re_translated;
						
						//Vector3 FIND_moved_normal_toZERO = script.Registered_initial_normal_rotation[i];
										
						if(i < script.Gameobj_instances.Count){
										
						}
								

						Handles.color = Color.blue;
						//Handles.SphereCap(i,FIND_re_translated,Quaternion.identity,script.Marker_size);
						Handles.SphereHandleCap(i,FIND_re_translated,Quaternion.identity,script.Marker_size,EventType.ContextClick); //v2.3

		
				}
			}
		}

	}
	
}

}