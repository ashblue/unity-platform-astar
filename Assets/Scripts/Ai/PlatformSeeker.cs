using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pathfinding {
	public delegate void OnLinkPathDelegate(LinkPath path);

	public class LinkPoint {
		public Astar.LinkType type;
		public Vector3 pos;

		public LinkPoint (Astar.LinkType type, Vector3 pos) {
			this.type = type;
			this.pos = pos;
		}
	}

	public class LinkPath {
		public static bool debug = false;
		public bool ready = false;
		public bool error;
		public List<LinkPoint> links;

		// Delegate to store callback for firing when complete
		public OnLinkPathDelegate OnLinkPathDelegate;

		public LinkPath (Seeker seeker, Vector3 start, Vector3 end, OnLinkPathDelegate callback) {
			OnLinkPathDelegate = callback;
			seeker.StartPath(start, end, PathPostProcess);
		}

		void PathPostProcess (Path p) {
			links = new List<LinkPoint>();
			for (int i = 0, l = p.path.Count; i < l; i++) {
				Astar.LinkType type;
				if (i != p.path.Count - 1) {
					type = Astar.AstarPlatformHelper.current.GetLinkType(p.path[i], p.path[i + 1]);
				} else {
					// Last item so the link type is going to be none
					type = Astar.LinkType.Undefined;
				}

				if (debug) Debug.Log(string.Format("Node {0} type {1}", p.vectorPath[i].ToString(), type.ToString()));
				links.Add(new LinkPoint(type, p.vectorPath[i]));
			}

			// Mark as ready
			error = p.error;
			ready = true;
			OnLinkPathDelegate(this);
		}
	}

	[RequireComponent(typeof(Seeker))]
	public class PlatformSeeker : MonoBehaviour {
		Seeker seeker;

		void Awake () {
			seeker = GetComponent<Seeker>();
		}

		public LinkPath GetPath (Vector3 start, Vector3 end, OnLinkPathDelegate callback) {
			// Combine the callback with our PathPostProcess callback somehow

			return new LinkPath(seeker, start, end, callback);
		}


	}
}
