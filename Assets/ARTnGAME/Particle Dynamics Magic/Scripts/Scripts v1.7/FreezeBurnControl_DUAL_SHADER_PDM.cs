using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {

	public class FreezeBurnControl_DUAL_SHADER_PDM : MonoBehaviour {
			
		//float Start_time;
		Animation Animator;
		public GameObject Character_root;

		//v1.7.1
		public Shader Dual_shader;
		public Texture2D Dual_shader_ICE;

	void Start () {

			//Animator = this.gameObject.transform.root.gameObject.animation;
			//Animator = this.gameObject.transform.root.GetComponentInChildren(typeof(Animation)) as Animation;
			if(Character_root!=null){
				Animator = Character_root.GetComponent(typeof(Animation)) as Animation;



			}
			//if(Animator!=null){
				//Animator.s
				//Debug.Log ("IN " + Character_root.gameObject.name);
				//foreach ( AnimationState state in Animator) {
					//state.speed = 0.05f;
				//}
			//}

			//Freeze_speed = 40;
		//	Thaw_speed = 0.01f;

			current_time = Time.fixedTime;

			if(this.GetComponent<Renderer>()!=null){

				if(!Use_DUAL_SHADER){
					Start_color = this.GetComponent<Renderer>().material.color; 
				}else{

					//v1.7.1
					if(Dual_shader_ICE!=null){
						Frozen=Dual_shader_ICE;
					}else{
						//Frozen = Resources.Load("Crystal4",typeof(Texture2D)) as Texture2D;
					}

					Unfrozen = (Texture2D)this.GetComponent<Renderer>().material.mainTexture;

					if(this.GetComponent<Renderer>().material.HasProperty("_Color")){
						this.GetComponent<Renderer>().material.color = Color.Lerp(this.GetComponent<Renderer>().material.color,Color.cyan,Freeze_speed*Time.deltaTime); 
					}

					//v1.7.1
					if(Dual_shader!=null){
						this.GetComponent<Renderer>().material.shader = Dual_shader;
					}else{
						//this.renderer.material.shader = Shader.Find ("DualShader");
					}

					//this.renderer.material.SetFloat("_ADif", Unfrozen);
					this.GetComponent<Renderer>().material.SetTexture("_ADif",Unfrozen);
					this.GetComponent<Renderer>().material.SetTexture("_BDif",Frozen);

					//_FallOffStrength 0 - > 10
					this.GetComponent<Renderer>().material.SetFloat("_FallOffStrength", 0);

					//_ABAmount ("A_B_Amount", Range(0, 40)) = 0
					this.GetComponent<Renderer>().material.SetFloat("_ABAmount", 2);
					//_AFlood ("A_Flood", Range(0, 5)) = 0              ->   5
					this.GetComponent<Renderer>().material.SetFloat("_AFlood", 0);
					// _BEmAmount ("B_Em_Amount", Float ) = 0  -> 0.8f
					this.GetComponent<Renderer>().material.SetFloat("_BEmAmount", 0);
					// _BEmColor ("B_Em_Color", Color) = (1,1,1,1)
				}

			}
	}

		Texture2D Unfrozen;
		Texture2D Frozen;

		private Color Start_color;
		public float freeze_ammount=0;//inject/increase from Propagation script
		public float burn_ammount=0;
		public float max_burn_ammount=25; //inject from Propagation script
		public float max_freeze_ammount=15;

		public float Thaw_speed=0;
		public float Freeze_speed=0.15f;

		public float check_time=0.2f;
		public float current_time;


		public bool Use_DUAL_SHADER = true;
		bool start_thaw=false;

		public float Start_delay;
		bool Start_delay_done = false;

		public float _BEmAmount_inc_speed=0.07f;
		public float _BEmAmount_dec_speed=0.4f;
		public float _AFlood_inc_speed=0.3f;
		public float _AFlood_dec_speed=1.2f;

		public bool Disable_mesh = false;

		public TransitionsMusicManagerPDM TransitioManager;
		public int this_chosen_target;

		public float Animation_speed_drop = 3.5f;
		public float Animation_speed_restore = 13.5f;
		public float Restore_anim_to_speed = 1f;
		public float Low_freeze_anim_speed = 0;
		public float Delay_freeze_anim_factor = 0.15f;

	void Update () {

//			if(burn_ammount < (max_burn_ammount/2)){
//
//			}else{
//
//				if(Time.fixedTime - current_time > check_time){
//				//make black
//				if(this.renderer!=null){
// 
//						this.renderer.material.color = Color.Lerp(this.renderer.material.color,Color.black,Freeze_speed*Time.deltaTime); 
//				}
//				}else{
//					current_time = Time.fixedTime;
//				}
//			}

		//	if(freeze_ammount < (max_freeze_ammount/10)){
			if((freeze_ammount > max_freeze_ammount) | start_thaw){

				start_thaw = true;

				if(Disable_mesh){
					if(TransitioManager!=null){
						if((TransitioManager.chosen_target != this_chosen_target) | TransitioManager.Loop_ended){
							this.GetComponent<Renderer>().enabled = false;
							Destroy (this);
						}
					}
				}

				if(Thaw_speed>0){

				if(freeze_ammount <0){
					//kill script
					//Debug.Log("Killed");

					

					Destroy (this);



				}else{
						freeze_ammount -= Thaw_speed;

						if(Animator!=null){
							//Animator.s
							//Debug.Log ("IN");
							foreach ( AnimationState state in Animator) {
								if(state.speed < Restore_anim_to_speed){
									state.speed = state.speed + (Animation_speed_restore*Time.deltaTime);
								}
							}
						}
				}

			//	if(Thaw_speed>0){
					//lerp back to start color
					if(Time.fixedTime - current_time > check_time){

						current_time = Time.fixedTime;

						//make black
						if(this.GetComponent<Renderer>()!=null){

							if(!Use_DUAL_SHADER){
								if(this.GetComponent<Renderer>().material.color != Start_color){
									this.GetComponent<Renderer>().material.color = Color.Lerp(this.GetComponent<Renderer>().material.color,Start_color,Freeze_speed*Time.deltaTime); 
								}
							}else{
								//this.renderer.material.SetFloat("_BEmAmount", Mathf.Lerp(0.8f,0,Thaw_speed*Time.deltaTime));
								//this.renderer.material.SetFloat("_AFlood", Mathf.Lerp(5f,0,Thaw_speed*Time.deltaTime));

								this.GetComponent<Renderer>().material.SetFloat("_BEmAmount", this.GetComponent<Renderer>().material.GetFloat("_BEmAmount")-_BEmAmount_dec_speed*Time.deltaTime);
								this.GetComponent<Renderer>().material.SetFloat("_AFlood", this.GetComponent<Renderer>().material.GetFloat("_AFlood")-_AFlood_dec_speed*Time.deltaTime);
							}
						}
					}else{
						//current_time = Time.fixedTime;
					}
				}
			}else{

				if(Disable_mesh){
					if(TransitioManager!=null){
						if((TransitioManager.chosen_target != this_chosen_target) | TransitioManager.Loop_ended){
							this.GetComponent<Renderer>().enabled = false;
							Destroy (this);
						}
					}
				}

				//if ( ((Time.fixedTime - last_auto_cycle > Auto_cycle_time) & !use_timer) | ((Time.fixedTime - last_auto_cycle > Time_to_next[chosen_target-1]) & use_timer) )
				//if ( ((Time.fixedTime - last_auto_cycle > Auto_cycle_time) & !use_timer) | ((Time.fixedTime - last_auto_cycle > Time_to_next[chosen_target-1]) & use_timer) )
				if(Time.fixedTime - current_time > Start_delay | Start_delay_done)
				{
					Start_delay_done = true;

					if(Time.fixedTime - current_time > check_time){

						current_time = Time.fixedTime;

						freeze_ammount += Freeze_speed; 

						//Debug.Log ("NAME_ROOT = "+this.gameObject.transform.parent.parent.gameObject.name);
						if(Animator==null){
							//Debug.Log ("ANIMATOR NULL");
						}
						//control animation
						//If you want to explicitly synchronize two animations, you can query the time of one and set the time of the other to be the same.
						//animation["dancingB"].time = characterA.animation["dancingA"].time;
						//You can see all the AnimationState variables here: http://unity3d.com/support/documentation/ScriptReference/AnimationState.html

						if(Animator!=null){
							//Animator.s
							//Debug.Log ("IN");
							foreach ( AnimationState state in Animator) {
								if(freeze_ammount > (max_freeze_ammount*Delay_freeze_anim_factor)){
									if(state.speed >= Low_freeze_anim_speed){
										state.speed = state.speed - (Animation_speed_drop*Time.deltaTime);

										//Debug.Log ("NAME = "+Animator.gameObject.name);

										if(state.speed < Low_freeze_anim_speed){
											state.speed=Low_freeze_anim_speed;
										}
									}else{
										state.speed=Low_freeze_anim_speed;
									}
								}
							}
						}

						//make black
						if(this.GetComponent<Renderer>()!=null){

							if(Use_DUAL_SHADER){
                                if (this.GetComponent<Renderer>().material.HasProperty("_BEmAmount"))
                                {
                                    this.GetComponent<Renderer>().material.SetFloat("_BEmAmount", this.GetComponent<Renderer>().material.GetFloat("_BEmAmount") + _BEmAmount_inc_speed * Time.deltaTime);
                                }
                                if (this.GetComponent<Renderer>().material.HasProperty("_AFlood"))
                                {
                                    this.GetComponent<Renderer>().material.SetFloat("_AFlood", this.GetComponent<Renderer>().material.GetFloat("_AFlood") + _AFlood_inc_speed * Time.deltaTime);
                                }
							}else{
								//this.renderer.material.SetFloat("_BEmAmount", Mathf.Lerp(0,0.8f,Freeze_speed*Time.deltaTime));
								//this.renderer.material.SetFloat("_AFlood", Mathf.Lerp(0,5f,Freeze_speed*Time.deltaTime));

								this.GetComponent<Renderer>().material.color = Color.Lerp(this.GetComponent<Renderer>().material.color,Color.cyan,Freeze_speed*Time.deltaTime); 
							}
							
						}
					}else{
						//current_time = Time.fixedTime;
					}
				}

			}

//			if(Thaw_speed>0){
//				if(freeze_ammount>0){
//					freeze_ammount = freeze_ammount - Thaw_speed*Time.deltaTime;
//				}
//			}

	}	

}
}