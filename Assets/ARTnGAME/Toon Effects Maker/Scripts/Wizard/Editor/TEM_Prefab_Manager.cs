using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Artngame.TEM;

public class TEM_Prefab_Manager : EditorWindow {

	[MenuItem ("Window/Toon Effects Maker")]
	public static void ShowWindow () {
		TEM_Prefab_Manager TEM_Wizard = GetWindow<TEM_Prefab_Manager>(true);
		//TEM_Wizard.title = "Toon Effects Maker";
		TEM_Wizard.titleContent = new GUIContent ("Toon Effects Maker"); //v2.3 PDM

		TEM_Wizard.Show();

		TEM_Wizard.minSize = new Vector2(225, 740);
		TEM_Wizard.maxSize = new Vector2(1250, 790);
	}
	public static string PrefabWizard_path = "Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/";
	public void OnFocus () {
		Draw_Icons();
	}	
	public void OnEnable () {
		Draw_Icons();
	}	
	public void OnProjectChange () {
		Draw_Icons();
	}

	public static List<string> Loaded_Prefabs ;

	public static List<Texture2D> PDM_Prefabs_icons;

	string Prefab_Path = "Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/";

	public bool Preview_mode=false;

	public AnimationCurve curveX  = AnimationCurve.Linear(0,0,1,1);
	public AnimationCurve curveY  = AnimationCurve.Linear(0,0,1,1);
	public AnimationCurve curveZ  = AnimationCurve.Linear(0,0,1,1);

	public AnimationCurve ForceCurveX  = AnimationCurve.Linear(0,0,1,1);
	public AnimationCurve ForceCurveY  = AnimationCurve.Linear(0,0,1,1);
	public AnimationCurve ForceCurveZ  = AnimationCurve.Linear(0,0,1,1);

	public AnimationCurve SizeCurveX  = AnimationCurve.Linear(0,0,1,1);
	public AnimationCurve SizeCurveY  = AnimationCurve.Linear(0,0,1,1);
	public AnimationCurve SizeCurveZ  = AnimationCurve.Linear(0,0,1,1);

	public AnimationCurve TEXTScalecurve  = AnimationCurve.Linear(0,0,1,1);
	public float Text_delay = 1f;
	public Color TEXTColor = Color.white;
	public Color TEXTColorB = Color.black;
	public int TEXTSize = 200;
	public string TEXTText = "BOOM";
	public bool Add_text_background = false;
	public Vector2 Text_back_scale=new Vector2(1,1.2f);
	public bool TextLookCam=false;

	public float Light_delay = 0f;
	public Color LightColor = Color.white;
	public Color LightColorB = Color.white;
	public int LightSize = 50;
	public bool LightLoop=false;

	public bool Create_atlas=true;
	public int SheetRows=2;
	public int SheetColumns =2;
	public int SpriteEvery=4;
	public float Sheet_cam_dist = 15;
	public Color SheetColor = new Color(140f/255f,60f/255f,80f/255f,255f/255f);
	public Color SheetColorB = new Color(0,0,0,1);
	public Color SheetColorC = new Color(36f/255f,4f/255f,15f/255f,210f/255f);
	public bool Move_object = false;
	public bool Remove_start_col_check = false;
	public Vector4 Remove_border = new Vector4(7,7,7,7);
	public bool Save_debug_frames = false;

	public bool Gradual_edge_fade = true;
	public float Color_dist = 0.2f;
	public float CheckColor_dist = 0.2f;

	void Draw_Icons(){

		PDM_Prefabs_icons= new List<Texture2D>();
		Counter_starting_points = new List<int>();
		Descriptions= new List<string>();

		//CATEGORIES
		Category_Icons = new List<Texture2D>();
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_1.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_2.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_3.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_4.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_5.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_6.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_7.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_8.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_9.jpg", typeof(Texture2D)) as Texture2D);
		Category_Icons.Add (AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Toon Effects Maker/PrefabWizard/Icons/DEMO_ICON_10.jpg", typeof(Texture2D)) as Texture2D);
		Counter_starting_points.Add(0);

		//SUB CATEGORIES 
		Loaded_Prefabs = new List<string>();
		
		int START_COUNTING=0;

		//lists
		List<string> Prefabs_names = new List<string>();

		string[] Prefabs_ALL = Directory.GetFiles(Prefab_Path+"Prefabs/", "*.prefab", SearchOption.AllDirectories);

		for(int i=0;i<Prefabs_ALL.Length;i++){
			Prefabs_names.Add(Prefabs_ALL[i].Replace(Prefab_Path+"Prefabs/", "").Replace('\\', '/').Replace(".prefab", ""));
		}

		for(int j=0;j<10;j++){
			for(int i=0;i<Prefabs_names.Count;i++){
				// 1. FIRE - SMOKE 
				if(j==0 & (
					Prefabs_names[i].ToUpper().Contains("FIRE") 
					| Prefabs_names[i].ToUpper().Contains("SMOKE")
					| Prefabs_names[i].ToUpper().Contains("FIREBALL")
					| Prefabs_names[i].ToUpper().Contains("FIRERAIN")
					| Prefabs_names[i].ToUpper().Contains("VOLCANO")
					| Prefabs_names[i].ToUpper().Contains("FLAME")
					| Prefabs_names[i].ToUpper().Contains("MAGMA")
					| Prefabs_names[i].ToUpper().Contains("LAVA")
						)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 2. WATER - ICE 
				if(j==1 & (
					Prefabs_names[i].ToUpper().Contains("WATER") 
					| Prefabs_names[i].ToUpper().Contains("ICE")
					| Prefabs_names[i].ToUpper().Contains("WATERFALL")
					//| Prefabs_names[i].ToUpper().Contains("RAIN")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 3. AIR 
				if(j==2 & (
					Prefabs_names[i].ToUpper().Contains("LIGHTNING") 
					| Prefabs_names[i].ToUpper().Contains("LIGHT")
					| Prefabs_names[i].ToUpper().Contains("CLOUD")
					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
					| Prefabs_names[i].ToUpper().Contains("STARS")
					| Prefabs_names[i].ToUpper().Contains("TORNADO")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 4. AURA 
				if(j==3 & (
					Prefabs_names[i].ToUpper().Contains("AURA") 
//					| Prefabs_names[i].ToUpper().Contains("LIGHT")
//					| Prefabs_names[i].ToUpper().Contains("CLOUD")
//					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
//					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
//					| Prefabs_names[i].ToUpper().Contains("STARS")
//					| Prefabs_names[i].ToUpper().Contains("TORNADO")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 5. NATURE 
				if(j==4 & (
					Prefabs_names[i].ToUpper().Contains("NATURE") 
										| Prefabs_names[i].ToUpper().Contains("POISON")
					//					| Prefabs_names[i].ToUpper().Contains("CLOUD")
					//					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
					//					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
					//					| Prefabs_names[i].ToUpper().Contains("STARS")
					//					| Prefabs_names[i].ToUpper().Contains("TORNADO")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 6. EFFECTS 
				if(j==5 & (
					Prefabs_names[i].ToUpper().Contains("EFFECTS") 
					| Prefabs_names[i].ToUpper().Contains("ORBITAL")
					//					| Prefabs_names[i].ToUpper().Contains("CLOUD")
					//					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
					//					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
					//					| Prefabs_names[i].ToUpper().Contains("STARS")
					//					| Prefabs_names[i].ToUpper().Contains("TORNADO")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 7. EFFECTS 
				if(j==6 & (
					Prefabs_names[i].ToUpper().Contains("CREATURE") 
					| Prefabs_names[i].ToUpper().Contains("CREATURES")
					//					| Prefabs_names[i].ToUpper().Contains("CLOUD")
					//					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
					//					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
					//					| Prefabs_names[i].ToUpper().Contains("STARS")
					//					| Prefabs_names[i].ToUpper().Contains("TORNADO")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 8. EFFECTS 
				if(j==7 & (
					Prefabs_names[i].ToUpper().Contains("DARK") 
					| Prefabs_names[i].ToUpper().Contains("DARKNESS")
					//					| Prefabs_names[i].ToUpper().Contains("CLOUD")
					//					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
					//					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
					//					| Prefabs_names[i].ToUpper().Contains("STARS")
					//					| Prefabs_names[i].ToUpper().Contains("TORNADO")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 9. SYSTEMS 
				if(j==8 & (
					Prefabs_names[i].ToUpper().Contains("SYSTEM") 
					| Prefabs_names[i].ToUpper().Contains("SHEET")
					| Prefabs_names[i].ToUpper().Contains("PROJ")
					| Prefabs_names[i].ToUpper().Contains("BEAM")
					//					| Prefabs_names[i].ToUpper().Contains("CLOUD")
					//					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
					//					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
					//					| Prefabs_names[i].ToUpper().Contains("STARS")
					//					| Prefabs_names[i].ToUpper().Contains("TORNADO")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
				// 10. BUILDING BLOCKS 
				if(j==9 & (
					Prefabs_names[i].ToUpper().Contains("BUILD_BLOCK") 
					//| Prefabs_names[i].ToUpper().Contains("DARKNESS")
					//					| Prefabs_names[i].ToUpper().Contains("CLOUD")
					//					| Prefabs_names[i].ToUpper().Contains("AIRPLANE")
					//					| Prefabs_names[i].ToUpper().Contains("SUNBEAMS")
					//					| Prefabs_names[i].ToUpper().Contains("STARS")
					//					| Prefabs_names[i].ToUpper().Contains("TORNADO")
					)
				   )
				{
					Loaded_Prefabs.Add(Prefab_Path+"Prefabs/" + Prefabs_names[i] + ".prefab");
					PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/"+ Prefabs_names[i] +".png", typeof(Texture2D)) as Texture2D);
					Descriptions.Add (Prefabs_names[i]);///
					START_COUNTING +=1;
				}
			}
			if(j<10){
				Counter_starting_points.Add(START_COUNTING);
			}
		}
	
	}

	Vector2 scrollPosition = Vector2.zero;

	public static  List<Texture2D> Category_Icons;
	//categories
	public static int active_category = 0; // 1 = spline, 2 = ...

	public static int Spline_Category_Start = 0 ; //use these to start the counter for each category, set counter when category is clicked
	public static int Auras_Flows_Category_Start = 15 ;
	public static int Turbulence_Category_Start = 30 ;
	public static int Weapons_Category_Start = 0 ;

	public static int Mesh_Category_Start = 0 ;
	public static int Projection_Category_Start = 0 ;

	public static int Fire_Ice_Category_Start = 0 ;
	public static int Projectiles_Category_Start = 0 ;

	public static int Image_Category_Start = 0 ;

	public static int Transition_Category_Start = 0 ;

	public static List<int> Counter_starting_points;
	public static List<string> Descriptions;

	public float ParticlePlaybackScaleFactor = 1f;

	public float ParticleScaleFactor = 3f;
	public float ParticleDelay = 0f;
	public bool Remove_rigid_constraints = true;
	public float Rigid_mass = 1;

	void Scale_inner(AnimationCurve AnimCurve){

		for(int i = 0; i < AnimCurve.keys.Length; i++)
		{
			AnimCurve.keys[i].value = AnimCurve.keys[i].value * ParticleScaleFactor;
		}
	}

	void ScaleMe(){
	
		if(1==1)
		{
			GameObject ParticleHolder = Selection.activeGameObject;
			//scale parent object

			if(Exclude_children){

			
					
			ParticleSystem ParticleParent = ParticleHolder.GetComponent(typeof(ParticleSystem)) as ParticleSystem;

				if(ParticleParent != null){
					Object[] ToUndo = new Object[2];
					ToUndo[0] = ParticleParent as Object;
					ToUndo[1] = Selection.activeGameObject.transform as Object;
					
					Undo.RecordObjects(ToUndo,"scale");

					ParticleHolder.transform.localScale = ParticleHolder.transform.localScale * ParticleScaleFactor;
				}

			if(ParticleParent!=null){

					//v2.3 PDM
					ParticleSystem.MainModule mainModule = ParticleParent.main;
					ParticleSystem.MinMaxCurve Curve1 = mainModule.startSize;
					ParticleSystem.MinMaxCurve Curve2 = mainModule.startSpeed;
					Curve1.constant = ParticleParent.main.startSize.constant * ParticleScaleFactor;
					Curve2.constant = ParticleParent.main.startSpeed.constant * ParticleScaleFactor;				

				SerializedObject SerializedParticle = new SerializedObject(ParticleParent);				

				if(SerializedParticle.FindProperty("VelocityModule.enabled").boolValue)
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
				
				if(SerializedParticle.FindProperty("ForceModule.enabled").boolValue)
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

			if(!Exclude_children){

				ParticleSystem[] ParticleParents = ParticleHolder.GetComponentsInChildren<ParticleSystem>(true);

				if(ParticleParents != null){
					Object[] ParticleParentsOBJ = new Object[ParticleParents.Length+1];
					for(int i=0;i<ParticleParents.Length;i++){
						ParticleParentsOBJ[i] = ParticleParents[i] as Object;
					}
					ParticleParentsOBJ[ParticleParentsOBJ.Length-1] = Selection.activeGameObject.transform as Object;

					Undo.RecordObjects(ParticleParentsOBJ,"scale");

					ParticleHolder.transform.localScale = ParticleHolder.transform.localScale * ParticleScaleFactor;
				}

				foreach(ParticleSystem ParticlesA in ParticleHolder.GetComponentsInChildren<ParticleSystem>(true))
				{

					//v2.3 PDM
					ParticleSystem.MainModule mainModule = ParticlesA.main;
					ParticleSystem.MinMaxCurve Curve1 = mainModule.startSize;
					ParticleSystem.MinMaxCurve Curve2 = mainModule.startSpeed;
					Curve1.constant = ParticlesA.main.startSize.constant * ParticleScaleFactor;
					Curve2.constant = ParticlesA.main.startSpeed.constant * ParticleScaleFactor;

					//ParticlesA.startSize = ParticlesA.startSize * ParticleScaleFactor;
					//ParticlesA.startSpeed = ParticlesA.startSpeed * ParticleScaleFactor;					

					SerializedObject SerializedParticle = new SerializedObject(ParticlesA);

					if(SerializedParticle.FindProperty("VelocityModule.enabled").boolValue)
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

					if(SerializedParticle.FindProperty("ForceModule.enabled").boolValue)
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
	public bool Add_to_selection=false;
	public bool Copy_properties_mode=false;
	public bool Include_inactive = true;

	public bool As_projectile = false;
	public bool Curved = false;
	public float Curve_factor = 0.5f;
	public bool OnMouseClick = true;
	public bool Turret = false; 
	public float Force_factor = 1900;
	public float Range_factor = 1000;
	public float Dist_from_cam = 7;
	public float Click_delay = 0.5f;
	public bool Add_on_collision = false;
	public bool Blast_world_space = true;
	public bool Blast_radial = false;

	public bool Radial_force = false;
	public float Radial_radius = 70;
	public float Blast_power = 1800;
	public float Destroy_delay = 2.2f;
	public bool Add_collider = false;
	public bool Blast_Collider_trigger = false;
	public float Blast_Collider_radius = 0.5f;

	public bool previous_Add_on_collision=false;
	public bool previous_Add_to_selection=false;
	public bool previous_Copy_properties_mode=false;

	//PROPERTIES

	public bool props_folder=false;
	public bool props_folder1=false;
	public bool props_folder2=false;
	public bool text_props_folder=false;
	public bool light_props_folder=false;
	public bool sheet_props_folder=false;
	public bool sound_props_folder = false;
	public bool library_folder=false;
	public bool maker_props_folder=false;
	public bool scaler_folder = false;

	public bool Assign_Looping = false;
	public bool Assign_Prewarm = false;
	public bool Assign_PlayOnAwake = false;
	public bool Assign_Start_Delay=false;
	public bool Assign_Start_Lifetime=false;
	public bool Assign_Start_Speed=false;
	public bool Assign_Start_Size=false;
	public bool Assign_Start_Rotation=false;
	public bool Assign_Start_Color=false;
	public bool Assign_Gravity=false;
	public bool Assign_Inherit_velocity=false;
	public bool Assign_Simulation_Space=false;
	public bool Assign_Max_Particles=false;

	public bool Assign_Emission_Group=false;
	public bool Assign_Shape_Group=false;
	public bool Assign_LimitVelocity_Group=false;
	public bool Assign_ForceOverLifetime_Group=false;
	public bool Assign_ColorBySpeed_Group=false;
	public bool Assign_SizeOverLifetime_Group=false;
	public bool Assign_SizeBySpeed_Group=false;
	public bool Assign_RotOverLifetime_Group=false;
	public bool Assign_RotBySpeed_Group=false;

	public bool Assign_ExternalForces_Group=false;
	public bool Assign_Collision_Group=false;
	public bool Assign_TextureSheet_Group=false;

	public bool Assign_SubEmitters_Group=false;

	public bool Assign_velocity = false;
	public bool Assign_LifetimeColor = false;
	public bool Assign_Material = false;

	public Object ParticleToCopyFROM;

	public bool Toggle_all = false;
	public bool Toggle_all1 = false;

	//SOUND
	public Object Sound_throw;
	public Object Sound_travel;
	public Object Sound_crash_soft;
	public Object Sound_crash_hard;
	public bool Sound_on_collision = false;
	public bool Sound_on_start = false;
	public bool Sound_on_travel = false;
	public bool Count_speed = false;
	public float Sound_delay=0;
	public float lowPitchRange = 0.75f;
	public float highPitchRange = 1.5f;
	public float velToVol = 0.2f;
	public float velocityClipSplit = 10f;
	public float volLowRange = 0.5f;
	public float volHighRange = 1.0f;

	public float Collider_radius = 1.0f;
	public bool Collider_trigger = false;
	public bool Collider_existing = true;

	public Object MainParticle;

	void AddCurveToSelectedGameObject() {

	}

	void OnGUI(){

		///// --------------------------------------------------------				
		EditorGUILayout.BeginHorizontal();

		if(GUILayout.Button(new GUIContent("Play","Play effects"),EditorStyles.miniButtonLeft, GUILayout.Width(50))){

			//// Start particle & text effect play
			if(Selection.activeGameObject != null){

				int count_items = 0;

				ParticleSystem[] PSystems = Selection.activeGameObject.GetComponents<ParticleSystem>();
				if(PSystems!=null){
					Debug.Log(PSystems.Length);
					if(PSystems.Length >0){
						for(int i=0;i<PSystems.Length;i++){
							PSystems[i].Stop(true);
							PSystems[i].Play(true);
							count_items++;
						}
					}
				}

				ParticleSystem[] PSystemsC = Selection.activeGameObject.GetComponentsInChildren<ParticleSystem>(true);
				if(PSystemsC!=null){
					Debug.Log(PSystemsC.Length);
					if(PSystemsC.Length >0){
						for(int i=0;i<PSystemsC.Length;i++){
							PSystemsC[i].Stop(true);
							PSystemsC[i].Play(true);
							count_items++;
						}
					}
				}

				List<Object> ToSelect = new List<Object>();
				if(PSystems!=null){
					if(PSystems.Length >0){
						for(int i=0;i<PSystems.Length;i++){
							ToSelect.Add(PSystems[i].gameObject);
						}
					}
				}
				if(PSystemsC!=null){
					if(PSystemsC.Length >0){
						for(int i=0;i<PSystemsC.Length;i++){
							ToSelect.Add(PSystemsC[i].gameObject);
						}
					}
				}
				Selection.objects = ToSelect.ToArray();

				TEM_Text_Effects[] TSystems = Selection.activeGameObject.GetComponentsInChildren<TEM_Text_Effects>();
				if(TSystems!=null){
					Debug.Log(TSystems.Length);
					if(TSystems.Length >0){
						for(int i=0;i<TSystems.Length;i++){
							TSystems[i].Reset();
							TSystems[i].preview=true;
						}
					}
				}

				Light[] LiSystems = Selection.activeGameObject.GetComponentsInChildren<Light>();
				if(LiSystems!=null){
					if(LiSystems.Length >0){
						for(int i=0;i<LiSystems.Length;i++){
							LiSystems[i].intensity = 0;
						}
					}
				}
				TEM_Light_Effects[] LSystems = Selection.activeGameObject.GetComponentsInChildren<TEM_Light_Effects>();
				if(LSystems!=null){
					if(LSystems.Length >0){
						for(int i=0;i<LSystems.Length;i++){
							LSystems[i].Reset();
							LSystems[i].preview=true;						
						}
					}
				}
			}
		}
		if(GUILayout.Button(new GUIContent("Stop","Stop effects"),EditorStyles.miniButtonRight, GUILayout.Width(50))){
			
			//// Start particle & text effect play
			if(Selection.activeGameObject != null){
				
				ParticleSystem[] PSystems = Selection.activeGameObject.GetComponents<ParticleSystem>();
				if(PSystems!=null){
					Debug.Log(PSystems.Length);
					if(PSystems.Length >0){
						for(int i=0;i<PSystems.Length;i++){
							PSystems[i].Stop(true);
							PSystems[i].Clear(true);
						}
					}
				}

				ParticleSystem[] PSystemsC = Selection.activeGameObject.GetComponentsInChildren<ParticleSystem>(true);
				if(PSystemsC!=null){
					if(PSystemsC.Length >0){
						for(int i=0;i<PSystemsC.Length;i++){
							PSystemsC[i].Stop(true);
							PSystemsC[i].Clear(true);
						}
					}
				}
				
				TEM_Text_Effects[] TSystems = Selection.activeGameObject.GetComponentsInChildren<TEM_Text_Effects>();
				if(TSystems!=null){
					if(TSystems.Length >0){
						for(int i=0;i<TSystems.Length;i++){
							TSystems[i].Reset();
							TSystems[i].preview=false;
						}
					}
				}

				Light[] LiSystems = Selection.activeGameObject.GetComponentsInChildren<Light>();
				if(LiSystems!=null){
					if(LiSystems.Length >0){
						for(int i=0;i<LiSystems.Length;i++){
							LiSystems[i].intensity = 0;
						}
					}
				}
				TEM_Light_Effects[] LSystems = Selection.activeGameObject.GetComponentsInChildren<TEM_Light_Effects>();
				if(LSystems!=null){
					if(LSystems.Length >0){
						for(int i=0;i<LSystems.Length;i++){
							LSystems[i].Reset();
							LSystems[i].preview=false;
						}
					}
				}
			}
			Selection.activeObject = null;
		}

		EditorGUILayout.TextArea("Preview On",GUILayout.MaxWidth(73.0f));
		Preview_mode = EditorGUILayout.Toggle(Preview_mode,GUILayout.MaxWidth(8.0f));

		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();

		string Lib_state = "Close";
		if(library_folder){
			Lib_state = "Close";
		}else{
			Lib_state = "Open";
		}
		if(GUILayout.Button(new GUIContent(Lib_state+" Effect Library",Lib_state+" Effect Library"),GUILayout.Width(190),GUILayout.Height(18))){	
			if(library_folder){
				library_folder = false;
			}else{
				library_folder = true;
			}
		}

		EditorGUILayout.EndHorizontal();

		float X_offset_left = 200;
		float Y_offset_top = 0;

		EditorGUILayout.LabelField("Scaler",EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
		scaler_folder = EditorGUILayout.Foldout(scaler_folder,"Scale size and speed");
		EditorGUILayout.EndHorizontal();

		if(scaler_folder){
		EditorGUILayout.BeginHorizontal();
		if(GUILayout.Button(new GUIContent("Scale particles","Scale particle systems"),GUILayout.Width(95))){			

				if(Selection.activeGameObject != null){
			ScaleMe();
				}else{
					Debug.Log ("Please select a particle system to scale");
				}
		}

		if(GUILayout.Button(new GUIContent("Scale speed","Scale playback speed"),GUILayout.Width(95))){

				if(Selection.activeGameObject != null){
			ParticleSystem[] PSystems = Selection.activeGameObject.GetComponentsInChildren<ParticleSystem>(Include_inactive);

				Undo.RecordObjects(PSystems,"speed scale");

			if(PSystems!=null){
				if(PSystems.Length >0){
					for(int i=0;i<PSystems.Length;i++){
						//PSystems[i].playbackSpeed = ParticlePlaybackScaleFactor;	
								ParticleSystem.MainModule mainMmodule = PSystems[i].main;
								mainMmodule.simulationSpeed = ParticlePlaybackScaleFactor;	//v2.3 PDM 
					}
				}
			}
				}else{
					Debug.Log ("Please select a particle system to scale");
				}
		}
		EditorGUILayout.EndHorizontal();

		EditorGUIUtility.wideMode = false;

		EditorGUILayout.BeginHorizontal();

		ParticleScaleFactor = EditorGUILayout.FloatField(ParticleScaleFactor,GUILayout.MaxWidth(95.0f));
		ParticlePlaybackScaleFactor = EditorGUILayout.FloatField(ParticlePlaybackScaleFactor,GUILayout.MaxWidth(95.0f));
		EditorGUILayout.EndHorizontal();

		MainParticle =  EditorGUILayout.ObjectField(Selection.activeGameObject,typeof( GameObject ),true,GUILayout.MaxWidth(180.0f));

		Exclude_children = EditorGUILayout.Toggle("Exclude children", Exclude_children,GUILayout.MaxWidth(180.0f));
		Include_inactive = EditorGUILayout.Toggle("Include inactive", Include_inactive,GUILayout.MaxWidth(180.0f));
		}







		///// -------------------------------------------------------- MAKER - use to insert sample to selected gameobject, with delay etc params
		GUILayout.Box("",GUILayout.Height(2),GUILayout.Width(192));
		EditorGUILayout.LabelField("Particle System maker",EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));
		maker_props_folder = EditorGUILayout.Foldout(maker_props_folder,"Add effects");
		EditorGUILayout.EndHorizontal();

		if(maker_props_folder){
			//EditorGUILayout.LabelField("Enable to add prefab to selection",GUILayout.MaxWidth(200.0f));
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(180.0f));
			EditorGUILayout.HelpBox("Enable to add library item to selection", MessageType.Info, true);
			EditorGUILayout.EndVertical();

			Add_to_selection = EditorGUILayout.Toggle("Add lib effect to selection", Add_to_selection,GUILayout.MaxWidth(180.0f));

			As_projectile = EditorGUILayout.Toggle("As projectile", As_projectile,GUILayout.MaxWidth(180.0f));
			Curved = EditorGUILayout.Toggle("Curved projectile", Curved,GUILayout.MaxWidth(180.0f));
			Curve_factor = EditorGUILayout.FloatField("Curve factor",Curve_factor,GUILayout.MaxWidth(180.0f));
			OnMouseClick = EditorGUILayout.Toggle("Cast on click", OnMouseClick,GUILayout.MaxWidth(180.0f));
			Turret = EditorGUILayout.Toggle("Turret mode", Turret,GUILayout.MaxWidth(180.0f));
			Force_factor = EditorGUILayout.FloatField("Cast force",Force_factor,GUILayout.MaxWidth(190.0f));
			Range_factor = EditorGUILayout.FloatField("Range",Range_factor,GUILayout.MaxWidth(190.0f));
			Dist_from_cam = EditorGUILayout.FloatField("Distance from camera",Dist_from_cam,GUILayout.MaxWidth(180.0f));
			Click_delay = EditorGUILayout.FloatField("Click delay",Click_delay,GUILayout.MaxWidth(180.0f));

			ParticleDelay = EditorGUILayout.FloatField("Effect delay",ParticleDelay,GUILayout.MaxWidth(180.0f));
			Remove_rigid_constraints = EditorGUILayout.Toggle("Remove rigidbody locks", Remove_rigid_constraints,GUILayout.MaxWidth(180.0f));
			Rigid_mass = EditorGUILayout.FloatField("Rigid body mass",Rigid_mass,GUILayout.MaxWidth(180.0f));

			GUILayout.Box("",GUILayout.Height(1),GUILayout.Width(190));
			GUILayout.Box("",GUILayout.Height(1),GUILayout.Width(190));
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(180.0f));
			EditorGUILayout.HelpBox("Add destroyer on collision", MessageType.Info, true);
			EditorGUILayout.EndVertical();

			Radial_force = EditorGUILayout.Toggle("Radial force", Radial_force,GUILayout.MaxWidth(180.0f));
			Radial_radius = EditorGUILayout.FloatField("Radial force radius",Radial_radius,GUILayout.MaxWidth(190.0f));
			Blast_power = EditorGUILayout.FloatField("Blast Power",Blast_power,GUILayout.MaxWidth(190.0f));
			Destroy_delay = EditorGUILayout.FloatField("Destruction delay",Destroy_delay,GUILayout.MaxWidth(180.0f));
			Add_collider = EditorGUILayout.Toggle("Add collider", Add_collider,GUILayout.MaxWidth(180.0f));
			Blast_Collider_trigger = EditorGUILayout.Toggle("Is Trigger", Blast_Collider_trigger,GUILayout.MaxWidth(180.0f));
			Blast_Collider_radius = EditorGUILayout.FloatField("Collider radius",Blast_Collider_radius,GUILayout.MaxWidth(180.0f));

			if(GUILayout.Button(new GUIContent("Add destroyer","Add destroyer"),GUILayout.Width(100))){

				if(Selection.activeGameObject != null){

					SphereCollider[] Colliders = Selection.activeGameObject.GetComponentsInChildren<SphereCollider>(true);
					
					bool Found_col = false;
					
					if(Colliders !=null){
						if(Colliders.Length > 0){
							Found_col = true;
						}
					}
					
					if(!Found_col){
						Debug.Log ("no rigidbody found, adding rigidbody and collider to effect");
						//add collider
						Selection.activeGameObject.AddComponent(typeof(SphereCollider));
						//Collider_radius
						SphereCollider Blast_collider = Selection.activeGameObject.GetComponent(typeof(SphereCollider)) as SphereCollider;
						if(Blast_collider !=null){
							Blast_collider.radius = Blast_Collider_radius;
							Blast_collider.isTrigger = Blast_Collider_trigger;
						}
						
						Rigidbody Blast_rigid_body = Selection.activeGameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
						if(Blast_rigid_body == null){
							Selection.activeGameObject.AddComponent(typeof(Rigidbody));
							Blast_rigid_body = Selection.activeGameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
						}
						
						if(Blast_rigid_body !=null){
							Blast_rigid_body.useGravity = true;
							Blast_rigid_body.mass = Rigid_mass;
							if(Remove_rigid_constraints){
								Blast_rigid_body.isKinematic = false;
								Blast_rigid_body.constraints = RigidbodyConstraints.None;
							}
						}

						Selection.activeGameObject.AddComponent(typeof(DestroyOnImpactTEM));
						DestroyOnImpactTEM Blaster = Selection.activeGameObject.GetComponent(typeof(DestroyOnImpactTEM)) as DestroyOnImpactTEM;
						
						Blaster.Radial_force = Radial_force;
						Blaster.radius = Radial_radius;
						Blaster.power = Blast_power;
						Blaster.destroy_time = Destroy_delay;
						Blaster.FullDestroy = true;
						Blaster.Destroy_after_time = true;
						
						Blaster.To_Destroy = Selection.activeGameObject;
						
					}else{
						Colliders[0].radius = Blast_Collider_radius;
						Colliders[0].isTrigger = Blast_Collider_trigger;
						
						Rigidbody Blast_rigid_body = Colliders[0].gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
						if(Blast_rigid_body == null){
							Colliders[0].gameObject.AddComponent(typeof(Rigidbody));
							Blast_rigid_body = Colliders[0].gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
						}
						
						if(Blast_rigid_body !=null){
							Blast_rigid_body.useGravity = true;
							Blast_rigid_body.mass = Rigid_mass;
							if(Remove_rigid_constraints){
								Blast_rigid_body.isKinematic = false;
								Blast_rigid_body.constraints = RigidbodyConstraints.None;
							}
						}

						Colliders[0].gameObject.AddComponent(typeof(DestroyOnImpactTEM));
						DestroyOnImpactTEM Blaster = Colliders[0].gameObject.GetComponent(typeof(DestroyOnImpactTEM)) as DestroyOnImpactTEM;
						
						Blaster.Radial_force = Radial_force;
						Blaster.radius = Radial_radius;
						Blaster.power = Blast_power;
						Blaster.destroy_time = Destroy_delay;
						Blaster.FullDestroy = true;
						Blaster.Destroy_after_time = true;
						
						Blaster.To_Destroy = Colliders[0].gameObject;

					}




					if(Add_collider){
						//add collider
						Selection.activeGameObject.AddComponent(typeof(SphereCollider));
						//Collider_radius
						SphereCollider Blast_collider = Selection.activeGameObject.GetComponent(typeof(SphereCollider)) as SphereCollider;
						if(Blast_collider !=null){
							Blast_collider.radius = Blast_Collider_radius;
							Blast_collider.isTrigger = Blast_Collider_trigger;
						}
						
						Selection.activeGameObject.AddComponent(typeof(Rigidbody));
						Rigidbody Blast_rigid_body = Selection.activeGameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
						if(Blast_rigid_body !=null){
							Blast_rigid_body.useGravity = true;
							Blast_rigid_body.mass = Rigid_mass;
							if(Remove_rigid_constraints){
								Blast_rigid_body.isKinematic = false;
								Blast_rigid_body.constraints = RigidbodyConstraints.None;
							}
						}
					}
				}
			}

			GUILayout.Box("",GUILayout.Height(1),GUILayout.Width(190));
			GUILayout.Box("",GUILayout.Height(1),GUILayout.Width(190));
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(180.0f));
			EditorGUILayout.HelpBox("Enable to add library item on collision. Add collision script above first.", MessageType.Info, true);
			EditorGUILayout.EndVertical();

			Add_on_collision = EditorGUILayout.Toggle("Add lib effect on collision", Add_on_collision,GUILayout.MaxWidth(180.0f));
			Blast_world_space = EditorGUILayout.Toggle("Blast in World Space", Blast_world_space,GUILayout.MaxWidth(180.0f));
			Blast_radial = EditorGUILayout.Toggle("Force radial blast", Blast_radial,GUILayout.MaxWidth(180.0f));

		}



		///// -------------------------------------------------------- TEXT EFFECTS, add delay, animation curve for scale and apply it to script, also attach script and create text object

	
		GUILayout.Box("",GUILayout.Height(2),GUILayout.Width(192));
		
		EditorGUILayout.LabelField("Text effects",EditorStyles.boldLabel);
		
		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

		text_props_folder = EditorGUILayout.Foldout(text_props_folder,"Text effects");

		EditorGUILayout.EndHorizontal();
		if(text_props_folder){

			if(GUILayout.Button(new GUIContent("Add text effect","Adds the text effect"),GUILayout.Width(100))){
				
				GameObject ParticleHolder = Selection.activeGameObject;

				if(ParticleHolder != null){

				//create new gameobject, add script, assing curve and delay and parent to particle
				GameObject Text_Obj = new GameObject("Text Object");
				Text_Obj.transform.parent = ParticleHolder.transform;

				Text_Obj.transform.position= ParticleHolder.transform.position;
				Text_Obj.transform.localScale = Vector3.one;
				Text_Obj.transform.rotation = Quaternion.identity;
				Text_Obj.AddComponent(typeof(TEM_Text_Effects));

				TEM_Text_Effects Texter = Text_Obj.GetComponent(typeof(TEM_Text_Effects)) as TEM_Text_Effects;
				if(Texter !=null){
					Texter.Delay = Text_delay;
				}

				//grab font + material from object
				Font fonty = new Font("Arial");
				Material material_font = null;

				bool found_font=false;
				Object Loaded_prefab_item = (Object)AssetDatabase.LoadAssetAtPath(Prefab_Path + "TEXT_PROPS.prefab", typeof(Object));
				if (Loaded_prefab_item!=null){
					GameObject Grab_text_props = Loaded_prefab_item as GameObject;
					if(Grab_text_props!=null){
						TextMesh Texty1 = Grab_text_props.GetComponent(typeof(TextMesh)) as TextMesh;
						if(Texty1 !=null){
							found_font = true;
							fonty = Texty1.font;
							MeshRenderer rend1 = Grab_text_props.GetComponent<MeshRenderer>();
							if(rend1!=null){
								material_font = rend1.sharedMaterial;
							}
							TEM_Text_Effects Texter1 = Grab_text_props.GetComponent(typeof(TEM_Text_Effects)) as TEM_Text_Effects;
							if(Texter1!=null){
								Texter.Curve = Texter1.Curve;
								Texter.LookAtCamera = TextLookCam;
							}
						}
					}
				}

				if(found_font){
					Text_Obj.AddComponent(typeof(TextMesh));

					TextMesh Texty = Text_Obj.GetComponent(typeof(TextMesh)) as TextMesh;
					if(Texty !=null){
						Texty.text = TEXTText;
						Texty.fontSize = TEXTSize;
						Texty.color = TEXTColor;
						Texty.anchor = TextAnchor.MiddleCenter;

						Texty.font = fonty;
					}

					MeshRenderer rend = Text_Obj.GetComponent<MeshRenderer>();
					if(rend !=null){
						rend.sharedMaterial = material_font; 
					}

					if(Add_text_background){
						GameObject Text_Obj2 = new GameObject("Text Object");
						Text_Obj2.transform.parent = Text_Obj.transform;
						Text_Obj2.transform.position= Text_Obj.transform.position;
						Text_Obj2.transform.localScale = new Vector3(Text_back_scale.x,Text_back_scale.y,1);
						Text_Obj2.transform.rotation = Quaternion.identity;
						Text_Obj2.transform.localPosition = new Vector3(0,0,0.03f);

						Text_Obj2.AddComponent(typeof(TextMesh));
						Texty = Text_Obj2.GetComponent(typeof(TextMesh)) as TextMesh;
						if(Texty !=null){
							Texty.text = TEXTText;
							Texty.fontSize = TEXTSize;
							Texty.color = TEXTColorB;
							Texty.anchor = TextAnchor.MiddleCenter;
							Texty.font = fonty;
						}
						
						rend = Text_Obj2.GetComponent<MeshRenderer>();
						if(rend !=null){
							rend.sharedMaterial = material_font; 
						}
					}

				}else{
					Debug.Log ("Please add 'TEXT_PROPS' prefab in the PrefabWizard folder and make sure the asset is in the project root");
				}
			  }//END check if selection exists
			}
			


			TEXTColor = EditorGUILayout.ColorField("Font color",TEXTColor,GUILayout.MaxWidth(180.0f));
			TEXTColorB = EditorGUILayout.ColorField("Back color",TEXTColorB,GUILayout.MaxWidth(180.0f));
			TEXTSize = EditorGUILayout.IntField("Text size",TEXTSize,GUILayout.MaxWidth(180.0f));
			TEXTText = EditorGUILayout.TextField(TEXTText,GUILayout.MaxWidth(180.0f));
			
			Text_delay = EditorGUILayout.FloatField("Text delay",Text_delay,GUILayout.MaxWidth(180.0f));
			Text_back_scale = EditorGUILayout.Vector2Field("Scale Background",Text_back_scale,GUILayout.MaxWidth(180.0f));
			Add_text_background = EditorGUILayout.Toggle("Add text background", Add_text_background,GUILayout.MaxWidth(180.0f)); 

			TextLookCam = EditorGUILayout.Toggle("Look at camera", TextLookCam,GUILayout.MaxWidth(180.0f));
		}




		///// -------------------------------------------------------- LIGHT EFFECTS, add delay, animation curve for scale and apply it to script, also attach script and create text object

		GUILayout.Box("",GUILayout.Height(2),GUILayout.Width(192));
		
		EditorGUILayout.LabelField("Light effects",EditorStyles.boldLabel);

		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

		light_props_folder = EditorGUILayout.Foldout(light_props_folder,"Light effects");

		EditorGUILayout.EndHorizontal();
		if(light_props_folder){
			//Add text effect
			if(GUILayout.Button(new GUIContent("Add light effect","Adds the light effect"),GUILayout.Width(100))){
				
				GameObject ParticleHolder = Selection.activeGameObject;
				
				if(ParticleHolder != null){
					
					//create new gameobject, add script, assing curve and delay and parent to particle
					GameObject Light_Obj = new GameObject("Light Object");
					Light_Obj.transform.parent = ParticleHolder.transform;
					Light_Obj.transform.position= ParticleHolder.transform.position;
					Light_Obj.transform.localScale = Vector3.one;
					Light_Obj.transform.rotation = Quaternion.identity;
					
					Light_Obj.AddComponent(typeof(TEM_Light_Effects));
					
					TEM_Light_Effects Lighter = Light_Obj.GetComponent(typeof(TEM_Light_Effects)) as TEM_Light_Effects;
					if(Lighter !=null){
						Lighter.Delay = Light_delay;
						Lighter.StartLightColor = LightColor;
						Lighter.EndLightColor = LightColorB;
						Lighter.loop = LightLoop;
					}

					//grab font + material from object



					bool found_light=false;
					Object Loaded_prefab_item = (Object)AssetDatabase.LoadAssetAtPath(Prefab_Path + "LIGHT_PROPS.prefab", typeof(Object));
					if (Loaded_prefab_item!=null){
						GameObject Grab_text_props = Loaded_prefab_item as GameObject;
						if(Grab_text_props!=null){
							TEM_Light_Effects Lighter1 = Grab_text_props.GetComponent(typeof(TEM_Light_Effects)) as TEM_Light_Effects;
							if(Lighter1!=null){
								Lighter.Curve = Lighter1.Curve;
								found_light = true;
							}
						}
					}
					
					if(found_light){
						Light_Obj.AddComponent(typeof(Light));
						
						Light Lighty = Light_Obj.GetComponent(typeof(Light)) as Light;
						if(Lighty != null){
							Lighty.color = LightColor;
							Lighty.intensity = 0;
							Lighty.range = LightSize;
						}						
					}else{
						Debug.Log ("Please add 'LIGHT_PROPS' prefab in the PrefabWizard folder and make sure the asset is in the project root");
					}				
				}//END check if selection exists
			}
			

			
			LightColor = EditorGUILayout.ColorField("Light start color",LightColor,GUILayout.MaxWidth(180.0f));
			LightColorB = EditorGUILayout.ColorField("Light end color",LightColorB,GUILayout.MaxWidth(180.0f));
			LightSize = EditorGUILayout.IntField("Light radius",LightSize,GUILayout.MaxWidth(180.0f));
			LightLoop = EditorGUILayout.Toggle("Loop light", LightLoop,GUILayout.MaxWidth(180.0f));
	
			Light_delay = EditorGUILayout.FloatField("Start delay",Light_delay,GUILayout.MaxWidth(180.0f));
		}




		///// -------------------------------------------------------- SOUND EFFECTS, add delay, animation curve for scale and apply it to script, also attach script and create text object

		float width_max = 180;
		
		GUILayout.Box("",GUILayout.Height(2),GUILayout.Width(192));
		
		EditorGUILayout.LabelField("Sound effects",EditorStyles.boldLabel);
		
		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

		sound_props_folder = EditorGUILayout.Foldout(sound_props_folder,"Sound effects");

		EditorGUILayout.EndHorizontal();
		if(sound_props_folder){
			//Add text effect
			if(GUILayout.Button(new GUIContent("Add Sound effect","Add Sound effect"),GUILayout.Width(120))){
				
				GameObject ParticleHolder = Selection.activeGameObject;
				
				if(ParticleHolder != null){

					GameObject Light_Obj=null;
					//create new gameobject, add script, assing curve and delay and parent to particle
					if(!Collider_existing){
						Light_Obj = new GameObject("Sound Object");
						Light_Obj.transform.parent = ParticleHolder.transform;
						Light_Obj.transform.position= ParticleHolder.transform.position;
						Light_Obj.transform.localScale = Vector3.one;
						Light_Obj.transform.rotation = Quaternion.identity;
					}else{
						Collider[] Colliders = Selection.activeGameObject.GetComponentsInChildren<Collider>(true);
						if(Colliders!=null){
							if(Colliders.Length >0){
								Light_Obj = Colliders[0].gameObject;
							}else{
								Light_Obj = new GameObject("Sound Object");
								Light_Obj.transform.parent = ParticleHolder.transform;
								Light_Obj.transform.position= ParticleHolder.transform.position;
								Light_Obj.transform.localScale = Vector3.one;
								Light_Obj.transform.rotation = Quaternion.identity;
							}
						}else{
							Light_Obj = new GameObject("Sound Object");
							Light_Obj.transform.parent = ParticleHolder.transform;
							Light_Obj.transform.position= ParticleHolder.transform.position;
							Light_Obj.transform.localScale = Vector3.one;
							Light_Obj.transform.rotation = Quaternion.identity;
						}
					}
					Light_Obj.AddComponent(typeof(AudioSource));
					Light_Obj.AddComponent(typeof(SoundEffectsTEM));

					if(Sound_on_collision){
						//add collider
						SphereCollider Sound_collider1 = Light_Obj.GetComponent(typeof(SphereCollider)) as SphereCollider;
						if(Sound_collider1 == null){
							Light_Obj.AddComponent(typeof(SphereCollider));
							//Collider_radius
							SphereCollider Sound_collider = Light_Obj.GetComponent(typeof(SphereCollider)) as SphereCollider;
							if(Sound_collider !=null){
								Sound_collider.radius = Collider_radius;
								Sound_collider.isTrigger = Collider_trigger;
							}
						}else{
							Sound_collider1.radius = Collider_radius;
							Sound_collider1.isTrigger = Collider_trigger;
						}

						Rigidbody Sound_rigid_body1 = Light_Obj.GetComponent(typeof(Rigidbody)) as Rigidbody;
						if(Sound_rigid_body1==null){
							Light_Obj.AddComponent(typeof(Rigidbody));
							Rigidbody Sound_rigid_body = Light_Obj.GetComponent(typeof(Rigidbody)) as Rigidbody;
							if(Sound_rigid_body !=null){
								Sound_rigid_body.useGravity = true;
							}
						}else{
							Sound_rigid_body1.useGravity = true;
						}
					}
					
					SoundEffectsTEM Sounder = Light_Obj.GetComponent(typeof(SoundEffectsTEM)) as SoundEffectsTEM;
					if(Sounder !=null){
						Sounder.Affect_by_speed = Count_speed;

						if(Sound_crash_hard != null){
							Sounder.crashHard_Blast = (AudioClip)Sound_crash_hard;
						}
						if(Sound_crash_soft != null){
							Sounder.crashSoft = (AudioClip)Sound_crash_soft;
						}
						if(Sound_travel != null){
							Sounder.travelSound = (AudioClip)Sound_travel;
						}

						if(Sound_throw != null){
							Sounder.shootSound = (AudioClip)Sound_throw;
						}

						Sounder.Delay = Sound_delay;

						Sounder.Sound_on_start = Sound_on_start;
						Sounder.Sound_on_travel = Sound_on_travel;
						Sounder.Sound_on_collision = Sound_on_collision;

						Sounder.lowPitchRange = lowPitchRange;
						Sounder.highPitchRange = highPitchRange;
						Sounder.velToVol = velToVol;
						Sounder.velocityClipSplit = velocityClipSplit;
						Sounder.volLowRange = volLowRange;
						Sounder.volHighRange = volHighRange;
					}								
				}//END check if selection exists
			}
			
					

			Sound_on_start = EditorGUILayout.Toggle("Sound on start", Sound_on_start,GUILayout.MaxWidth(180.0f));
			Sound_throw = EditorGUILayout.ObjectField(Sound_throw,typeof(AudioClip),true,GUILayout.MaxWidth(width_max));

			Sound_on_travel = EditorGUILayout.Toggle("Sound on travel", Sound_on_travel,GUILayout.MaxWidth(180.0f));
			Sound_travel = EditorGUILayout.ObjectField(Sound_travel,typeof(AudioClip),true,GUILayout.MaxWidth(width_max));

			Sound_on_collision = EditorGUILayout.Toggle("Sound on collision", Sound_on_collision,GUILayout.MaxWidth(180.0f));
			Sound_crash_soft = EditorGUILayout.ObjectField(Sound_crash_soft,typeof(AudioClip),true,GUILayout.MaxWidth(width_max));
			Sound_crash_hard = EditorGUILayout.ObjectField(Sound_crash_hard,typeof(AudioClip),true,GUILayout.MaxWidth(width_max));

			Count_speed = EditorGUILayout.Toggle("Affect by speed", Count_speed,GUILayout.MaxWidth(180.0f));

			Collider_existing = EditorGUILayout.Toggle("Use on existing collider", Collider_existing,GUILayout.MaxWidth(180.0f));
			Collider_trigger = EditorGUILayout.Toggle("Is Trigger", Collider_trigger,GUILayout.MaxWidth(180.0f));
			Collider_radius = EditorGUILayout.FloatField("Collider radius",Collider_radius,GUILayout.MaxWidth(180.0f));

			Sound_delay = EditorGUILayout.FloatField("Sound delay",Sound_delay,GUILayout.MaxWidth(180.0f));
			lowPitchRange = EditorGUILayout.FloatField("Low Pitch Range",lowPitchRange,GUILayout.MaxWidth(180.0f));
			highPitchRange = EditorGUILayout.FloatField("High Pitch Range",highPitchRange,GUILayout.MaxWidth(180.0f));
			velToVol = EditorGUILayout.FloatField("Velocity to Volume",velToVol,GUILayout.MaxWidth(180.0f));
			velocityClipSplit = EditorGUILayout.FloatField("Vel.Clip Speed",velocityClipSplit,GUILayout.MaxWidth(180.0f));
			volLowRange = EditorGUILayout.FloatField("Low Volume",volLowRange,GUILayout.MaxWidth(180.0f));
			volHighRange = EditorGUILayout.FloatField("High Volum",volHighRange,GUILayout.MaxWidth(180.0f));
		}




		///// -------------------------------------------------------- SHEET EFFECTS, add delay, animation curve for scale and apply it to script, also attach script and create text object
				
		GUILayout.Box("",GUILayout.Height(2),GUILayout.Width(192));
		
		EditorGUILayout.LabelField("Sprite sheet creator",EditorStyles.boldLabel);
		
		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

		sheet_props_folder = EditorGUILayout.Foldout(sheet_props_folder,"Create sprite sheet");

		EditorGUILayout.EndHorizontal();
		if(sheet_props_folder){
			//Add text effect

			//GUILayout.Label("Select a scene item, enter play mode and press below to create sprite sheet",GUILayout.Width(150));
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(180.0f));
			EditorGUILayout.HelpBox("Select a scene item, enter play mode and press below to create sprite sheet. Platform must not be webplayer.", MessageType.Info, true);
			EditorGUILayout.EndVertical();

			if(GUILayout.Button(new GUIContent("Generate sheet","Generate sheet"),GUILayout.Width(100))){
				
				GameObject ParticleHolder = Selection.activeGameObject;
				
				if(ParticleHolder != null & Application.isPlaying){
						
						Camera.main.cullingMask = (1 << LayerMask.NameToLayer("TEM_Sheet_Maker")) ;
						
					Camera.main.backgroundColor = SheetColor; //new Color(140f/255f,60f/255f,80f/255f,255f/255f); //v1.1 fix

						Camera.main.clearFlags = CameraClearFlags.SolidColor;


					Object Loaded_prefab_item = (Object)AssetDatabase.LoadAssetAtPath(Prefab_Path + "SPRITE SHEET CAPTURE.prefab", typeof(Object));
					if (Loaded_prefab_item!=null){
						GameObject Grab_text_props = Loaded_prefab_item as GameObject;
						if(Grab_text_props!=null){

							GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item,Camera.main.transform.position,Camera.main.transform.rotation);
							SPHERE.name = "Sprite Sheet Maker";
							SPHERE.transform.parent = Camera.main.transform;
							SPHERE.transform.position = Camera.main.transform.position + Sheet_cam_dist*Camera.main.transform.forward;
							SPHERE.transform.parent = null;
							
							MoveToLayer(SPHERE.transform, LayerMask.NameToLayer("TEM_Sheet_Maker"));							

							if(Move_object){
								Selection.activeGameObject.transform.position = SPHERE.transform.position;
								Camera.main.transform.LookAt(Selection.activeGameObject.transform.position);
							}

							TEM_Sheet_Maker SheetMaker = SPHERE.GetComponent(typeof(TEM_Sheet_Maker)) as TEM_Sheet_Maker;
							if(SheetMaker!=null){

								SheetMaker.Objects.Clear();
								SheetMaker.Objects.Add(Selection.activeGameObject);

								SheetMaker.Cam_init_col = SheetColor;
								SheetMaker.Cut_off_color = SheetColorB;
								SheetMaker.Check_color = SheetColorC;

								SheetMaker.Row_count = SheetRows;
								SheetMaker.Col_count = SheetColumns;

								SheetMaker.CaptureEvery = SpriteEvery;
								SheetMaker.Use_unity_atlas = Create_atlas;

								SheetMaker.enabled=true;
								SheetMaker.StartCreatingSheet = true;

								SheetMaker.Use_GUI = false;
								SheetMaker.Remove_border = Remove_border;
								if(Remove_start_col_check){
									SheetMaker.Remove_init_check = true;
								}

								SheetMaker.Save_debug_frames = Save_debug_frames;
								SheetMaker.grad_edge_fade = Gradual_edge_fade;
								SheetMaker.Cut_off_dist = Color_dist;
								SheetMaker.Check_Cut_off = CheckColor_dist;
							}
						}
					}				
									
				}//END check if selection exists
				else{
					Debug.Log("Please select an item to create a sprite sheet from and enter play mode");
				}
			}
			

			
			SheetColor = EditorGUILayout.ColorField("Start color",SheetColor,GUILayout.MaxWidth(180.0f));
			SheetColorB = EditorGUILayout.ColorField("Cutoff color",SheetColorB,GUILayout.MaxWidth(180.0f));
			SheetColorC = EditorGUILayout.ColorField("Check color",SheetColorC,GUILayout.MaxWidth(180.0f));

			SheetRows = EditorGUILayout.IntField("Sheet rows",SheetRows,GUILayout.MaxWidth(180.0f));
			SheetColumns = EditorGUILayout.IntField("Sheet colums",SheetColumns,GUILayout.MaxWidth(180.0f));
			SpriteEvery = EditorGUILayout.IntField("Grab insterval",SpriteEvery,GUILayout.MaxWidth(180.0f));
			Create_atlas = EditorGUILayout.Toggle("Create atlas", Create_atlas,GUILayout.MaxWidth(180.0f));

			Sheet_cam_dist = EditorGUILayout.FloatField("Distance from camera",Sheet_cam_dist,GUILayout.MaxWidth(180.0f));
			Move_object = EditorGUILayout.Toggle("Center object", Move_object,GUILayout.MaxWidth(180.0f));

			Gradual_edge_fade = EditorGUILayout.Toggle("Fade to cutoff color", Gradual_edge_fade,GUILayout.MaxWidth(180.0f));
			Color_dist = EditorGUILayout.FloatField("Max Color Distance",Color_dist,GUILayout.MaxWidth(180.0f));
			CheckColor_dist = EditorGUILayout.FloatField("Max Check color Distance",CheckColor_dist,GUILayout.MaxWidth(180.0f));

			Save_debug_frames = EditorGUILayout.Toggle("Save_debug frames", Save_debug_frames,GUILayout.MaxWidth(180.0f));

			Remove_start_col_check = EditorGUILayout.Toggle("Remove start color check", Remove_start_col_check,GUILayout.MaxWidth(180.0f));
			Remove_border = EditorGUILayout.Vector4Field("Cut Pixels(left-right, up-down)",Remove_border,GUILayout.MaxWidth(180.0f));
		}




		///// -------------------------------------------------------- PROPERTY GRAB, CHANGE AND APPLY - use to insert sample to selected gameobject, with delay etc params
		GUILayout.Box("",GUILayout.Height(2),GUILayout.Width(192));
		EditorGUILayout.LabelField("Particle Properties",EditorStyles.boldLabel);		

		//EditorGUILayout.LabelField("Copy properties from library item",GUILayout.Width(200));
		EditorGUILayout.BeginVertical(GUILayout.MaxWidth(180.0f));
		EditorGUILayout.HelpBox("Copy properties from library items or another particle", MessageType.Info, true);
		//EditorGUILayout.EndVertical();

		Copy_properties_mode = EditorGUILayout.Toggle("Copy properties mode", Copy_properties_mode,GUILayout.MaxWidth(180.0f));

		if(GUILayout.Button(new GUIContent("Copy from","Copy properties from particle"),GUILayout.Width(120))){
						
			if(Selection.activeGameObject!=null){
				GameObject ParticleHolder = Selection.activeGameObject;		
				ParticleSystem ParticleParent = ParticleHolder.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
				
				GameObject ParticleHolderSOURCE = (GameObject)ParticleToCopyFROM;		
				ParticleSystem ParticleParentSOURCE = ParticleHolderSOURCE.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
				
				if(ParticleParent!=null & ParticleParentSOURCE!=null){	

//					Object[] ToUndo = new Object[2];
//					ToUndo[0] = ParticleParentSOURCE as Object;
//					ToUndo[1] = ParticleParent as Object;
//					Undo.RecordObjects(ToUndo,"Props copy");

					ApplyParticleChanges(ParticleHolderSOURCE,ParticleParentSOURCE,ParticleHolder,ParticleParent);
					
				}else{					
					Debug.Log ("Please select a particle to copy to and insert a particle in the 'Copy From' field");					
				}
			}else{				
				Debug.Log ("Please select a particle to copy to and insert a particle in the 'Copy From' field");				
			}
		}
		
		ParticleToCopyFROM =  EditorGUILayout.ObjectField(ParticleToCopyFROM,typeof( GameObject ),true,GUILayout.MaxWidth(width_max));

		if(GUILayout.Button(new GUIContent("Deselect all","Deselect all"),GUILayout.Width(120))){
		
			Toggle_all = true;

		}
		if(GUILayout.Button(new GUIContent("Select all","Select all"),GUILayout.Width(120))){
			Toggle_all1 = true;
		}
		if(Toggle_all1){
			
			Toggle_all1 = false;

			Assign_Looping =true;
			Assign_Prewarm =true;
			Assign_PlayOnAwake =true;
			Assign_Start_Delay=true;
			Assign_Start_Lifetime=true;
			Assign_Start_Speed=true;
			Assign_Start_Size=true;
			Assign_Start_Rotation=true;
			Assign_Start_Color=true;
			Assign_Gravity=true;
			Assign_Inherit_velocity=true;
			Assign_Simulation_Space=true;
			Assign_Max_Particles=true;
			
			Assign_Emission_Group=true;
			Assign_Shape_Group=true;
			Assign_LimitVelocity_Group=true;
			Assign_ForceOverLifetime_Group=true;
			Assign_ColorBySpeed_Group=true;
			Assign_SizeOverLifetime_Group=true;
			Assign_SizeBySpeed_Group=true;
			Assign_RotOverLifetime_Group=true;
			Assign_RotBySpeed_Group=true;
			
			Assign_ExternalForces_Group=true;
			Assign_Collision_Group=true;
			Assign_TextureSheet_Group=true;
			
			Assign_SubEmitters_Group=true;
			
			Assign_velocity =true;
			Assign_LifetimeColor =true;
			Assign_Material =true;
		}
		if(Toggle_all){

			Toggle_all = false;

			Assign_Looping = false;
			Assign_Prewarm = false;
			Assign_PlayOnAwake = false;
			Assign_Start_Delay=false;
			Assign_Start_Lifetime=false;
			Assign_Start_Speed=false;
			Assign_Start_Size=false;
			Assign_Start_Rotation=false;
			Assign_Start_Color=false;
			Assign_Gravity=false;
			Assign_Inherit_velocity=false;
			Assign_Simulation_Space=false;
			Assign_Max_Particles=false;
			
			Assign_Emission_Group=false;
			Assign_Shape_Group=false;
			Assign_LimitVelocity_Group=false;
			Assign_ForceOverLifetime_Group=false;
			Assign_ColorBySpeed_Group=false;
			Assign_SizeOverLifetime_Group=false;
			Assign_SizeBySpeed_Group=false;
			Assign_RotOverLifetime_Group=false;
			Assign_RotBySpeed_Group=false;
			
			Assign_ExternalForces_Group=false;
			Assign_Collision_Group=false;
			Assign_TextureSheet_Group=false;
			
			Assign_SubEmitters_Group=false;
			
			Assign_velocity = false;
			Assign_LifetimeColor = false;
			Assign_Material = false;
		}

		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

		props_folder = EditorGUILayout.Foldout(props_folder,"Initialization");

		GUILayout.Label("", GUILayout.Width(0));
		EditorGUILayout.EndHorizontal();

		if(props_folder){
			Assign_Looping = EditorGUILayout.Toggle("Looping", Assign_Looping,GUILayout.MaxWidth(width_max));
			Assign_Prewarm = EditorGUILayout.Toggle("Prewarm", Assign_Prewarm,GUILayout.MaxWidth(width_max));
			Assign_PlayOnAwake = EditorGUILayout.Toggle("Play On Awake", Assign_PlayOnAwake,GUILayout.MaxWidth(width_max));
			Assign_Start_Delay = EditorGUILayout.Toggle("Start Delay", Assign_Start_Delay,GUILayout.MaxWidth(width_max));
			Assign_Start_Lifetime = EditorGUILayout.Toggle("Start Lifetime", Assign_Start_Lifetime,GUILayout.MaxWidth(width_max));
			Assign_Start_Speed = EditorGUILayout.Toggle("Start Speed", Assign_Start_Speed,GUILayout.MaxWidth(width_max));
			Assign_Start_Size = EditorGUILayout.Toggle("Start Size", Assign_Start_Size,GUILayout.MaxWidth(width_max));
			Assign_Start_Rotation = EditorGUILayout.Toggle("Start Rotation", Assign_Start_Rotation,GUILayout.MaxWidth(width_max));
			Assign_Start_Color = EditorGUILayout.Toggle("Start Color", Assign_Start_Color,GUILayout.MaxWidth(width_max));
			Assign_Gravity = EditorGUILayout.Toggle("Gravity", Assign_Gravity,GUILayout.MaxWidth(width_max));
			Assign_Inherit_velocity = EditorGUILayout.Toggle("Inherit Velocity", Assign_Inherit_velocity,GUILayout.MaxWidth(width_max));
			Assign_Simulation_Space = EditorGUILayout.Toggle("Simulation Space", Assign_Simulation_Space,GUILayout.MaxWidth(width_max));
			Assign_Max_Particles = EditorGUILayout.Toggle("Max Particles", Assign_Max_Particles,GUILayout.MaxWidth(width_max));
		}

		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

		props_folder1 = EditorGUILayout.Foldout(props_folder1,"Velocity-Shape-Size-Color");

		EditorGUILayout.EndHorizontal();
		if(props_folder1){
			Assign_Emission_Group = EditorGUILayout.Toggle("Emission_Group", Assign_Emission_Group,GUILayout.MaxWidth(width_max));
			Assign_Shape_Group = EditorGUILayout.Toggle("Shape_Group", Assign_Shape_Group,GUILayout.MaxWidth(width_max));		
			Assign_velocity = EditorGUILayout.Toggle("Velocity", Assign_velocity,GUILayout.MaxWidth(width_max));		
			Assign_LimitVelocity_Group = EditorGUILayout.Toggle("Limit Velocity", Assign_LimitVelocity_Group,GUILayout.MaxWidth(width_max));
			Assign_ForceOverLifetime_Group = EditorGUILayout.Toggle("Force Over Lifetime", Assign_ForceOverLifetime_Group,GUILayout.MaxWidth(width_max));		
			Assign_LifetimeColor = EditorGUILayout.Toggle("Color Over Lifetime", Assign_LifetimeColor,GUILayout.MaxWidth(width_max));		
			Assign_ColorBySpeed_Group = EditorGUILayout.Toggle("Color By Speed ", Assign_ColorBySpeed_Group,GUILayout.MaxWidth(width_max));
			Assign_SizeOverLifetime_Group = EditorGUILayout.Toggle("Size Over Lifetime", Assign_SizeOverLifetime_Group,GUILayout.MaxWidth(width_max));
			Assign_SizeBySpeed_Group = EditorGUILayout.Toggle("Size By Speed", Assign_SizeBySpeed_Group,GUILayout.MaxWidth(width_max));
		}
		EditorGUILayout.BeginHorizontal(GUILayout.Width(200));

		props_folder2 = EditorGUILayout.Foldout(props_folder2,"Material-Subemitters-Forces");

		EditorGUILayout.EndHorizontal();
		if(props_folder2){
			Assign_RotOverLifetime_Group = EditorGUILayout.Toggle("Rotation Over Lifetime", Assign_RotOverLifetime_Group,GUILayout.MaxWidth(width_max));
			Assign_RotBySpeed_Group = EditorGUILayout.Toggle("Rotation By Speed", Assign_RotBySpeed_Group,GUILayout.MaxWidth(width_max));			
			Assign_ExternalForces_Group = EditorGUILayout.Toggle("External Forces", Assign_ExternalForces_Group,GUILayout.MaxWidth(width_max));
			Assign_Collision_Group = EditorGUILayout.Toggle("Collision", Assign_Collision_Group,GUILayout.MaxWidth(width_max));
			Assign_TextureSheet_Group = EditorGUILayout.Toggle("Texture Sheet", Assign_TextureSheet_Group,GUILayout.MaxWidth(width_max));			
			Assign_SubEmitters_Group = EditorGUILayout.Toggle("Sub Emitters", Assign_SubEmitters_Group,GUILayout.MaxWidth(width_max));
			Assign_Material = EditorGUILayout.Toggle("Material", Assign_Material,GUILayout.MaxWidth(width_max));
		}





		// Disable option if next one is enabled
		if (Add_on_collision != previous_Add_on_collision){
			Add_to_selection = false;
			Copy_properties_mode = false;Repaint();
			
			previous_Add_on_collision = Add_on_collision;
			previous_Add_to_selection = Add_to_selection;
			previous_Copy_properties_mode = Copy_properties_mode;
		}else		
		if (Add_to_selection != previous_Add_to_selection){
			Add_on_collision  = false;
			Copy_properties_mode = false;Repaint();
			
			previous_Add_on_collision = Add_on_collision;
			previous_Add_to_selection = Add_to_selection;
			previous_Copy_properties_mode = Copy_properties_mode;
		}else		
		if (Copy_properties_mode != previous_Copy_properties_mode){
			Add_to_selection = false;
			Add_on_collision  = false;Repaint();
			
			previous_Add_on_collision = Add_on_collision;
			previous_Add_to_selection = Add_to_selection;
			previous_Copy_properties_mode = Copy_properties_mode;
		}





		if(library_folder){
			this.minSize = new Vector2(1000,15);
			this.maxSize = new Vector2(1000,790);
		}else{
			this.minSize = new Vector2(203,15);
			this.maxSize = new Vector2(203,790);
		}

		if(library_folder){
		
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
		
		if(active_category==0){active1_factor=1;}
		if(active_category==1){active2_factor=1;}
		if(active_category==2){active3_factor=1;}
		if(active_category==3){active4_factor=1;}
		if(active_category==4){active5_factor=1;}
		
		if(active_category==5){active6_factor=1;}
		if(active_category==6){active7_factor=1;}
		if(active_category==7){active8_factor=1;}
		if(active_category==8){active9_factor=1;}
		if(active_category==9){active10_factor=1;}

		GUI.TextField(new Rect(X_offset_left,5+Y_offset_top,80,CATEGORIES_Y),"Fire - Smoke - Lava");
		if(GUI.Button(new Rect(X_offset_left,CATEGORIES_Y+(activeY_extra*active1_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;

			counter=Spline_Category_Start;
			quad_counter = 0;

			//v1.6
			scrollPosition = Vector2.zero;
		}

		category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Water - Ice - Ocean");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active2_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Air - Sky - Lightning");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active3_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Auras");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active4_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Nature - Poison");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active5_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Effects - Icons");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active6_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Creatures");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active7_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Darkness");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active8_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Systems");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active9_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}category_counter+=1;
		GUI.TextField(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,5+Y_offset_top,80,CATEGORIES_Y),"Building Blocks");
		if(GUI.Button(new Rect(X_offset_left+CATEGORIES_X_STEP*category_counter,CATEGORIES_Y+(activeY_extra*active10_factor)+Y_offset_top,80,80),Category_Icons[category_counter])){

			active_category=category_counter;
			quad_counter = 0;
			counter=Auras_Flows_Category_Start;
			//v1.6
			scrollPosition = Vector2.zero;
		}
		#endregion


		GUI.Box(new Rect(X_offset_left,CATEGORIES_Y+87+Y_offset_top,800,1),"");

		int Y_WIDTH = 43;
		int ICON_HEIGHT = 150;

		int Y_offset= 0-15;
		int X_offset= 150;

		int item_count=0;

		Texture2D Icon_Texture = AssetDatabase.LoadAssetAtPath(Prefab_Path + "Icons/Prefab1.png", typeof(Texture2D)) as Texture2D;

		if(PDM_Prefabs_icons !=null){

		#region MENU HANDLE

			if(PDM_Prefabs_icons.Count > 0){

				counter = Counter_starting_points[active_category];
				if(active_category==9){
					//item_count = 19 ;
						item_count = Counter_starting_points[active_category+1]-counter ;
				}else{
					item_count = Counter_starting_points[active_category+1]-counter ;
				}

				//v1.6
				float category_height = 5000;

				int row_count = Mathf.FloorToInt(item_count/5);
				if( ((float)item_count/5) > row_count){
					category_height = (row_count+2)*(ICON_HEIGHT+Y_WIDTH)+30;

				}else{
					category_height = (row_count+1)*(ICON_HEIGHT+Y_WIDTH)+30;
				}
				
				scrollPosition = GUI.BeginScrollView (new Rect (-10, CATEGORIES_Y+85+5+Y_offset_top, 1010, 640), scrollPosition, new Rect (0, 0, 1280, category_height),false,false);

				int completed_5=0;
				for(int i=0;i<item_count;i++){

					Icon_Texture = PDM_Prefabs_icons[counter];
					EditorStyles.textField.wordWrap = true;		
					if(GUI.Button(new Rect(X_offset_left + 10+completed_5*X_offset,-15 + Y_offset+Y_WIDTH+(ICON_HEIGHT+0)*quad_counter+Y_offset_top,ICON_HEIGHT,ICON_HEIGHT),Icon_Texture)){
						if(Loaded_Prefabs.Count>counter){
							Object Loaded_prefab_item = (Object)AssetDatabase.LoadAssetAtPath(Loaded_Prefabs[counter], typeof(Object));
							if (Loaded_prefab_item!=null){

							  if(!Preview_mode){
								if(!Copy_properties_mode){
									if(!Add_to_selection & !Add_on_collision){

										GameObject TEMP = Loaded_prefab_item as GameObject;
										GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item,Vector3.zero,TEMP.transform.rotation);
										SPHERE.name = Descriptions[counter];
									}else{

										if(Add_to_selection){
											GameObject TEMP = Loaded_prefab_item as GameObject;
											GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item,Selection.activeGameObject.transform.position,TEMP.transform.rotation);
											SPHERE.name = Descriptions[counter];
											SPHERE.transform.parent = Selection.activeGameObject.transform;

													if(As_projectile){
														//disable so it is controlled by script
														SPHERE.SetActive(false);
														//add script controller
														Selection.activeGameObject.AddComponent(typeof(ShooterTEM));
														ShooterTEM Shooter = Selection.activeGameObject.GetComponent(typeof(ShooterTEM)) as ShooterTEM;
														
														Shooter.Curved = Curved;
														Shooter.curve_factor = Curve_factor	;
														Shooter.OnMouseClick = OnMouseClick;
														Shooter.Turret_mode = Turret;
														Shooter.Turret = Selection.activeGameObject.transform;

														Shooter.shotForce = Force_factor;
														Shooter.range = Range_factor;
														Shooter.Ahead_of_camera = Dist_from_cam;
														Shooter.Click_Delay = Click_delay;

														if(SPHERE.GetComponent<Rigidbody>() != null){
															Shooter.projectile = SPHERE.GetComponent<Rigidbody>();
														}else{

															SphereCollider[] Colliders = SPHERE.GetComponentsInChildren<SphereCollider>(true);

															bool Found_col = false;
															//bool Found_rig = false;

															if(Colliders !=null){
																if(Colliders.Length > 0){
																	Found_col = true;
																}
															}

															if(!Found_col){
																Debug.Log ("no rigidbody found, adding rigidbody and collider to effect");
																//add collider
																SPHERE.AddComponent(typeof(SphereCollider));
																//Collider_radius
																SphereCollider Blast_collider = SPHERE.GetComponent(typeof(SphereCollider)) as SphereCollider;
																if(Blast_collider !=null){
																	Blast_collider.radius = Blast_Collider_radius;
																	Blast_collider.isTrigger = Blast_Collider_trigger;
																}

																Rigidbody Blast_rigid_body = SPHERE.GetComponent(typeof(Rigidbody)) as Rigidbody;
																if(Blast_rigid_body == null){
																	SPHERE.AddComponent(typeof(Rigidbody));
																	Blast_rigid_body = SPHERE.GetComponent(typeof(Rigidbody)) as Rigidbody;
																}

																if(Blast_rigid_body !=null){
																	Blast_rigid_body.useGravity = true;
																	Blast_rigid_body.mass = Rigid_mass;
																	if(Remove_rigid_constraints){
																		Blast_rigid_body.isKinematic = false;
																		Blast_rigid_body.constraints = RigidbodyConstraints.None;
																	}
																}
																Shooter.projectile = SPHERE.GetComponent<Rigidbody>();

															}else{
																Colliders[0].radius = Blast_Collider_radius;
																Colliders[0].isTrigger = Blast_Collider_trigger;

																Rigidbody Blast_rigid_body = Colliders[0].gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
																if(Blast_rigid_body == null){
																	Colliders[0].gameObject.AddComponent(typeof(Rigidbody));
																	Blast_rigid_body = Colliders[0].gameObject.GetComponent(typeof(Rigidbody)) as Rigidbody;
																}

																if(Blast_rigid_body !=null){
																	Blast_rigid_body.useGravity = true;
																	Blast_rigid_body.mass = Rigid_mass;
																	if(Remove_rigid_constraints){
																		Blast_rigid_body.isKinematic = false;
																		Blast_rigid_body.constraints = RigidbodyConstraints.None;
																	}
																}
																Shooter.projectile = Colliders[0].gameObject.GetComponent<Rigidbody>();
															}
															

														}

													}

											if(ParticleDelay > 0){
												ParticleSystem InstanceParticle = SPHERE.GetComponent(typeof(ParticleSystem)) as ParticleSystem;

												if(InstanceParticle !=null){
															ParticleSystem.MainModule mainModule = InstanceParticle.main;
															ParticleSystem.MinMaxCurve Curve1 = mainModule.startDelay;
															Curve1.constant = ParticleDelay;
															//InstanceParticle.main.startDelay.constant = ParticleDelay;												
												}
												
												foreach(ParticleSystem ParticlesA in SPHERE.GetComponentsInChildren<ParticleSystem>(true))
												{
													ParticleSystem.MainModule mainModule = ParticlesA.main;
													ParticleSystem.MinMaxCurve Curve1 = mainModule.startDelay;
													Curve1.constant = ParticleDelay;
													//ParticlesA.startDelay = ParticleDelay;
												}
											}

													Selection.activeGameObject = SPHERE;

										}else if(Add_on_collision){

													int found_dest=0;

													DestroyOnImpactTEM[] Destroyers = Selection.activeGameObject.GetComponentsInChildren<DestroyOnImpactTEM>(true);

													if(Destroyers!=null){
														if(Destroyers.Length > 0){
															found_dest = Destroyers.Length;
														}
													}

//											foreach(DestroyOnImpactTEM Destroyer in Selection.activeGameObject.GetComponentsInChildren<DestroyOnImpactTEM>(true))
//											{
//														//Destroyer.To_Spawn = ;
//														found_dest++;
//											}
													if(found_dest == 0){
														Debug.Log ("Please add a collision script from the step above");
													}else{
														GameObject TEMP = Loaded_prefab_item as GameObject;
														GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item,Selection.activeGameObject.transform.position,TEMP.transform.rotation);
														SPHERE.name = Descriptions[counter];
														SPHERE.SetActive(false);
														SPHERE.transform.parent = Selection.activeGameObject.transform;
														foreach(DestroyOnImpactTEM Destroyer in Selection.activeGameObject.GetComponentsInChildren<DestroyOnImpactTEM>(true))
														{
															Destroyer.To_Spawn = SPHERE;
															SPHERE.transform.parent = Destroyer.gameObject.transform;
															//found_dest++;
														}

														//world space use for correct blasts
														if(Blast_world_space){
															foreach(ParticleSystem ParticleS in SPHERE.GetComponentsInChildren<ParticleSystem>(true))
															{
																//ParticleS.main.simulationSpace = ParticleSystemSimulationSpace.World;
																//v2.3 PDM
																ParticleSystem.MainModule mainModule = ParticleS.main;
																mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
															}
														}
														if(Blast_radial){
															foreach(ParticleSystem ParticleParent in SPHERE.GetComponentsInChildren<ParticleSystem>(true))
															{
																SerializedObject SerializedParticle = new SerializedObject(ParticleParent);
																SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("ShapeModule");
																SerializedParticleP.FindPropertyRelative("type").intValue = 0; 
																//ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
																SerializedParticle.ApplyModifiedProperties();

																//ParticleParent.renderer.me
																//ParticleSystemRenderer RENDERER1 = ParticleParent.GetComponent(typeof(ParticleSystemRenderer)) as  ParticleSystemRenderer;
																//SerializedObject SerializedParticleMAT = new SerializedObject(RENDERER1);
																//SerializedParticleMAT.FindProperty("m_Mesh").objectReferenceValue = GameObject.CreatePrimitive(PrimitiveType.Sphere);
															}
														}

													}
													if(found_dest > 1){
														Debug.Log ("More than one destroyers found, parenting on the last one");
													}

										}
									}
								}//END if !Copy_properties_mode
								else{									

									if(Selection.activeGameObject!=null){
										GameObject ParticleHolder = Selection.activeGameObject;		
										ParticleSystem ParticleParent = ParticleHolder.GetComponent(typeof(ParticleSystem)) as ParticleSystem;

										GameObject ParticleHolderSOURCE = Loaded_prefab_item as GameObject;
										ParticleSystem ParticleParentSOURCE = ParticleHolderSOURCE.GetComponent<ParticleSystem>();
										
										if(ParticleParentSOURCE == null){
											ParticleParentSOURCE = ParticleHolderSOURCE.GetComponentInChildren(typeof(ParticleSystem)) as ParticleSystem;
										}

										if(ParticleParent!=null & ParticleParentSOURCE!=null){	

//													Object[] ToUndo = new Object[2];
//													ToUndo[0] = ParticleParentSOURCE as Object;
//													ToUndo[1] = ParticleParent as Object;
//													
//													Undo.RecordObjects(ToUndo,"Props copy");

											ApplyParticleChanges(ParticleHolderSOURCE,ParticleParentSOURCE,ParticleHolder,ParticleParent);

										}else{
											Debug.Log ("Please select a particle to copy to and insert a particle in the 'Copy From' field");
										}
									}else{										
										Debug.Log ("Please select a particle to copy to and insert a particle in the 'Copy From' field");										
									}

								}//END if Copy_properties_mode
							  }//END if !preview mode
							  else{
									//handle LIVE preview
									//EditorApplication.ExecuteMenuItem("Edit/Play");
									if(Application.isPlaying){

										Camera.main.cullingMask = (1 << LayerMask.NameToLayer("TEM_Sheet_Maker")) ;

										Camera.main.backgroundColor = new Color(140f/255f,60f/255f,80f/255f,255f/255f);


										GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item,Camera.main.transform.position,Camera.main.transform.rotation);
										SPHERE.name = Descriptions[counter];
										SPHERE.transform.parent = Camera.main.transform;
										SPHERE.transform.position = Camera.main.transform.position + 15*Camera.main.transform.forward;
										SPHERE.transform.parent = null;

										MoveToLayer(SPHERE.transform, LayerMask.NameToLayer("TEM_Sheet_Maker"));

										//select item in hierarchy
										Selection.activeGameObject = SPHERE;
									}else{
										Debug.Log ("Please enter play mode to preview effects");
									}
							  }
							}
						}
					}	
					counter = counter+1;

					if(completed_5 == 4){
						completed_5=0;
						quad_counter=quad_counter+1;
					}else{
						completed_5=completed_5+1;
					}

				}
				
				GUI.EndScrollView();

			}
			#endregion
		
		}	
		}
		
	}//END OnGUI


	void MoveToLayer(Transform root, int layer) {
		root.gameObject.layer = layer;
		foreach(Transform child in root)
			MoveToLayer(child, layer);
	}

	void ApplySubChanges(SerializedProperty SerializedParticleSOURCEP, SerializedProperty SerializedParticleP)
	{
		int count=0;
		while(true){
			
			//Debug.Log(SerializedParticleSOURCEP.name + " : " + SerializedParticleSOURCEP.propertyType + " : " + count);
			
			if(count==0){
				
			}else if(count > 0){
				//fix error of not supported type
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.AnimationCurve){
					SerializedParticleP.animationCurveValue = SerializedParticleSOURCEP.animationCurveValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.ArraySize){
					//SerializedParticleP.arraySize = SerializedParticleSOURCEP.arraySize;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Boolean){
					SerializedParticleP.boolValue = SerializedParticleSOURCEP.boolValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Bounds){
					SerializedParticleP.boundsValue = SerializedParticleSOURCEP.boundsValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Character){
					//SerializedParticleP.Character = SerializedParticleSOURCEP.;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Color){
					SerializedParticleP.colorValue = SerializedParticleSOURCEP.colorValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Enum){
					//SerializedParticleP.enumNames = SerializedParticleSOURCEP.enumNames;
					SerializedParticleP.enumValueIndex = SerializedParticleSOURCEP.enumValueIndex;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Float){
					SerializedParticleP.floatValue = SerializedParticleSOURCEP.floatValue;
				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Generic){
				
				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Gradient){
					
					while(true)
					{
						if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Color){
							SerializedParticleP.colorValue = SerializedParticleSOURCEP.colorValue;
							//Debug.Log ("IN5");
						}
						if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Integer){
							SerializedParticleP.intValue = SerializedParticleSOURCEP.intValue;

						}
						
						SerializedParticleSOURCEP.Next(true);
						SerializedParticleP.Next(true);
						
						if(SerializedParticleSOURCEP.depth < 3){
							break;
						}						
					}
				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Integer){
					SerializedParticleP.intValue = SerializedParticleSOURCEP.intValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.LayerMask){

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.ObjectReference){
					SerializedParticleP.objectReferenceValue = SerializedParticleSOURCEP.objectReferenceValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Quaternion){
					SerializedParticleP.quaternionValue = SerializedParticleSOURCEP.quaternionValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Rect){
					SerializedParticleP.rectValue = SerializedParticleSOURCEP.rectValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.String){
					SerializedParticleP.stringValue = SerializedParticleSOURCEP.stringValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Vector2){
					SerializedParticleP.vector2Value = SerializedParticleSOURCEP.vector2Value;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Vector3){
					SerializedParticleP.vector3Value = SerializedParticleSOURCEP.vector3Value;

				}	
			}
			
			if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Gradient){
				SerializedParticleSOURCEP.Next(true);
				SerializedParticleP.Next(true);
			}else{
				SerializedParticleSOURCEP.Next(true);
				SerializedParticleP.Next(true);
				
			}
			
			if(SerializedParticleSOURCEP.depth <=1){
				break;
			}
			count++;
			
		}
		
		SerializedParticleP.serializedObject.ApplyModifiedProperties();
		
	}


	void ApplyChanges(SerializedProperty SerializedParticleSOURCEP, SerializedProperty SerializedParticleP)
	{

		int count=0;
		while(true){

			if(count==1 ){

			}else if(count > 1){
				//fix error of not supported type
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.AnimationCurve){
					SerializedParticleP.animationCurveValue = SerializedParticleSOURCEP.animationCurveValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.ArraySize){
					SerializedParticleP.arraySize = SerializedParticleSOURCEP.arraySize;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Boolean){
					SerializedParticleP.boolValue = SerializedParticleSOURCEP.boolValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Bounds){
					SerializedParticleP.boundsValue = SerializedParticleSOURCEP.boundsValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Character){

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Color){
					SerializedParticleP.colorValue = SerializedParticleSOURCEP.colorValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Enum){

					SerializedParticleP.enumValueIndex = SerializedParticleSOURCEP.enumValueIndex;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Float){
					SerializedParticleP.floatValue = SerializedParticleSOURCEP.floatValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Generic){

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Gradient){
				
					while(true)
					{
						if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Color){
							SerializedParticleP.colorValue = SerializedParticleSOURCEP.colorValue;

						}
						if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Integer){
							SerializedParticleP.intValue = SerializedParticleSOURCEP.intValue;

						}

						SerializedParticleSOURCEP.Next(true);
						SerializedParticleP.Next(true);
						
						if(SerializedParticleSOURCEP.depth < 3){
							break;
						}						
					}
				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Integer){
					SerializedParticleP.intValue = SerializedParticleSOURCEP.intValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.LayerMask){
				
				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.ObjectReference){
					SerializedParticleP.objectReferenceValue = SerializedParticleSOURCEP.objectReferenceValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Quaternion){
					SerializedParticleP.quaternionValue = SerializedParticleSOURCEP.quaternionValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Rect){
					SerializedParticleP.rectValue = SerializedParticleSOURCEP.rectValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.String){
					SerializedParticleP.stringValue = SerializedParticleSOURCEP.stringValue;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Vector2){
					SerializedParticleP.vector2Value = SerializedParticleSOURCEP.vector2Value;

				}
				if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Vector3){
					SerializedParticleP.vector3Value = SerializedParticleSOURCEP.vector3Value;

				}	
			}

			if(SerializedParticleSOURCEP.propertyType == SerializedPropertyType.Gradient){
				SerializedParticleSOURCEP.Next(true);
				SerializedParticleP.Next(true);
			}else{
				SerializedParticleSOURCEP.NextVisible(true);
				SerializedParticleP.NextVisible(true);
				
			}
			
			if(SerializedParticleSOURCEP.depth == 0){
				break;
			}
			count++;

		}
		
		SerializedParticleP.serializedObject.ApplyModifiedProperties();

	}


	void ApplyParticleChanges(GameObject ParticleHolderSOURCE, ParticleSystem ParticleParentSOURCE, 
	                          GameObject ParticleHolder, ParticleSystem ParticleParent
	)
	{

		SerializedObject SerializedParticle = new SerializedObject(ParticleParent);		
		SerializedObject SerializedParticleSOURCE = new SerializedObject(ParticleParentSOURCE);	

		if(Assign_Looping){	

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("looping");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("looping");
			SerializedParticleP.boolValue = SerializedParticleSOURCEP.boolValue;
		}
		if(Assign_Prewarm){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("prewarm");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("prewarm");
			SerializedParticleP.boolValue = SerializedParticleSOURCEP.boolValue;
		}
		if(Assign_PlayOnAwake){		

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("playOnAwake");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("playOnAwake");
			SerializedParticleP.boolValue = SerializedParticleSOURCEP.boolValue;
		}
		if(Assign_Start_Delay){		

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("startDelay");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("startDelay");
			SerializedParticleP.floatValue = SerializedParticleSOURCEP.floatValue;
		}
		if(Assign_Start_Lifetime){		

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("InitialModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("InitialModule");
			SerializedProperty SerializedParticleP1 =  SerializedParticleP.FindPropertyRelative("startLifetime");
			SerializedProperty SerializedParticleSOURCEP1 =  SerializedParticleSOURCEP.FindPropertyRelative("startLifetime");
			ApplySubChanges(SerializedParticleSOURCEP1, SerializedParticleP1);

			SerializedProperty SerializedParticlePA =  SerializedParticle.FindProperty("lengthInSec");
			SerializedProperty SerializedParticleSOURCEPA =  SerializedParticleSOURCE.FindProperty("lengthInSec");
			SerializedParticlePA.floatValue = SerializedParticleSOURCEPA.floatValue;
		}
		if(Assign_Start_Speed){		

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("InitialModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("InitialModule");
			SerializedProperty SerializedParticleP1 =  SerializedParticleP.FindPropertyRelative("startSpeed");
			SerializedProperty SerializedParticleSOURCEP1 =  SerializedParticleSOURCEP.FindPropertyRelative("startSpeed");
			ApplySubChanges(SerializedParticleSOURCEP1, SerializedParticleP1);

			SerializedProperty SerializedParticlePA =  SerializedParticle.FindProperty("speed");
			SerializedProperty SerializedParticleSOURCEPA =  SerializedParticleSOURCE.FindProperty("speed");
			SerializedParticlePA.floatValue = SerializedParticleSOURCEPA.floatValue;
		}
		if(Assign_Start_Rotation){		

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("InitialModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("InitialModule");
			SerializedProperty SerializedParticleP1 =  SerializedParticleP.FindPropertyRelative("startRotation");
			SerializedProperty SerializedParticleSOURCEP1 =  SerializedParticleSOURCEP.FindPropertyRelative("startRotation");
			ApplySubChanges(SerializedParticleSOURCEP1, SerializedParticleP1);
		}
		if(Assign_Gravity){		

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("InitialModule.gravityModifier");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("InitialModule.gravityModifier");
			SerializedParticleP.floatValue = SerializedParticleSOURCEP.floatValue;
		}
		if(Assign_Max_Particles){		

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("InitialModule.maxNumParticles");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("InitialModule.maxNumParticles");
			SerializedParticleP.intValue = SerializedParticleSOURCEP.intValue;
		}
		if(Assign_Inherit_velocity){		

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("InitialModule.inheritVelocity");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("InitialModule.inheritVelocity");
			SerializedParticleP.floatValue = SerializedParticleSOURCEP.floatValue;
		}
		if(Assign_Simulation_Space){

			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("moveWithTransform");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("moveWithTransform");
			SerializedParticleP.boolValue = SerializedParticleSOURCEP.boolValue;
		}


		if(Assign_Start_Size){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("InitialModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("InitialModule");
			SerializedProperty SerializedParticleP1 =  SerializedParticleP.FindPropertyRelative("startSize");
			SerializedProperty SerializedParticleSOURCEP1 =  SerializedParticleSOURCEP.FindPropertyRelative("startSize");
			ApplySubChanges(SerializedParticleSOURCEP1, SerializedParticleP1);

		}
		if(Assign_Start_Color){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("InitialModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("InitialModule");
			SerializedProperty SerializedParticleP1 =  SerializedParticleP.FindPropertyRelative("startColor");
			SerializedProperty SerializedParticleSOURCEP1 =  SerializedParticleSOURCEP.FindPropertyRelative("startColor");
			ApplySubChanges(SerializedParticleSOURCEP1, SerializedParticleP1);
		}


		if(Assign_Emission_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("EmissionModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("EmissionModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_Shape_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("ShapeModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("ShapeModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_LimitVelocity_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("ClampVelocityModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("ClampVelocityModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_ForceOverLifetime_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("ForceModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("ForceModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_ColorBySpeed_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("ColorBySpeedModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("ColorBySpeedModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_SizeOverLifetime_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("SizeModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("SizeModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if( Assign_SizeBySpeed_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("SizeBySpeedModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("SizeBySpeedModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_RotOverLifetime_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("RotationModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("RotationModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_RotBySpeed_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("RotationBySpeedModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("RotationBySpeedModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_ExternalForces_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("ExternalForcesModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("ExternalForcesModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_Collision_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("CollisionModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("CollisionModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_TextureSheet_Group & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("UVModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("UVModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}

		if(Assign_velocity & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("VelocityModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("VelocityModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		if(Assign_LifetimeColor & 1==1){
			SerializedProperty SerializedParticleP =  SerializedParticle.FindProperty("ColorModule");
			SerializedProperty SerializedParticleSOURCEP =  SerializedParticleSOURCE.FindProperty("ColorModule");
			SerializedParticleP.FindPropertyRelative("enabled").boolValue = SerializedParticleSOURCEP.FindPropertyRelative("enabled").boolValue;
			ApplyChanges(SerializedParticleSOURCEP, SerializedParticleP);
		}
		
		if(Assign_SubEmitters_Group & 1==1){

			SerializedProperty P =  SerializedParticle.FindProperty("SubModule");
			SerializedProperty SOURCEP =  SerializedParticleSOURCE.FindProperty("SubModule");
			P.FindPropertyRelative("enabled").boolValue = SOURCEP.FindPropertyRelative("enabled").boolValue;
			
			SerializedProperty SerializedParticleSOURCE_OBJ =  SOURCEP.FindPropertyRelative("subEmitterBirth");
			if(SerializedParticleSOURCE_OBJ.objectReferenceValue !=null){

				//Undo.IncrementCurrentGroup();

				ParticleSystem AAA = (ParticleSystem)SerializedParticleSOURCE_OBJ.objectReferenceValue;
				if(AAA != null){
					GameObject ToInstantiate = AAA.gameObject;
					GameObject Instance = (GameObject)Instantiate(ToInstantiate,ToInstantiate.transform.position,ToInstantiate.transform.rotation);

					//Undo.IncrementCurrentGroup();

					//Undo.RegisterCreatedObjectUndo(Instance,"Subemitter");

					//Undo.IncrementCurrentGroup();

//					Object[] ToUndo = new Object[2];
//					ToUndo[0] = ParticleParentSOURCE as Object;
//					ToUndo[1] = ParticleParent as Object;					
//					Undo.RecordObjects(ToUndo,"Props copy");
					
					if(Instance !=null){
						Instance.transform.parent = ParticleHolder.transform;
						SerializedProperty SerializedParticle_OBJ =  P.FindPropertyRelative("subEmitterBirth");

						//Undo.RecordObject(ParticleHolder as Object,"Props copy");

						SerializedParticle_OBJ.objectReferenceValue = Instance;
						SerializedParticle.ApplyModifiedProperties();
					}

				}
			}
			SerializedProperty SerializedParticleSOURCE_OBJ1 =  SOURCEP.FindPropertyRelative("subEmitterBirth1");
			if(SerializedParticleSOURCE_OBJ1.objectReferenceValue !=null){
				ParticleSystem AAA = (ParticleSystem)SerializedParticleSOURCE_OBJ1.objectReferenceValue;
				GameObject ToInstantiate = AAA.gameObject;
				GameObject Instance = (GameObject)Instantiate(ToInstantiate,ToInstantiate.transform.position,ToInstantiate.transform.rotation);
				Instance.transform.parent = ParticleHolder.transform;
				SerializedProperty SerializedParticle_OBJ =  P.FindPropertyRelative("subEmitterBirth1");
				SerializedParticle_OBJ.objectReferenceValue = Instance;
				SerializedParticle.ApplyModifiedProperties();
			}
			
			SerializedProperty SerializedParticleSOURCE_OBJ2 =  SOURCEP.FindPropertyRelative("subEmitterCollision");
			if(SerializedParticleSOURCE_OBJ2.objectReferenceValue !=null){
				ParticleSystem AAA = (ParticleSystem)SerializedParticleSOURCE_OBJ2.objectReferenceValue;
				GameObject ToInstantiate = AAA.gameObject;
				GameObject Instance = (GameObject)Instantiate(ToInstantiate,ToInstantiate.transform.position,ToInstantiate.transform.rotation);
				Instance.transform.parent = ParticleHolder.transform;
				SerializedProperty SerializedParticle_OBJ =  P.FindPropertyRelative("subEmitterCollision");
				SerializedParticle_OBJ.objectReferenceValue = Instance;
				SerializedParticle.ApplyModifiedProperties();
			}
			SerializedProperty SerializedParticleSOURCE_OBJ3 =  SOURCEP.FindPropertyRelative("subEmitterCollision1");
			if(SerializedParticleSOURCE_OBJ3.objectReferenceValue !=null){
				ParticleSystem AAA = (ParticleSystem)SerializedParticleSOURCE_OBJ3.objectReferenceValue;
				GameObject ToInstantiate = AAA.gameObject;
				GameObject Instance = (GameObject)Instantiate(ToInstantiate,ToInstantiate.transform.position,ToInstantiate.transform.rotation);
				Instance.transform.parent = ParticleHolder.transform;
				SerializedProperty SerializedParticle_OBJ =  P.FindPropertyRelative("subEmitterCollision1");
				SerializedParticle_OBJ.objectReferenceValue = Instance;
				SerializedParticle.ApplyModifiedProperties();
			}
			
			SerializedProperty SerializedParticleSOURCE_OBJ4 =  SOURCEP.FindPropertyRelative("subEmitterDeath");
			if(SerializedParticleSOURCE_OBJ4.objectReferenceValue !=null){
				ParticleSystem AAA = (ParticleSystem)SerializedParticleSOURCE_OBJ4.objectReferenceValue;
				GameObject ToInstantiate = AAA.gameObject;
				GameObject Instance = (GameObject)Instantiate(ToInstantiate,ToInstantiate.transform.position,ToInstantiate.transform.rotation);
				Instance.transform.parent = ParticleHolder.transform;
				SerializedProperty SerializedParticle_OBJ =  P.FindPropertyRelative("subEmitterDeath");
				SerializedParticle_OBJ.objectReferenceValue = Instance;
				SerializedParticle.ApplyModifiedProperties();
			}
			SerializedProperty SerializedParticleSOURCE_OBJ5 =  SOURCEP.FindPropertyRelative("subEmitterDeath1");
			if(SerializedParticleSOURCE_OBJ5.objectReferenceValue !=null){
				ParticleSystem AAA = (ParticleSystem)SerializedParticleSOURCE_OBJ5.objectReferenceValue;
				GameObject ToInstantiate = AAA.gameObject;
				GameObject Instance = (GameObject)Instantiate(ToInstantiate,ToInstantiate.transform.position,ToInstantiate.transform.rotation);
				Instance.transform.parent = ParticleHolder.transform;
				SerializedProperty SerializedParticle_OBJ =  P.FindPropertyRelative("subEmitterDeath1");
				SerializedParticle_OBJ.objectReferenceValue = Instance;
				SerializedParticle.ApplyModifiedProperties();
			}
		}
		
		if(Assign_Material){
			ParticleSystemRenderer RENDERER1 = ParticleParent.GetComponent(typeof(ParticleSystemRenderer)) as  ParticleSystemRenderer;
			ParticleSystemRenderer RENDERER2 = ParticleParentSOURCE.GetComponent(typeof(ParticleSystemRenderer)) as  ParticleSystemRenderer;

			SerializedObject SerializedParticleMAT = new SerializedObject(RENDERER1);		
			SerializedObject SerializedParticleSOURCEMAT = new SerializedObject(RENDERER2);
								
			SerializedParticleMAT.FindProperty("m_CastShadows").boolValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_CastShadows").boolValue;
			SerializedParticleMAT.FindProperty("m_ReceiveShadows").boolValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_ReceiveShadows").boolValue; 
			
			SerializedParticleMAT.FindProperty("m_UseLightProbes").boolValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_UseLightProbes").boolValue;
			SerializedParticleMAT.FindProperty("m_LightProbeAnchor").objectReferenceValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_LightProbeAnchor").objectReferenceValue; 
			
			SerializedParticleMAT.FindProperty("m_RenderMode").intValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_RenderMode").intValue;
			SerializedParticleMAT.FindProperty("m_MaxParticleSize").floatValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_MaxParticleSize").floatValue; 
			
			SerializedParticleMAT.FindProperty("m_CameraVelocityScale").floatValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_CameraVelocityScale").floatValue; 
			SerializedParticleMAT.FindProperty("m_VelocityScale").floatValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_VelocityScale").floatValue; 
			
			SerializedParticleMAT.FindProperty("m_LengthScale").floatValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_LengthScale").floatValue; 
			SerializedParticleMAT.FindProperty("m_NormalDirection").floatValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_NormalDirection").floatValue; 
			
			SerializedParticleMAT.FindProperty("m_Mesh").objectReferenceValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_Mesh").objectReferenceValue; 
			SerializedParticleMAT.FindProperty("m_Mesh1").objectReferenceValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_Mesh1").objectReferenceValue; 
			SerializedParticleMAT.FindProperty("m_Mesh2").objectReferenceValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_Mesh2").objectReferenceValue; 
			SerializedParticleMAT.FindProperty("m_Mesh3").objectReferenceValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_Mesh3").objectReferenceValue; 

			SerializedParticleMAT.FindProperty("m_SortMode").intValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_SortMode").intValue;
			SerializedParticleMAT.FindProperty("m_SortingFudge").floatValue = 
				SerializedParticleSOURCEMAT.FindProperty("m_SortingFudge").floatValue; 
		
			SerializedProperty SEP1 = SerializedParticleMAT.FindProperty("m_Materials"); 
			SerializedProperty SEP2 = SerializedParticleSOURCEMAT.FindProperty("m_Materials");

			SerializedProperty SEP11 = SEP1.FindPropertyRelative("Array");
			SerializedProperty SEP22 = SEP2.FindPropertyRelative("Array");
			SerializedProperty SEP111 = SEP11.GetArrayElementAtIndex(0);
			SerializedProperty SEP222 = SEP22.GetArrayElementAtIndex(0);

			SEP111.objectReferenceValue = SEP222.objectReferenceValue;
			
			SerializedParticleMAT.ApplyModifiedProperties();
		}
				
	}

}