using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class VolumeParticlesPDM : MonoBehaviour {
	
	void Start () {

		p11 = this.gameObject.GetComponent<ParticleSystem>();

		 resolution = 10;
		p11.Emit(resolution * resolution * resolution);

	}
	int resolution;
	int INITIALIZED;

	Color32[] Pcolors;

	public ParticleSystem p11;
	ParticleSystem.Particle[] ParticleList;

	public bool Color_by_vertex;
	public bool Let_loose=false;

	public GameObject emitter;

	Mesh currentMesh;
	
	Color[] colors  ;
	Color32[] colorsA  ;
	
	Color32[] colors32  ;
	
	Vector3[] vertices ;

	public float Scale_factor=1f;

	private void CreatePoints () {

		points = new ParticleSystem.Particle[resolution * resolution * resolution];
		
		float increment = 10f / (resolution - 1);
		int i = 0;
		for (int x = 0; x < resolution; x++) {
			for (int z = 0; z < resolution; z++) {
				for (int y = 0; y < resolution; y++) {
					Vector3 p = new Vector3(x, y, z) * increment + p11.transform.position;
					points[i].position = p;
					points[i].startColor = new Color(p.x, p.y, p.z);

						points[i].startColor = Color.blue;

					points[i++].startSize =1.1f;
				}
			}
		}
	}
	
	
	private ParticleSystem.Particle[] points;

	void Update () {

		if ( points == null) {

		}


		if (Color_by_vertex & 1==1){

			if(p11 != null)
			{ 

				p11.Emit(resolution * resolution * resolution);
				ParticleList = new ParticleSystem.Particle[resolution * resolution * resolution];
				p11.GetParticles(ParticleList);


				if(ParticleList.Length >= (resolution*resolution*resolution)){
					float increment = 10f / (resolution - 1);
					int i = 0;
					for (int x = 0; x < resolution; x++) {
						for (int z = 0; z < resolution; z++) {
							for (int y = 0; y < resolution; y++) {

								Vector3 p = new Vector3(x, y, z) * increment;


								if(!Let_loose){
									ParticleList[i].position = p + p11.transform.position;
								}

								if(Let_loose){
									if (ParticleList[i].remainingLifetime > ParticleList[i].startLifetime*0.9f){
									ParticleList[i].position = p + p11.transform.position;
									}
								}

								ParticleList[i].startColor = new Color(p.x*2, p.y, p.z);
								ParticleList[i].startSize = 1.1f;
								i++;
							}
						}
					}
				}

				p11.SetParticles(ParticleList,resolution * resolution * resolution);

			}

		}

	}
}
}