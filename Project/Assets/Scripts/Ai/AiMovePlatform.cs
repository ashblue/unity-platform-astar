using UnityEngine;
using System.Collections;

namespace Ai {
	[RequireComponent(typeof(JumpController))]
	[RequireComponent(typeof(FallController))]
	public class AiMovePlatform : MonoBehaviour {
		[SerializeField] bool debug;

		[Tooltip("Target we are getting our destination from")]
		public Transform targetTransform;
		[HideInInspector] public Seeker seeker;
		MoveController controller;
		Pathfinding.PlatformSeeker platformSeeker;
		JumpController jump;
		FallController fall;
		
		[HideInInspector] public Pathfinding.Path path; //The calculated path
		Pathfinding.LinkPath linkPath;

		[Tooltip("Minimum move distance")]
		[SerializeField] float distanceThreshold = 0.5f;

		[Tooltip("The AI's speed per second")]
		public float speed = 2f;

		[Tooltip("The max distance from the AI to a waypoint for it to continue to the next waypoint")]
		public float nextWaypointDistance = 0.2f;

		bool pause = false; // Pause is triggered when the AI should await a callback from another module
		int currentWaypoint = 0; // The waypoint we are currently moving towards
		Vector3 destination = Vector3.zero;

		[Tooltip("Amount of time between updating the destination search")]
		[SerializeField] float delay = 0.5f;
		float delayCountdown;

		[SerializeField] LayerMask whatIsGround;

		[Header("Debugging")]
		bool logPathLinks;
		
		void Start () {
			Pathfinding.LinkPath.debug = logPathLinks;

			seeker = GetComponent<Seeker>();
			platformSeeker = GetComponent<Pathfinding.PlatformSeeker>();
			controller = GetComponent<MoveController>();
			jump = GetComponent<JumpController>();
			fall = GetComponent<FallController>();
			
			delayCountdown = delay;

			// Force snap character's feet to the ground
			transform.position = GetFootingPos(transform.position);
		}

		// Returns a downward raycast point, upon failure returns passed position
		// @NOTE Assumes the player's center is at the bottom middle of their feet
		Vector3 GetFootingPos (Vector3 pos) {
			RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.up * -1f, Mathf.Infinity, whatIsGround);
			if (hit.collider != null) return hit.point;

			return pos;
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
				linkPath = lp;
				currentWaypoint = 0;
			} else {
				LogError("Path failed to properly initialize");
			}
		}
		
		void Update () {
			delayCountdown -= Time.deltaTime;

			if (pause) return;
			
			if (delayCountdown <= 0f ) {
				// Minimum distance to prevent buggy movement
				if (Vector3.Distance(destination, targetTransform.position) > distanceThreshold) {
					platformSeeker.GetPath(transform.position, targetTransform.position, OnLinkPathComplete);
					destination = targetTransform.position;
				}
				
				delayCountdown = delay;
			}
		}


		void SkipNode () {
			Log("Resumed after skipping a node");
			pause = false;
		}

		void Log (string m) {
			if (debug) Debug.Log(m);
		}

		void LogError (string m) {
			if (debug) Debug.LogError(m);
		}

		void FixedUpdate () {
			if (pause) return;

			if (linkPath == null) {
				return; //We have no path to follow
			}
			
			if (currentWaypoint >= linkPath.links.Count) {
				return; // End Of Path Reached
			}

			// Make it so the position is on the same level as our feet
			Vector3 adjustedPos = linkPath.links[currentWaypoint].pos;
			adjustedPos.y = transform.position.y;
			
			// Check if we are close enough to the next waypoint
			// If we are, proceed to follow the next waypoint
			if (Vector3.Distance(transform.position, adjustedPos) < nextWaypointDistance) {
				Log(string.Format("End {0} type: {1}", linkPath.links[currentWaypoint].pos, linkPath.links[currentWaypoint].type));

				// Certain link types will require special move logic
				Astar.LinkType type = linkPath.links[currentWaypoint].type;
				if (type == Astar.LinkType.Jump || type == Astar.LinkType.Runoff) {
					bool jumpValid = jump.SetPos(linkPath.links[currentWaypoint + 1].pos, SkipNode);
					Log(string.Format("Begin {0} type: {1}", linkPath.links[currentWaypoint + 1].pos, linkPath.links[currentWaypoint + 1].type));

					if (!jumpValid) {
						LogError("Path raycast failed, destroying path");
						linkPath = null;
						return;
					}

					pause = true;
					currentWaypoint++;
					return;

				} else if (type == Astar.LinkType.Fall) {
					bool fallValid = fall.SetPos(linkPath.links[currentWaypoint + 1].pos, speed * 2f, SkipNode);
					Log(string.Format("Begin {0} type: {1}", linkPath.links[currentWaypoint + 1].pos, linkPath.links[currentWaypoint + 1].type));

					if (!fallValid) {
						LogError("Path raycast failed, destroying path");
						linkPath = null;
						return;
					}

					pause = true;
					currentWaypoint++;
					return;
				}

				currentWaypoint++;
			}

			// Direction to the next waypoint
			Vector3 dir = (adjustedPos - transform.position).normalized;
			dir *= speed * Time.fixedDeltaTime;
			controller.FeetMove(dir);
		}
	}
}