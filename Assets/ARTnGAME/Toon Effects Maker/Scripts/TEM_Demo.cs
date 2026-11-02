using UnityEngine;
using System.Collections;

public class TEM_Demo: MonoBehaviour {

	#pragma warning disable 414

	GameObject SPHERE;

	void Start () {

		MODE = 3;
		PROJECTILES_COUNTER=0;
		
		PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true); 		
		
		SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject,PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position,PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);
		
		SPHERE.transform.parent=null;
		
		PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false); 
		
		for (int j =0; j< PROJECTILE_NAMES.Length;j++){
			
			if(j == PROJECTILES_COUNTER){
				PROJECTILE_NAMES[j].gameObject.SetActive(true);
			}else{PROJECTILE_NAMES[j].gameObject.SetActive(false);}
		}
		PROJECTILES_COUNTER=1;

		PROJECTILES_FIRE = PROJECTILES;
	}

	public GameObject[] PROJECTILES;
	public GameObject[] PROJECTILE_NAMES;

	public GameObject[] PROJECTILES_FIRE;
	public GameObject[] PROJECTILES_WATER;
	public GameObject[] PROJECTILES_DARKNESS;
	public GameObject[] PROJECTILES_ICONS;
	public GameObject[] PROJECTILES_NATURE_CREATURES;
	public GameObject[] PROJECTILES_AIR;


	public int PROJECTILES_COUNTER;
	public Texture  PROJECTILES_Texture;

	private int MODE = 0;
	public float Slide_left=0f;

	void OnGUI() {

		GUI.color = new Color32(255, 255, 255, 201);	


		
		int BOX_WIDTH = 70;
		int BOX_HEIGHT = 70;
		
		BOX_WIDTH = 100;

		if(1==1){

//		if (GUI.Button(new Rect(0*BOX_WIDTH+10, PIXELS_DOWN, BOX_WIDTH, BOX_HEIGHT), PROJECTILES_Texture)){
//			if(MODE ==3){
//				MODE = 0;
//				for (int i=0;i<PROJECTILES.Length;i++){
//					PROJECTILES[i].gameObject.SetActive(false); 
//				}
//				Destroy(SPHERE);
//			}else{
//				MODE = 3;}
//		}

			float To_right = 10;
			float To_down = 15;
		bool Reset = false;
		string Category = "Fire";
			if (GUI.Button(new Rect(0*BOX_WIDTH+To_right, To_down, BOX_WIDTH+0, 17), "Fire Effects")){
				PROJECTILES = new GameObject[PROJECTILES_FIRE.Length];
				PROJECTILES = PROJECTILES_FIRE;
				PROJECTILES_COUNTER = 1;
				Reset = true;
				Category = "Fire";
		}
			if (GUI.Button(new Rect(1*BOX_WIDTH+To_right,To_down, BOX_WIDTH+0, 17), "Water Effects")){
				PROJECTILES = new GameObject[PROJECTILES_WATER.Length];
				PROJECTILES = PROJECTILES_WATER;
				PROJECTILES_COUNTER = 1;
				Reset = true;
				Category = "Water";
			}
			if (GUI.Button(new Rect(2*BOX_WIDTH+To_right, To_down, BOX_WIDTH+0, 17), "Air Effects")){
				PROJECTILES = new GameObject[PROJECTILES_AIR.Length];
				PROJECTILES = PROJECTILES_AIR;
				PROJECTILES_COUNTER = 1;
				Reset = true;
				Category = "Air";
			}
			if (GUI.Button(new Rect(3*BOX_WIDTH+To_right, To_down, BOX_WIDTH+0, 17), "Nature Effects")){
				PROJECTILES = new GameObject[PROJECTILES_NATURE_CREATURES.Length];
				PROJECTILES = PROJECTILES_NATURE_CREATURES;
				PROJECTILES_COUNTER = 1;
				Reset = true;
				Category = "Nature";
			}
			if (GUI.Button(new Rect(4*BOX_WIDTH+To_right, To_down, BOX_WIDTH+0, 17), "Dark Effects")){
				PROJECTILES = new GameObject[PROJECTILES_DARKNESS.Length];
				PROJECTILES = PROJECTILES_DARKNESS;
				PROJECTILES_COUNTER = 1;
				Reset = true;
				Category = "Darkness";
			}
			if (GUI.Button(new Rect(5*BOX_WIDTH+To_right, To_down, BOX_WIDTH+0, 17), "Orbital Effects")){
				PROJECTILES = new GameObject[PROJECTILES_ICONS.Length];
				PROJECTILES = PROJECTILES_ICONS;
				PROJECTILES_COUNTER = 1;
				Reset = true;
				Category = "Orbital";
			}


		if(Reset){

				PROJECTILES_COUNTER = 0;

				for (int i=0;i<PROJECTILES.Length;i++){
					PROJECTILES[i].gameObject.SetActive(false); 
				}
				PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true); 				
				Destroy(SPHERE);				
				Transform Save_transf = PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.parent;
				PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.parent = null;				
				SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject,PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position,PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);
				SPHERE.transform.parent = Save_transf;				
				PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false);

				PROJECTILES_COUNTER = 1;
				Reset = false;
		}

			GUI.TextArea(new Rect(0*BOX_WIDTH+10, 1+35+22, BOX_WIDTH+10, 21),"Category: "+Category);




			BOX_WIDTH = 70;
			BOX_HEIGHT = 80;
			
			BOX_WIDTH = (int)Slide_left+BOX_WIDTH;



			GUI.TextArea(new Rect(0*BOX_WIDTH+10, 1+35, BOX_WIDTH+15, 21),"Effect "+PROJECTILES_COUNTER+"/"+PROJECTILES.Length);

		if(MODE==0 ){
							
			//3
			for (int i=0;i<PROJECTILES.Length;i++){
				PROJECTILES[i].gameObject.SetActive(false);
			}
			Destroy(SPHERE);			

		}else if(MODE!=0 ){}

		if(MODE==3){ //draw MODE 3  - PROJECTILES

			if (GUI.Button(new Rect(0*BOX_WIDTH+10, BOX_HEIGHT+50, BOX_WIDTH, 30), "Respawn")){

				Debug.Log (PROJECTILES_COUNTER);

				int COUNT_ME = PROJECTILES_COUNTER-1;
				if (PROJECTILES_COUNTER==0){

					COUNT_ME = PROJECTILES.Length-1;
				}
				PROJECTILES[COUNT_ME].gameObject.SetActive(true); 

					Destroy(SPHERE);

					//

					Transform Save_transf = PROJECTILES[COUNT_ME].gameObject.transform.parent;
					PROJECTILES[COUNT_ME].gameObject.transform.parent = null;

					SPHERE = (GameObject)Instantiate(PROJECTILES[COUNT_ME].gameObject,PROJECTILES[COUNT_ME].gameObject.transform.position,PROJECTILES[COUNT_ME].gameObject.transform.rotation);
					
					//SPHERE.transform.parent=null;
					SPHERE.transform.parent = Save_transf;
					
				PROJECTILES[COUNT_ME].gameObject.SetActive(false); 

					#region EXTRA

					for (int j =0; j< PROJECTILE_NAMES.Length;j++){
						
						if(j == PROJECTILES_COUNTER){
							PROJECTILE_NAMES[j].gameObject.SetActive(true);
						}else{PROJECTILE_NAMES[j].gameObject.SetActive(false);}
					}

					#endregion
			}
				#region BACK BUTTON

				if (GUI.Button(new Rect(0*BOX_WIDTH+10, BOX_HEIGHT+80, BOX_WIDTH, 30), "Back")){
					
					Debug.Log ("Projectile "+PROJECTILES_COUNTER);

					if(PROJECTILES_COUNTER == 0){PROJECTILES_COUNTER=PROJECTILES.Length-2;}
					else if(PROJECTILES_COUNTER >= 2){
					PROJECTILES_COUNTER = PROJECTILES_COUNTER-2;
					}
					else{
						PROJECTILES_COUNTER=PROJECTILES.Length-1;
					}
					
					for (int i=0;i<PROJECTILES.Length;i++){
						PROJECTILES[i].gameObject.SetActive(false); 
					}
					PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true); 
					
					Destroy(SPHERE);

					Transform Save_transf = PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.parent;
					PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.parent = null;

					SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject,PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position,PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);
					
					//SPHERE.transform.parent=null;
					SPHERE.transform.parent = Save_transf;
					
					PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false); 
					
					for (int j =0; j< PROJECTILE_NAMES.Length;j++){
						
						if(j == PROJECTILES_COUNTER){
							PROJECTILE_NAMES[j].gameObject.SetActive(true);
						}else{PROJECTILE_NAMES[j].gameObject.SetActive(false);}
					}		
					
					if (PROJECTILES_COUNTER > PROJECTILES.Length-2){
						PROJECTILES_COUNTER = 0;
					}else{
						PROJECTILES_COUNTER = PROJECTILES_COUNTER + 1;
					}
				}
				#endregion

			if (GUI.Button(new Rect(0*BOX_WIDTH+10, BOX_HEIGHT+20, BOX_WIDTH, 30), "Next")){
				
				//Debug.Log ("Projectile "+PROJECTILES_COUNTER);
				
				for (int i=0;i<PROJECTILES.Length;i++){
					PROJECTILES[i].gameObject.SetActive(false); 
				}
				PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(true); 

				Destroy(SPHERE);

					Transform Save_transf = PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.parent;
					PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.parent = null;

				SPHERE = (GameObject)Instantiate(PROJECTILES[PROJECTILES_COUNTER].gameObject,PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.position,PROJECTILES[PROJECTILES_COUNTER].gameObject.transform.rotation);

				//SPHERE.transform.parent=null;
					SPHERE.transform.parent = Save_transf;

				PROJECTILES[PROJECTILES_COUNTER].gameObject.SetActive(false); 

					for (int j =0; j< PROJECTILE_NAMES.Length;j++){

						if(j == PROJECTILES_COUNTER){
							PROJECTILE_NAMES[j].gameObject.SetActive(true);
						}else{PROJECTILE_NAMES[j].gameObject.SetActive(false);}
					}

				if (PROJECTILES_COUNTER > PROJECTILES.Length-2){
					PROJECTILES_COUNTER = 0;
				}else{
					PROJECTILES_COUNTER = PROJECTILES_COUNTER + 1;
				}
			}
		}	

	 }
	
	} //END ON_GUI

}
