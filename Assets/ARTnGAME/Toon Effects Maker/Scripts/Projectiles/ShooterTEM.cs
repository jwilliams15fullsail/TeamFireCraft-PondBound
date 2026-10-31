using UnityEngine;
using System.Collections;

namespace Artngame.TEM {

public class ShooterTEM : MonoBehaviour
{
	public Rigidbody projectile;
	//public Transform shotPos;
	public float shotForce = 1000f;
	//public float moveSpeed = 10f;

		RaycastHit hit;
		public float range = 100.0f;
		Vector3 Direction;
		public float Ahead_of_camera = 10;

		float Timer;
		public float Click_Delay=0.5f;//delay to avoid collisions on birth
		public bool No_delay = false;

		void Start ()
		{
			Timer = Time.fixedTime;
		}

		public bool Curved = false;
		public bool OnMouseClick = true;
		public float curve_factor = 0.5f;
		public bool Turret_mode =false;
		public Transform Turret;//use instead of camera, for 3rd person situations
	
	void Update ()
	{
		//float h = Input.GetAxis("Horizontal") * Time.deltaTime * moveSpeed;
		//float v = Input.GetAxis("Vertical") * Time.deltaTime * moveSpeed;		
		//transform.Translate(new Vector3(h, v, 0f));

			if(((Input.GetMouseButton(0) & OnMouseClick)|(Input.GetButtonUp("Fire1") & !OnMouseClick)) & ((Time.fixedTime - Timer > Click_Delay) | No_delay))
			{
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);		


				if(Physics.Raycast(ray, out hit, range))
				{

					Direction = hit.point - Camera.main.transform.position;
					if(Turret_mode & Turret !=null){
						Direction = hit.point - Turret.position;
					}
				}	

				if(!OnMouseClick){
					Direction = Camera.main.transform.forward;
					if(Turret_mode & Turret !=null){
						Direction = Turret.forward;
					}
				}

				Timer = Time.fixedTime;

				//if(Input.GetButtonUp("Fire1"))
				{
					//Rigidbody shot = Instantiate(projectile, shotPos.position, shotPos.rotation) as Rigidbody;
					//shot.AddForce(shotPos.forward * shotForce);

					Vector3 Pos = Camera.main.transform.position+Camera.main.transform.forward*Ahead_of_camera;
					Quaternion Rot = Camera.main.transform.rotation;
					if(Turret_mode & Turret !=null){
						Pos = Turret.position;
						Rot = Turret.rotation;
					}

					Rigidbody shot = Instantiate(projectile, Pos, Rot) as Rigidbody;

					shot.gameObject.SetActive(true);

					if(!Curved){
						shot.AddForce(Direction.normalized * shotForce);
					}else{
						shot.AddForce(Vector3.Lerp(Direction.normalized,Vector3.up,curve_factor) * shotForce);
					}
					//Debug.Log (Direction);
				}
			}
	}
}
}