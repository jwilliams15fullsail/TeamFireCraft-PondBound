using UnityEngine;
using System.Collections;

namespace Artngame.PDM {

//[ExecuteInEditMode]
public class Light_EffectsPDM : MonoBehaviour {

	void Start () {

		Light3D = GetComponent(typeof(Light)) as Light;
		start_time = Time.fixedTime;

		Light3D.intensity = 0.0001f;

		Editor_time = 0;
		if(!Application.isPlaying){
			start_time = 0;
		}
	}

	float start_time;
	
	public bool reset = false;

	Light Light3D;

	public AnimationCurve Curve = AnimationCurve.Linear(0,0,1,1);

	public float Delay=1f;
	public bool preview=false;

	public Color StartLightColor;
	public Color EndLightColor;

	public bool loop=false;

	public void Reset () {

		Editor_time = 0;
		start_time = Time.fixedTime;
		if(Curve!=null & Light3D!=null){
			Light3D.intensity = 0.0001f;
		}
		preview = false;
	}

	float Editor_time;
	public float Lerp_speed = 20;


	void Update () {

		if(reset){
			reset=false;
			Reset();
		}

		if(!Application.isPlaying & preview){
			Editor_time+=0.01f;
			//Debug.Log (Editor_time);
			if(Editor_time > 5){
				preview = false;
			}
		}		

		if(Curve!=null & Light3D!=null){
			if(Application.isPlaying){
				if(Time.fixedTime - start_time > Delay){

					if(!loop){
						Light3D.intensity = Curve.Evaluate(Time.fixedTime - (start_time + Delay));
						Light3D.color = Color.Lerp(StartLightColor, EndLightColor,Lerp_speed*Time.deltaTime);
					}else{
						if(Curve[Curve.length-1].time > Time.fixedTime - (start_time + Delay)){ 
							Light3D.intensity = Curve.Evaluate(Time.fixedTime - (start_time + Delay));
							Light3D.color = Color.Lerp(StartLightColor, EndLightColor,Lerp_speed*Time.deltaTime);
						}else{
							start_time = Time.fixedTime;
							Light3D.color = StartLightColor;
						}
					}
				}
			}else if(preview){
				if(Editor_time - start_time > Delay){

					if(!loop){
						Light3D.intensity = Curve.Evaluate(Editor_time - (start_time + Delay));
						Light3D.color = Color.Lerp(StartLightColor, EndLightColor,Lerp_speed*Editor_time);
					}else{
						if(Curve[Curve.length-1].time > Editor_time - (start_time + Delay)){ 
							Light3D.intensity = Curve.Evaluate(Editor_time - (start_time + Delay));
							Light3D.color = Color.Lerp(StartLightColor, EndLightColor,Lerp_speed*Editor_time);
						}else{
							start_time = Editor_time;
							Light3D.color = StartLightColor;
						}
					}					
				}
			}
		}
	}
  }
}