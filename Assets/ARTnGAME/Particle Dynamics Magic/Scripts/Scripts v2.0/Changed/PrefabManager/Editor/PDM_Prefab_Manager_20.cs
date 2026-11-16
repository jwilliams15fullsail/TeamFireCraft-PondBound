using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class PDM_Prefab_Manager_20 : EditorWindow
	{

		// Prefabs are named by category and description is separated by "-". e.g. CAT_A - DecalA. 

		[MenuItem("Window/Particle Dynamic Magic/Effects Library 2")]
		public static void ShowWindow()
		{
			PDM_Prefab_Manager_20 TEM_Wizard = GetWindow<PDM_Prefab_Manager_20>(true);
			//TEM_Wizard.title = "PDM 2.0";
			TEM_Wizard.titleContent.text = "PDM 2.0";
			TEM_Wizard.Show();

			TEM_Wizard.minSize = new Vector2(225, 740);
			TEM_Wizard.maxSize = new Vector2(1250, 790);
		}
		//public static string PrefabWizard_path = "Assets/Particle Dynamics Magic/PrefabWizard/";
		public void OnFocus()
		{
			Draw_Icons();
		}
		public void OnEnable()
		{
			Draw_Icons();
		}
		public void OnProjectChange()
		{
			Draw_Icons();
		}

		public static List<string> Loaded_Prefabs;

		public static List<Texture2D> PDM_Prefabs_icons;

		string Prefab_Path = "Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Effects20/";

		public bool Preview_mode = false;



		void Draw_Icons()
		{

			PDM_Prefabs_icons = new List<Texture2D>();
			Counter_starting_points = new List<int>();
			Descriptions = new List<string>();

			//CATEGORIES
			Category_Icons = new List<Texture2D>();
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_1.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_2.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_3.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_4.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_5.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_6.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_7.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_8.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_9.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/PDM20_CAT_10.jpg", typeof(Texture2D)) as Texture2D);
			Counter_starting_points.Add(0);

			//SUB CATEGORIES 
			Loaded_Prefabs = new List<string>();

			int START_COUNTING = 0;

			//lists
			List<string> Prefabs_names = new List<string>();

			string[] Prefabs_ALL = Directory.GetFiles(Prefab_Path + "Prefabs/", "*.prefab", SearchOption.AllDirectories);

			for (int i = 0; i < Prefabs_ALL.Length; i++)
			{
				Prefabs_names.Add(Prefabs_ALL[i].Replace(Prefab_Path + "Prefabs/", "").Replace('\\', '/').Replace(".prefab", ""));
			}

			for (int j = 0; j < 10; j++)
			{
				for (int i = 0; i < Prefabs_names.Count; i++)
				{

					//Find name after CAT_x
					string[] strArr = Prefabs_names[i].Split('-');
					string Prefab_Name = strArr[1];
					// 1. FIRE - SMOKE 
					if (j == 0 & (
						Prefabs_names[i].ToUpper().Contains("CAT_A")
							//					| Prefabs_names[i].ToUpper().Contains("SMOKE")
							//					| Prefabs_names[i].ToUpper().Contains("FIREBALL")
							//					| Prefabs_names[i].ToUpper().Contains("FIRERAIN")
							//					| Prefabs_names[i].ToUpper().Contains("VOLCANO")
							//					| Prefabs_names[i].ToUpper().Contains("FLAME")
							//					| Prefabs_names[i].ToUpper().Contains("MAGMA")
							//					| Prefabs_names[i].ToUpper().Contains("LAVA")
							)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 2. WATER - ICE 
					if (j == 1 & (
						Prefabs_names[i].ToUpper().Contains("CAT_B")
						//| Prefabs_names[i].ToUpper().Contains("ICE")
						//| Prefabs_names[i].ToUpper().Contains("WATERFALL")
						//| Prefabs_names[i].ToUpper().Contains("RAIN")
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 3. AIR 
					if (j == 2 & (
						Prefabs_names[i].ToUpper().Contains("CAT_C")
						//					| Prefabs_names[i].ToUpper().Contains("LIGHT")
						//					| Prefabs_names[i].ToUpper().Contains("CLOUD")
						//					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
						//					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
						//					| Prefabs_names[i].ToUpper().Contains("STARS")
						//					| Prefabs_names[i].ToUpper().Contains("TORNADO")
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 4. AURA 
					if (j == 3 & (
						Prefabs_names[i].ToUpper().Contains("CAT_D")
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 5. NATURE 
					if (j == 4 & (
						Prefabs_names[i].ToUpper().Contains("CAT_E")
											| Prefabs_names[i].ToUpper().Contains("POISON")
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 6. EFFECTS 
					if (j == 5 & (
						Prefabs_names[i].ToUpper().Contains("CAT_F")
						//| Prefabs_names[i].ToUpper().Contains("ORBITAL")			
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 7. EFFECTS 
					if (j == 6 & (
						Prefabs_names[i].ToUpper().Contains("CAT_G")
						//| Prefabs_names[i].ToUpper().Contains("CREATURES")			
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 8. EFFECTS 
					if (j == 7 & (
						Prefabs_names[i].ToUpper().Contains("CAT_H")
						//| Prefabs_names[i].ToUpper().Contains("DARKNESS")				
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 9. SYSTEMS 
					if (j == 8 & (
						Prefabs_names[i].ToUpper().Contains("CAT_I")
						//					| Prefabs_names[i].ToUpper().Contains("SHEET")
						//					| Prefabs_names[i].ToUpper().Contains("PROJ")
						//					| Prefabs_names[i].ToUpper().Contains("BEAM")			
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
					// 10. BUILDING BLOCKS 
					if (j == 9 & (
						Prefabs_names[i].ToUpper().Contains("CAT_J")
						)
					   )
					{
						Loaded_Prefabs.Add(Prefab_Path + "Prefabs/" + Prefabs_names[i] + ".prefab");
						PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/" + Prefabs_names[i] + ".jpg", typeof(Texture2D)) as Texture2D);
						Descriptions.Add(Prefab_Name);///
						START_COUNTING += 1;
					}
				}
				if (j < 10)
				{
					Counter_starting_points.Add(START_COUNTING);
				}
			}

		}

		Vector2 scrollPosition = Vector2.zero;

		public static List<Texture2D> Category_Icons;
		//categories
		public static int active_category = 0; // 1 = spline, 2 = ...

		public static int Spline_Category_Start = 0; //use these to start the counter for each category, set counter when category is clicked
		public static int Auras_Flows_Category_Start = 15;
		public static int Turbulence_Category_Start = 30;
		public static int Weapons_Category_Start = 0;

		public static int Mesh_Category_Start = 0;
		public static int Projection_Category_Start = 0;

		public static int Fire_Ice_Category_Start = 0;
		public static int Projectiles_Category_Start = 0;

		public static int Image_Category_Start = 0;

		public static int Transition_Category_Start = 0;

		public static List<int> Counter_starting_points;
		public static List<string> Descriptions;

		public float ParticlePlaybackScaleFactor = 1f;

		public float ParticleScaleFactor = 3f;
		public float ParticleDelay = 0f;


		void Scale_inner(AnimationCurve AnimCurve)
		{

			for (int i = 0; i < AnimCurve.keys.Length; i++)
			{
				AnimCurve.keys[i].value = AnimCurve.keys[i].value * ParticleScaleFactor;
			}
		}

		void ScaleMe()
		{

			if (1 == 1)
			{
				GameObject ParticleHolder = Selection.activeGameObject;
				//scale parent object

				if (Exclude_children)
				{



					ParticleSystem ParticleParent = ParticleHolder.GetComponent(typeof(ParticleSystem)) as ParticleSystem;

					if (ParticleParent != null)
					{
						Object[] ToUndo = new Object[2];
						ToUndo[0] = ParticleParent as Object;
						ToUndo[1] = Selection.activeGameObject.transform as Object;

						Undo.RecordObjects(ToUndo, "scale");

						ParticleHolder.transform.localScale = ParticleHolder.transform.localScale * ParticleScaleFactor;
					}

					if (ParticleParent != null)
					{

						//ParticleParent.startSize = ParticleParent.startSize * ParticleScaleFactor;
						ParticleSystem.MainModule main = ParticleParent.main;//v2.3
						ParticleSystem.MinMaxCurve Curve = main.startSize;
						Curve.constant = ParticleParent.main.startSize.constant * ParticleScaleFactor;

						//ParticleParent.startSpeed = ParticleParent.startSpeed * ParticleScaleFactor;	
						ParticleSystem.MinMaxCurve CurvestartSpeed = main.startSpeed;
						CurvestartSpeed.constant = ParticleParent.main.startSpeed.constant * ParticleScaleFactor;

						SerializedObject SerializedParticle = new SerializedObject(ParticleParent);

						if (SerializedParticle.FindProperty("VelocityModule.enabled").boolValue)
						{
							SerializedParticle.FindProperty("VelocityModule.x.scalar").floatValue *= ParticleScaleFactor;
							SerializedParticle.FindProperty("VelocityModule.y.scalar").floatValue *= ParticleScaleFactor;
							SerializedParticle.FindProperty("VelocityModule.z.scalar").floatValue *= ParticleScaleFactor;

							Scale_inner(SerializedParticle.FindProperty("VelocityModule.x.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.x.maxCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.y.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.y.maxCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.z.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.z.maxCurve").animationCurveValue);
						}

						if (SerializedParticle.FindProperty("ForceModule.enabled").boolValue)
						{
							SerializedParticle.FindProperty("ForceModule.x.scalar").floatValue *= ParticleScaleFactor;
							SerializedParticle.FindProperty("ForceModule.y.scalar").floatValue *= ParticleScaleFactor;
							SerializedParticle.FindProperty("ForceModule.z.scalar").floatValue *= ParticleScaleFactor;

							Scale_inner(SerializedParticle.FindProperty("ForceModule.x.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.x.maxCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.y.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.y.maxCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.z.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.z.maxCurve").animationCurveValue);
						}

						SerializedParticle.ApplyModifiedProperties();
					}
				}

				if (!Exclude_children)
				{

					ParticleSystem[] ParticleParents = ParticleHolder.GetComponentsInChildren<ParticleSystem>(true);

					if (ParticleParents != null)
					{
						Object[] ParticleParentsOBJ = new Object[ParticleParents.Length + 1];
						for (int i = 0; i < ParticleParents.Length; i++)
						{
							ParticleParentsOBJ[i] = ParticleParents[i] as Object;
						}
						ParticleParentsOBJ[ParticleParentsOBJ.Length - 1] = Selection.activeGameObject.transform as Object;

						Undo.RecordObjects(ParticleParentsOBJ, "scale");

						ParticleHolder.transform.localScale = ParticleHolder.transform.localScale * ParticleScaleFactor;
					}

					foreach (ParticleSystem ParticlesA in ParticleHolder.GetComponentsInChildren<ParticleSystem>(true))
					{

						//ParticlesA.startSize = ParticlesA.startSize * ParticleScaleFactor;
						ParticleSystem.MainModule main = ParticlesA.main;//v2.3
						ParticleSystem.MinMaxCurve Curve = main.startSize;
						Curve.constant = ParticlesA.main.startSize.constant * ParticleScaleFactor;

						//ParticlesA.startSpeed = ParticlesA.startSpeed * ParticleScaleFactor;	
						ParticleSystem.MinMaxCurve CurvestartSpeed = main.startSpeed;
						CurvestartSpeed.constant = ParticlesA.main.startSpeed.constant * ParticleScaleFactor;

						SerializedObject SerializedParticle = new SerializedObject(ParticlesA);

						if (SerializedParticle.FindProperty("VelocityModule.enabled").boolValue)
						{
							SerializedParticle.FindProperty("VelocityModule.x.scalar").floatValue *= ParticleScaleFactor;
							SerializedParticle.FindProperty("VelocityModule.y.scalar").floatValue *= ParticleScaleFactor;
							SerializedParticle.FindProperty("VelocityModule.z.scalar").floatValue *= ParticleScaleFactor;

							Scale_inner(SerializedParticle.FindProperty("VelocityModule.x.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.x.maxCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.y.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.y.maxCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.z.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("VelocityModule.z.maxCurve").animationCurveValue);
						}

						if (SerializedParticle.FindProperty("ForceModule.enabled").boolValue)
						{
							SerializedParticle.FindProperty("ForceModule.x.scalar").floatValue *= ParticleScaleFactor;
							SerializedParticle.FindProperty("ForceModule.y.scalar").floatValue *= ParticleScaleFactor;
							SerializedParticle.FindProperty("ForceModule.z.scalar").floatValue *= ParticleScaleFactor;

							Scale_inner(SerializedParticle.FindProperty("ForceModule.x.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.x.maxCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.y.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.y.maxCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.z.minCurve").animationCurveValue);
							Scale_inner(SerializedParticle.FindProperty("ForceModule.z.maxCurve").animationCurveValue);
						}

						SerializedParticle.ApplyModifiedProperties();
					}
				}
			}
		}

		public bool Exclude_children = false;
		public bool Add_to_selection = false;
		public bool Copy_properties_mode = false;
		public bool Include_inactive = true;

		public bool previous_Add_to_selection = false;
		public bool previous_Copy_properties_mode = false;

		//PROPERTIES

		public bool props_folder = false;
		public bool library_folder = true;
		public bool scaler_folder = false;
		public bool Toggle_all = false;
		public bool Toggle_all1 = false;

		public Object MainParticle;

		void AddCurveToSelectedGameObject()
		{

		}

		void OnGUI()
		{

			///// --------------------------------------------------------				
			EditorGUILayout.BeginHorizontal();

			if (GUILayout.Button(new GUIContent("Play", "Play effects"), EditorStyles.miniButtonLeft, GUILayout.Width(50)))
			{

				//// Start particle & text effect play
				if (Selection.activeGameObject != null)
				{

					int count_items = 0;

					ParticleSystem[] PSystems = Selection.activeGameObject.GetComponents<ParticleSystem>();
					if (PSystems != null)
					{
						Debug.Log(PSystems.Length);
						if (PSystems.Length > 0)
						{
							for (int i = 0; i < PSystems.Length; i++)
							{
								PSystems[i].Stop(true);
								PSystems[i].Play(true);
								count_items++;
							}
						}
					}

					ParticleSystem[] PSystemsC = Selection.activeGameObject.GetComponentsInChildren<ParticleSystem>(true);
					if (PSystemsC != null)
					{
						Debug.Log(PSystemsC.Length);
						if (PSystemsC.Length > 0)
						{
							for (int i = 0; i < PSystemsC.Length; i++)
							{
								PSystemsC[i].Stop(true);
								PSystemsC[i].Play(true);
								count_items++;
							}
						}
					}

					List<Object> ToSelect = new List<Object>();
					if (PSystems != null)
					{
						if (PSystems.Length > 0)
						{
							for (int i = 0; i < PSystems.Length; i++)
							{
								ToSelect.Add(PSystems[i].gameObject);
							}
						}
					}
					if (PSystemsC != null)
					{
						if (PSystemsC.Length > 0)
						{
							for (int i = 0; i < PSystemsC.Length; i++)
							{
								ToSelect.Add(PSystemsC[i].gameObject);
							}
						}
					}
					Selection.objects = ToSelect.ToArray();


				}
			}
			if (GUILayout.Button(new GUIContent("Stop", "Stop effects"), EditorStyles.miniButtonRight, GUILayout.Width(50)))
			{

				//// Start particle & text effect play
				if (Selection.activeGameObject != null)
				{

					ParticleSystem[] PSystems = Selection.activeGameObject.GetComponents<ParticleSystem>();
					if (PSystems != null)
					{
						Debug.Log(PSystems.Length);
						if (PSystems.Length > 0)
						{
							for (int i = 0; i < PSystems.Length; i++)
							{
								PSystems[i].Stop(true);
								PSystems[i].Clear(true);
							}
						}
					}

					ParticleSystem[] PSystemsC = Selection.activeGameObject.GetComponentsInChildren<ParticleSystem>(true);
					if (PSystemsC != null)
					{
						if (PSystemsC.Length > 0)
						{
							for (int i = 0; i < PSystemsC.Length; i++)
							{
								PSystemsC[i].Stop(true);
								PSystemsC[i].Clear(true);
							}
						}
					}


				}
				Selection.activeObject = null;
			}

			EditorGUILayout.TextArea("Preview On", GUILayout.MaxWidth(73.0f));
			Preview_mode = EditorGUILayout.Toggle(Preview_mode, GUILayout.MaxWidth(8.0f));

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.BeginHorizontal();

			string Lib_state = "Close";
			if (library_folder)
			{
				Lib_state = "Close";
			}
			else
			{
				Lib_state = "Open";
			}
			if (GUILayout.Button(new GUIContent(Lib_state + " Effect Library", Lib_state + " Effect Library"), GUILayout.Width(190), GUILayout.Height(18)))
			{
				if (library_folder)
				{
					library_folder = false;
				}
				else
				{
					library_folder = true;
				}
			}

			EditorGUILayout.EndHorizontal();

			float X_offset_left = 200;
			float Y_offset_top = 0;

			EditorGUILayout.LabelField("Scaler", EditorStyles.boldLabel);

			EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
			scaler_folder = EditorGUILayout.Foldout(scaler_folder, "Scale size and speed");
			EditorGUILayout.EndHorizontal();

			if (scaler_folder)
			{
				EditorGUILayout.BeginHorizontal();
				if (GUILayout.Button(new GUIContent("Scale particles", "Scale particle systems"), GUILayout.Width(95)))
				{

					if (Selection.activeGameObject != null)
					{
						ScaleMe();
					}
					else
					{
						Debug.Log("Please select a particle system to scale");
					}
				}

				if (GUILayout.Button(new GUIContent("Scale speed", "Scale playback speed"), GUILayout.Width(95)))
				{

					if (Selection.activeGameObject != null)
					{
						ParticleSystem[] PSystems = Selection.activeGameObject.GetComponentsInChildren<ParticleSystem>(Include_inactive);

						Undo.RecordObjects(PSystems, "speed scale");

						if (PSystems != null)
						{
							if (PSystems.Length > 0)
							{
								for (int i = 0; i < PSystems.Length; i++)
								{
									//PSystems[i].playbackSpeed = ParticlePlaybackScaleFactor;	
									ParticleSystem.MainModule main = PSystems[i].main;//v2.3
									main.simulationSpeed = ParticlePlaybackScaleFactor; //v2.3
								}
							}
						}
					}
					else
					{
						Debug.Log("Please select a particle system to scale");
					}
				}
				EditorGUILayout.EndHorizontal();

				EditorGUIUtility.wideMode = false;

				EditorGUILayout.BeginHorizontal();

				ParticleScaleFactor = EditorGUILayout.FloatField(ParticleScaleFactor, GUILayout.MaxWidth(95.0f));
				ParticlePlaybackScaleFactor = EditorGUILayout.FloatField(ParticlePlaybackScaleFactor, GUILayout.MaxWidth(95.0f));
				EditorGUILayout.EndHorizontal();

				MainParticle = EditorGUILayout.ObjectField(Selection.activeGameObject, typeof(GameObject), true, GUILayout.MaxWidth(180.0f));

				Exclude_children = EditorGUILayout.Toggle("Exclude children", Exclude_children, GUILayout.MaxWidth(180.0f));
				Include_inactive = EditorGUILayout.Toggle("Include inactive", Include_inactive, GUILayout.MaxWidth(180.0f));
			}
















			// Disable option if next one is enabled

			if (Add_to_selection != previous_Add_to_selection)
			{

				Copy_properties_mode = false; Repaint();


				previous_Add_to_selection = Add_to_selection;
				previous_Copy_properties_mode = Copy_properties_mode;
			}
			else
			if (Copy_properties_mode != previous_Copy_properties_mode)
			{
				Add_to_selection = false;
				Repaint();


				previous_Add_to_selection = Add_to_selection;
				previous_Copy_properties_mode = Copy_properties_mode;
			}





			if (library_folder)
			{
				this.minSize = new Vector2(1000, 15);
				this.maxSize = new Vector2(1000, 790);
			}
			else
			{
				this.minSize = new Vector2(203, 15);
				this.maxSize = new Vector2(203, 790);
			}

			if (library_folder)
			{

				int counter = 0;

				int quad_counter = 0;

				int category_counter = 0;

				#region TOP MENU
				int CATEGORIES_X_STEP = 80;
				int CATEGORIES_Y = 35;

				int active1_factor = 0;
				int active2_factor = 0;
				int active3_factor = 0;
				int active4_factor = 0;
				int active5_factor = 0;

				int active6_factor = 0;
				int active7_factor = 0;
				int active8_factor = 0;
				int active9_factor = 0;
				int active10_factor = 0;

				int activeY_extra = 6;

				if (active_category == 0) { active1_factor = 1; }
				if (active_category == 1) { active2_factor = 1; }
				if (active_category == 2) { active3_factor = 1; }
				if (active_category == 3) { active4_factor = 1; }
				if (active_category == 4) { active5_factor = 1; }

				if (active_category == 5) { active6_factor = 1; }
				if (active_category == 6) { active7_factor = 1; }
				if (active_category == 7) { active8_factor = 1; }
				if (active_category == 8) { active9_factor = 1; }
				if (active_category == 9) { active10_factor = 1; }

				EditorStyles.textField.wordWrap = true;

				GUI.TextField(new Rect(X_offset_left, 5 + Y_offset_top, 80, CATEGORIES_Y), "Dynamic Decals");
				if (GUI.Button(new Rect(X_offset_left, CATEGORIES_Y + (activeY_extra * active1_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;

					counter = Spline_Category_Start;
					quad_counter = 0;

					//v1.6
					scrollPosition = Vector2.zero;
				}

				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "AI Particles");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active2_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "Ribbons");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active3_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "Shaders & Shadows");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active4_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "Effects v2.0");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active5_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "2D Particles");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active6_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "Skinned v2.0");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active7_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "Shields");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active8_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "Spell Casting");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active9_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				category_counter += 1;
				GUI.TextField(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, 5 + Y_offset_top, 80, CATEGORIES_Y), "Spline - Image v2.0");
				if (GUI.Button(new Rect(X_offset_left + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active10_factor) + Y_offset_top, 80, 80), Category_Icons[category_counter]))
				{

					active_category = category_counter;
					quad_counter = 0;
					counter = Auras_Flows_Category_Start;
					//v1.6
					scrollPosition = Vector2.zero;
				}
				#endregion


				GUI.Box(new Rect(X_offset_left, CATEGORIES_Y + 87 + Y_offset_top, 800, 1), "");

				int Y_WIDTH = 43;
				int ICON_HEIGHT = 150;

				int Y_offset = 0 - 15;
				int X_offset = 150;

				int item_count = 0;

				Texture2D Icon_Texture = AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/Prefab1.png", typeof(Texture2D)) as Texture2D;

				if (PDM_Prefabs_icons != null)
				{

					#region MENU HANDLE

					if (PDM_Prefabs_icons.Count > 0)
					{

						counter = Counter_starting_points[active_category];
						if (active_category == 9)
						{
							//item_count = 19 ;
							item_count = Counter_starting_points[active_category + 1] - counter;
						}
						else
						{
							item_count = Counter_starting_points[active_category + 1] - counter;
						}

						//v1.6
						float category_height = 5000;

						int row_count = Mathf.FloorToInt(item_count / 5);
						if (((float)item_count / 5) > row_count)
						{
							category_height = (row_count + 0) * (ICON_HEIGHT + Y_WIDTH) + 30;

						}
						else
						{
							category_height = (row_count - 1) * (ICON_HEIGHT + Y_WIDTH) + 30;
						}

						scrollPosition = GUI.BeginScrollView(new Rect(-10, CATEGORIES_Y + 85 + 5 + Y_offset_top, 1010, 640), scrollPosition, new Rect(0, 0, 1280, category_height), false, false);

						int completed_5 = 0;
						for (int i = 0; i < item_count; i++)
						{

							Icon_Texture = PDM_Prefabs_icons[counter];
							EditorStyles.textField.wordWrap = true;

							GUI.TextField(new Rect(X_offset_left + 10 + completed_5 * X_offset, -15 + Y_offset + Y_WIDTH + (ICON_HEIGHT + 0) * quad_counter + Y_offset_top, ICON_HEIGHT, 30), Descriptions[counter]);
							if (GUI.Button(new Rect(X_offset_left + 10 + completed_5 * X_offset, 30 - 15 + Y_offset + Y_WIDTH + (ICON_HEIGHT + 0) * quad_counter + Y_offset_top, ICON_HEIGHT, ICON_HEIGHT - 30), Icon_Texture))
							{
								if (Loaded_Prefabs.Count > counter)
								{
									Object Loaded_prefab_item = (Object)AssetDatabase.LoadAssetAtPath(Loaded_Prefabs[counter], typeof(Object));
									if (Loaded_prefab_item != null)
									{

										if (!Preview_mode)
										{
											if (!Copy_properties_mode)
											{
												if (!Add_to_selection)
												{

													GameObject TEMP = Loaded_prefab_item as GameObject;
													GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item, Vector3.zero, TEMP.transform.rotation);
													SPHERE.name = Descriptions[counter];
												}
												else
												{

													if (Add_to_selection)
													{
														GameObject TEMP = Loaded_prefab_item as GameObject;
														GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item, Selection.activeGameObject.transform.position, TEMP.transform.rotation);
														SPHERE.name = Descriptions[counter];
														SPHERE.transform.parent = Selection.activeGameObject.transform;

														if (ParticleDelay > 0)
														{
															ParticleSystem InstanceParticle = SPHERE.GetComponent(typeof(ParticleSystem)) as ParticleSystem;

															if (InstanceParticle != null)
															{
																//InstanceParticle.startDelay = ParticleDelay;
																ParticleSystem.MainModule main = InstanceParticle.main;//v2.3
																ParticleSystem.MinMaxCurve Curve = main.startDelay;
																Curve.constant = ParticleDelay;
															}

															foreach (ParticleSystem ParticlesA in SPHERE.GetComponentsInChildren<ParticleSystem>(true))
															{
																//ParticlesA.startDelay = ParticleDelay;
																ParticleSystem.MainModule main = ParticlesA.main;//v2.3
																ParticleSystem.MinMaxCurve Curve1 = main.startDelay;
																Curve1.constant = ParticleDelay;
															}
														}

														Selection.activeGameObject = SPHERE;

													}
												}
											}//END if !Copy_properties_mode
											else
											{



											}//END if Copy_properties_mode
										}//END if !preview mode
										else
										{
											//handle LIVE preview
											//EditorApplication.ExecuteMenuItem("Edit/Play");
											if (Application.isPlaying)
											{

												Camera.main.cullingMask = (1 << LayerMask.NameToLayer("TEM_Sheet_Maker"));

												Camera.main.backgroundColor = new Color(140f / 255f, 60f / 255f, 80f / 255f, 255f / 255f);


												GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item, Camera.main.transform.position, Camera.main.transform.rotation);
												SPHERE.name = Descriptions[counter];
												SPHERE.transform.parent = Camera.main.transform;
												SPHERE.transform.position = Camera.main.transform.position + 15 * Camera.main.transform.forward;
												SPHERE.transform.parent = null;

												MoveToLayer(SPHERE.transform, LayerMask.NameToLayer("TEM_Sheet_Maker"));

												//select item in hierarchy
												Selection.activeGameObject = SPHERE;
											}
											else
											{
												Debug.Log("Please enter play mode to preview effects");
											}
										}
									}
								}
							}
							counter = counter + 1;

							if (completed_5 == 4)
							{
								completed_5 = 0;
								quad_counter = quad_counter + 1;
							}
							else
							{
								completed_5 = completed_5 + 1;
							}

						}

						GUI.EndScrollView();

					}
					#endregion

				}
			}

		}//END OnGUI


		void MoveToLayer(Transform root, int layer)
		{
			root.gameObject.layer = layer;
			foreach (Transform child in root)
				MoveToLayer(child, layer);
		}


	}
}