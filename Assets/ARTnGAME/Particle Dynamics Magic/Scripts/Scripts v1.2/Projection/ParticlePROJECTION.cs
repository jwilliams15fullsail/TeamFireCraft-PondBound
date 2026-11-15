using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Artngame.PDM;

namespace Artngame.PDM {
	[ExecuteInEditMode]
public class ParticlePROJECTION : MonoBehaviour {
	
	void Start () {
	}

	void Awake () {
		p2 = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;

		Registered_paint_positions = new List<Vector3>(); 
	}

	public ParticleSystem p2;

	
	
	private bool make_circle=false;

	
	ParticleSystem.Particle[] ParticleList;


		private bool draw_in_sequence=false;
	private List<Vector3> Registered_paint_positions;  
	
	void OnDrawGizmos() {
		
	}

	public bool randomize;
	public float extend=1f;

	void Update () {

			if(p2==null){Debug.Log("Attach script to a particle system");return;}

		Registered_paint_positions.Clear(); 
		List<Vector3> ray_dest_positions = new List<Vector3>();

			int max_positions = (int)p2.main.maxParticles/2;//v2.1

		ray_dest_positions.Clear();

		int GET_X = (int)Mathf.Sqrt(max_positions);
		for (int m=0;m<GET_X;m++){
			for (int n=0;n<GET_X;n++){

				float X_AXIS = (extend*(GET_X/2)*m);
				float Z_AXIS = (extend*(GET_X/2)*n);
				if(randomize){
					X_AXIS = X_AXIS+Random.Range(0,extend*(GET_X/2)*m);
					Z_AXIS = Z_AXIS+Random.Range(0,extend*(GET_X/2)*n);
				}

				ray_dest_positions.Add(this.transform.parent.rotation*new Vector3(this.transform.position.x - X_AXIS, this.transform.position.y-1000,this.transform.position.z - Z_AXIS  ));
				ray_dest_positions.Add(this.transform.parent.rotation*new Vector3(this.transform.position.x + X_AXIS, this.transform.position.y-1000,this.transform.position.z - Z_AXIS ));
			}
		}

		for (int k=0;k<ray_dest_positions.Count;k++)
		{
						
			Vector3 ORIGIN = this.transform.position;
			Vector3 DEST = ray_dest_positions[k];

			RaycastHit hit = new RaycastHit();
			if (Physics.Raycast(ORIGIN,DEST, out hit, Mathf.Infinity))
			{
					if(Registered_paint_positions!=null){
						if(Registered_paint_positions.Count > (p2.main.maxParticles/2)){//v2.1
								//do nothing
							}else{
								Registered_paint_positions.Add(hit.point);
							}
					}
			}
		}

	
		if(1==1){

			ParticleSystem p11=p2;
				
				ParticleList = new ParticleSystem.Particle[p11.particleCount];
				p11.GetParticles(ParticleList);
				
					int counter_regsitered = 0; 
					for (int i=0; i < ParticleList.Length;i++)
					{
						
					if(!make_circle ){
				
					ParticleList[i].position  = new Vector3(0,ParticleList[i].position.y,0);

					if(Registered_paint_positions!=null & Registered_paint_positions.Count > 0 & !draw_in_sequence){ 
	
							float FIND_Y = ParticleList[i].position.y;
						Vector3 FIND_moved_pos = ParticleList[i].position;

						FIND_moved_pos = Registered_paint_positions[counter_regsitered];
						if(ParticleList[i].remainingLifetime > (ParticleList[i].startLifetime-0.1f*ParticleList[i].startLifetime)   ){
								FIND_Y = FIND_moved_pos.y;
							}
							
						ParticleList[i].position  = new Vector3(FIND_moved_pos.x,FIND_Y,FIND_moved_pos.z) ;
							
						counter_regsitered=counter_regsitered+1;
						if(counter_regsitered > Registered_paint_positions.Count-1 ){
							counter_regsitered=0;
						}
							
					}

				}

			}
			p2.SetParticles(ParticleList,p11.particleCount);

		}
		
	}
}
}