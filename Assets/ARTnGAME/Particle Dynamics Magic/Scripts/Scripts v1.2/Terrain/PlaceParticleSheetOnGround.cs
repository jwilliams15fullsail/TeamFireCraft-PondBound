using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
	public class PlaceParticleSheetOnGround : MonoBehaviour {

	void Start () {
		
			//if(Application.isPlaying)
			{
			nbb=this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;

			if(nbb!=null){
				nbb.Clear ();
				nbb.Emit(particle_count);
				aaa = new ParticleSystem.Particle[nbb.particleCount];
			}

			start_pos=this.transform.position;
			Cash_transform = transform;

		
			KTiles_X=Tiles_X;
				KTiles_Y=Tiles_Y;

			got_positions=false;
			}

	}

	void OnEnable(){



			nbb=this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;

			if(nbb==null){
				Debug.Log ("Please attach the script to a particle system");
			}
			
			if(nbb!=null){
				nbb.Clear ();
				nbb.Emit(particle_count);
				aaa = new ParticleSystem.Particle[nbb.particleCount];
			}
			
			start_pos=this.transform.position;
			Cash_transform = transform;

			got_positions=false;

			KTiles_X=Tiles_X;
			KTiles_Y=Tiles_Y;
	}

	public Vector3 start_pos;
	public int particle_count = 100;
	Transform Cash_transform;

	private bool got_positions=false;

	private Vector3[] positions;
	int[] tile;

	private ParticleSystem.Particle[] aaa;
	public ParticleSystem nbb;

	
	public bool letloose=false;
	private bool let_loose=false;
	private int place_start_pos;

	public bool Gravity_Mode=false;

		public int Tiles_X=4;
		public int Tiles_Y=4;

		private int KTiles_X=4;
		private int KTiles_Y=4;

		public float Y_offset=0;

	void Update () {

			if(nbb ==null){
				
				nbb = this.gameObject.GetComponent("ParticleSystem") as ParticleSystem;
				
				return;
			}



			if(aaa.Length != particle_count | KTiles_X!=Tiles_X | KTiles_Y!=Tiles_Y){
				Start ();
			}

		let_loose = letloose;
		if(!Application.isPlaying ){
			
			if(positions == null)
			{
				positions = new Vector3[particle_count];
				got_positions = false;
			}
			if(positions[0] == Vector3.zero){
				got_positions = false;
			}
			let_loose = false;

			aaa = new ParticleSystem.Particle[nbb.particleCount];
		}
			if(!let_loose){
			nbb.Clear();
			}


		nbb.Emit(particle_count);

			int tileCount = Tiles_X*Tiles_Y-1;  //15;
	
		nbb.GetParticles(aaa);

		if(!got_positions ){
				positions = new Vector3[aaa.Length];
				tile = new int[aaa.Length];
			got_positions = true;
			
			for(int i=0;i<aaa.Length;i++){
				positions[i] = aaa[i].position;
				tile[i] = Random.Range(0,15);
			}
		}


		for(int i=0;i<aaa.Length;i++){

			aaa[i].remainingLifetime = tileCount + 1 - tile[i];
			aaa[i].startLifetime = tileCount;

			aaa[i].startColor=Color.blue;

			if(!let_loose){
				aaa[i].position = positions[i]; 
			}

			float find_x =aaa[i].position.x;
			float find_z =aaa[i].position.z;
			float Dist_Above_Terrain = Y_offset;

			Vector3 DIST_vector = Cash_transform.position-start_pos;

				float find_y = 0;

				if(Terrain.activeTerrain != null){
					find_y=Terrain.activeTerrain.SampleHeight(new Vector3(find_x,0,find_z) + DIST_vector)+Dist_Above_Terrain+ Terrain.activeTerrain.transform.position.y;
				}
		
				Vector3 FINAL_POS = new Vector3(find_x,find_y,find_z)+new Vector3(DIST_vector.x,0,DIST_vector.z);

			if(!let_loose | (place_start_pos<1)){
				aaa[i].position= FINAL_POS;
			}

			if(!let_loose){
				aaa[i].angularVelocity =0;
				aaa[i].rotation =0;
				aaa[i].velocity=Vector3.zero;
			}

			//Gravity
			if(let_loose & Gravity_Mode){
		
				aaa[i].position = Vector3.Slerp(aaa[i].position, FINAL_POS,0.005f);

				aaa[i].velocity= Vector3.Slerp(aaa[i].velocity,Vector3.zero,0.05f);
			}

		}

		if(place_start_pos <1){
			place_start_pos = place_start_pos+1;
		}

		nbb.SetParticles(aaa,aaa.Length);
	}
}
}