using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Artngame.PDM{
	[CustomEditor(typeof(PatrolPoint))]
	public class PatrolPointEditor : Editor {

		public override void OnInspectorGUI () {
			PatrolPoint point  = target as PatrolPoint;
			PatrolRoute route  = point.transform.parent.GetComponent<PatrolRoute>();
			int thisIndex  = route.GetIndexOfPatrolPoint (point);

			if (GUILayout.Button ("Remove This Patrol Point")) {
				route.RemovePatrolPointAt (thisIndex);
				int newSelectionIndex  = Mathf.Clamp (thisIndex, 0, route.patrolPoints.Count - 1);
				Selection.activeGameObject = route.patrolPoints[newSelectionIndex].gameObject;
			}
			if (GUILayout.Button ("Insert Patrol Point Before")) {
				Selection.activeGameObject = route.InsertPatrolPointAt (thisIndex);
			}
			if (GUILayout.Button ("Insert Patrol Point After")) {
				Selection.activeGameObject = route.InsertPatrolPointAt (thisIndex + 1);
			}
		}

		void OnSceneGUI () {
			PatrolPoint point = target as PatrolPoint;
			PatrolRoute route = point.transform.parent.GetComponent<PatrolRoute>();

			PatrolRouteEditor.DrawPatrolRoute (route);
		}
}
}