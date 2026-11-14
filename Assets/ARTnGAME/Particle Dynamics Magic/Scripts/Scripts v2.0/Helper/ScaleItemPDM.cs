using UnityEngine;
using System.Collections;
namespace Artngame.PDM
{
	public class ScaleItemPDM : MonoBehaviour
	{

		public Vector2 Scale_bounds = new Vector2(1, 2);

		// Use this for initialization
		void Start()
		{
			this.transform.localScale *= Random.Range(Scale_bounds.x, Scale_bounds.y);
		}

		// Update is called once per frame
		void Update()
		{

		}
	}
}