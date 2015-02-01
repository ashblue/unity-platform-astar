using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pathfinding.Nodes;
using Pathfinding.Serialization.JsonFx;
using Pathfinding.Serialization;

namespace Astar {
	public class NodeLedge {
		public Pathfinding.GraphNode node;
		public bool facingRight;
		public bool facingLeft;
		public Vector3 pos;

		public void SetNode (Pathfinding.GraphNode node) {
			this.node = node;
			pos = (Vector3)node.position;
		}

		static public bool IsFacingNode (NodeLedge n1, NodeLedge n2) {
			return n1.facingLeft == n2.facingRight || n1.facingRight == n2.facingLeft;
		}
	}

	[JsonOptIn]
	public class AstarGraphPlatform : Pathfinding.GridGraph {
		List<Pathfinding.GraphNode> nodeBlacklist;
		List<NodeLedge> nodeLedges;

		bool logDetails = false;

		public override void ScanInternal (OnScanStatus statusCallback) {
			base.ScanInternal(statusCallback);

			DiscoverPlatforms();

			// Attempt to generate links
			Astar.AstarPlatformHelper platformHelper = Astar.AstarPlatformHelper.current;
			if (platformHelper != null) {
				platformHelper.CreateLinks(this, nodeLedges);
			}

			// Plug the graph points
			foreach (Pathfinding.GraphNode n in nodeBlacklist) {
				n.Walkable = false;
			}
		}

		void DiscoverPlatforms () {
			nodeBlacklist = new List<Pathfinding.GraphNode>();
			nodeLedges = new List<NodeLedge>();
			
			// Hit every node in the graph
			GetNodes(AnalyzeNode);
		}

		bool AnalyzeNode (Pathfinding.GraphNode node) {
			// Ignore already blocked tiles
			if (!node.Walkable) {
				nodeBlacklist.Add(node);
				return true;
			} 
			
			Vector3 pos = (Vector3)node.position;
			Pathfinding.GraphNode southNeighbor = GetNeighbor(node, 0, -1);
			
			if (southNeighbor == null || // No south neighbor means this tile cannot be stood upon
			    southNeighbor.Walkable == true) { // South tile is walkable, meaning no ledge to stand on
				nodeBlacklist.Add(node);
				return true;
			} 
			
			Log(pos.ToString());
			Log("Found a walkable tile");
			
			// Attempt to discover a ledge
			bool facingLeft = HasNeighbor(node, -1, -1);
			bool facingRight = HasNeighbor(node, 1, -1);
			
			if (facingLeft || facingRight) {
				nodeLedges.Add(new NodeLedge {
					node = node,
					facingLeft = facingLeft,
					facingRight = facingRight,
					pos = (Vector3)node.position
				});
			}
			
			return true;
		}
		
		public bool HasNeighbor (Pathfinding.GraphNode node, int xDir, int yDir, bool walkable = true) {
			Pathfinding.Int3 pos = node.position;
			pos.x += (int)(xDir * nodeSize * 1000);
			pos.y += (int)(yDir * nodeSize * 1000);
			
			Pathfinding.NNInfo posInfo = GetNearest((Vector3)pos);
			return posInfo.node.position == pos && posInfo.node.Walkable == walkable;
		}
		
		public Pathfinding.GraphNode GetNeighbor (Pathfinding.GraphNode node, int xDir, int yDir) {
			Pathfinding.GraphNode nodeTarget = null;
			
			// We must mimic the int format to try and discover the node we want
			Pathfinding.Int3 pos = node.position;
			pos.x += (int)(xDir * nodeSize * 1000);
			pos.y += (int)(yDir * nodeSize * 1000);
			
			Pathfinding.NNInfo posInfo = GetNearest((Vector3)pos);
			if (posInfo.node.position == pos)
				nodeTarget = posInfo.node;
			
			return nodeTarget;
		}
		
		void Log (string s) {
			if (logDetails) Debug.Log(s);
		}
	}
}

