using UnityEngine;
using System.Collections;

namespace Ai {
	public class AiMove : MonoBehaviour {
		public Transform targetTransform;
		[HideInInspector] public Seeker seeker;
		MoveController controller;

		[HideInInspector] public Pathfinding.Path path; //The calculated path
		[SerializeField] float distanceThreshold = 0.5f; // Minimum move distance
		public float speed = 100; // The AI's speed per second
		public float nextWaypointDistance = 3; // The max distance from the AI to a waypoint for it to continue to the next waypoint
		private int currentWaypoint = 0; // The waypoint we are currently moving towards
		Vector3 destination = Vector3.zero;

		[SerializeField] float delay = 0.5f;
		float delayCountdown;
		
		void Start () {
			seeker = GetComponent<Seeker>();
			controller = GetComponent<MoveController>();

			delayCountdown = delay;
		}
		
		void OnPathComplete (Pathfinding.Path p) {
			if (!p.error) {
				path = p;
				//Reset the waypoint counter
				currentWaypoint = 0;
			} else {
				Debug.Log(p.error);
			}
		}

		void Update () {
			delayCountdown -= Time.deltaTime;
			if (delayCountdown <= 0f ) {
				// Minimum distance to prevent buggy movement
				if (Vector3.Distance(destination, targetTransform.position) > distanceThreshold) {
					seeker.StartPath(transform.position, targetTransform.position, OnPathComplete);
					destination = targetTransform.position;
				}

				delayCountdown = delay;
			}
		}

		void FixedUpdate () {
			if (path == null) {
				return; //We have no path to move after yet
			}
			
			if (currentWaypoint >= path.vectorPath.Count) {
//				Debug.Log("End Of Path Reached");
				return;
			}
			
			//Direction to the next waypoint
			Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
			dir *= speed * Time.fixedDeltaTime;
			controller.SimpleMove(dir);
			
			//Check if we are close enough to the next waypoint
			//If we are, proceed to follow the next waypoint
			if (Vector3.Distance (transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
				currentWaypoint++;
				return;
			}
		}
	}
}

