using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {

	public class CycleGameObject_PDM : MonoBehaviour {		

	void Start () {	
			current_target=0;
	}

	public List<GameObject> Objects_to_cycle;

	public float cycle_interval = 0.5f;

	private float current_time;
	private int  current_target;

	void Update () {

			if(Objects_to_cycle!=null & Objects_to_cycle.Count > 0){

				if(Time.fixedTime - current_time > cycle_interval){

					for (int i=0;i<Objects_to_cycle.Count;i++){

						Objects_to_cycle[i].SetActive(false);
						if(i == current_target){

							Objects_to_cycle[i].SetActive(true);
						}
					}

					current_target = current_target+1;
					if(current_target>=Objects_to_cycle.Count){ //v2.3

						current_target=0;
					}

					current_time = Time.fixedTime;
				}
			}

	}	

}
}