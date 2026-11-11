using UnityEngine;
using System.Collections;
using Artngame.PDM;
namespace Artngame.PDM
{
	public class Put_item_in_PDM_Batch_pool : MonoBehaviour
	{

		// Use this for initialization
		void Start()
		{

		}

		public string Prefab_in_resources;
		//public GameObject Prefab_in_resources;

		// Update is called once per frame
		void Update()
		{

			//find pool by tag "PDM_Batch_Pool"

			GameObject Pool_obj = GameObject.FindGameObjectWithTag("PDM_Batch_Pool");

			if (Pool_obj == null)
			{
				//add a new gameobject in scene, add script
				GameObject go = new GameObject("PDM_Batch_Pool");

				go.tag = "PDM_Batch_Pool";
				//go.transform.parent = transform;
				go.transform.localScale = Vector3.one;
				go.transform.localRotation = Quaternion.identity;
				go.transform.localPosition = Vector3.zero;
				go.AddComponent(typeof(ControlCombineChildrenPDM));
				//go.AddComponent("MeshRenderer");
				Pool_obj = GameObject.FindGameObjectWithTag("PDM_Batch_Pool");
				ControlCombineChildrenPDM Combiner = Pool_obj.GetComponent(typeof(ControlCombineChildrenPDM)) as ControlCombineChildrenPDM;
				Combiner.MakeActive = true;
				Combiner.Auto_Disable = true;
				Combiner.MakeActive = true;
				Combiner.Self_dynamic_check_rot = true;
				Combiner.Self_dynamic_check_scale = true;
				Combiner.Self_dynamic_enable = true;
			}

			//find position
			Vector3 Placement = this.transform.position;

			//Instantiate item and add to dynamic batching pool
			GameObject Instance = Instantiate(Resources.Load(Prefab_in_resources), Placement, Quaternion.identity) as GameObject;
			Instance.transform.parent = Pool_obj.transform;

			Destroy(this.gameObject);


		}
	}
}