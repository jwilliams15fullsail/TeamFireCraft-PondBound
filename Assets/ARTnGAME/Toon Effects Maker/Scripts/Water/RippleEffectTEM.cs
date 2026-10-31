using UnityEngine;
using System.Collections;

namespace Artngame.TEM {

public class RippleEffectTEM : MonoBehaviour
{
	Vector3[] StartMesh;

	public MeshFilter AA;
	
	public float Wave_str = 0.02f;
	public float Wave_freq = 5;
	public float Wave_noise = 2;

	public float noiseWalk = 1f; 
	public float noiseStrength = 0.02f;

    void Update()
    {
		if(StartMesh==null){
			StartMesh = AA.mesh.vertices;
		}

			Vector3[] vertices = AA.mesh.vertices;

			for(int i=0;i<AA.mesh.vertices.Length;i++){

				Vector3 vertex = StartMesh[i];
				vertex.y = vertex.y + Wave_str*(Mathf.Cos(Time.fixedTime*Wave_freq + StartMesh[i].x + StartMesh[i].y + StartMesh[i].z)+1* Mathf.Sin(Time.fixedTime*Wave_freq - (StartMesh[i].x- StartMesh[i].y- StartMesh[i].z)));

				vertex.y += Mathf.PerlinNoise(StartMesh[i].x + noiseWalk, StartMesh[i].y + Mathf.Sin(Time.time * 0.1f) ) * noiseStrength;
				vertices[i] = vertex;

			}
			AA.mesh.vertices=vertices;
			AA.mesh.RecalculateNormals();

		if(AA.gameObject.GetComponent<MeshCollider>() !=null){
			//AA.gameObject.GetComponent<MeshCollider>().mesh = null;
			AA.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
			//AA.gameObject.GetComponent<MeshCollider>().mesh = AA.mesh;
			AA.gameObject.GetComponent<MeshCollider>().sharedMesh = AA.sharedMesh;
		}

    }
  }
}