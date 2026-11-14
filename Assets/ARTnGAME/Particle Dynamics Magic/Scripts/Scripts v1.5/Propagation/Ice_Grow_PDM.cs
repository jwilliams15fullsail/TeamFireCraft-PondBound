using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {

	public class Ice_Grow_PDM : MonoBehaviour {
			
	public bool trigger_ice_grow=false;
	
	public GameObject ICE_SYSTEM;
	private ParticleSystem ICE_Particles;
	private SKinColoredMasked ICE_Script;

		public float melt_speed;
	public float grow_speed;
		public bool start_melt;
		bool zeroed_scale=false;
		public float max_particle_size = 0.05f;
		bool triggered_ice_grow=false;

		public bool is_ice = false;

	void Start () {
					
		ICE_Particles = ICE_SYSTEM.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
		ICE_Script = ICE_SYSTEM.GetComponent(typeof(SKinColoredMasked)) as SKinColoredMasked;		
				
	}	

	void Update () {

		//SPELL1
		if(ICE_Particles==null){
			ICE_Particles = ICE_SYSTEM.GetComponent(typeof(ParticleSystem)) as ParticleSystem;
		}
		if(ICE_Script==null){
			ICE_Script = ICE_SYSTEM.GetComponent(typeof(SKinColoredMasked)) as SKinColoredMasked;
		}

			if(trigger_ice_grow){

				if(!triggered_ice_grow){
					triggered_ice_grow=true;
					trigger_ice_grow=false;
				}else
				if(triggered_ice_grow){ //if already triggered, reset
					triggered_ice_grow=true;
					trigger_ice_grow=false;
					start_melt=false;
				}
			}
		
			if(triggered_ice_grow){
		if(!zeroed_scale){
			
			ICE_Script.Start_size =0;
			//ICE_Particles.startSize=0;
					ParticleSystem.MainModule main = ICE_Particles.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve.constant = 0;

			zeroed_scale=true;
		}else{
			
			if(!start_melt){
				if(ICE_Script.Start_size < max_particle_size){
					ICE_Script.Start_size += Time.deltaTime*Time.deltaTime*(grow_speed);
				}else{
							start_melt=true;
				}
			}else if (start_melt){

				if(ICE_Script.Start_size > 0){
							ICE_Script.Start_size -= Time.deltaTime*Time.deltaTime*(melt_speed);
				}else{

							triggered_ice_grow = false;
							ICE_Script.Start_size=0;
							start_melt=false;

				}

			}
			
		}
		}
	}

	

}
}