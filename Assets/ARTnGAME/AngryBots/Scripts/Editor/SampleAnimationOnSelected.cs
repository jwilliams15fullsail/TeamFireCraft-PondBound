using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Artngame.PDM{
public class SampleAnimationOnSelected : MonoBehaviour {

		//@MenuItem ("Tools/Sample Animation On Selected")
		[MenuItem("Tools/Sample Animation(s) On Selected")]
		static void SampleAnimation () {
			Animation anim = Selection.activeGameObject.GetComponent<Animation>();
			if (anim != null) {
				anim.clip.SampleAnimation(Selection.activeGameObject, 0);
			}
		}
}
}