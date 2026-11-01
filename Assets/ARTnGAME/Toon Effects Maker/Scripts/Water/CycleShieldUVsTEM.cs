using UnityEngine;
using System.Collections;

namespace Artngame.TEM {

public class CycleShieldUVsTEM : MonoBehaviour {
	
	void Start () {

	}
	
	Vector2 Dist = new Vector2 (0f, 0f);
	public Vector2 Speed = new Vector2 (0.04f , 0.04f);
	
	void LateUpdate () {

		Dist = Dist+ Speed * Time.deltaTime;
		GetComponent<Renderer>().material.SetTextureOffset ("_MainTex", Dist);

	}
}
}