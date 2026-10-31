using UnityEngine;
using System.Collections;

namespace Artngame.TEM {

	[ExecuteInEditMode]
public class WaterControllerTEM : MonoBehaviour {


		public Weather current_weather;
		public Day_time current_day_time;
		public enum Day_time{Initial,Morning, Midday, Aftenoon, Night};
		public enum Weather{Initial,Lightning, Storm, Calm, Foam };
		Weather start_weather;
		//Day_time start_day_time;


	void Start () {
		SetParams();

			start_weather = current_weather;
			//start_day_time = current_day_time;

			noise = new PerlinTEM ();

			ColorR = Glow_color.r;
			ColorG = Glow_color.g;
			ColorB = Glow_color.b;
			ColorA = Glow_color.a;
			Glow_start_intensity = Glow_intensity;
			Buldge_shape_Start = Buldge_shape;
			ShapeControl2_x_Start = ShapeControl2.x;
			CenterControl_y_Start = CenterControl.y;

			cur_time = Time.fixedTime;

			if(Box !=null){
				Box_transform = Box.transform;
			}
	}

		public float delay = 1.5f;		
		float cur_time;
		float start_emit_time;
		bool emits = false;
		public float emit_time = 1;		
		public float speed = 1;		
		//PerlinTEM noise;
		public WaterControllerTEM Transfer_to;
		public bool Keep_init_color=true;

	void SetParams() {
		if(OceanMaterial != null){
			
			if(OceanMaterial.HasProperty("_GlowColor")){
				OceanMaterial.SetVector("_GlowColor",new Vector4(Glow_color.r,Glow_color.g,Glow_color.b,Glow_color.a));								
				OceanMaterial.SetFloat("_BulgeScale",Buldge_scale);
				OceanMaterial.SetFloat("_BulgeShape",Buldge_shape);
				OceanMaterial.SetFloat("_GlowIntensity",Glow_intensity);
				OceanMaterial.SetFloat("_BulgeScale_copy",Buldge_scale2);
			}
			
			if(OceanMaterial.HasProperty("_WaveControl1")){
				OceanMaterial.SetVector("_WaveControl1",WaveControl);
				OceanMaterial.SetVector("_TimeControl1",TimeControl);
				OceanMaterial.SetVector("_OceanCenter",CenterControl);
				OceanMaterial.SetVector("_Params1",ShapeControl1);
				
			}
				if(OceanMaterial.HasProperty("_Params2")){
					OceanMaterial.SetVector("_Params2",ShapeControl2);
				}
		}
	}

	public bool Dynamic=false;

	public Material OceanMaterial;

	//complex params
	public Vector4 WaveControl= new Vector4(0, 0, 0, 0);
	public Vector4 TimeControl= new Vector4(1f, 0.25f, 107.46f, 0.001f);
	public Vector4 CenterControl= new Vector4(210, 0, 0, 0);
	public Vector4 ShapeControl1 = new Vector4(1.333f, 1,0.8f,0);
	public Vector4 ShapeControl2= new Vector4(1f, 0.75f, 0.5f, 0.106f);

	//shared properties
	public Color Glow_color =  new Vector4(119f/255f, 113f/255f,214f/255f,255f/255f);
	public float Buldge_scale = 34.8f;
	public float Buldge_shape = 2.4f;
	public float Glow_intensity = 0.54f;
	public float Buldge_scale2 = 5.1f;
			
	public bool useGUI=false;
	public bool Extra_motion=false;
		private PerlinTEM  noise;

	
	//EXPERIMENTAL BUOYANCY
	public GameObject Box;
	public bool Experim_bouan=false;
	Transform Box_transform;
	public Transform Water_tile_transform;
		public float Amp_adjust =1;
		public float Freq_adjust=1;
		public float Lerp_speed =1;

	// Update is called once per frame
	void Update () {


			//EXPERIMENTAL BUOYANCY
			if(Box != null & Experim_bouan & Box_transform!=null){
	
				Vector3 Box_pos = Box_transform.position;
				Vector4 _OceanCenter1 = OceanMaterial.GetVector("_OceanCenter");
				Vector3 _OceanCenter = new Vector3(_OceanCenter1.x,_OceanCenter1.y,_OceanCenter1.z);
	
				Vector4 _TimeControl11 = OceanMaterial.GetVector("_TimeControl1");
				Vector3 _TimeControl1 = new Vector3(_TimeControl11.x,_TimeControl11.y,_TimeControl11.z);
	
				Vector4 _WaveControl11 = OceanMaterial.GetVector("_WaveControl1");
				Vector3 _WaveControl1 = new Vector3(_WaveControl11.x,_WaveControl11.y,_WaveControl11.z);
	
				float _BulgeShape = OceanMaterial.GetFloat("_BulgeShape");
				float _BulgeScale = OceanMaterial.GetFloat("_BulgeScale");
	
				Vector3 Boxy_pos = new Vector3(Mathf.Abs(Box_pos.x-Water_tile_transform.position.x),0,Mathf.Abs(Box_pos.z-Water_tile_transform.position.z));
	

				float dist = Vector3.Distance(_OceanCenter, new Vector3(_WaveControl1.x*Boxy_pos.y,_WaveControl1.y*Boxy_pos.x,_WaveControl1.z*Boxy_pos.z ));
				
				float dist2 = Vector3.Distance(_OceanCenter, new Vector3(Boxy_pos.y,Boxy_pos.x*0.10f,0.1f*Box_pos.z) );
				float node_5027 = Time.fixedTime*_TimeControl1.x + dist2*_TimeControl1.y;
				float node_133 = Mathf.Pow(Mathf.RoundToInt(Mathf.Abs((((new Vector2(Boxy_pos.x/100,Boxy_pos.y)+node_5027*new Vector2(0.2f,0.1f)).x)/2-0.5f))*2.0f),_BulgeShape); // Panning gradient, 0.2 is speed or wave !!!
	
				Vector3 multiplier = node_133*(_BulgeScale*Mathf.Sin(_TimeControl11.w*dist + _TimeControl11.w*dist*3.14f))*new Vector3(0,1,0);

				Vector3 multiplier2 = node_133*(_BulgeScale*Mathf.Sin(_TimeControl11.w*dist + _TimeControl11.w*dist*3.14f))*new Vector3(1,1,1);
				float BOYANCY =  Amp_adjust*Mathf.Cos(Freq_adjust*multiplier.y); 

	
				Box.transform.rotation = Quaternion.Lerp(Box.transform.rotation, Quaternion.AngleAxis (120*Mathf.Cos(Time.fixedTime*node_5027*0.001f),Mathf.Cos(Time.fixedTime*node_5027*0.001f)*multiplier2.normalized),Time.deltaTime*Lerp_speed);

				Box.transform.position = Vector3.Lerp(Box.transform.position,new Vector3 (Box.transform.position.x, Box.transform.position.y+BOYANCY, Box.transform.position.z), Time.deltaTime*Lerp_speed);
			}







		if(Dynamic){
				SetParams();

				if(Extra_motion){
					float CHANGE_FACTOR = 1;
					float speed = 5;

					float timex = Time.time * speed + 0.1365143f * CHANGE_FACTOR;					


					Buldge_scale += 0.008f*Mathf.Cos (Time.fixedTime)*Mathf.Cos (Time.fixedTime);					
					Buldge_scale2 += 0.007f*Mathf.Sin (Time.fixedTime*0.8f+2+noise.Noise(timex + Buldge_scale, timex + Buldge_scale2));	

					if(Mathf.Abs (Buldge_scale) < 0.2f){
						Buldge_scale = 0.2f;
					}
					if(Mathf.Abs (Buldge_scale2) < 0.1f){
						Buldge_scale2 = 0.1f;
					}
					if(Mathf.Abs (Buldge_scale) > 2.2f){
						Buldge_scale = 1.99f;
					}
					if(Mathf.Abs (Buldge_scale2) > 3.1f){
						Buldge_scale2 = 2.99f;
					}

				}

				//

				if(Application.isPlaying){

					if(current_weather == Weather.Lightning){
						Lightning();
					}
					if(current_weather != start_weather){
						if(current_weather == Weather.Initial){


						}

						start_weather = current_weather;
					}


					if(current_day_time == Day_time.Initial){
						//reset
						if(Keep_init_color){
							Glow_color = new Color(ColorR,ColorG,ColorB,ColorA);
						}
					}
					if(current_day_time == Day_time.Aftenoon){
						Glow_color = Color.Lerp(Glow_color,Afternoon,Day_speed*Time.deltaTime);
						Glow_intensity = Mathf.Lerp(Glow_intensity, 1f,Day_speed*Time.deltaTime);
					}
					if(current_day_time == Day_time.Night){
						Glow_color = Color.Lerp(Glow_color,Night,Day_speed*Time.deltaTime);
						Glow_intensity = Mathf.Lerp(Glow_intensity, 0.3f,Day_speed*Time.deltaTime);
					}
					if(current_day_time == Day_time.Midday){
						Glow_color = Color.Lerp(Glow_color,Day,Day_speed*Time.deltaTime);
						Glow_intensity = Mathf.Lerp(Glow_intensity, 1.2f,Day_speed*Time.deltaTime);
					}
					if(current_day_time == Day_time.Morning){
						Glow_color = Color.Lerp(Glow_color,Morning,Day_speed*Time.deltaTime);
						Glow_intensity = Mathf.Lerp(Glow_intensity, 1.1f,Day_speed*Time.deltaTime);
					}

					if(Transfer_to != null){
						Transfer_to.Glow_color = Glow_color;
						Transfer_to.Glow_intensity = Glow_intensity;
					}
				}
		}
	}

		public Color Afternoon = new Color(255f/255f,211f/255f,181f/255f,67f/255f);
		public Color Day= new Color(79f/255f,97f/255f,150f/255f,212f/255f);
		public Color Morning= new Color(252f/255f,253f/255f,255f/255f,136f/255f);
		public Color Night= new Color(40f/255f,80f/255f,120f/255f,80f/255f);

		public bool Artistic = false;
		public float Day_speed = 0.5f;

		float ColorR;
		float ColorG;
		float ColorB;
		float ColorA;
		float Glow_start_intensity;
		float Buldge_shape_Start;
		float ShapeControl2_x_Start;
		float CenterControl_y_Start;

		public bool Extra_motion_GUI=false;

		public bool Weather_GUI_on=false;
		public bool Day_cycle_GUI_on=false;

		void OnGUI(){

				if(Weather_GUI_on){
					if(GUI.Button(new Rect(250,65,150,30),"Lightning")){
						if(current_weather != Weather.Lightning){
							current_weather = Weather.Lightning;
						}else{
							current_weather = Weather.Initial;
						}
					}
				}
				if(Day_cycle_GUI_on){
					if(GUI.Button(new Rect(250,35,150,30),"Day")){
							current_day_time = Day_time.Midday;
					}
					if(GUI.Button(new Rect(250+150,35,150,30),"Afternoon")){
						current_day_time = Day_time.Aftenoon;
					}
					if(GUI.Button(new Rect(250+150+150,35,150,30),"Night")){
						current_day_time = Day_time.Night;
					}
					if(GUI.Button(new Rect(250+150+150+150,35,150,30),"Morning")){
						current_day_time = Day_time.Morning;
					}
				}

			if(useGUI & Application.isPlaying){

				if(Extra_motion_GUI){

					string Onoff="Activate";
					if(Extra_motion){
						Onoff="Deactivate";
					}

					if(GUI.Button(new Rect(300,65,170,30),Onoff+" Extra Motion")){
					if(!Extra_motion){
						Extra_motion=true;
					}else{
						Extra_motion = false;
					}
				}
				}
				float Y_dist = 30;
				GUI.Label(new Rect(150,30+1*Y_dist,150,30),"Top Color");
				ColorR = GUI.HorizontalSlider(new Rect(150,30+2*Y_dist,150,30),ColorR,0,1);
				ColorG = GUI.HorizontalSlider(new Rect(150,30+3*Y_dist,150,30),ColorG,0,1);
				ColorB = GUI.HorizontalSlider(new Rect(150,30+4*Y_dist,150,30),ColorB,0,1);

				Y_dist = 30;

				GUI.Label(new Rect(150,30+5*Y_dist,150,30),"Low Brigthness");
				ColorA = GUI.HorizontalSlider(new Rect(150,30+6*Y_dist,150,30),ColorA,0,3);
				Glow_color = new Color(ColorR,ColorG,ColorB,ColorA);


				GUI.Label(new Rect(150,30+7*Y_dist,150,30),"Top Brigthness");
				Glow_start_intensity = GUI.HorizontalSlider(new Rect(150,30+8*Y_dist,150,30),Glow_start_intensity,-0.5f,3);
				Glow_intensity = Glow_start_intensity;

				GUI.Label(new Rect(150,30+9*Y_dist,150,30),"Wave edge size");
				Buldge_shape_Start = GUI.HorizontalSlider(new Rect(150,30+10*Y_dist,150,30),Buldge_shape_Start,1,2.5f);
				Buldge_shape = Buldge_shape_Start;

				if(OceanMaterial.HasProperty("_Params2")){
				GUI.Label(new Rect(150,30+11*Y_dist,150,30),"Wave control");
				ShapeControl2_x_Start = GUI.HorizontalSlider(new Rect(150,30+12*Y_dist,150,30),ShapeControl2_x_Start,100,800);
				ShapeControl2.x = ShapeControl2_x_Start;
				}

				GUI.Label(new Rect(150,30+13*Y_dist,150,30),"Wave direction");
				CenterControl_y_Start = GUI.HorizontalSlider(new Rect(150,30+14*Y_dist,150,30),CenterControl_y_Start,-800,800);
				CenterControl.y = CenterControl_y_Start;
			}
		}

		void Lightning() {
			
			if (noise == null)
			{noise = new PerlinTEM();}
			
			float timex = Time.time * speed * 0.1365143f;
			float timey = Time.time * speed * 1.21688f;
			float timez = Time.time * speed * 2.5564f;
			
			if(Time.fixedTime - cur_time > delay & !emits){
				start_emit_time = Time.fixedTime;
				emits = true;
				cur_time = Time.fixedTime;
				
			}
			
			if(emits){
				if(Time.fixedTime - start_emit_time <= emit_time){

					Glow_color.a = Random.Range(1,3)* noise.Noise(timex + Glow_color.a, timey + Glow_color.a, timez + Glow_color.a);

					if(Glow_color.a < 0.11f){
						Glow_color.a = 0.11f;
					}

				}else{
					emits = false;
					Glow_color.a = 0.11f;
					start_emit_time = 0;
				}
				
			}
			
		}

}
}