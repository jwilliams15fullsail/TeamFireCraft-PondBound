using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Artngame.PDM {

	[RequireComponent(typeof(Collider))]
	public class PatrolRoute : MonoBehaviour {
		
		public bool pingPong = false;
		// TODO: In Unity 3.3 remove the System.Collections.Generic and impoprt the namespace instead
		public List<PatrolPoint> patrolPoints = new List<PatrolPoint> ();

		private List<GameObject> activePatrollers= new List<GameObject> ();

		public void Register (GameObject go) {
			activePatrollers.Add (go);
		}

		public void UnRegister (GameObject go) {
			activePatrollers.Remove (go);
		}

		void OnTriggerEnter (Collider other) {
			if (activePatrollers.Contains (other.gameObject)) {
				AI ai = other.gameObject.GetComponentInChildren<AI> ();
				if (ai)
					ai.OnEnterInterestArea ();
			}
		}

		void OnTriggerExit (Collider other) {
			if (activePatrollers.Contains (other.gameObject)) {
				AI ai = other.gameObject.GetComponentInChildren<AI> ();
				if (ai)
					ai.OnExitInterestArea ();
			}
		}

		public int GetClosestPatrolPoint (Vector3 pos) {
			if (patrolPoints.Count == 0)
				return 0;

			float shortestDist = Mathf.Infinity;
			int shortestIndex = 0;
			for (int i = 0; i < patrolPoints.Count; i++) {
				//patrolPoints[i].position = patrolPoints[i].transform.position;
				patrolPoints[i].position = patrolPoints[i].gameObject.transform.position;//v2.4
				float dist = (patrolPoints[i].position - pos).sqrMagnitude;
				if (dist < shortestDist) {
					shortestDist = dist;
					shortestIndex = i;
				}
			}

			// If going towards the closest point makes us go in the wrong direction,
			// choose the next point instead.
			if (!pingPong || shortestIndex < patrolPoints.Count - 1) {
				int nextIndex = (shortestIndex + 1) % patrolPoints.Count;
				float angle = Vector3.Angle (
					patrolPoints[nextIndex].position - patrolPoints[shortestIndex].position,
					patrolPoints[shortestIndex].position - pos
				);
				if (angle > 120)
					shortestIndex = nextIndex;
			}

			return shortestIndex;
		}

		void OnDrawGizmos () {
			if (patrolPoints.Count == 0)
				return;

			Gizmos.color = new Color (0.5f, 0.5f, 1.0f);

			Vector3 lastPoint = patrolPoints[0].gameObject.transform.position;
			int loopCount = patrolPoints.Count;
			if (pingPong)
				loopCount--;
			for (int i = 0; i < loopCount; i++) {
				if (!patrolPoints[i])
					break;
				Vector3 newPoint = patrolPoints[(i + 1) % patrolPoints.Count].transform.position;
				Gizmos.DrawLine (lastPoint, newPoint);
				lastPoint = newPoint;
			}
		}

		public int GetIndexOfPatrolPoint (PatrolPoint point) {
			for (int i = 0; i < patrolPoints.Count; i++) {
				if (patrolPoints[i] == point)
					return i;
			}
			return -1;
		}

		public GameObject InsertPatrolPointAt (int index) {
			GameObject go = new GameObject ("PatrolPoint", typeof(PatrolPoint)); //v2.3
			go.transform.parent = transform;
			int count = patrolPoints.Count;

			if (count == 0) {
				go.transform.localPosition = Vector3.zero;
				patrolPoints.Add(go.GetComponent<PatrolPoint>());
			}
			else {
				if (!pingPong || (index > 0 && index < count) || count < 2) {
					index = index % count;
					int prevIndex = index - 1;
					if (prevIndex < 0)
						prevIndex += count;

					go.transform.position = (
						patrolPoints[prevIndex].transform.position
						+ patrolPoints[index].transform.position
					) * 0.5f;
				}
				else if (index == 0) {
					go.transform.position = (
						patrolPoints[0].transform.position * 2
						- patrolPoints[1].transform.position
					);
				}
				else {
					go.transform.position = (
						patrolPoints[count-1].transform.position * 2
						- patrolPoints[count-2].transform.position
					);
				}
				patrolPoints.Insert(index, go.GetComponent<PatrolPoint>());
			}

			return go;
		}

		public void RemovePatrolPointAt (int index) {
			GameObject go = patrolPoints[index].gameObject;
			patrolPoints.RemoveAt (index);
			DestroyImmediate (go);
		}

	}
}