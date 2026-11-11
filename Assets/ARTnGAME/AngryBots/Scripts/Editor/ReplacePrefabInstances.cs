using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Artngame.PDM{
	public class ReplacePrefabInstances : ScriptableWizard {

		public GameObject originalPrefab ;
		public GameObject replacementPrefab ;

		[MenuItem("Tools/Replace Prefab(s) Instances")]//@MenuItem ("Tools/Replace Prefab Instances") 
		static void CreateWizard () {
			ScriptableWizard.DisplayWizard<ReplacePrefabInstances> ("Replace Prefab Instances", "Replace");
		}
		void OnWizardCreate () {
			if (!originalPrefab || !replacementPrefab)
				return;

			UnityEngine.Object[] gos  = FindObjectsOfType (typeof(GameObject));
			for (int i = 0; i<gos.Length; i++) {
                //if (PrefabUtility.GetPrefabParent (gos[i]) == originalPrefab) {
                if (PrefabUtility.GetCorrespondingObjectFromSource(gos[i]) == originalPrefab)
                {
                    GameObject oldGo = gos[i] as GameObject;
					GameObject newGo = PrefabUtility.InstantiatePrefab (replacementPrefab) as GameObject;
					newGo.transform.parent = oldGo.transform.parent;
					newGo.transform.localPosition = oldGo.transform.localPosition;
					newGo.transform.localRotation = oldGo.transform.localRotation;
					newGo.transform.localScale = oldGo.transform.localScale;
					newGo.isStatic = oldGo.isStatic;
					newGo.layer = oldGo.layer;
					newGo.tag = oldGo.tag;
					newGo.name = oldGo.name.Replace (originalPrefab.name, replacementPrefab.name);
					DestroyImmediate (oldGo);
				}
			}
		}
}
}