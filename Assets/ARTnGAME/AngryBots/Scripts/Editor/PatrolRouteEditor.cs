using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Artngame.PDM{
	[CustomEditor(typeof(PatrolRoute))]
	public class PatrolRouteEditor : Editor {

		public override void OnInspectorGUI () {
			PatrolRoute route = target as PatrolRoute;

			GUILayout.Label (route.patrolPoints.Count+" Patrol Points in Route");

			route.pingPong = EditorGUILayout.Toggle ("Ping Pong", route.pingPong);
			if (GUI.changed) {
				SceneView.RepaintAll ();
			}

			if (GUILayout.Button("Reverse Direction")) {
				route.patrolPoints.Reverse ();
				SceneView.RepaintAll ();
			}

			if (GUILayout.Button("Add Patrol Point")) {
				Selection.activeGameObject = route.InsertPatrolPointAt (route.patrolPoints.Count);
			}
		}

		void OnSceneGUI () {
			PatrolRoute route  = target as PatrolRoute;

			DrawPatrolRoute (route);
		}

		public static void DrawPatrolRoute (PatrolRoute route) {
			if (route.patrolPoints.Count == 0)
				return;

			Vector3 lastPoint = route.patrolPoints[0].transform.position;

			int loopCount = route.patrolPoints.Count;
			if (route.pingPong)
				loopCount--;

			for (int i = 0; i < loopCount; i++) {
				if (!route.patrolPoints[i])
					break;

				Vector3 newPoint = route.patrolPoints[(i + 1) % route.patrolPoints.Count].transform.position;
				if (newPoint != lastPoint) {
					Handles.color = new Color (0.5f, 0.5f, 1.0f);
					DrawPatrolArrow (lastPoint, newPoint);
					if (route.pingPong) {
						Handles.color = new Color (1.0f, 1.0f, 1.0f, 0.2f);
						DrawPatrolArrow (newPoint, lastPoint);
					}
				}
				lastPoint = newPoint;
			}
		}

		static void DrawPatrolArrow (Vector3 a ,Vector3 b ) {
			Quaternion directionRotation  = Quaternion.LookRotation(b - a);
			//Handles.ConeCap (0, (a + b) * 0.5f - directionRotation * Vector3.forward * 0.5f, directionRotation, 0.7f);
			Handles.ConeHandleCap (0, (a + b) * 0.5f - directionRotation * Vector3.forward * 0.5f, directionRotation, 0.7f,EventType.ContextClick); //v2.3
		}
}
}