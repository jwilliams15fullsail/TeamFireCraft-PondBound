using UnityEngine;
using System.Collections;
using Artngame.PDM;

namespace Artngame.PDM {

public class Make_appear_PDM : MonoBehaviour {

	void Start () {
		HERO_transform = HERO.transform;
		thisTransform=this.gameObject.transform;
	}

	Transform HERO_transform;
	Transform thisTransform;
	public GameObject HERO;
	public GameObject Obj_to_appear;

	bool appeared = false;
	
	void Update () {
	
		if( Vector3.Distance(HERO_transform.position, thisTransform.position) < 15f & !appeared){

			Obj_to_appear.SetActive(true);
			appeared = true;
		}else if ( Vector3.Distance(HERO_transform.position, thisTransform.position) > 15f & appeared){
			Obj_to_appear.SetActive(false);
			appeared = false;
		}
	}
}
}