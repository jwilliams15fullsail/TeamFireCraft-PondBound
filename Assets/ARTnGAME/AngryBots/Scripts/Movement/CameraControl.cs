using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Artngame.PDM {
public class CameraControl : MonoBehaviour {

		//public Joystick m_LeftJoystick; 

		public Vector2 m_LeftStickPosition;

		public Vector3 moved;
		public Transform cursorObject;
		public Transform m_Player;

		public Vector3 m_Offset2Player;

		// Use this for initialization
		public void Start () { 
			if(!m_Player)
				Debug.LogError("No player found or player is not tagged!");
			m_Offset2Player = transform.position-m_Player.position;
		}

		// Update is called once per frame
		public void Update () {
			if(Application.platform != RuntimePlatform.IPhonePlayer) {
				// Left stick update
				m_LeftStickPosition.x = Input.GetAxis("Horizontal");
				m_LeftStickPosition.y = Input.GetAxis("Vertical");

				// Make sure direction vector doesn't exceed length of 1
				if (m_LeftStickPosition.sqrMagnitude > 1)
					m_LeftStickPosition.Normalize();
			} else {
				//m_LeftStickPosition = m_LeftJoystick.position;
			}
		}
}
}