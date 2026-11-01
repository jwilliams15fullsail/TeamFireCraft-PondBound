using UnityEngine;
using System.Collections;

namespace Artngame.TEM {
	[ExecuteInEditMode]
public class ParticleRotationAxisTEM : MonoBehaviour {

	// Use this for initialization
	void Start () {
		myArrayA = this.GetComponent<ParticleSystem>();
	}

	ParticleSystem myArrayA;
	public Vector3 Axis=new Vector3(0,1,0);

		public bool Keep_alive=true;
		public bool Full_lifetime=true;
	
	// Update is called once per frame
	void Update () {

		if(myArrayA!=null){
			ParticleSystem.Particle[] myArray = new ParticleSystem.Particle[myArrayA.particleCount];
			int count = myArrayA.GetParticles(myArray);
			for(int i = 0;i < count;i++)
			{
				myArray[i].axisOfRotation = Axis;

					if(Keep_alive){
						if(Full_lifetime){
							if(myArray[i].remainingLifetime < 0.1f){
								myArray[i].remainingLifetime = myArrayA.main.startLifetime.constant; //v2.3 PDM
								//myArray[i].startLifetime=15;
							}
						}
						else{
							myArray[i].remainingLifetime = 15;
							myArray[i].startLifetime=15;
						}
					}
			}
			myArrayA.SetParticles(myArray,myArrayA.particleCount);
		}	
	}
}
}