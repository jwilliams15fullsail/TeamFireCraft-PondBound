using UnityEngine;
using System.Collections;

namespace Artngame.PDM {

[ExecuteInEditMode()]
public class PlaceParticleOnGround : MonoBehaviour {
	
		void Start () {
			p11 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;
			
			if(p11==null){
				Debug.Log ("Please attach the script to a particle system");
			}
			
			This_transform = this.transform;
		}
		
		public ParticleSystem p11;
		
		Transform This_transform;
		
		public bool make_circle=false;
		public float Circle_radius = 5f;
		public int loosen_circle=1; 
		public bool is_target=false;
		public float spread = 0f;
		
		//public Particle[] particles;
		ParticleSystem.Particle[] ParticleList;
		
		Transform thisTransform;
		
		public Vector2 Grass_Up_Low_Threshold = new Vector2(1f,1f);
		
		public bool relaxed = true;
		public float Dist_Above_Terrain = 1;
		
		public bool Outwards = false;//rotate flat particles to look out of center
		public bool Angled = false;
		public bool Radial_velocity = false;
		public float Outward_speed = 1f;
		public bool Invert_dir=false;
		
		void LateUpdate () {
			
			if(p11 ==null){
				
				p11 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;
				
				return;
			}
			
			if(This_transform ==null){
				This_transform = this.transform;
			}
			
			if(1==1 & Terrain.activeTerrain != null){
				
				ParticleList = new ParticleSystem.Particle[p11.particleCount];
				p11.GetParticles(ParticleList);
				
				if(1==1){
					
					for (int i=0; i < ParticleList.Length;i++)
					{
						
						Vector3 Fix_pos = Vector3.zero;
						if(p11.main.simulationSpace == ParticleSystemSimulationSpace.Local){ //v2.3
							Fix_pos = This_transform.position;
						}
						
						if(!make_circle ){
							int ARAND = Random.Range(1,ParticleList.Length);
							if(i == ARAND | !relaxed ){
								ParticleList[i].position  =  new Vector3(ParticleList[i].position.x,Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight(ParticleList[i].position)+Dist_Above_Terrain,ParticleList[i].position.z);
							}
							
							if(relaxed & this.gameObject.tag == "Grass"){
								
								float FIND_Y_MARGINED = Terrain.activeTerrain.transform.position.y + Terrain.activeTerrain.SampleHeight(ParticleList[i].position)+Dist_Above_Terrain;
								
								float UPPER_MARGIN = FIND_Y_MARGINED+Grass_Up_Low_Threshold.x;
								float LOWER_MARGIN = FIND_Y_MARGINED-Grass_Up_Low_Threshold.y;
								
								float FIND_PARTICLE_Y = ParticleList[i].position.y;
								
								if(ParticleList[i].position.y > UPPER_MARGIN){
									FIND_PARTICLE_Y=UPPER_MARGIN;
								}
								if(ParticleList[i].position.y < LOWER_MARGIN){
									FIND_PARTICLE_Y=LOWER_MARGIN;
								}
								
								ParticleList[i].position  =  new Vector3(ParticleList[i].position.x,FIND_PARTICLE_Y,ParticleList[i].position.z);
								
							}
						}
						
						int ARAND1 = Random.Range(1,loosen_circle);
						if(make_circle & ( (ARAND1 == 1 ) |    (is_target & i < (p11.particleCount/1))     )  ){
							
							float find_x = This_transform.position.x + Mathf.Sin (i)*(Circle_radius+(i*spread*0.01f));
							float find_z = This_transform.position.z + Mathf.Cos (i)*(Circle_radius+(i*spread*0.01f));
							
							float find_y = ParticleList[i].position.y;
							
							if(!is_target){
								find_y = Terrain.activeTerrain.SampleHeight(new Vector3(find_x,0,find_z))+Dist_Above_Terrain+ Terrain.activeTerrain.transform.position.y;
							}
							
							if(is_target){
								find_y = Terrain.activeTerrain.SampleHeight(new Vector3(find_x,0,find_z))+Dist_Above_Terrain+ Terrain.activeTerrain.transform.position.y;
							}
							
							
							
							ParticleList[i].position  = (new Vector3(find_x,find_y + 0,find_z) - new Vector3(Fix_pos.x,0,Fix_pos.z));
							
							if(Outwards){
								
								float Invert = 180;
								if(Invert_dir){
									Invert = 0;
								}
								
								ParticleList[i].axisOfRotation = Vector3.up;
								//ParticleList[i].rotation = 90;
								Vector3 Dist = ParticleList[i].position - This_transform.position + Fix_pos;
								Vector3 Axis = new Vector3(0,0,1);
								if(Angled){
									Axis = new Vector3(1,0,0);
									
									if(Dist.z < 0){
										ParticleList[i].rotation = Vector3.Angle(Axis,new Vector3(Dist.x,0,Dist.z))+Invert; //right = new Vector3(1,0,0)
									}else{
										ParticleList[i].rotation = 360-(Vector3.Angle(Axis,new Vector3(Dist.x,0,Dist.z))+Invert);
									}
									
								}else{
									
									if(Dist.x > 0){
										ParticleList[i].rotation = Vector3.Angle(Axis,new Vector3(Dist.x,0,Dist.z))+Invert; //right = new Vector3(1,0,0)
									}else{
										ParticleList[i].rotation = 360-(Vector3.Angle(Axis,new Vector3(Dist.x,0,Dist.z))+Invert);
									}
								}
								if(Radial_velocity){
									Vector3 Out = Dist*Outward_speed;
									ParticleList[i].velocity = new Vector3(Out.x,0,Out.z);
								}
							}
							
						}
						
					}
				}
				p11.SetParticles(ParticleList,p11.particleCount);
				
			}		
		}
	}
}