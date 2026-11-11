using UnityEditor;
using UnityEditor.Macros;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace Artngame.PDM {

	[CustomEditor(typeof(SplinerP))] 

	public class SplinePInfiniLANDEditor : Editor {

		void Awake()
		{
			script = (SplinerP)target;

			Editor_Initialized=0;
		}

		#region PARAMETERS
		List<LandSplineNode[]> Keep_Curve;
		//LandSplineNode[] Curve; //v2.1
		bool replot =true;
		private SplinerP script;

		int selectedSegment=-1;
		int Editor_Initialized=0;
		int Selected_items_count;

		#endregion


		private void OnScene(SceneView sceneview)
		{
			if(script!=null){
				if (script.Always_on){

					if(script.gameObject!=null){
						if(script.gameObject.activeInHierarchy){ 
							OnSceneGUI();
						}
					}

				}
			}

			if(script!=null & 1==1){
				if(script.control_points_children != null & script.SplinePoints != null   & Editor_Initialized==4)
				{


					if(script.control_points_children.Count == script.SplinePoints.Count | 1==1)
					{


						for(int i=script.control_points_children.Count-1;i>=2;i--){

							if(script.control_points_children[i] ==null){

								Object[] AA0 = new Object[1]; AA0[0] = script.gameObject;
								Object[] AA1 = EditorUtility.CollectDeepHierarchy(AA0);


								if(i>1){

									Undo.RecordObjects(AA1,"Restore point");


									if(i>0){
										script.Overide_seg_detail.RemoveAt(i-1);
									}



									script.SplinePoints.RemoveAt(i);
									script.control_points_children.RemoveAt(i);
									replot=true;
								}


								Debug.Log (i); Repaint ();
							}

						}

						if(script.control_points_children.Count>0){
							if(script.control_points_children[0] ==null){

								GameObject SPHERE_SMALL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
								Undo.RegisterCreatedObjectUndo(SPHERE_SMALL, "create " + SPHERE_SMALL.name);

								script.control_points_children[0] = SPHERE_SMALL;
								script.control_points_children[0].transform.parent = script.transform;
								script.control_points_children[0].transform.position = script.SplinePoints[0].position ;
								script.control_points_children[0].transform.localScale = (1/script.gameObject.transform.localScale.x)*0.3f*new Vector3(script.Handle_scale,script.Handle_scale,script.Handle_scale);
							}
							if(script.control_points_children[1] ==null){
								Debug.Log ("RESTORE 2");

								GameObject SPHERE_SMALL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
								Undo.RegisterCreatedObjectUndo(SPHERE_SMALL, "create " + SPHERE_SMALL.name);

								script.control_points_children[1] = SPHERE_SMALL;
								script.control_points_children[1].transform.parent = script.transform;
								script.control_points_children[1].transform.position = script.SplinePoints[1].position ;
								script.control_points_children[1].transform.localScale = (1/script.gameObject.transform.localScale.x)*0.3f*new Vector3(script.Handle_scale,script.Handle_scale,script.Handle_scale);
							}
						}



						if(script.SplinePoints.Count < 3){  

							if(script.Overide_seg_detail.Count > 2){
								script.Overide_seg_detail.RemoveAt(script.Overide_seg_detail.Count-1);
								script.Overide_seg_detail.RemoveAt(script.Overide_seg_detail.Count-1);
							}

						}



					}


					if(script.control_points_children != null & script.SplinePoints!=null )
					{
						if(script.SplinePoints.Count <3 | script.control_points_children.Count < 3){return;}
					}


					/////
				}
			}



		}

		public void Update()
		{

		}

		public void OnDestroy()
		{

		}

		public static void Init()
		{

		}

		public void OnInspectorUpdate()
		{

		}

		public void OnGUI()
		{

		}



		public void OnEnable(){

			//SceneView.onSceneGUIDelegate -= OnScene;
			//SceneView.onSceneGUIDelegate += OnScene;
            SceneView.duringSceneGui -= OnScene;
            SceneView.duringSceneGui += OnScene;

            Multithreaded = serializedObject.FindProperty ("Multithreaded");
			DefineRotations= serializedObject.FindProperty ("DefineRotations");


			Overide_seg_detail= serializedObject.FindProperty ("Overide_seg_detail");
			SplinePoints= serializedObject.FindProperty ("SplinePoints");

			Add_mid_point= serializedObject.FindProperty ("Add_mid_point");
			Add_in_Segment= serializedObject.FindProperty ("Add_in_Segment");
			Node_toggle_dist = serializedObject.FindProperty ("Node_toggle_dist"); 

			Handle_scale = serializedObject.FindProperty ("Handle_scale");

			CurveQuality= serializedObject.FindProperty ("CurveQuality");
			follow_curve= serializedObject.FindProperty ("follow_curve");
			parent_moving= serializedObject.FindProperty ("parent_moving");
			Motion_Speed= serializedObject.FindProperty ("Motion_Speed");

			show_controls_play_mode= serializedObject.FindProperty ("show_controls_play_mode");
			Game_Mode_On= serializedObject.FindProperty ("Game_Mode_On");
			Alter_mode= serializedObject.FindProperty ("Alter_mode");
			Always_on= serializedObject.FindProperty ("Always_on");
			show_controls_edit_mode= serializedObject.FindProperty ("show_controls_edit_mode");
			Alter_mode2= serializedObject.FindProperty ("Alter_mode2");
			Accelerate= serializedObject.FindProperty ("Accelerate");
			Add_point= serializedObject.FindProperty ("Add_point");
			With_manip= serializedObject.FindProperty ("With_manip");

		}

		public void  OnSceneGUI () {

			#region Curve Init
			if (Event.current.commandName == "UndoRedoPerformed") {
				replot=true;
				return;
			}




			if(script!=null){

				////AAAA
				if(script.Overide_seg_detail.Count < script.SplinePoints.Count-1){
					script.Overide_seg_detail = new List<int>(); 
					for (int i=0;i<script.SplinePoints.Count-1;i++) 
					{

						script.Overide_seg_detail.Add(0);

					}

				}



				if (Event.current.type == EventType.MouseDrag)
				{
					//Undo.RecordObject(script,"Spline Node Move");

					if(script.control_points_children.Count > 0){

						Object[] AA0 = new Object[1]; AA0[0] = script.gameObject;
						Object[] AA1 = EditorUtility.CollectDeepHierarchy(AA0);




						Undo.RecordObjects(AA1,"Undo Move");


						replot=true;




						Repaint ();
					}
				}


				if(script.control_points_children != null & script.SplinePoints!=null & Editor_Initialized==4)

				{



					if(script.control_points_children.Count == script.SplinePoints.Count | 1==1)
					{


						for(int i=script.control_points_children.Count-1;i>=2;i--){

							if(script.control_points_children[i] ==null){

								Object[] AA0 = new Object[1]; AA0[0] = script.gameObject;
								Object[] AA1 = EditorUtility.CollectDeepHierarchy(AA0);


								if(i>1){

									Undo.RecordObjects(AA1,"Restore point");


									if(i>0){
										script.Overide_seg_detail.RemoveAt(i-1);
									}

									script.SplinePoints.RemoveAt(i);
									script.control_points_children.RemoveAt(i);
									replot=true;
								}


								Debug.Log (i); Repaint ();
							}

						}

						if(script.control_points_children.Count>0){
							if(script.control_points_children[0] ==null){

								GameObject SPHERE_SMALL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
								Undo.RegisterCreatedObjectUndo(SPHERE_SMALL, "create " + SPHERE_SMALL.name);

								script.control_points_children[0] = SPHERE_SMALL;
								script.control_points_children[0].transform.parent = script.transform;
								script.control_points_children[0].transform.position = script.SplinePoints[0].position ;
								script.control_points_children[0].transform.localScale = (1/script.gameObject.transform.localScale.x)*0.3f*new Vector3(script.Handle_scale,script.Handle_scale,script.Handle_scale);
							}
							if(script.control_points_children[1] ==null){
								Debug.Log ("RESTORE 2");

								GameObject SPHERE_SMALL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
								Undo.RegisterCreatedObjectUndo(SPHERE_SMALL, "create " + SPHERE_SMALL.name);

								script.control_points_children[1] = SPHERE_SMALL;
								script.control_points_children[1].transform.parent = script.transform;
								script.control_points_children[1].transform.position = script.SplinePoints[1].position ;
								script.control_points_children[1].transform.localScale = (1/script.gameObject.transform.localScale.x)*0.3f*new Vector3(script.Handle_scale,script.Handle_scale,script.Handle_scale);
							}
						}
					}


					if(script.control_points_children != null & script.SplinePoints!=null )
					{
						if(script.SplinePoints.Count <3 | script.control_points_children.Count < 3){return;}
					}


				}

				if(script.control_points_children != null & script.SplinePoints!=null )
				{
					if(script.control_points_children.Count != script.SplinePoints.Count){

						replot=true;

					}
				}



				if(Editor_Initialized<4){
					Editor_Initialized=Editor_Initialized+1;
				}

			}


			if (Keep_Curve == null | replot ){Keep_Curve = new List<LandSplineNode[]>();}
			Event _Event = Event.current;
			EventType _Type = _Event.type;
			LandSplineNode[]  SplinePoints = script.SplinePoints.ToArray();

			if(SplinePoints.Length <3){return;}

			int Control_points_Count = SplinePoints.Length;
			#endregion



			//scale
			#region Curve Scale
			if(script.Keep_last_scale == 0)
			{script.Keep_last_scale=1;}
			if(script.Keep_last_scale != script.Scale_curve){ 
				for (int i=0;i<Control_points_Count;i++) {

					Vector3 Move_to_0 = script.SplinePoints[i].position - script.gameObject.transform.position;
					script.SplinePoints[i].position = script.Scale_curve * Move_to_0 + script.gameObject.transform.position;

				}

				script.Scale_curve = script.Keep_last_scale ;
			}

			ControlMe(SplinePoints, Control_points_Count);
			#endregion

			#region Curve full
			Handles.color = Color.red;

			if ((script.Multithreaded && script.Thead_finished) | !script.Multithreaded) {
				script.Curve.Clear();
			}

			//v2.1
			if(script.Keep_Curve != null){
				script.Keep_Curve.Clear();
			}

			for (int i=0;i<Control_points_Count;i++) {
				Handles.color = Color.cyan;

				Draw_Curve(i,SplinePoints, Control_points_Count);

				#region Spheres
				Draw_Spheres(i, SplinePoints, Control_points_Count);
				#endregion
			}

			if (replot == true){replot = false;}

			#endregion

			#region Curve Handle
			//TRANSLATE
			if(script.Keep_last_pos != script.gameObject.transform.position | 1==0){ 

				for (int i=0;i<script.SplinePoints.Count;i++) 
				{
					Vector3 Distance = script.gameObject.transform.position-script.Keep_last_pos;
					script.SplinePoints[i].position = script.SplinePoints[i].position + Distance;

				}
				SplinePoints =  script.SplinePoints.ToArray();
				script.Keep_last_pos = script.gameObject.transform.position;

				replot=true;
			}
			else
			{}

			//ROTATE
			if(script.Keep_last_rot != script.gameObject.transform.eulerAngles){ 

				for (int i=0;i<script.SplinePoints.Count;i++) 
				{
					Vector3 Distance = script.gameObject.transform.localEulerAngles-script.Keep_last_rot;

					Vector3 Move_to_0 = script.SplinePoints[i].position - script.gameObject.transform.position;
					script.SplinePoints[i].position = Quaternion.AngleAxis(Distance.y, new Vector3(0,1,0) ) * Move_to_0 + script.gameObject.transform.position;

					Move_to_0 = script.SplinePoints[i].position - script.gameObject.transform.position;
					script.SplinePoints[i].position = Quaternion.AngleAxis(Distance.z, new Vector3(0,0,1) ) * Move_to_0 + script.gameObject.transform.position;

					Move_to_0 = script.SplinePoints[i].position - script.gameObject.transform.position;
					script.SplinePoints[i].position = Quaternion.AngleAxis(Distance.x, new Vector3(1,0,0) ) * Move_to_0 + script.gameObject.transform.position;

				}
				SplinePoints =  script.SplinePoints.ToArray();
				script.Keep_last_rot = script.gameObject.transform.eulerAngles;

				replot=true;
			}



			if (_Type == EventType.MouseDown)
			{

			}

			script.last_object_position_inspector_saw = script.transform.position;

			script.last_object_rotation_inspector_saw = script.transform.eulerAngles;


			Selected_items_count = Selection.gameObjects.Length;


			//add spheres in control points

			if(script.control_points_children !=null){
				if(script.control_points_children.Count == 0)
				{
					for (int i=0;i<script.SplinePoints.Count;i++){

						GameObject SPHERE_SMALL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
						Undo.RegisterCreatedObjectUndo(SPHERE_SMALL, "create " + SPHERE_SMALL.name);

						script.control_points_children.Add(SPHERE_SMALL);

						script.control_points_children[i].transform.parent = script.transform;
						script.control_points_children[i].transform.position = script.SplinePoints[i].position ;
						script.control_points_children[i].transform.localScale = (1/script.gameObject.transform.localScale.x)*0.3f*new Vector3(script.Handle_scale,script.Handle_scale,script.Handle_scale);
					}
				}
			}

			if(script.control_points_children !=null){
				if(script.control_points_children.Count !=0){

					//v2.1
					//bool one_changed = false;
					for (int i=0;i<script.SplinePoints.Count;i++){
						if(script.control_points_children[i] !=null){

							//v2.1
							if(script.SplinePoints[i].position == script.control_points_children[i].transform.position)
							{
								//do nothing
							}else{
								SplinePoints[i].position = script.control_points_children[i].transform.position;
								//one_changed = true;
							}

							if(script.control_points_children[i].name != "Sphere"+i){
								script.control_points_children[i].name = "Sphere"+i;
							}


						}
					}
					//if(one_changed){//v2.1
					replot=true;
					//Debug.Log("d");
					//}
				}
			}
			#endregion

			#region ENABLE/DISABLE spheres

			if(!script.show_controls_edit_mode & script.NODES_DISABLED ==0){
				for(int i=0;i<script.control_points_children.Count;i++){
					MeshRenderer TEMP = script.control_points_children[i].GetComponent(typeof(MeshRenderer)) as MeshRenderer;
					TEMP.enabled = false;
				}
				script.NODES_DISABLED=1; 
			}else if(script.show_controls_edit_mode & script.NODES_DISABLED ==1){
				for(int i=0;i<script.control_points_children.Count;i++){
					MeshRenderer TEMP = script.control_points_children[i].GetComponent(typeof(MeshRenderer)) as MeshRenderer;
					TEMP.enabled = true;
				}
				script.NODES_DISABLED=0;
			}


			#endregion
			if (GUI.changed){EditorUtility.SetDirty(script);replot = true;


			}


		}

		void Draw_Spheres(int i, LandSplineNode[]  SplinePoints, int Control_points_Count){
			if (script.PointGizmo_On) {Handles.color = Color.blue;
				//Handles.SphereCap(i,SplinePoints[i].position,Quaternion.identity,script.Handle_scale*1f); 
				Handles.SphereHandleCap(i,SplinePoints[i].position,Quaternion.identity,script.Handle_scale*1f,EventType.ContextClick); //v2.3

				if( script.control_points_children != null){
					if(script.control_points_children.Count !=0){
						if( script.control_points_children[i] !=null){
							script.control_points_children[i].transform.localScale = (1/script.gameObject.transform.localScale.x)*0.3f*new Vector3(script.Handle_scale,script.Handle_scale,script.Handle_scale);
						}
					}
				}
			}
		}

		void Draw_Curve(int i, LandSplineNode[]  SplinePoints, int Control_points_Count){
			if (i<=Control_points_Count-2){ 
				#region PARAMETER IN
				Control_in(i,SplinePoints);
				#endregion
				#region Curve
				if (Keep_Curve.Count<(i+1)|replot){

					//Debug.Log(Keep_Curve.Count + " bb = "+(i+1).ToString());
					//Debug.Log("a="+replot);

					//NEW APPROACH
					if(script.Curve1 != null & script.Keep_Curve != null){
						script.Draw_Curve(i,SplinePoints,Control_points_Count,false);
					}else{
						script.Curve1 = new LandSplineNode[1];	
						script.Keep_Curve = new List<LandSplineNode[]>();
					}
	//				Curve = script.Curve1;
					Keep_Curve = script.Keep_Curve;
					//END NEW APPROACH


					//				float detail = script.CurveQuality;
					//					if(detail < 1){detail=1;}
					//
					//
					//					if(script.Overide_seg_detail[i] > 0){
					//						
					//						detail = script.Overide_seg_detail[i];
					//						
					//					}
					//
					//				Vector3 Handler_start  = SplinePoints[i].position;
					//				Vector3 Handler_end  = SplinePoints[i+1].position;
					//				Vector3 Vector_along_direction=Handler_start-Handler_end;
					//				Vector3 Vector_along_direction_INV=Handler_end-Handler_start;
					//				Vector3 Vector_along_direction_normalized  = (Vector_along_direction).normalized;
					//				
					//				Vector3 Curve_starting_direction = Vector3.zero;
					//				Vector3 Curve_ending_direction  = Vector3.zero;
					//				
					//				if (i==0 | i <= Control_points_Count-3 ) {
					//					if (i==0){
					//						Curve_starting_direction  = Vector_along_direction_INV.normalized;
					//						SplinePoints[i].direction = Curve_starting_direction;
					//					}
					//					else{
					//						Curve_starting_direction = SplinePoints[i].direction;
					//
					////							//v2.1
					////							if(SplinePoints[i+0].Type == 1){
					////								Curve_starting_direction = Curve_starting_direction + SplinePoints[i+0].directionB;
					////							}
					//					}
					//					Curve_ending_direction = -1.0f*((SplinePoints[i+2].position-Handler_end).normalized - Vector_along_direction_normalized).normalized;
					//					SplinePoints[i+1].direction = -1.0f*Curve_ending_direction;
					//					
					//						//v2.1
					//						if(SplinePoints[i+1].Type == 2){
					//							//Curve_starting_direction = Curve_starting_direction + SplinePoints[i+0].directionB;
					//							Curve_ending_direction = Curve_ending_direction - SplinePoints[i+1].directionB;			//USE TO AFFECT BOTH SIDES !!! (type 2)
					//						}
					//						if(SplinePoints[i+1].Type == 1){
					//							//Curve_starting_direction = Curve_starting_direction + SplinePoints[i+0].directionB;
					//							Curve_ending_direction = Curve_ending_direction - SplinePoints[i+1].directionB;			//USE TO AFFECT BOTH SIDES !!! (type 2)
					//						}
					//						if(SplinePoints[i+0].Type == 1 | SplinePoints[i+1].Type == 2){
					//							SplinePoints[i+0].direction = SplinePoints[i+0].direction + SplinePoints[i+0].directionB;
					//							//Curve_ending_direction = Curve_ending_direction +SplinePoints[i+0].directionB;
					//							if(SplinePoints[i+0].directionB.magnitude == 0){
					//								SplinePoints[i+0].directionB = SplinePoints[i+0].direction; 
					//							}
					//						}
					//						if(SplinePoints[i+1].Type == 1){
					//							SplinePoints[i+1].direction = SplinePoints[i+1].direction + SplinePoints[i+1].directionB;
					//							//Curve_ending_direction = Curve_ending_direction +SplinePoints[i+1].directionB;
					//							if(SplinePoints[i+1].directionB.magnitude == 0){
					//								SplinePoints[i+1].directionB = SplinePoints[i+1].direction;
					//							}
					//						}
					//						if(SplinePoints[i+1].Type == 2){
					//							SplinePoints[i+1].direction = SplinePoints[i+1].direction + SplinePoints[i+1].directionC;
					//							//Curve_ending_direction = Curve_ending_direction +SplinePoints[i+1].directionB;
					//							if(SplinePoints[i+1].directionC.magnitude == 0){
					//								SplinePoints[i+1].directionC = SplinePoints[i+1].direction;
					//							}
					//						}
					//						if(SplinePoints[i+2].Type == 1 | SplinePoints[i+1].Type == 2){
					//							SplinePoints[i+2].direction = SplinePoints[i+2].direction + SplinePoints[i+2].directionB;
					//							//Curve_ending_direction = Curve_ending_direction +SplinePoints[i+2].directionB;
					//							if(SplinePoints[i+2].directionB.magnitude == 0){
					//								SplinePoints[i+2].directionB = SplinePoints[i+2].direction;
					//							}
					//						}
					//
					//				} else {
					//					Curve_starting_direction = SplinePoints[i].direction;
					//					Curve_ending_direction = Vector_along_direction.normalized;
					//					SplinePoints[i+1].direction = -1.0f*Curve_ending_direction;
					//
					//					//v2.1
					//					if(SplinePoints[i+0].Type == 1 | SplinePoints[i+0].Type == 2){
					//						Curve_starting_direction = Curve_starting_direction + SplinePoints[i+0].directionB;
					//					}
					//					
					//				} 
					//				
					//					List<LandSplineNode> a = new List<LandSplineNode>();
					//				
					//				
					//				
					//				for (float j = 0;j<1;j+= (1/detail)) {
					//					
					//					Vector3 a1 = Handler_start;
					//					Vector3 b1 = Vector_along_direction.magnitude*Curve_starting_direction/2;
					//					Vector3 c1 = Vector_along_direction.magnitude*Curve_ending_direction/2;
					//					Vector3 d1 = Handler_end;
					//					Vector3 C1 = ( d1 - (3.0f * (c1+d1)) + (3.0f * (b1+a1)) - a1 );
					//					Vector3 C2 = ( (3.0f * (c1+d1)) - (6.0f * (b1+a1)) + (3.0f * a1) );
					//					Vector3 C3 = ( (3.0f * (b1+a1)) - (3.0f * a1) );
					//					Vector3 C4 = ( a1 );
					//					Vector3 p = C1*j*j*j + C2*j*j + C3*j + C4;
					//					
					//						LandSplineNode NEW_POINT = new LandSplineNode(Vector3.zero,Quaternion.identity);
					//					NEW_POINT.position = p;
					//					
					//					a.Add (NEW_POINT);
					//				}
					//				
					//				Curve = a.ToArray();
					//					List<LandSplineNode> Curve_Temp =  new List<LandSplineNode>();
					//				Curve_Temp.AddRange(Curve);
					//				Curve_Temp.Add(SplinePoints[i+1]);
					//				Curve = Curve_Temp.ToArray();
					//				Keep_Curve.Add(Curve);
					//				script.Curve.AddRange(Curve);

				}else{
//					Curve = Keep_Curve[i];
				}
				//				script.Curve.AddRange(Curve);



				Draw_lines(i);
				#endregion
			}

		}


		void ControlMe(LandSplineNode[]  SplinePoints, int Control_points_Count){
			#region Curve Select
			if (Event.current.type == EventType.MouseUp & Event.current.button==0){ //v2.1 - select spline with right mouse button to fix Unity 5 issue where editor view is rotated on left click
				if( HandleUtility.nearestControl < Control_points_Count ){



					selectedSegment = HandleUtility.nearestControl;	

					//Debug.Log("PREV="+selectedSegment);

//					for (int i=0;i<Control_points_Count;i++) {
//						for (int k=0;k<=Curve.Length-2;k++) {
//							int Check = - (Curve.Length * i + k );
//							if(selectedSegment == Check){
//								selectedSegment = i;
//								break;
//							}
//						}
//					}

					//v2.1
					for (int i=0;i<Control_points_Count;i++) {

						int len_to_i = 0;
						for (int m = 0; m < i; m++) {
							len_to_i += script.Keep_Curve [m].Length;
						}
						if(i<script.Keep_Curve.Count){
							for (int k=0;k<=script.Keep_Curve[i].Length-2;k++) {
								//int Check = - (Curve.Length * i + k );
								int Check = - (len_to_i + k );
								if(selectedSegment == Check){
									selectedSegment = i;
									break;
								}
							}
						}
					}

					//Debug.Log("NEXT="+selectedSegment);

					HandleUtility.Repaint();


					if(Selection.activeGameObject != script.gameObject){ 



						selectedSegment=-1;
					}


				}
			}
			else if (Event.current.type == EventType.MouseUp & Event.current.button==1){



				selectedSegment=-1;


			}
			#endregion
			#region Curve Deselect
			if (Event.current.type == EventType.MouseUp & Event.current.button==0){



				//selectedSegment=-1;
			}
			#endregion

		}

//		void Draw_lines(int i){
//
//			for (int k=0;k<=Curve.Length-2;k++) {
//				//HandleUtility.AddControl(i, HandleUtility.DistanceToLine(Curve[k+1].position,Curve[k].position));
//
//				int Check = - (Curve.Length * i + k );
//				HandleUtility.AddControl(Check, HandleUtility.DistanceToLine(Curve[k+1].position,Curve[k].position));
//
//				Handles.DrawPolyLine(Curve[k+1].position,Curve[k].position);
//			}
//		}

		//v2.1
		void Draw_lines(int i){

			int len_to_i = 0;
			for (int m = 0; m < i; m++) {
				len_to_i += script.Keep_Curve [m].Length;
			}

			if (i < script.Keep_Curve.Count) {
				for (int k = 0; k <= script.Keep_Curve [i].Length - 2; k++) {
					//HandleUtility.AddControl(i, HandleUtility.DistanceToLine(Curve[k+1].position,Curve[k].position));
					//int Check = - (script.Keep_Curve[i].Length * i + k );
					int Check = -(len_to_i + k);
					HandleUtility.AddControl (Check, HandleUtility.DistanceToLine (script.Keep_Curve [i] [k + 1].position, script.Keep_Curve [i] [k].position));

					Handles.DrawPolyLine (script.Keep_Curve [i] [k + 1].position, script.Keep_Curve [i] [k].position);
				}
			}
		}

		void Control_in(int i, LandSplineNode[]  SplinePoints){

			if (selectedSegment==i){
				Handles.color = Color.red;

				//v2.1
				//Handles.sc
				if (script.DefineRotations) {
					Quaternion TempRot = Handles.RotationHandle (SplinePoints [i].rotation, SplinePoints [i].position);
					SplinePoints [i].rotation = TempRot;
				}
				//HandleUtility.Repaint ();

				//v2.1
				if(script.SplinePoints[selectedSegment].Type == 1 | script.SplinePoints[selectedSegment].Type == 2){
					//if(script.SplinePoints[selectedSegment].direction
					Vector3 TempLoc = Handles.PositionHandle (SplinePoints[i].position + SplinePoints[i].directionB*30 ,Quaternion.identity);
					//Vector3 TempLoc1 = Handles.PositionHandle (SplinePoints[i+1].position + SplinePoints[i+1].directionB*30 ,Quaternion.identity);
					//SplinePoints[i].direction = TempLoc - SplinePoints[i].position;
					//script.SplinePoints[selectedSegment].direction = TempLoc - SplinePoints[i].position;
					Handles.DrawLine(SplinePoints[i].position + SplinePoints[i].directionB*30, SplinePoints[i].position);
					SplinePoints[i].directionB = (TempLoc - SplinePoints[i].position)/30;

					//Handles.DrawLine(SplinePoints[i+1].position + SplinePoints[i+1].directionB*30, SplinePoints[i+1].position);
					//SplinePoints[i+1].directionB = (TempLoc1 - SplinePoints[i+1].position)/30;

					if(i == script.SplinePoints.Count-2){
						Vector3 TempLoc1 = Handles.PositionHandle (SplinePoints[i+1].position + SplinePoints[i+1].directionB*30 ,Quaternion.identity);
						Handles.DrawLine(SplinePoints[i+1].position + SplinePoints[i+1].directionB*30, SplinePoints[i+1].position);
						SplinePoints[i+1].directionB = (TempLoc1 - SplinePoints[i+1].position)/30;
					}
				}

				//Debug.Log ("B="+SplinePoints [i].directionB);
				//Debug.Log ("B="+SplinePoints [i].directionC);
				//Debug.Log (i);

				if(script.SplinePoints[selectedSegment].Type == 2){

					//if(script.SplinePoints[selectedSegment].direction
					Vector3 TempLoc = Handles.PositionHandle (SplinePoints[i].position + SplinePoints[i].directionC*30 ,Quaternion.identity);
					//Vector3 TempLoc1 = Handles.PositionHandle (SplinePoints[i+1].position + SplinePoints[i+1].directionC*30 ,Quaternion.identity);
					//SplinePoints[i].direction = TempLoc - SplinePoints[i].position;
					//script.SplinePoints[selectedSegment].direction = TempLoc - SplinePoints[i].position;
					Handles.DrawLine(SplinePoints[i].position + SplinePoints[i].directionC*30, SplinePoints[i].position);
					SplinePoints[i].directionC = (TempLoc - SplinePoints[i].position)/30;

					//Handles.DrawLine(SplinePoints[i+1].position + SplinePoints[i+1].directionC*30, SplinePoints[i+1].position);
					//SplinePoints[i+1].directionC = (TempLoc1 - SplinePoints[i+1].position)/30;
				}

				SplinePoints[i].position = Handles.PositionHandle (SplinePoints[i].position,Quaternion.identity);
				SplinePoints[i+1].position = Handles.PositionHandle (SplinePoints[i+1].position,Quaternion.identity);
				if(script.control_points_children.Count !=0){

					//HandleUtility.Repaint ();

					script.control_points_children[i].transform.position = script.SplinePoints[i].position ;
					script.control_points_children[i+1].transform.position = script.SplinePoints[i+1].position ;
				}

			} else {
				//Handles.color = Color.magenta;

				//v2.1
				//if (script.SplinePoints [selectedSegment].Type == 1) {
				//Handles.DrawLine (SplinePoints [i].position + SplinePoints [i].directionB.normalized * 20, SplinePoints [i].position);
				//}
				//if (script.SplinePoints [selectedSegment].Type == 2) {
				//Handles.color = Color.green;
				//Handles.DrawLine (SplinePoints [i].position + SplinePoints [i].directionC.normalized * 20, SplinePoints [i].position);
				//}
				Handles.color = Color.cyan;
			}

		}



		//add prefab bolding to certain params
		SerializedProperty Multithreaded;
		SerializedProperty DefineRotations;

		SerializedProperty	Overide_seg_detail;
		SerializedProperty	SplinePoints;

		SerializedProperty	Add_mid_point;
		SerializedProperty	Add_in_Segment;
		SerializedProperty	Node_toggle_dist;

		SerializedProperty Handle_scale;
		SerializedProperty CurveQuality;
		SerializedProperty follow_curve;
		SerializedProperty parent_moving;
		SerializedProperty Motion_Speed;

		SerializedProperty show_controls_play_mode;
		SerializedProperty Game_Mode_On;
		SerializedProperty Alter_mode;
		SerializedProperty Always_on;
		SerializedProperty show_controls_edit_mode;
		SerializedProperty Alter_mode2;
		SerializedProperty Accelerate;
		SerializedProperty Add_point;
		SerializedProperty With_manip;

		public override void  OnInspectorGUI() {

			Selected_items_count = Selection.gameObjects.Length;
			if(Selected_items_count > 1){
				EditorGUILayout.HelpBox("Multi edit not supported, please select only a single spline item to edit",MessageType.Info);
			}

			//v2.1
			EditorGUILayout.HelpBox("Select spline segment with the Left Mouse Button and drag to define new spline node position",MessageType.Info);

			serializedObject.Update ();

			EditorGUILayout.PropertyField(Multithreaded);
			EditorGUILayout.PropertyField(DefineRotations);

			EditorGUILayout.PropertyField(CurveQuality);
			EditorGUILayout.PropertyField(follow_curve);
			EditorGUILayout.PropertyField(parent_moving);
			EditorGUILayout.PropertyField(Motion_Speed);
			EditorGUILayout.PropertyField(Handle_scale);
			EditorGUILayout.PropertyField(Node_toggle_dist);

			EditorGUILayout.PropertyField(show_controls_play_mode);
			EditorGUILayout.PropertyField(Game_Mode_On);
			EditorGUILayout.PropertyField(Alter_mode);
			EditorGUILayout.PropertyField(Always_on);
			EditorGUILayout.PropertyField(show_controls_edit_mode);
			EditorGUILayout.PropertyField(Alter_mode2);
			EditorGUILayout.PropertyField(Accelerate);
			EditorGUILayout.PropertyField(Add_point);
			EditorGUILayout.PropertyField(With_manip);


			EditorGUILayout.PropertyField(Add_mid_point);
			EditorGUILayout.PropertyField(Add_in_Segment);
			EditorGUILayout.PropertyField(Overide_seg_detail,true);
			EditorGUILayout.PropertyField(SplinePoints,true);

			PrefabUtility.RecordPrefabInstancePropertyModifications(script); 

			serializedObject.ApplyModifiedProperties ();




			Event _Event = Event.current;
			EventType _Type = _Event.type;
			if (_Type == EventType.MouseDown)
			{

			}

			script.Scale_curve = EditorGUILayout.FloatField("Curve Scale",script.Scale_curve); 



			if (script.SplinePoints.Count <1)
			{
				script.SplinePoints.Add(new LandSplineNode(script.gameObject.transform.position,Quaternion.identity));

				script.SplinePoints.Add(new LandSplineNode(new Vector3(script.gameObject.transform.position.x,script.gameObject.transform.position.y+25,script.gameObject.transform.position.z),Quaternion.identity));
				script.SplinePoints.Add(new LandSplineNode(new Vector3(script.gameObject.transform.position.x,script.gameObject.transform.position.y+50,script.gameObject.transform.position.z),Quaternion.identity));
				script.SplinePoints.Add(new LandSplineNode(new Vector3(script.gameObject.transform.position.x,script.gameObject.transform.position.y+75,script.gameObject.transform.position.z),Quaternion.identity));
				script.SplinePoints.Add(new LandSplineNode(new Vector3(script.gameObject.transform.position.x,script.gameObject.transform.position.y+100,script.gameObject.transform.position.z),Quaternion.identity));

				script.SplinePoints.Add(new LandSplineNode(new Vector3(script.gameObject.transform.position.x,script.gameObject.transform.position.y+125,script.gameObject.transform.position.z),Quaternion.identity));
				script.SplinePoints.Add(new LandSplineNode(new Vector3(script.gameObject.transform.position.x,script.gameObject.transform.position.y+150,script.gameObject.transform.position.z),Quaternion.identity));
				script.SplinePoints.Add(new LandSplineNode(new Vector3(script.gameObject.transform.position.x,script.gameObject.transform.position.y+175,script.gameObject.transform.position.z),Quaternion.identity));
				script.SplinePoints.Add(new LandSplineNode(new Vector3(script.gameObject.transform.position.x,script.gameObject.transform.position.y+200,script.gameObject.transform.position.z),Quaternion.identity));


				script.Keep_last_pos = script.gameObject.transform.position;


				script.Overide_seg_detail = new List<int>(); 
				for (int i=0;i<script.SplinePoints.Count-1;i++) 
				{

					script.Overide_seg_detail.Add(0);


				}


			}


			//v2.1 - change node type
			if (GUILayout.Button ("Change to auto")) {

				Undo.RecordObject (script, "Undo add");			
				//change type of selected
				script.SplinePoints[selectedSegment].Type = 0;
			}
			if (GUILayout.Button ("Change to custom angle")) {

				Undo.RecordObject (script, "Undo add");			
				//change type of selected
				script.SplinePoints[selectedSegment].Type = 1;
			}
			if (GUILayout.Button ("Change to split")) {

				Undo.RecordObject (script, "Undo add");			
				//change type of selected
				script.SplinePoints[selectedSegment].Type = 2;
			}


			if (GUILayout.Button ("Add another point")) {

				Undo.RecordObject(script,"Undo add");

				Vector3 Vector_along_last_two_points = (script.SplinePoints[script.SplinePoints.Count-1].position-script.SplinePoints[script.SplinePoints.Count-2].position);

				Vector3  Moved_vector_to_last_point = script.SplinePoints[script.SplinePoints.Count-1].position + 1*Vector_along_last_two_points;

				script.SplinePoints.Add(new LandSplineNode(Moved_vector_to_last_point,Quaternion.identity));

				GameObject SPHERE_SMALL = GameObject.CreatePrimitive(PrimitiveType.Sphere);
				script.control_points_children.Add(SPHERE_SMALL);
				Undo.RegisterCreatedObjectUndo(SPHERE_SMALL, "create " + SPHERE_SMALL.name);

				script.control_points_children[script.control_points_children.Count-1].transform.position = Moved_vector_to_last_point;
				script.control_points_children[script.control_points_children.Count-1].transform.localScale = (1/script.gameObject.transform.localScale.x)*0.3f*new Vector3(script.Handle_scale,script.Handle_scale,script.Handle_scale);

				script.control_points_children[script.control_points_children.Count-1].transform.parent = script.transform;


				script.Overide_seg_detail.Add(0);

			}

			if(script.SplinePoints.Count > 3){
				if (GUILayout.Button ("Remove last point")) {



					Object[] AA0 = new Object[1]; AA0[0] = script.gameObject;
					Object[] AA1 = EditorUtility.CollectDeepHierarchy(AA0);

					#pragma warning disable 612, 618
					Undo.RegisterUndo(AA1,"Spline Edit");
					#pragma warning restore 612, 618


					script.Overide_seg_detail.RemoveAt(script.SplinePoints.Count-2);


					script.SplinePoints.RemoveAt(script.SplinePoints.Count-1);

					DestroyImmediate (script.control_points_children[script.control_points_children.Count-1].gameObject);

					script.control_points_children.RemoveAt(script.control_points_children.Count-1);

				}
			}

			if(script.SplinePoints.Count > 2 ){
				if (GUILayout.Button ("Add point between")) {
					if(selectedSegment != -1){


						Undo.RecordObject(script,"Add between");


						Vector3 Moved_vector_to_last_point = script.SplinePoints[selectedSegment].position -(script.SplinePoints[selectedSegment].position-script.SplinePoints[selectedSegment+1].position)/2;


						script.SplinePoints.Insert(selectedSegment+1, new LandSplineNode(Moved_vector_to_last_point,Quaternion.identity));

						GameObject SPHERE_SMALL = GameObject.CreatePrimitive(PrimitiveType.Sphere);

						script.control_points_children.Insert(selectedSegment+1,SPHERE_SMALL);



						script.Overide_seg_detail.Insert(selectedSegment,0);



						Undo.RegisterCreatedObjectUndo(SPHERE_SMALL, "create " + SPHERE_SMALL.name);

						script.control_points_children[selectedSegment+1].transform.position = Moved_vector_to_last_point;
						script.control_points_children[selectedSegment+1].transform.localScale = (1/script.gameObject.transform.localScale.x)*0.3f*new Vector3(script.Handle_scale,script.Handle_scale,script.Handle_scale);

						script.control_points_children[selectedSegment+1].transform.parent = script.transform;


					}
				}
			}

			if (GUI.changed) {
				replot = true;
				EditorUtility.SetDirty (script);
			}
		}


	}
}