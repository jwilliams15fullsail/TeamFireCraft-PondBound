using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class Provide_Transition_Target_PDM : MonoBehaviour
	{

		TransitionsMusicManagerPDM Transition_Manager;

		//v1.7.1
		public Shader Dual_shader;
		public Texture2D Dual_shader_ICE;
		public bool Disable_mesh = false;

		public float Thaw_speed = 1f;//0.8f
		public float Freeze_speed = 0.4f;
		public float _BEmAmount_inc_speed = 3.07f;
		public float _BEmAmount_dec_speed = 3.4f;
		public float _AFlood_inc_speed = 3.3f;
		public float _AFlood_dec_speed = 4.2f;
		public float max_freeze_ammount = 5;

		public float Animation_speed_drop = 3.5f;
		public float Animation_speed_restore = 13.5f;
		public float Restore_anim_to_speed = 1f;
		public float Low_freeze_anim_speed = 0;
		public float Delay_freeze_anim_factor = 0.15f;

		public float check_time = 0.2f;

		// Use this for initialization
		void Start()
		{

			Transition_Manager = this.GetComponent(typeof(TransitionsMusicManagerPDM)) as TransitionsMusicManagerPDM;

			//1. handle case where it is inserted beforehand
			if (!Find_By_Code)
			{
				if (!Multitarget)
				{
					if (TransitionTarget != null)
					{
						SkinnedMeshRenderer Find_Object = TransitionTarget.GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
						if (Find_Object != null)
						{
							//apply target to script
							//						TransitionsMusicManagerPDM Transition_Manager = this.GetComponent(typeof(TransitionsMusicManagerPDM)) as TransitionsMusicManagerPDM;
							Transition_Manager.Targets[1] = Find_Object.gameObject; //apply object with skinned mesh to target
						}
					}
				}
				else
				{
					if (TransitionTargets != null)
					{
						//					TransitionsMusicManagerPDM Transition_Manager = this.GetComponent(typeof(TransitionsMusicManagerPDM)) as TransitionsMusicManagerPDM;
						for (int i = 0; i < TransitionTargets.Count; i++)
						{
							SkinnedMeshRenderer Find_Object = TransitionTargets[i].GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
							if (Find_Object != null)
							{
								//apply target to script	
								if (i < Transition_Manager.Targets.Count)
								{
									Transition_Manager.Targets[i] = Find_Object.gameObject; //apply object with skinned mesh to target
								}
							}
						}
					}
				}
			}


			//2. handle case where target is found by code
			if (Find_By_Code)
			{

				if (!Multitarget)
				{
					//code to give target gameobject here, and apply to TransitionTarget

					//find object ADD

					// TransitionTarget = Object ADD

					if (TransitionTarget != null)
					{
						SkinnedMeshRenderer Find_Object = TransitionTarget.GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
						if (Find_Object != null)
						{
							//apply target to script
							//					TransitionsMusicManagerPDM Transition_Manager = this.GetComponent(typeof(TransitionsMusicManagerPDM)) as TransitionsMusicManagerPDM;
							//Transition_Manager.Targets[0] = Find_Object.gameObject; //apply object with skinned mesh to target
							Transition_Manager.Targets[1] = Find_Object.gameObject;
						}
					}
				}
				else
				{

					// find objects - get all needed targets ADD

					// TransitionTargets[i] = Object ADD

					if (TransitionTargets != null)
					{
						//				TransitionsMusicManagerPDM Transition_Manager = this.GetComponent(typeof(TransitionsMusicManagerPDM)) as TransitionsMusicManagerPDM;
						for (int i = 0; i < TransitionTargets.Count; i++)
						{
							SkinnedMeshRenderer Find_Object = TransitionTargets[i].GetComponentInChildren(typeof(SkinnedMeshRenderer)) as SkinnedMeshRenderer;
							if (Find_Object != null)
							{
								//apply target to script	
								if (i < Transition_Manager.Targets.Count)
								{
									Transition_Manager.Targets[i] = Find_Object.gameObject; //apply object with skinned mesh to target
								}
							}
						}
					}
				}

			}

		}

		public GameObject TransitionTarget;
		public bool Find_By_Code = false;
		public bool Multitarget = false;
		public List<GameObject> TransitionTargets;
		public List<bool> InjectionTargets;//select where to inject freeze script, eg in skinnedmeshes only

		public bool Use_DUAL_SHADER = false;
		public bool Inject_Freeze = false;

		int previous_target = -1;

		public float injection_delay = 0.5f;

		//float current_delay_time=0;
		float start_delay_time = 0;
		int target_changed = 0;
		int delay_over = 0;
		float injection_delay_inner = 0;

		public bool Dist_to_previous = false;

		// Update is called once per frame
		void Update()
		{

			//Inject scripts based on timing, check Transition script for current target and times

			int chosen_target = Transition_Manager.chosen_target;
			//float injection_delay_inner = injection_delay;
			//int target_changed = 0;
			if (previous_target != chosen_target)
			{

				//extend delay if target very far
				if (previous_target != -1 & chosen_target > 1)
				{
					Vector3 dist = Vector3.zero;
					if (Dist_to_previous)
					{
						dist = (Transition_Manager.Targets[chosen_target - 1].transform.position - Transition_Manager.Targets[chosen_target - 2].transform.position);
					}
					else
					{
						dist = (Transition_Manager.Targets[chosen_target - 1].transform.position - Transition_Manager.Targets[0].transform.position);
					}

					//Debug.Log ("Dist = "+dist.magnitude);
					if (dist.magnitude > 0.1f)
					{
						//increase delay
						injection_delay_inner = injection_delay + (dist.magnitude / 20);//this should also count in the gravity speed
					}
				}
				else
				{
					//Vector3 dist = (Transition_Manager.Targets[chosen_target].transform.position-Transition_Manager.Targets[chosen_target-1].transform.position);
					//if(dist.magnitude > 10f){
					//increase delay
					//	injection_delay = injection_delay + (dist.magnitude/10);//this should also count in the gravity speed
					//}
				}

				previous_target = chosen_target;
				target_changed = 1;
				//Debug.Log ("target changed to "+chosen_target);
				start_delay_time = Time.fixedTime;

				delay_over = 0;
			}


			//Debug.Log ("INNER = " + injection_delay_inner);

			if (Time.fixedTime - start_delay_time > injection_delay_inner & delay_over == 0)
			{
				delay_over = 1;

				//Debug.Log ("delay ="+injection_delay_inner);

				injection_delay_inner = injection_delay;

				//start_delay_time = Time.fixedTime;
			}

			//only in injection targets
			int insideInjectionTargets = 0;
			if (InjectionTargets[chosen_target - 1])
			{
				insideInjectionTargets = 1;
			}

			if (Inject_Freeze & chosen_target != 1 & target_changed == 1 & delay_over == 1 & insideInjectionTargets == 1)
			{
				//BURN-FREEZE
				insideInjectionTargets = 0;
				target_changed = 0;
				delay_over = 0;
				injection_delay_inner = injection_delay;
				//if(enable_freeze & Emitter_objects[i].gameObject.tag =="Freezable"){
				if (!Use_DUAL_SHADER)
				{
					//search for script in emitter, if not there inject it
					//FreezeBurnControlPDM Burner = Transition_Manager.Targets[chosen_target-1].GetComponent(typeof(FreezeBurnControlPDM)) as FreezeBurnControlPDM;
					FreezeBurnControl_DUAL_SHADER_PDM Burner = Transition_Manager.Targets[chosen_target - 1].GetComponent(typeof(FreezeBurnControl_DUAL_SHADER_PDM)) as FreezeBurnControl_DUAL_SHADER_PDM;

					if (Burner == null)
					{
						//Transition_Manager.Targets[chosen_target-1].AddComponent(typeof(FreezeBurnControlPDM));
						//Burner = Transition_Manager.Targets[chosen_target-1].GetComponent(typeof(FreezeBurnControlPDM)) as FreezeBurnControlPDM;

						Transition_Manager.Targets[chosen_target - 1].AddComponent(typeof(FreezeBurnControl_DUAL_SHADER_PDM));
						Burner = Transition_Manager.Targets[chosen_target - 1].GetComponent(typeof(FreezeBurnControl_DUAL_SHADER_PDM)) as FreezeBurnControl_DUAL_SHADER_PDM;


						//Burner.max_burn_ammount=max_burn_ammount; //inject to Propagation script
						//Burner.max_freeze_ammount=max_freeze_ammount;
						//Burner.Thaw_speed=Thaw_speed;
						//Burner.Freeze_speed=Freeze_speed;

						Burner.Thaw_speed = Thaw_speed;
						Burner.Freeze_speed = Freeze_speed;

						Burner.max_freeze_ammount = max_freeze_ammount;
						Burner.check_time = check_time;

						Burner._BEmAmount_dec_speed = _BEmAmount_dec_speed;
						Burner._BEmAmount_inc_speed = _BEmAmount_inc_speed;
						Burner._AFlood_dec_speed = _AFlood_dec_speed;
						Burner._AFlood_inc_speed = _AFlood_inc_speed;

						Burner.Animation_speed_drop = Animation_speed_drop;
						Burner.Animation_speed_restore = Animation_speed_restore;
						Burner.Restore_anim_to_speed = Restore_anim_to_speed;
						Burner.Low_freeze_anim_speed = Low_freeze_anim_speed;
						Burner.Delay_freeze_anim_factor = Delay_freeze_anim_factor;

						Burner.Use_DUAL_SHADER = false;

						float Delay_calc = 0f;
						//if ( ((Time.fixedTime - Transition_Manager.last_auto_cycle > Transition_Manager.Auto_cycle_time) & !Transition_Manager.use_timer) | ((Time.fixedTime - Transition_Manager.last_auto_cycle > Transition_Manager.Time_to_next[Transition_Manager.chosen_target-1]) & Transition_Manager.use_timer) )
						//if ( ((Time.fixedTime - Transition_Manager.last_auto_cycle > Transition_Manager.Auto_cycle_time) & !Transition_Manager.use_timer) | ((Time.fixedTime - Transition_Manager.last_auto_cycle > Transition_Manager.Time_to_next[Transition_Manager.chosen_target-1]) & Transition_Manager.use_timer) )
						//{
						Delay_calc = Transition_Manager.Time_to_next[Transition_Manager.chosen_target - 1] * 2;
						//}
						Delay_calc = 0f;

						Burner.TransitioManager = Transition_Manager;
						Burner.this_chosen_target = chosen_target;

						Burner.Start_delay = Delay_calc;

						//v1.7.1
						if (Disable_mesh)
						{
							Burner.Disable_mesh = true;
						}
						else
						{
							Burner.Disable_mesh = false;
						}
						if (Dual_shader != null)
						{
							Burner.Dual_shader = Dual_shader;
						}
						if (Dual_shader_ICE != null)
						{
							Burner.Dual_shader_ICE = Dual_shader_ICE;
						}


						//Burner.Character_root = Transition_Manager.Targets[chosen_target-1];
						if ((chosen_target - 1) >= 0 & (chosen_target - 1) < TransitionTargets.Count)
						{
							Burner.Character_root = TransitionTargets[chosen_target - 1];
						}

					}
					if (Burner.freeze_ammount < Burner.max_freeze_ammount)
					{
						//Burner.freeze_ammount += 1; 
					}
				}
				else
				{
					FreezeBurnControl_DUAL_SHADER_PDM Burner = Transition_Manager.Targets[chosen_target - 1].GetComponent(typeof(FreezeBurnControl_DUAL_SHADER_PDM)) as FreezeBurnControl_DUAL_SHADER_PDM;

					if (Burner == null)
					{
						Transition_Manager.Targets[chosen_target - 1].AddComponent(typeof(FreezeBurnControl_DUAL_SHADER_PDM));
						Burner = Transition_Manager.Targets[chosen_target - 1].GetComponent(typeof(FreezeBurnControl_DUAL_SHADER_PDM)) as FreezeBurnControl_DUAL_SHADER_PDM;

						//Burner.max_burn_ammount=max_burn_ammount; //inject to Propagation script
						//Burner.max_freeze_ammount=max_freeze_ammount;
						//Burner.Thaw_speed=Thaw_speed;

						Burner.Thaw_speed = Thaw_speed;
						Burner.Freeze_speed = Freeze_speed;

						Burner.max_freeze_ammount = max_freeze_ammount;
						Burner.check_time = check_time;

						Burner._BEmAmount_dec_speed = _BEmAmount_dec_speed;
						Burner._BEmAmount_inc_speed = _BEmAmount_inc_speed;
						Burner._AFlood_dec_speed = _AFlood_dec_speed;
						Burner._AFlood_inc_speed = _AFlood_inc_speed;

						Burner.Animation_speed_drop = Animation_speed_drop;
						Burner.Animation_speed_restore = Animation_speed_restore;
						Burner.Restore_anim_to_speed = Restore_anim_to_speed;
						Burner.Low_freeze_anim_speed = Low_freeze_anim_speed;
						Burner.Delay_freeze_anim_factor = Delay_freeze_anim_factor;

						float Delay_calc = 0f;
						//if ( ((Time.fixedTime - Transition_Manager.last_auto_cycle > Transition_Manager.Auto_cycle_time) & !Transition_Manager.use_timer) | ((Time.fixedTime - Transition_Manager.last_auto_cycle > Transition_Manager.Time_to_next[Transition_Manager.chosen_target-1]) & Transition_Manager.use_timer) )
						//if ( ((Time.fixedTime - Transition_Manager.last_auto_cycle > Transition_Manager.Auto_cycle_time) & !Transition_Manager.use_timer) | ((Time.fixedTime - Transition_Manager.last_auto_cycle > Transition_Manager.Time_to_next[Transition_Manager.chosen_target-1]) & Transition_Manager.use_timer) )
						//{
						Delay_calc = Transition_Manager.Time_to_next[Transition_Manager.chosen_target - 1] * 2;
						//}
						Delay_calc = 0f;

						Burner.TransitioManager = Transition_Manager;
						Burner.this_chosen_target = chosen_target;

						Burner.Start_delay = Delay_calc;

						//v1.7.1
						if (Disable_mesh)
						{
							Burner.Disable_mesh = true;
						}
						else
						{
							Burner.Disable_mesh = false;
						}
						if (Dual_shader != null)
						{
							Burner.Dual_shader = Dual_shader;
						}
						if (Dual_shader_ICE != null)
						{
							Burner.Dual_shader_ICE = Dual_shader_ICE;
						}


						//Burner.Character_root = Transition_Manager.Targets[chosen_target-1];
						if ((chosen_target - 1) >= 0 & (chosen_target - 1) < TransitionTargets.Count)
						{
							Burner.Character_root = TransitionTargets[chosen_target - 1];
						}

						//Burner.Freeze_speed=Freeze_speed;
					}
					if (Burner.freeze_ammount < Burner.max_freeze_ammount)
					{
						//Burner.freeze_ammount += 1; 
					}
				}
			}
			//		if(enable_burn & Emitter_objects[i].gameObject.tag =="Flammable"){
			//			
			//			//search for script in emitter, if not there inject it
			//			FreezeBurnControlPDM Burner = Emitter_objects[i].gameObject.GetComponent(typeof(FreezeBurnControlPDM)) as FreezeBurnControlPDM;
			//			
			//			if(Burner == null){
			//				
			//				Emitter_objects[i].gameObject.AddComponent(typeof(FreezeBurnControlPDM));
			//				Burner = Emitter_objects[i].gameObject.GetComponent(typeof(FreezeBurnControlPDM)) as FreezeBurnControlPDM;
			//				
			//				Burner.max_burn_ammount=max_burn_ammount; //inject to Propagation script
			//				Burner.max_freeze_ammount=max_freeze_ammount;
			//				Burner.Thaw_speed=Thaw_speed;
			//				Burner.Freeze_speed=Freeze_speed;
			//			}
			//			if(Burner.burn_ammount < Burner.max_burn_ammount){
			//				Burner.burn_ammount += 1; 
			//			}							
			//			
			//		}
			//END BURN-FREEZE



		}








	}
}