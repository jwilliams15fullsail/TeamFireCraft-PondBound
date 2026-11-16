using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class Control_WIND : MonoBehaviour {

	// Use this for initialization
	void Start () {
		AAA = POOL.GetComponent("PlaceGameobjectFREEFORM") as PlaceGameobjectFREEFORM;
		wind_speed=0;
	}

	private PlaceGameobjectFREEFORM AAA ; 
	public GameObject POOL;
	

	void Update () {
	
	}

	float wind_speed;
	float rot_x;
	float rot_y;

	void OnGUI(){

		if(AAA==null){

			AAA = POOL.GetComponent("PlaceGameobjectFREEFORM") as PlaceGameobjectFREEFORM;

		}

		if(AAA!=null){
		GUI.TextField(new Rect(5,26,100,20),"Wind speed");
		wind_speed = GUI.HorizontalSlider(new Rect(5,50,150,17),AAA.Wind_speed,0,10);
		AAA.Wind_speed = wind_speed;

		GUI.TextField(new Rect(5,50+26,100,20),"Rotate A");
		rot_x = GUI.HorizontalSlider(new Rect(5,50+50,150,17),AAA.Local_rot.x,-2,3);
		AAA.Local_rot.x = rot_x;

		GUI.TextField(new Rect(5,50+50+26,100,20),"Rotate B");
		rot_y = GUI.HorizontalSlider(new Rect(5,50+50+50,150,17),AAA.Local_rot.z,0,5);
		AAA.Local_rot.z = rot_y;

		string AB = "Initial position";
		if(AAA.Angled){
			AB = "Windy position";
		}

		
		if( GUI.Button(new Rect(5,50+50+50+25,100,20),AB)){
			if(AAA.Angled){AAA.Angled=false;}else{AAA.Angled=true;}
		}

			AB = "Erase off";
			if(AAA.Erase_mode){
				AB = "Erase on";
			}
			if( GUI.Button(new Rect(5,50+50+50+50,100,20),AB)){
				if(AAA.Erase_mode){AAA.Erase_mode=false;}else{AAA.Erase_mode=true;}
			}

			AB = "Brush off";
			if(AAA.Use_stencil){
				AB = "Brush on";

				AAA.Coloration_ammount = GUI.HorizontalSlider(new Rect(5,50+50+50+50+80,150,17),AAA.Coloration_ammount,0,20);

				if( GUI.Button(new Rect(5,50+50+50+50+100,100,100),AAA.Stencil)){

				}
			}
			if( GUI.Button(new Rect(5,50+50+50+50+50,100,20),AB)){
				if(AAA.Use_stencil){AAA.Use_stencil=false;}else{AAA.Use_stencil=true;}
			}



		}

	}

}
}