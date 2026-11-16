using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace Artngame.PDM{
public class PatrolRouteConnector : MonoBehaviour {

		[MenuItem("Tools/Assign Closest Patrol Route(s)")]
		//@MenuItem ("Tools/Assign Closest Patrol Routes") 
		static void AssignPatrolRoutes () {
			PatrolPoint[] points  = FindObjectsOfType (typeof(PatrolPoint)) as PatrolPoint[];
			PatrolMoveController[] patrollers= FindObjectsOfType (typeof(PatrolMoveController)) as PatrolMoveController[];
			int connected  = 0;

			foreach (PatrolMoveController patroller in patrollers) {//for (var patroller : PatrolMoveController in patrollers) {
				float closestDist = Mathf.Infinity;
				PatrolPoint closestPoint = new PatrolPoint();
				foreach (PatrolPoint point in points) {//for (var point : PatrolPoint in points) {
					float dist = (patroller.transform.position - point.transform.position).magnitude;
					if (dist < closestDist) {
						closestPoint = point;
						closestDist = dist;
					}
				}
				if (closestDist < Mathf.Infinity) {//if (closestDist != null) { //v2.3
					patroller.patrolRoute = closestPoint.transform.parent.GetComponent<PatrolRoute>();
					connected++;
				}
			}

			Debug.Log("Successfully connected routes to "+connected+" out of "+patrollers.Length+" patrollers.");
		}
}
}