using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {
public class SplineToTarget : MonoBehaviour {

	//#pragma warning disable 414

	void Start () {

			//instantiate spline trail to target and activate it 
//			SPHERE = (GameObject)Instantiate(Spline,Spline.transform.position,Spline.transform.rotation);
//			
//			SPHERE.transform.parent=null;
//
//			SplineParticlePlacer = SPHERE.GetComponentInChildren(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;
//			Phoenix_Spline = SPHERE.GetComponentInChildren(typeof(SplinerP)) as SplinerP;
//
//			if(Phoenix_Spline==null){
//				Debug.Log("Please define and place a spline object on SPHERE variable");
//			}

			Spliners = new List<SplinerP>();
			Spliner_instances=new List<GameObject>();
			
			Spliner_collider_script=new List<ParticleCollisionsPDM>();
			Spliner_particle_script=new List<PlaceParticleOnSpline>();

			//ADD ORK code here to add targets


	}

	public GameObject Spline; //spline to instantiate, deactivated initially

	SplinerP Phoenix_Spline;
	PlaceParticleOnSpline SplineParticlePlacer;
	GameObject SPHERE;

	//private bool Settings_mounted=true;

	float Spline_grow;
	float Particle_size;
	int Spline_Quality;
	List<SplinerP> Spliners;
		List<GameObject> Spliner_instances;

		List<ParticleCollisionsPDM> Spliner_collider_script;
		List<PlaceParticleOnSpline> Spliner_particle_script;

	public List<GameObject> Targets;

	void Update () {

			//get target(s), instantiate splines and add last point, move it near target and sub divide the last segment(s).
			if(Targets != null){
				if(Targets.Count>0 & Spliners.Count != Targets.Count){
				//if(Targets.Count>0 & Spliners == null){
					for (int i=0;i<Targets.Count;i++){
						GameObject Instance =  (GameObject)Instantiate(Spline,Spline.transform.position,Spline.transform.rotation);
						Instance.SetActive(true);
						SplinerP Spliner = Instance.GetComponent(typeof(SplinerP)) as SplinerP;

						Spliners.Add(Spliner);
						Spliner_instances.Add(Instance);

						ParticleCollisionsPDM Temp_col_script = Instance.GetComponentInChildren(typeof(ParticleCollisionsPDM)) as ParticleCollisionsPDM;
						PlaceParticleOnSpline Temp_part_script= Instance.GetComponentInChildren(typeof(PlaceParticleOnSpline)) as PlaceParticleOnSpline;

						Spliner_collider_script.Add(Temp_col_script);
						Spliner_particle_script.Add(Temp_part_script);
					}
				}
			}

			for (int i=0;i<Targets.Count;i++){

				if(Spliners[i].SplinePoints.Count < 10){
					Spliners[i].Add_point = true;
				}

				if(Spliners[i].SplinePoints.Count >= 10){
					Spliners[i].control_points_children[Spliners[i].control_points_children.Count-1].transform.position = Targets[i].transform.position;
				}
			}

	}

	void OnGUI() {


			//Colliders_to_hide.SetActive(false);






		
	
	} //END ON_GUI

	//Lightning stuff
		public Transform target;
		public int zigs = 100;
		public float speed = 1f;
		public float scale = 1f;
		public Light startLight;
		public Light endLight;
		
		PerlinPDM noise;
		float oneOverZigs;
		
		//private Particle[] particles;
		ParticleSystem.Particle[] particles; //v2.3
		
		void StartA()
		{
			oneOverZigs = 1f / (float)zigs;

			ParticleSystem.EmissionModule emitModule = GetComponent<ParticleSystem> ().emission;
			emitModule.enabled = false;//v2.3
			
			GetComponent<ParticleSystem>().Emit(zigs);

			//particles = GetComponent<ParticleEmitter>().particles;
			particles = new ParticleSystem.Particle[GetComponent<ParticleSystem>().particleCount];
			GetComponent<ParticleSystem>().GetParticles(particles);
		}
		
		void UpdateA ()
		{
			if (noise == null)
				noise = new PerlinPDM();
			
			float timex = Time.time * speed * 0.1365143f;
			float timey = Time.time * speed * 1.21688f;
			float timez = Time.time * speed * 2.5564f;
			
			for (int i=0; i < particles.Length; i++)
			{
				Vector3 position = Vector3.Lerp(transform.position, target.position, oneOverZigs * (float)i);
				Vector3 offset = new Vector3(noise.Noise(timex + position.x, timex + position.y, timex + position.z),
				                             noise.Noise(timey + position.x, timey + position.y, timey + position.z),
				                             noise.Noise(timez + position.x, timez + position.y, timez + position.z));
				position += (offset * scale * ((float)i * oneOverZigs));
				
				particles[i].position = position;
				particles[i].startColor = Color.white;
				//particles[i].energy = 1f;
				particles[i].startLifetime = 1f;
			}
			
			//GetComponent<ParticleEmitter>().particles = particles;
			GetComponent<ParticleSystem>().SetParticles(particles, GetComponent<ParticleSystem>().particleCount);
			
			if (GetComponent<ParticleSystem>().particleCount >= 2)
			{
				if (startLight)
					startLight.transform.position = particles[0].position;
				if (endLight)
					endLight.transform.position = particles[particles.Length - 1].position;
			}
		}

	
}//END class definition

}