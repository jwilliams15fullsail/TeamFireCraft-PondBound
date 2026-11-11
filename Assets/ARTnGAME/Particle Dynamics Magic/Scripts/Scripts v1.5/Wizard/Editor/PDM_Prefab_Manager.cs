using UnityEngine;
using System.IO;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class PDM_Prefab_Manager : EditorWindow
	{

		[MenuItem("Window/Particle Dynamic Magic/Effects Library 1")]
		public static void ShowWindow()
		{
			PDM_Prefab_Manager PDM_Wizard = GetWindow<PDM_Prefab_Manager>();
			//PDM_Wizard.title = "Particle Dynamic Magic";
			PDM_Wizard.titleContent.text = "Particle Dynamic Magic";
			PDM_Wizard.Show();
		}
		//public static string PrefabWizard_path = "Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/";
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

		void Draw_Icons()
		{

			PDM_Prefabs_icons = new List<Texture2D>();
			Counter_starting_points = new List<int>();
			Descriptions = new List<string>();

			//CATEGORIES
			Category_Icons = new List<Texture2D>();
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_SPLINES.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_AURAS 1.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_VORTEXES 1.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_SWORDS 1.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_SKINNED_MESH 1.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_PAINT_MANIP 1.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_FIRE_ICE.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_PROJECTILES 1.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_IMAGE_EMISSION 1.jpg", typeof(Texture2D)) as Texture2D);
			Category_Icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/DEMO_ICON_TRANSITIONS.jpg", typeof(Texture2D)) as Texture2D);
			Counter_starting_points.Add(0);

			//SUB CATEGORIES 
			Loaded_Prefabs = new List<string>();

			int START_COUNTING = 0;

			// SPLINES
			// SPLINES
			// SPLINES
			//1 BASE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab1.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Base Spline");///
			START_COUNTING += 1;
			//2 PLAY MODE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab2.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Play mode Spline");///
			START_COUNTING += 1;
			//3 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab3.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Editable Spline");///
			START_COUNTING += 1;
			//4 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab4.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Editable Spline");///
			START_COUNTING += 1;
			//5 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab5.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Medusa");///
			START_COUNTING += 1;
			//6 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab6.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Phoenix");///
			START_COUNTING += 1;
			//7 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab7.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Spline way");///
			START_COUNTING += 1;
			//8 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab8.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Phoenix");///
			START_COUNTING += 1;
			//9 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab9.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Web");///
			START_COUNTING += 1;
			//10 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab10.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Hair");///
			START_COUNTING += 1;
			//11 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab11.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Hair on model");///
			START_COUNTING += 1;
			//12 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab12.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Spline emission from parent spline points");///
			START_COUNTING += 1;
			//13 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab13.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab13.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Umbrela");///
			START_COUNTING += 1;
			//14 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab14.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab14.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Curtain");///
			START_COUNTING += 1;
			//15 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab15.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab15.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Letters");///
			START_COUNTING += 1;
			///////////15 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab16.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/SPLINES_16.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Turbulence on spline particles");///
			START_COUNTING += 1;
			///////////15 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab17.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/SPLINES_17.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Particle emission from spline");///
			START_COUNTING += 1;

			Counter_starting_points.Add(START_COUNTING);


			//AURAS - FLOWS
			//AURAS - FLOWS
			//AURAS - FLOWS
			//16 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab16.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Target aura");///
			START_COUNTING += 1;
			//17 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab17.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Wide target aura");///
			START_COUNTING += 1;
			//18 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab18.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("White target");///
			START_COUNTING += 1;
			//19 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab19.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fancy Target");///
			START_COUNTING += 1;
			//20 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab20.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Energy Aura");///
			START_COUNTING += 1;
			//21 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab21.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire Aura");///
			START_COUNTING += 1;
			//22 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab22.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire ring - ground");///
			START_COUNTING += 1;
			//23 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab23.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire blast aura");///
			START_COUNTING += 1;
			//24 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab24.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire ring");///
			START_COUNTING += 1;
			//25 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab25.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Butterfly aura");///
			START_COUNTING += 1;
			//26 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab26.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Dome aura");///
			START_COUNTING += 1;
			//27 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab27.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Aura Combination");///
			START_COUNTING += 1;
			//28 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_13.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab28.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Hovering aura");///
			START_COUNTING += 1;
			//29 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_14.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab29.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Flowers");///
			START_COUNTING += 1;
			//30 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/AURA_15.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab30.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Lava flow");///
			START_COUNTING += 1;

			Counter_starting_points.Add(START_COUNTING);


			//TURBULENCE
			//TURBULENCE
			//TURBULENCE
			//31 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab31.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Vortex formation");///
			START_COUNTING += 1;
			//32 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab32.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Splash effects");///
			START_COUNTING += 1;
			//33 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab33.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Noisy Turbulence");///
			START_COUNTING += 1;
			//34 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab34.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Planar Gravity");///
			START_COUNTING += 1;
			//35 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab35.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Gravity effect");///
			START_COUNTING += 1;
			//36 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab36.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Paint splash effect");///
			START_COUNTING += 1;
			//37 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab37.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Turbulence painting");///
			START_COUNTING += 1;
			//38 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab38.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Gravity multi particle");///
			START_COUNTING += 1;
			//39 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab39.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Gravity paint splash");///
			START_COUNTING += 1;
			//40 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab40.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Break fire turbulence");///
			START_COUNTING += 1;
			//41 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab41.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Comet");///
			START_COUNTING += 1;
			//42 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab42.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Splash paint");///
			START_COUNTING += 1;
			//43 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_13.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab43.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Radiating");///
			START_COUNTING += 1;
			//44 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_14.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab44.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Splash trail");///
			START_COUNTING += 1;
			//45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_15.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab45.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Feather fire");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_16.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_16.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Logo Turbulence");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_17.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_17.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Painting Turbulence fast mode");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_18.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_18.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Playfull Turbulence");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_19.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_19.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Nerves");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_20.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_20.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Nerves Complex");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_21.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_21.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Wild Fire");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_22.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_22.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Ripples in trail");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_23.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_23.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Color effects");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_24.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_24.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Ripple paint");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_25.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_25.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Painting letters");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_26.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_26.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Logo explode");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_27.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_27.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Logo explode - high frequency");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TURB_28.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TURB_28.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire with Turbulence");///
			START_COUNTING += 1;

			Counter_starting_points.Add(START_COUNTING);


			//WEAPONS
			//WEAPONS
			//WEAPONS
			//31 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab46.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Ice sword");///
			START_COUNTING += 1;
			//32 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab47.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire sword");///
			START_COUNTING += 1;
			//33 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab48.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire wand");///
			START_COUNTING += 1;
			//34 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab49.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire circles");///
			START_COUNTING += 1;
			//35 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab50.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Magic vortex");///
			START_COUNTING += 1;
			//36 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab51.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Repel effect");///
			START_COUNTING += 1;
			//37 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab52.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Magic orb - attractor effect");///
			START_COUNTING += 1;
			//38 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab53.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Enery wand");///
			START_COUNTING += 1;
			//39 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab54.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire shield");///
			START_COUNTING += 1;
			//40 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab55.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Water effects");///
			START_COUNTING += 1;
			//41 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab56.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire effects");///
			START_COUNTING += 1;
			//42 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab57.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Space vortex");///
			START_COUNTING += 1;
			//43 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_13.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab58.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Space art");///
			START_COUNTING += 1;
			//44 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_14.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab59.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Magic vortex");///
			START_COUNTING += 1;
			//45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_15.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab60.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Repel Shield");///
			START_COUNTING += 1;
			////////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_16.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/WEAPONS_16.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Water shield (Unity PRO)");///
			START_COUNTING += 1;
			////////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_17.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/WEAPONS_17.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Water shield dripping (Unity PRO)");///
			START_COUNTING += 1;
			////////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_18.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/WEAPONS_18.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Water shield (Unity Free)");///
			START_COUNTING += 1;
			////////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_19.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/WEAPONS_19.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Magic circles");///
			START_COUNTING += 1;
			////////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_20.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/WEAPONS_20.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Portal wide (Unity PRO)");///
			START_COUNTING += 1;
			////////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/WEAPONS_21.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/WEAPONS_21.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Portal (Unity PRO)");///
			START_COUNTING += 1;

			Counter_starting_points.Add(START_COUNTING);


			//MESH
			//MESH
			//MESH
			//31 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab61.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Skinned Mesh emission masked (fire)");///
			START_COUNTING += 1;
			//32 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab62.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Skinned Mesh emission masked free");///
			START_COUNTING += 1;
			//33 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab63.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Skinned Mesh emission");///
			START_COUNTING += 1;
			//34 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab64.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Skinned Mesh emission free");///
			START_COUNTING += 1;
			//35 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab65.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Skinned Mesh emission masked (ice)");///
			START_COUNTING += 1;
			//36 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab66.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Procedural Mesh emission masked (ice)");///
			START_COUNTING += 1;
			//37 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab67.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Procedural Blob emission");///
			START_COUNTING += 1;
			//38 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab68.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Procedural Mesh emission");///
			START_COUNTING += 1;
			//39 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab69.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Procedural Mesh emission - hidden mesh");///
			START_COUNTING += 1;
			//40 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab70.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Procedural Mesh emission coloration effects");///
			START_COUNTING += 1;
			//41 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab71.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Skinned Mesh Particle & Gameobject emission, break & reform");///
			START_COUNTING += 1;
			//42 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab72.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Rubber spider");///
			START_COUNTING += 1;
			//43 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_13.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab73.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Gameobject emission from procedural mesh");///
			START_COUNTING += 1;
			//44 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_14.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab74.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Skinned mesh emission - fire");///
			START_COUNTING += 1;
			//45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_15.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab75.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Procedural mesh emission offset mode");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_16.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_16.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Targeting animated mesh with particles");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_17.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_17.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Animated mesh emission");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_18.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_18.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Cube particles");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_19.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_19.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Transparent cube particles");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_20.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_20.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Procedural follow");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_21.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_21.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Gravity and collisions");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_22.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_22.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Color cube emission");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_23.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_23.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Targeting procedural mesh with particles");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_24.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_24.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Colorfall");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_25.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_25.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Color tail");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_26.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_26.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Bio");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_27.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_27.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Looping effect");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_28.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_28.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Tunnel");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_29.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_29.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Targeting animated mesh with particles - coloration");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_30.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_30.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("The eye");///
			START_COUNTING += 1;
			//////46 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/MESH_31.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/MESH_31.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Splash ball");///
			START_COUNTING += 1;


			Counter_starting_points.Add(START_COUNTING);


			//PROJECTION
			//PROJECTION
			//PROJECTION
			//31 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab76.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Grass (gameobject) and (flower) particle projection");///
			START_COUNTING += 1;
			//32 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab77.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Lightning cicle:projection, gravity & lightning combo");///
			START_COUNTING += 1;
			//33 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab78.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire cicle:gameobject projection, gravity & fire propagation");///
			START_COUNTING += 1;
			//34 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab79.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Whale flock (requires free Whale Asset from store)");///
			START_COUNTING += 1;
			//35 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab80.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Flocking dynamics and control");///
			START_COUNTING += 1;
			//36 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab81.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Multisize flock");///
			START_COUNTING += 1;
			//37 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab82.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Flowers");///
			START_COUNTING += 1;
			//38 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab83.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Swirling gameobjects");///
			START_COUNTING += 1;
			//39 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab84.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Tornado in flower field");///
			START_COUNTING += 1;
			//40 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab85.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Water dynamics");///
			START_COUNTING += 1;
			//41 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTION_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab86.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Water dynamics and refraction (Unity Pro)");///
			START_COUNTING += 1;
			//42 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab87.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Empty");///
			START_COUNTING += 1;
			//43 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab88.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Empty");///
			START_COUNTING += 1;
			//44 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab89.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Empty");///
			START_COUNTING += 1;
			//45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab90.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Empty");///
			START_COUNTING += 1;


			Counter_starting_points.Add(START_COUNTING);


			//FIRE & ICE
			//FIRE & ICE
			//FIRE & ICE
			//31 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab91.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Spray fire");///
			START_COUNTING += 1;
			//32 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab92.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Spray Ice");///
			START_COUNTING += 1;
			//33 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab93.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Chain Lightning");///
			START_COUNTING += 1;
			//34 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab94.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Particle to Particle Collision (with self collision)");///
			START_COUNTING += 1;
			//35 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab95.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Gameobject painting (with brush)");///
			START_COUNTING += 1;
			//36 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab96.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Gameobject painting");///
			START_COUNTING += 1;
			//37 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab97.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Animated Butterfly spray");///
			START_COUNTING += 1;
			//38 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab98.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Animated Butterfly spray and local fly away");///
			START_COUNTING += 1;
			//39 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab99.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Freeze effects");///
			START_COUNTING += 1;
			//40 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab100.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Particle to Particle Collision (no self collision)");///
			START_COUNTING += 1;
			//41 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab101.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Dynamic grass-butterfly spread and local disturb effects");///
			START_COUNTING += 1;
			//42 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab102.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Melting ice with fire");///
			START_COUNTING += 1;
			//43 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_13.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab103.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Freeze and burn effects");///
			START_COUNTING += 1;
			//44 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_14.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab104.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Freeze effects");///
			START_COUNTING += 1;
			//45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/FIRE_ICE_15.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab105.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Burn wood");///
			START_COUNTING += 1;


			Counter_starting_points.Add(START_COUNTING);


			//PROJECTILES
			//PROJECTILES
			//PROJECTILES
			//31 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab106.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Paint ball");///
			START_COUNTING += 1;
			//32 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab107.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Turbulence in trail");///
			START_COUNTING += 1;
			//33 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab108.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Splash effects");///
			START_COUNTING += 1;
			//34 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab109.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Procedural head");///
			START_COUNTING += 1;
			//35 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab110.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Local vortexes");///
			START_COUNTING += 1;
			//36 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab111.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Firefall");///
			START_COUNTING += 1;
			//37 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab112.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Missiles");///
			START_COUNTING += 1;
			//38 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab113.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Missiles fast");///
			START_COUNTING += 1;
			//39 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab114.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Missiles Wild");///
			START_COUNTING += 1;
			//40 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab115.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Missiles - Trail with Turbulence");///
			START_COUNTING += 1;
			//41 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab116.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fire projectile - pool handled, stick effect");///
			START_COUNTING += 1;
			//42 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/PROJECTILES_12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab117.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Ice prohectile - pool handled stick effect");///
			START_COUNTING += 1;
			//43 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab118.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Empty");///
			START_COUNTING += 1;
			//44 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab119.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Empty");///
			START_COUNTING += 1;
			//45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Prefab3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab120.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Empty");///
			START_COUNTING += 1;


			Counter_starting_points.Add(START_COUNTING);


			//IMAGE
			//IMAGE
			//IMAGE
			//31 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab121.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Image emission - Depth map");///
			START_COUNTING += 1;
			//32 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab122.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Image emission - Depth map");///
			START_COUNTING += 1;
			//33 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab123.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Image emission - Depth map & Highlights");///
			START_COUNTING += 1;
			//34 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab124.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Image emission - Particle size");///
			START_COUNTING += 1;
			//35 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab125.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Image emission - Depth Grey Scale");///
			START_COUNTING += 1;
			//36 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab126.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Per Image point emission, with Turbulence");///
			START_COUNTING += 1;
			//37 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab127.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Per Image point emission, with Vortexes");///
			START_COUNTING += 1;
			//38 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab128.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Per Image point emission, side view");///
			START_COUNTING += 1;
			//39 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab129.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Logo image emission and reconstruction");///
			START_COUNTING += 1;
			//40 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab130.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Logo image emission and Turbulence");///
			START_COUNTING += 1;
			//41 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab131.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Sand storm");///
			START_COUNTING += 1;
			//42 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab132.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Snow storm");///
			START_COUNTING += 1;
			//43 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_13.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab133.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Heavy Snow storm");///
			START_COUNTING += 1;
			//44 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_14.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab134.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Tornado");///
			START_COUNTING += 1;
			//45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_15.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab135.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Lightning storm");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_16.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_16.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon fire, blast and spread");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_17.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_17.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon red blast");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_18.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_18.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon fire");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_19.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_19.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon fire narrow");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_20.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_20.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon trail");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_21.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_21.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon trail with sparks");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_22.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_22.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Red trail plus smoke");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_23.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_23.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Pink trail plus smoke");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_24.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_24.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Pink trail narrow plus smoke");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_25.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_25.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Smoke blast");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_26.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_26.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Dark blast");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_27.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_27.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon Blast A");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_28.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_28.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon Blast B");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_29.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_29.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon Blast C");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_30.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_30.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Angled toon fire");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_31.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_31.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon fire - wide");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_32.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_32.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon smoke");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_33.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_33.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon smoke and lava particles");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_34.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_34.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon smoke emission, blurred");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_35.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_35.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon smoke emission");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_36.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_36.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon smoke floor, blurred");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_37.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_37.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon smoke floor");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_38.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_38.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon smoke tall");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_39.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_39.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon bomb blast A");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_40.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_40.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon bomb blast B, lights");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_41.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_41.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon bomb blast C, blur");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_42.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_42.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Toon bomb blast D, more blur");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_43.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_43.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Vines emit from ground");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_44.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_44.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Ice falling");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_45.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_45.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Ice falling, stretched");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_46.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_46.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Ice floor emission");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_47.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_47.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Angled ice falling");///
			START_COUNTING += 1;
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/IMAGE_48.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/IMAGE_48.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Waterfall");///
			START_COUNTING += 1;

			Counter_starting_points.Add(START_COUNTING);


			//TRANSITIONS
			//TRANSITIONS
			//TRANSITIONS
			//31 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_1.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab136.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Mesh to Spline Transitions");///
			//32 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_2.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab137.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("MUSICles - Dancing particles");///
			//33 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_3.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab138.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Change particle event");///
			//34 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_4.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab139.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Regrowing field, grass and flowers");///
			//35 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_5.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab140.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Mushroom field, dynamic");///
			//36 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_6.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab141.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Transitions - Ice");///
			//37 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_7.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab142.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Transitions - Magma");///
			//38 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_8.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab143.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Transitions - Crystal Water (Unity PRO)");///
			//39 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_9.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab144.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Transitions - Ice thin");///
			//40 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_10.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab145.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Transitions - Multiple particle systems (Unity PRO)");///
			//41 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_11.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab146.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Change particle event");///
			//42 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_12.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab147.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Fountain");///
			//43 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_13.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab148.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Transitions - Dynamic particle system change");///
			//44 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_14.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab149.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Transitions - Smooth particle system change");///
			//45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_15.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab150.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Turbulant Transitions - Rock");///
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_16.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TRANSITIONS_16.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Turbulant Transitions - Ice");///
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_17.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TRANSITIONS_17.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("On the fly particle type change - Energy & Ice");///
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_18.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TRANSITIONS_18.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("On the fly particle type change - Fire & Rock");///
			//////////45 EDITABLE SPLINE
			Loaded_Prefabs.Add("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/TRANSITIONS_19.prefab");
			PDM_Prefabs_icons.Add(AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/TRANSITIONS_19.png", typeof(Texture2D)) as Texture2D);
			Descriptions.Add("Gameobject particle transitions");///

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

		void OnGUI()
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

			GUI.TextField(new Rect(0, 5, 80, CATEGORIES_Y), "Splines");
			if (GUI.Button(new Rect(0, CATEGORIES_Y + (activeY_extra * active1_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;

				counter = Spline_Category_Start;
				quad_counter = 0;

				//v1.6
				scrollPosition = Vector2.zero;
			}

			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Auras - Flows");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active2_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Turbulence - Forces");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active3_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Weapons, Spells,Gates");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active4_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Mesh - GameObject");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active5_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Projection, Flocks,Water");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active6_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Fire & Ice");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active7_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Projectiles");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active8_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Image,Toon, Weather");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active9_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			category_counter += 1;
			GUI.TextField(new Rect(0 + CATEGORIES_X_STEP * category_counter, 5, 80, CATEGORIES_Y), "Transitions");
			if (GUI.Button(new Rect(0 + CATEGORIES_X_STEP * category_counter, CATEGORIES_Y + (activeY_extra * active10_factor), 80, 80), Category_Icons[category_counter]))
			{
				//Debug.Log (PDM_Prefabs.Count);
				active_category = category_counter;
				quad_counter = 0;
				counter = Auras_Flows_Category_Start;
				//v1.6
				scrollPosition = Vector2.zero;
			}
			#endregion

			GUI.Box(new Rect(0, CATEGORIES_Y + 87, 800, 1), "");

			int Y_WIDTH = 43;
			int ICON_HEIGHT = 150;
			float text_to_icon_factor = 1f;
			int Y_offset = 0 + 25;
			int X_offset = 150;

			int item_count = 0;

			Texture2D Icon_Texture = AssetDatabase.LoadAssetAtPath("Assets/ARTnGAME/Particle Dynamics Magic/PrefabWizard/Icons/Prefab1.png", typeof(Texture2D)) as Texture2D;

			if (PDM_Prefabs_icons != null)
			{


				#region GLOBAL

				if (PDM_Prefabs_icons.Count > 0)
				{

					counter = Counter_starting_points[active_category];
					if (active_category == 9)
					{
						item_count = 19;
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
						category_height = (row_count + 1) * (ICON_HEIGHT + Y_WIDTH) + 30;
						//Debug.Log ("aa");
					}
					else
					{
						category_height = (row_count + 0) * (ICON_HEIGHT + Y_WIDTH) + 30;
					}
					//Debug.Log ("Items = " +item_count + "Rows = " +row_count +"Items/Rows = "+((float)item_count/5)+" ");

					scrollPosition = GUI.BeginScrollView(new Rect(0, CATEGORIES_Y + 85 + 5, 800, 640), scrollPosition, new Rect(0, 0, 1280, category_height), false, false);

					int completed_5 = 0;
					for (int i = 0; i < item_count; i++)
					{

						Icon_Texture = PDM_Prefabs_icons[counter];
						EditorStyles.textField.wordWrap = true;
						GUI.TextField(new Rect(10 + completed_5 * X_offset, Y_offset + 0 + (ICON_HEIGHT + Y_WIDTH) * quad_counter, text_to_icon_factor * ICON_HEIGHT, Y_WIDTH), Descriptions[counter]);
						if (GUI.Button(new Rect(10 + completed_5 * X_offset, Y_offset + Y_WIDTH + (ICON_HEIGHT + Y_WIDTH) * quad_counter, ICON_HEIGHT, ICON_HEIGHT), Icon_Texture))
						{
							if (Loaded_Prefabs.Count > counter)
							{
								Object Loaded_prefab_item = (Object)AssetDatabase.LoadAssetAtPath(Loaded_Prefabs[counter], typeof(Object));
								if (Loaded_prefab_item != null)
								{
									//GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item,Vector3.zero,Quaternion.identity);
									GameObject TEMP = Loaded_prefab_item as GameObject;
									GameObject SPHERE = (GameObject)Instantiate(Loaded_prefab_item, Vector3.zero, TEMP.transform.rotation);
									SPHERE.name = Descriptions[counter];
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

		}//END OnGUI

	}
}