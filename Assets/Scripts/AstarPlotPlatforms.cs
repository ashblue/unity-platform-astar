using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Astar {
	// Modify the graph at run-time to discover platforms
	public class AstarPlotPlatforms : MonoBehaviour {
		[SerializeField] bool logDetails;

		List<Pathfinding.GraphNode> nodeBlacklist;
		Pathfinding.NavGraph navGraph;
		Pathfinding.GridGraph gridGraph;

		void Start () {
			navGraph = AstarPath.active.graphs[0];
			gridGraph = AstarPath.active.astarData.gridGraph;
			DiscoverPlatforms();
		}

		// @TODO This should all be moved into a custom scanner so it can be updated at run-time with dynamic blockers
		void DiscoverPlatforms () {
			nodeBlacklist = new List<Pathfinding.GraphNode>();

			// Hit every node in the graph
			navGraph.GetNodes(AnalyzeNode);

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

			// Plug the graph points
			foreach (Pathfinding.GraphNode n in nodeBlacklist) {
				n.Walkable = false;
			}
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
			if (HasNeighbor(node, -1, -1) || HasNeighbor(node, 1, -1)) {
				Log("Discovered ledge");
			}

			return true;
		}

		bool HasNeighbor (Pathfinding.GraphNode node, int xDir, int yDir, bool walkable = true) {
			Pathfinding.Int3 pos = node.position;
			pos.x += (int)(xDir * gridGraph.nodeSize * 1000);
			pos.y += (int)(yDir * gridGraph.nodeSize * 1000);

			Pathfinding.NNInfo posInfo = gridGraph.GetNearest((Vector3)pos);
			return posInfo.node.position == pos && posInfo.node.Walkable == walkable;
		}

		Pathfinding.GraphNode GetNeighbor (Pathfinding.GraphNode node, int xDir, int yDir) {
			Pathfinding.GraphNode nodeTarget = null;

			// We must mimic the int format to try and discover the node we want
			Pathfinding.Int3 pos = node.position;
			pos.x += (int)(xDir * gridGraph.nodeSize * 1000);
			pos.y += (int)(yDir * gridGraph.nodeSize * 1000);

			Pathfinding.NNInfo posInfo = gridGraph.GetNearest((Vector3)pos);
			if (posInfo.node.position == pos)
				nodeTarget = posInfo.node;

			return nodeTarget;
		}

		void Log (string s) {
			if (logDetails) Debug.Log(s);
		}
	}
}

