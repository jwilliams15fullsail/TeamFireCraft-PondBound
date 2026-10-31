using UnityEngine;
using System.Collections;

namespace Artngame.TEM {

public class SoundEffectsTEM : MonoBehaviour {	

	public AudioClip crashSoft;
	public AudioClip crashHard_Blast;//for bombs and heavy impact
	public AudioClip shootSound;
	public AudioClip travelSound;

	public bool Affect_by_speed=false;
	public bool Sound_on_collision = false;
	public bool Sound_on_travel = false;
	public bool Sound_on_start = false;	
	private AudioSource source;
	public float lowPitchRange = 0.75f;
	public float highPitchRange = 1.5f;
	public float velToVol = 0.2f;
	public float velocityClipSplit = 10f;
		public float volLowRange = 0.5f;
		public float volHighRange = 1.0f;
	float Timing;
		public float Delay;

	void Awake () {		
		source = GetComponent<AudioSource>();
	}

	void Start(){
			Timing = Time.fixedTime;

			this_transform=this.transform;
	}

		Transform this_transform;

	bool played_start = false;
	bool played_travel = false;

		public float Start_elim_dist = 50;

	void Update(){

			if((this_transform.position-Camera.main.transform.position).magnitude > Start_elim_dist){
				volLowRange = Mathf.Lerp(volLowRange,0,Time.deltaTime*0.3f);
				volHighRange = Mathf.Lerp(volHighRange,0,Time.deltaTime*0.3f);
				source.volume = volHighRange;
			}


			float hitVol = Random.Range(volLowRange, volHighRange);

			if(Sound_on_start){
				//Play sound effect
				if(!played_start){
					source.PlayOneShot(shootSound,hitVol);
					played_start = true;
					source.loop = false;
				}
			}
			if(Sound_on_travel & (!Sound_on_start | (Sound_on_start & played_start ))){
				if(Time.fixedTime - Timing > Delay){
					//Play sound effect
					if(!played_travel){

						source.clip = travelSound;
						source.volume = hitVol;
						source.Play();
						source.loop = true;
						played_travel = true;
					}
				}
			}
	}
	void OnCollisionEnter (Collision coll)
	{
			if(Sound_on_collision & (crashSoft!=null | crashHard_Blast!=null)){
			source.pitch = Random.Range (lowPitchRange,highPitchRange);
			float hitVol = coll.relativeVelocity.magnitude * velToVol;
			if(!Affect_by_speed){
				hitVol = Random.Range(volLowRange, volHighRange);
			}
			if (coll.relativeVelocity.magnitude < velocityClipSplit){
					if(crashSoft!=null){
						source.Stop();
						source.PlayOneShot(crashSoft,hitVol);
						source.loop = false;
					}else{
						if(crashHard_Blast!=null){
							source.Stop();
							source.PlayOneShot(crashHard_Blast,hitVol);
							source.loop = false;
						}
					}
			}
			else{
					if(crashHard_Blast!=null){
						source.Stop();
						source.PlayOneShot(crashHard_Blast,hitVol);
						source.loop = false;
					}else{
						if(crashSoft!=null){
							source.Stop();
							source.PlayOneShot(crashSoft,hitVol);
							source.loop = false;
						}
					}
			}
		}
	}	
  }
}