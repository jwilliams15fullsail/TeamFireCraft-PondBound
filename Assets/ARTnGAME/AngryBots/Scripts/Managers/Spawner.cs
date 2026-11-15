using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {

	[System.Serializable]
	public class ObjectCache {
		public GameObject prefab;
		public int cacheSize = 10;

		private GameObject[] objects;
		private int cacheIndex = 0;

		public void Initialize ()
		{
			objects = new GameObject[cacheSize];

			// Instantiate the objects in the array and set them to be inactive
			for (int i = 0; i < cacheSize; i++)
			{
				objects[i] = MonoBehaviour.Instantiate (prefab) as GameObject;
				objects[i].SetActive (false);
				objects[i].name = objects[i].name + i;
			}
		}

		public GameObject GetNextObjectInCache () {
			GameObject obj = null;

			// The cacheIndex starts out at the position of the object created
			// the longest time ago, so that one is usually free,
			// but in case not, loop through the cache until we find a free one.
			for (int i = 0; i < cacheSize; i++) {
				obj = objects[cacheIndex];

				// If we found an inactive object in the cache, use that.
				if (!obj.activeSelf)
					break;

				// If not, increment index and make it loop around
				// if it exceeds the size of the cache
				cacheIndex = (cacheIndex + 1) % cacheSize;
			}

			// The object should be inactive. If it's not, log a warning and use
			// the object created the longest ago even though it's still active.
			if (obj.activeSelf) {
				Debug.LogWarning (
					"Spawn of " + prefab.name +
					" exceeds cache size of " + cacheSize +
					"! Reusing already active object.", obj);
				Spawner.Destroy (obj);
			}

			// Increment index and make it loop around
			// if it exceeds the size of the cache
			cacheIndex = (cacheIndex + 1) % cacheSize;

			return obj;
		}
	}

	public class Spawner : MonoBehaviour {

		static Spawner spawner;

		public ObjectCache[] caches;

		public Hashtable activeCachedObjects;

		void Awake () {
			// Set the global variable
			spawner = this;

			// Total number of cached objects
			int amount = 0;

			// Loop through the caches
			for (int i = 0; i < caches.Length; i++) {
				// Initialize each cache
				caches[i].Initialize ();

				// Count
				amount += caches[i].cacheSize;
			}

			// Create a hashtable with the capacity set to the amount of cached objects specified
			activeCachedObjects = new Hashtable (amount);
		}

		public static GameObject Spawn (GameObject prefab, Vector3 position, Quaternion rotation) {
			ObjectCache cache = null;

			// Find the cache for the specified prefab
			if (spawner) {
				for (int i = 0; i < spawner.caches.Length; i++) {
					if (spawner.caches[i].prefab == prefab) {
						cache = spawner.caches[i];
					}
				}
			}

			// If there's no cache for this prefab type, just instantiate normally
			if (cache == null) {
				return Instantiate (prefab, position, rotation) as GameObject;
			}

			// Find the next object in the cache
			GameObject obj = cache.GetNextObjectInCache ();

			// Set the position and rotation of the object
			obj.transform.position = position;
			obj.transform.rotation = rotation;

			// Set the object to be active
			obj.SetActive (true);
			spawner.activeCachedObjects[obj] = true;

			return obj;
		}

		static void Destroy (GameObject objectToDestroy) {
			if (spawner && spawner.activeCachedObjects.ContainsKey (objectToDestroy)) {
				objectToDestroy.SetActive (false);
				spawner.activeCachedObjects[objectToDestroy] = false;
			}
			else {
				//objectToDestroy.Destroy (objectToDestroy); //v2.3			
				Destroy (objectToDestroy);
			}
		}

	}
}
