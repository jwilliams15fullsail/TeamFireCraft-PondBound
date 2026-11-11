using UnityEngine;
using System.Collections;

namespace Artngame.PDM {
public class ColliderMessagePDM : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		Spline_to_Check = SplinerP_OBJ.GetComponent("SplinerP") as SplinerP;
	}
	
	// Update is called once per frame
	void Update () {
		
		for(int i=0;i<Spline_to_Check.Curve.Count;i++){
			if (  Vector3.Distance(BOX.transform.position,Spline_to_Check.Curve[i].position) < 8)
			{
				Debug.Log("Collision in curve point "+i);
			}
		}
		
	}
	
	void  OnParticleCollision(){
		
		//Debug.Log("Collision");
	}
	
	public GameObject SplinerP_OBJ;
	SplinerP Spline_to_Check;
	public GameObject BOX;
	
	void OnCollisionEnter (){
		
		
	}
}
}