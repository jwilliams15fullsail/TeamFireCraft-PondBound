using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

[ExecuteInEditMode()]
[System.Serializable()]
	public class SKinnedPArticleEmit_StaticNonPrefab : MonoBehaviour {
	
	void Start () {

		if(Application.isPlaying){

			p11 = this.gameObject.GetComponent<ParticleSystem>();

			p11.Clear ();
                animated_mesh = new Mesh();

        }
	}

	void OnEnable(){
		if(!Application.isPlaying){
							
			p11 = this.gameObject.GetComponent<ParticleSystem>();
		}
	}

	public float Start_size=0.2f;

	Color[] Pcolors;

	public ParticleSystem p11;
	ParticleSystem.Particle[] ParticleList;

	public bool Color_by_vertex;

	public GameObject emitter;

	[SerializeField]
	public SkinnedMeshRenderer mesh ;

	[SerializeField]
	public MeshFilter simple_mesh ;

	[SerializeField]
	public Mesh animated_mesh;

	public Color Coloring=Color.blue;
	
	void LateUpdate () {

	}

	void FixedUpdate () {

	}
	
	void Update () {

		if(emitter ==null | p11 == null){return;}

		if(p11 != null){ 
			if(p11.main.startSize.constant < Start_size & Application.isPlaying){
				//p11.startSize = p11.startSize+(Start_size/3);
					ParticleSystem.MainModule main = p11.main;//v2.3
					ParticleSystem.MinMaxCurve Curve = main.startSize;
					Curve = p11.main.startSize.constant+(Start_size/3); 
			}else{
					if(!p11.emission.enabled){
						//v2.1
						ParticleSystem.EmissionModule em = p11.emission;
						em.enabled = true;
						//p11.enableEmission = true;
				}
			}
		
		}

		if (Color_by_vertex){

			if(p11 != null){ 

				ParticleList = new ParticleSystem.Particle[p11.particleCount];
				p11.GetParticles(ParticleList);

				for (int i=0; i < ParticleList.Length;i++)
				{
				
					ParticleList[i].startColor = Coloring;

				}
				p11.SetParticles(ParticleList,p11.particleCount);

			}

		}

		if(mesh!=null){
				if(animated_mesh!=null){

			        mesh.BakeMesh(animated_mesh);

                    ParticleSystem.MainModule main = p11.main;
                    ParticleSystem.ShapeModule shape = p11.shape;
                   // shape.mesh = mesh.sharedMesh;
                    shape.mesh = animated_mesh;
                    //Debug.Log(shape.mesh.name);
                }
                else{
					//Debug.Log ("ooo");
                }

		}
            else
            {
                //Debug.Log("ooo1");
            }

        }
}
}