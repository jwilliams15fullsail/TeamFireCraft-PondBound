using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {

	[System.Serializable]
	public class MaterialImpact {
		public PhysicsMaterial physicMaterial;
		public AudioClip[] playerFootstepSounds;
		public AudioClip[] mechFootstepSounds;
		public AudioClip[] spiderFootstepSounds;
		public AudioClip[] bulletHitSounds;
	}

public class MaterialImpactManager : MonoBehaviour {

		public MaterialImpact[] materials;

		private static Dictionary<PhysicsMaterial, MaterialImpact> dict;
		private static MaterialImpact defaultMat;

		void Awake () {
			defaultMat = materials[0];

			dict = new Dictionary<PhysicsMaterial, MaterialImpact> ();
			for (int i = 0; i < materials.Length; i++) {
				dict.Add (materials[i].physicMaterial, materials[i]);
			}
		}

		public static AudioClip GetPlayerFootstepSound (PhysicsMaterial mat) {
			MaterialImpact imp = GetMaterialImpact (mat);

			//v2.3
			if (imp != null) {
				return GetRandomSoundFromArray (imp.playerFootstepSounds);
			} else {
				return null;
			}
		}

		public static AudioClip GetMechFootstepSound (PhysicsMaterial mat) {
			MaterialImpact imp = GetMaterialImpact (mat);

			//v2.3
			if (imp != null) {
				return GetRandomSoundFromArray(imp.mechFootstepSounds);
			} else {
				return null;
			}

		}

		public static AudioClip GetSpiderFootstepSound (PhysicsMaterial mat) {
			MaterialImpact imp = GetMaterialImpact (mat);

			//v2.3
			if (imp != null) {
				return GetRandomSoundFromArray(imp.spiderFootstepSounds);
			} else {
				return null;
			}

		}

		public static AudioClip GetBulletHitSound (PhysicsMaterial mat) {
			MaterialImpact imp = GetMaterialImpact (mat);

			//v2.3
			if (imp != null) {
				return GetRandomSoundFromArray(imp.bulletHitSounds);
			} else {
				Debug.Log ("no sound");
				return null;
			}


			//return GetRandomSoundFromArray(imp.bulletHitSounds);
		}

		public static MaterialImpact GetMaterialImpact (PhysicsMaterial mat)  {
			if (mat && dict != null && dict.ContainsKey (mat))
				return dict[mat];
			return defaultMat;
		}

		public static AudioClip GetRandomSoundFromArray (AudioClip[] audioClipArray) {
			if (audioClipArray.Length > 0)
				return audioClipArray[Random.Range (0, audioClipArray.Length)];
			return null;
		}
}
}