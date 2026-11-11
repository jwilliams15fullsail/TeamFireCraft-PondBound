using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Artngame.PDM {

	public class CycleGameObjectAfter_PDM : MonoBehaviour {		

	void Start () {	
			current_target=0;
	}

	public List<GameObject> Objects_to_cycle;

	public float cycle_interval = 0.5f;

	private float current_time;
	private int  current_target;

		private int OBJ_chosen=0;

	void Update () {

			int Chance = Random.Range(1,10);

			if(Objects_to_cycle!=null & Objects_to_cycle.Count > 0 & OBJ_chosen < 4 & Chance==(2+current_target)){

				if(Time.fixedTime - current_time > cycle_interval){

					for (int i=0;i<Objects_to_cycle.Count;i++){

						Objects_to_cycle[i].SetActive(false);
						if(i == current_target){

							Objects_to_cycle[i].SetActive(true);
						}
					}

					current_target = current_target+1;
					if(current_target>Objects_to_cycle.Count){

						current_target=0;
					}

					current_time = Time.fixedTime;
				}

				OBJ_chosen=OBJ_chosen+1;
			}

	}	

}
}