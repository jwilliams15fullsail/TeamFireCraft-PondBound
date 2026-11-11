using UnityEngine;
using System.Collections;

namespace Artngame.PDM {

	public class RibbonsPDM : MonoBehaviour {

		// Use this for initialization
		void Start () {
			if(!started){
			//Start_pos = transform.position; //grab starting position of item, to check with later
			ThisTransform = transform;
			if(Ribbon != null){
				//start_time = Ribbon.time;
			}
				started = true;
			}
			//prev_position = ThisTransform.position;
			prev_position = emitter.position;

			if (null != Ribbon)
			{
				//StartCoroutine(ResetTrails());
			}
			


		}

		IEnumerator ResetTrails()
		{
			Ribbon.time = 0;
			
			yield return new WaitForEndOfFrame();
			
			Ribbon.time = time;
		}

		bool started = false;

		public Transform emitter;

		// Vector3 Start_pos;
		public TrailRenderer Ribbon;
		Transform ThisTransform;
		public float max_dist = 5;
		public float min_dist = 1;
		public float time = 0.2f;
		//float start_time;
		float counter = 0;
		public int skip_frames = 5;
		Vector3 prev_position;
		// Update is called once per frame
		void LateUpdate () { //void Update () { //v2.2 fixes ribbons issue in hand, done after item has been moved by mesh emitter script to origin

//			if(Ribbon != null & emitter !=null){
//				if(Ribbon.time != -1){
//					Ribbon.time = -1;
//				}else{
//					Ribbon.time = time;
//				}
//
//			}

			if(Ribbon != null & emitter !=null & 1==1){
				//if(Vector3.Distance(ThisTransform.position, Start_pos) > max_dist){
				if(Vector3.Distance(ThisTransform.position, emitter.position) < min_dist
				   | Vector3.Distance(ThisTransform.position, prev_position) > max_dist
				   ){

//					GameObject RibbonObj = Instantiate(Ribbon.gameObject,transform.position,transform.rotation);
//					RibbonObj.transform.parent = Ribbon.gameObject.transform.parent;
//					RibbonObj.AddComponent(typeof(TrailRenderer));
//
//					TrailRenderer RendererLine = RibbonObj.GetComponent(typeof(TrailRenderer)) as TrailRenderer;
//					if(RendererLine !=null){
//						RendererLine = Ribbon;
//					}
//					Destroy(Ribbon.gameObject);
//					Ribbon.gameObject = RibbonObj;


					Ribbon.time = -1;
					counter = 0;
					prev_position =  ThisTransform.position;
					//Ribbon.enabled = false;
				}else{
					counter = counter+1;
					if(counter > skip_frames){
						Ribbon.time = time;
						counter = 0;
					}
//					if(counter > skip_frames){
//						Ribbon.enabled = true;
//						counter = 0;
//					}
					//Debug.Log (Start_pos);
					prev_position =  ThisTransform.position;
				}
			}

//			if(!Ribbon.gameObject.activeInHierarchy){
//
//				Destroy(Ribbon.gameObject);
//			}
		}
	}

}
