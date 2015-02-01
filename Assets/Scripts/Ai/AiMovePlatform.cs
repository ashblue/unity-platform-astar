using UnityEngine;
using System.Collections;

namespace Ai {
	public class AiMovePlatform : MonoBehaviour {
		[Tooltip("Target we are getting our destination from")]
		public Transform targetTransform;
		[HideInInspector] public Seeker seeker;
		MoveController controller;
		Pathfinding.PlatformSeeker platformSeeker;
		
		[HideInInspector] public Pathfinding.Path path; //The calculated path

		[Tooltip("Minimum move distance")]
		[SerializeField] float distanceThreshold = 0.5f;

		[Tooltip("The AI's speed per second")]
		public float speed = 100;

		[Tooltip("The max distance from the AI to a waypoint for it to continue to the next waypoint")]
		public float nextWaypointDistance = 3;

		int currentWaypoint = 0; // The waypoint we are currently moving towards
		Vector3 destination = Vector3.zero;

		[Tooltip("Amount of time between updating the destination search")]
		[SerializeField] float delay = 0.5f;
		float delayCountdown;

		[Header("Debugging")]
		bool logPathLinks;
		
		void Start () {
			Pathfinding.LinkPath.debug = logPathLinks;

			seeker = GetComponent<Seeker>();
			platformSeeker = GetComponent<Pathfinding.PlatformSeeker>();
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

		void OnLinkPathComplete (Pathfinding.LinkPath lp) {
			if (!lp.error) {
				Debug.Log("AI script received the path successfully");
			} else {
				Debug.LogError("Path failed to properly initialize");
			}
		}
		
		void Update () {
			delayCountdown -= Time.deltaTime;
			if (delayCountdown <= 0f ) {
				// Minimum distance to prevent buggy movement
				if (Vector3.Distance(destination, targetTransform.position) > distanceThreshold) {
//					seeker.StartPath(transform.position, targetTransform.position, OnPathComplete);
					platformSeeker.GetPath(transform.position, targetTransform.position, OnLinkPathComplete);
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
				return; // End Of Path Reached
			}

			// This is the actual node we are currently examining, look forward 1 step for a comparison
			// path.path[currentWaypoint]

//			Debug.Log(string.Format("Current target {0}", ((Vector3)path.path[currentWaypoint].position).ToString()));

			// @TODO Debug log here the path type we are currently on
			// If we can get the start and end node positions, we can discover if we are stepping into a dynamic link

			// @TODO Special foot placement normalizing
			
			//Direction to the next waypoint
			Vector3 dir = (path.vectorPath[currentWaypoint] - transform.position).normalized;
			dir *= speed * Time.fixedDeltaTime;
			controller.SimpleMove(dir);
			
			//Check if we are close enough to the next waypoint
			//If we are, proceed to follow the next waypoint
			if (Vector3.Distance (transform.position, path.vectorPath[currentWaypoint]) < nextWaypointDistance) {
				// @TODO Here is where we determine if we should change to a different follow type
				currentWaypoint++;
				return;
			}
		}
	}
}