using UnityEngine;
using System.Collections;

namespace Artngame.TEM {
[ExecuteInEditMode]
public class TEM_Text_Effects : MonoBehaviour {

	void Start () {
		this_transform = transform;
		Text3D = GetComponent(typeof(TextMesh)) as TextMesh;
		start_time = Time.fixedTime;

		this_transform.localScale = 0.0001f*Vector3.one;

		Editor_time = 0;
		if(!Application.isPlaying){
			start_time = 0;
		}
	}

	float start_time;
	public bool LookAtCamera=false;
	public bool reset = false;

	TextMesh Text3D;
	Transform this_transform;
	public AnimationCurve Curve = AnimationCurve.Linear(0,0,1,1);

	public float Delay=1f;
	public bool preview=false;

	public void Reset () {		
		Editor_time = 0;
		start_time = Time.fixedTime;
		if(this_transform!=null){
			this_transform.localScale = 0.0001f*Vector3.one;
		}
		preview = false;
	}

	float Editor_time;

	void Update () {

		if(reset){
			reset=false;
			Reset();
		}
		if(LookAtCamera){

			if(Application.isPlaying){
				this_transform.LookAt(this_transform.position +(this_transform.position - Camera.main.transform.position));
			}

		}

		if(!Application.isPlaying & preview){
			Editor_time+=0.01f;
			Debug.Log (Editor_time);
			if(Editor_time > 5){
				preview = false;
			}
		}

		if(Curve!=null & Text3D!=null){
			if(Application.isPlaying){
				if(Time.fixedTime - start_time > Delay){
					if(Curve[Curve.length-1].time > Time.fixedTime - (start_time + Delay)){ 
						this_transform.localScale = Curve.Evaluate(Time.fixedTime - (start_time + Delay))*Vector3.one;
					}
				}
			}else if(preview){
				if(Editor_time - start_time > Delay){

					if(Curve[Curve.length-1].time > Editor_time - (start_time + Delay)){ 
						this_transform.localScale = Curve.Evaluate(Editor_time - (start_time + Delay))*Vector3.one;
					}
					else{
						preview = false;
					}
				}
			}
		}
	}
}
}