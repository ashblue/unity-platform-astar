using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Astar {
	public enum LinkType {
		Jump,
		Fall,
		Runoff,
		Move,
		Undefined
	}

	// Special link class aimed at housing a data structure for double dictionary keys
	public class PathLink {
		public Dictionary<Pathfinding.GraphNode, LinkType> links = new Dictionary<Pathfinding.GraphNode, LinkType>();

		public void SetLink (Pathfinding.GraphNode node, LinkType link) {
			links[node] = link;
		}

		public bool HasLink (Pathfinding.GraphNode node) {
			return links.ContainsKey(node);
		}

		public LinkType GetLinkType (Pathfinding.GraphNode node) {
			if (!HasLink(node)) return LinkType.Undefined;
			return links[node];
		}
	}

	// Modify the graph at run-time to discover platforms
	public class AstarPlatformHelper : MonoBehaviour {
		static public AstarPlatformHelper current;
		Astar.AstarGraphPlatform gridGraph;

		[SerializeField] float maxJumpDistance = 10f;
		[SerializeField] Vector2 runoffAngle;

		[Header("Link Prefabs")]
		[SerializeField] Transform linkOutput;
		[SerializeField] Pathfinding.NodeLink jumpPrefab;
		[SerializeField] Pathfinding.NodeLink fallPrefab;
		[SerializeField] Pathfinding.NodeLink runoffPrefab;

		[Header("Debug")]
		[SerializeField] bool logDetails;

		// To make sure our link data doesn't crash when updating the graph we have to swap it, but only when its ready
		Dictionary<Pathfinding.GraphNode, PathLink> linkDataStable = new Dictionary<Pathfinding.GraphNode, PathLink>();
		Dictionary<Pathfinding.GraphNode, PathLink> linkData;

		void Awake () {
			current = this;
		}

		public LinkType GetLinkType (Pathfinding.GraphNode n1, Pathfinding.GraphNode n2) {
			PathLink p = linkDataStable.ContainsKey(n1) ? linkDataStable[n1] : null;
			if (p != null) {
				return p.GetLinkType(n2);
			}

			return LinkType.Undefined;
		}

		public void CreateLinks (Astar.AstarGraphPlatform graph, List<NodeLedge> nodeLedges) {
			linkData = new Dictionary<Pathfinding.GraphNode, PathLink>();
			this.gridGraph = graph;

			// Clean out old links
			foreach (Transform child in linkOutput) {
				Destroy(child.gameObject);
			}

			Log("Link generation");
			foreach (NodeLedge ledge1 in nodeLedges) {
				Log("New Link Test");
				Log(ledge1.pos.ToString());
//				linkData.Add(ledge1.node, new PathLink());

				// Generate jump nodes between ledges
				foreach (NodeLedge ledge2 in nodeLedges) {
					if (ledge1 == ledge2) continue;
					if (!NodeLedge.IsFacingNode(ledge1, ledge2)) continue;

					// Verify heading to the new ledge is in the correct direction
					Vector3 heading = ledge2.pos - ledge1.pos;
					if (ledge1.facingRight && ledge2.facingLeft && heading.x < 0f) continue;
					if (ledge1.facingLeft && ledge2.facingRight && heading.x > 0f) continue;

					if (Vector3.Distance((Vector3)ledge1.node.position, (Vector3)ledge2.node.position) > maxJumpDistance) continue; // Within max jump distance

					if (!gridGraph.Linecast((Vector3)ledge1.node.position, (Vector3)ledge2.node.position)) {
						Log("Valid link found");
						Log(((Vector3)ledge2.node.position).ToString());
				
						SetLink(LinkType.Jump, ledge1.node, ledge2.node);
					}
				}

				// Drop linecast
				if (ledge1.facingRight) DropLine(ledge1, 1);
				if (ledge1.facingLeft) DropLine(ledge1, -1);

				// Ledge runoff
				// @TODO Needs to be a 2 way link
				if (ledge1.facingRight) {
					Vector3 corner = ledge1.pos;
					corner.x += (gridGraph.nodeSize / 2);
					corner.y += (gridGraph.nodeSize / 2);

					RaycastHit2D hit = Physics2D.Raycast(ledge1.pos, runoffAngle, Mathf.Infinity, gridGraph.collision.mask);
					if (hit.collider != null) {
						Pathfinding.GraphNode node = gridGraph.GetNeighbor(gridGraph.GetNearest(hit.point).node, 0, 1);
						if (node.Walkable) {
							bool withinJumpDistance = Vector3.Distance(ledge1.pos, (Vector3)node.position) < maxJumpDistance;
							SetLink(LinkType.Runoff, ledge1.node, node, !withinJumpDistance);
//							Pathfinding.NodeLink runoffLink = SetLink(LinkType.Runoff, ledge1.node, node);
//							if (Vector3.Distance(ledge1.pos, runoffLink.transform.position) < maxJumpDistance)
//								runoffLink.oneWay = false;
						}
					}
				}
			}

			// Attempt a linecast from every discovered ledge to 
			// - ledges in its facing direction
			// - within max jump distance
//			gridGraph.Linecast
			// These ledges should become jump links going one way

			// Attempt a drop linecast from every discoverd ledge
			// - max fall distance
			// These become fall links going one way

			// Attempt ledge runoff linecasts on every discovered ledge
			// - max jump distance
			// These become runoff links with the origin being the end of a jump (one way)
			// Maybe just make it a 2 way jump?
			linkDataStable = linkData;
			linkData = null;
		}

		void DropLine (NodeLedge l, int xDir) {
			Vector3 pos = (Vector3)gridGraph.GetNeighbor(l.node, xDir, 0).position;
			RaycastHit2D hit = Physics2D.Raycast(pos, Vector2.up * -1f, Mathf.Infinity, gridGraph.collision.mask);
			if (hit.collider != null) {
				Pathfinding.GraphNode node = gridGraph.GetNeighbor(gridGraph.GetNearest(hit.point).node, 0, 1);
				SetLink(LinkType.Fall, l.node, node);
			}
		}

		Pathfinding.NodeLink SetLink (LinkType linkType, Pathfinding.GraphNode start, Pathfinding.GraphNode end, bool oneWay = true) {		
			Pathfinding.NodeLink link = (Pathfinding.NodeLink)Instantiate(GetPrefab(linkType));
			link.transform.position = (Vector3)start.position;
			link.end.transform.position = (Vector3)end.position;
			link.transform.SetParent(linkOutput);
			link.oneWay = oneWay;

			if (!linkData.ContainsKey(start)) linkData[start] = new PathLink();
			linkData[start].SetLink(end, linkType);

			if (!oneWay) {
				if (!linkData.ContainsKey(end)) linkData[end] = new PathLink();
				linkData[end].SetLink(start, linkType);
			}

			return link;
		}

		Pathfinding.NodeLink GetPrefab (LinkType t) {
			if (t == LinkType.Fall) {
				return fallPrefab;
			} else if (t == LinkType.Jump) {
				return jumpPrefab;
			} else if (t == LinkType.Runoff) {
				return runoffPrefab;
			}

			Debug.LogError("Given link type is not supported");
			return null;
		}
		
		void Log (string s) {
			if (logDetails) Debug.Log(s);
		}
	}
}

