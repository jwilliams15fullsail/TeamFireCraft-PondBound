using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {
public class Health : MonoBehaviour {

		public float maxHealth = 100.0f;
		public float health = 100.0f;
		public float regenerateSpeed = 0.0f;
		public bool invincible = false;
		public bool dead = false;

		public GameObject damagePrefab;
		public Transform damageEffectTransform;
		public float damageEffectMultiplier = 1.0f;
		public bool damageEffectCentered = true;

		public GameObject scorchMarkPrefab = null;
		private GameObject scorchMark = null;

		private float lastDamageTime = 0;

		private ParticleSystem damageEffect;//private ParticleEmitter damageEffect; //v2.3

		private float damageEffectCenterYOffset;

		private float colliderRadiusHeuristic = 1.0f;

		//bool enabled;//v2.3

		void Awake () {
			enabled = false;
			if (damagePrefab) {
				if (damageEffectTransform == null)
					damageEffectTransform = transform;
				GameObject effect = Spawner.Spawn (damagePrefab, Vector3.zero, Quaternion.identity);
				effect.transform.parent = damageEffectTransform;
				effect.transform.localPosition = Vector3.zero;

				//v2.3
				//damageEffect = effect.GetComponent<ParticleEmitter>();
				damageEffect = effect.GetComponent<ParticleSystem>();

				Vector2 tempSize = new Vector2(GetComponent<Collider>().bounds.extents.x,GetComponent<Collider>().bounds.extents.z);
				colliderRadiusHeuristic = tempSize.magnitude * 0.5f;
				damageEffectCenterYOffset = GetComponent<Collider>().bounds.extents.y;

			}
			if (scorchMarkPrefab) {
				scorchMark = GameObject.Instantiate(scorchMarkPrefab, Vector3.zero, Quaternion.identity);
				scorchMark.SetActive (false);
			}
		}

		public void OnDamage (float amount, Vector3 fromDirection) {
			// Take no damage if invincible, dead, or if the damage is zero
			if(invincible)
				return;
			if (dead)
				return;
			if (amount <= 0)
				return;

			// Decrease health by damage and send damage signals

			// @HACK: this hack will be removed for the final game
			//  but makes playing and showing certain areas in the
			//  game a lot easier
			/*
	#if !UNITY_IPHONE && !UNITY_ANDROID && !UNITY_WP8
	if(gameObject.tag != "Player")
		amount *= 10.0;
	#endif
	*/
			health -= amount;
			damageSignals.SendSignals (this);
			lastDamageTime = Time.time;

			// Enable so the Update function will be called
			// if regeneration is enabled
			if (regenerateSpeed > 0)
				enabled = true;

			// Show damage effect if there is one
			if (damageEffect) {
				damageEffect.transform.rotation = Quaternion.LookRotation (fromDirection, Vector3.up);
				if(!damageEffectCentered) {
					Vector3 dir = fromDirection;
					dir.y = 0.0f;
					damageEffect.transform.position = (transform.position + Vector3.up * damageEffectCenterYOffset) + colliderRadiusHeuristic * dir;
				}
				// @NOTE: due to popular demand (ethan, storm) we decided
				// to make the amount damage independent ...
				//var particleAmount = Random.Range (damageEffect.minEmission, damageEffect.maxEmission + 1);
				//particleAmount = particleAmount * amount * damageEffectMultiplier;

				//v2.3
				//var particleAmount = Random.Range (damageEffect.minEmission, damageEffect.maxEmission + 1);
				float particleAmount = amount * damageEffectMultiplier * Random.Range (damageEffectMultiplier*2, damageEffectMultiplier*10 + 1);
				damageEffect.Emit((int)particleAmount); //damageEffect.Emit();// (particleAmount);
			}

			// Die if no health left
			if (health <= 0)
			{
				GameScore.RegisterDeath (gameObject);

				health = 0;
				dead = true;
				dieSignals.SendSignals (this);
				enabled = false;

				// scorch marks
				if (scorchMark) {
					scorchMark.SetActive (true);
					// @NOTE: maybe we can justify a raycast here so we can place the mark
					// on slopes with proper normal alignments
					// @TODO: spawn a yield Sub() to handle placement, as we can
					// spread calculations over several frames => cheap in total
					Vector3 scorchPosition = GetComponent<Collider>().ClosestPointOnBounds (transform.position - Vector3.up * 100);
					scorchMark.transform.position = scorchPosition + Vector3.up * 0.1f;
					//scorchMark.transform.eulerAngles.y = Random.Range (0.0f, 90.0f);
					scorchMark.transform.eulerAngles = new Vector3(scorchMark.transform.eulerAngles.x,Random.Range (0.0f, 90.0f),scorchMark.transform.eulerAngles.z);//v2.3
				}
			}
		}

		void OnEnable () {
			Regenerate ();
		}

		// Regenerate health
		IEnumerator Regenerate () {
			if (regenerateSpeed > 0.0f) {
				while (enabled) {
					if (Time.time > lastDamageTime + 3) {
						health += regenerateSpeed;

						//yield; //v2.3
						yield return true; //v2.3

						if (health >= maxHealth) {
							health = maxHealth;
							enabled = false;
						}
					}
					//yield WaitForSeconds (1.0f); //v2.3
					yield return new WaitForSeconds (1.0f); //v2.3
				}
			}
		}

		public SignalSender damageSignals;
		public SignalSender dieSignals;

	// Use this for initialization
//	void Start () {
//		
//	}
//	
//	// Update is called once per frame
//	void Update () {
//		
//	}
}
}
