using System.Collections;
using System;
using UnityEngine;

namespace Artngame.PDM {

	public class DisableAttractorPDM: MonoBehaviour {

	public Color mouseOverColor = Color.blue;
	private Color originalColor ;
	private AttractParticles ATTRACTOR;
	public GameObject disable_script;

	void Start() {
		originalColor = GetComponent<Renderer>().sharedMaterial.color;
			ATTRACTOR = this.gameObject.GetComponent("AttractParticles") as AttractParticles;

			if(!ATTRACTOR.enabled){
				GetComponent<Renderer>().material.color = Color.white;
			}else{
				GetComponent<Renderer>().material.color = originalColor;
			}
	}

	void OnMouseEnter() {

			if(Input.GetKeyDown(KeyCode.D)){
				
				if(!ATTRACTOR.enabled){
					ATTRACTOR.enabled = true;

				}else{ATTRACTOR.enabled=false;

				}
			}

			if(ATTRACTOR!=null){
				if(!ATTRACTOR.enabled){
					GetComponent<Renderer>().material.color = Color.white;
				}else{
					GetComponent<Renderer>().material.color = mouseOverColor;
				}
			}		

			if(Input.GetMouseButton(1)){

				if(!ATTRACTOR.enabled){
					ATTRACTOR.enabled = true;

				}else{ATTRACTOR.enabled=false;}
			}
	}

	void OnMouseExit() {

		if(ATTRACTOR!=null){
			if(!ATTRACTOR.enabled){
				GetComponent<Renderer>().material.color = Color.grey;
				}else{
				GetComponent<Renderer>().material.color = originalColor;
			}
		}

	}

	IEnumerator  OnMouseDown() {
		
		
		while (Input.GetMouseButton(0))
		{

				if(Input.GetKeyDown(KeyCode.D)){

					if(!ATTRACTOR.enabled){
						ATTRACTOR.enabled = true;

					}else{ATTRACTOR.enabled=false;

					}
				}

				if(Input.GetMouseButton(1)){
						
					if(!ATTRACTOR.enabled){
						ATTRACTOR.enabled = true;

					}else{ATTRACTOR.enabled=false;

					}
				}

			yield return 1;
		}
	}

}

}