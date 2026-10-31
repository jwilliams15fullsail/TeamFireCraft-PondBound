using System.Collections;
using System;
using UnityEngine;

namespace Artngame.TEM {

public class DragTransformTEM: MonoBehaviour {

	public Color mouseOverColor = Color.blue;
	private Color originalColor ;

	void Start() {
		
	}
	void OnMouseEnter() {
		
	}

	void OnMouseExit() {
		
	}

	IEnumerator  OnMouseDown() {
		Vector3 screenSpace = Camera.main.WorldToScreenPoint(transform.position);
		Vector3 offset = transform.position - Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z));
		while (Input.GetMouseButton(0))
		{
			Vector3 curScreenSpace = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenSpace.z);
			Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenSpace) + offset;
			transform.position = curPosition;

			yield return 1;
		}
	}

}

}