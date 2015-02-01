#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_4_3 || UNITY_4_4 || UNITY_4_5 || UNITY_4_6 || UNITY_4_7 || UNITY_4_8 || UNITY_4_9
#define UNITY_4
#endif

#if UNITY_4_0 || UNITY_4_1 || UNITY_4_2 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3
#define UNITY_LE_4_3
#endif

using UnityEngine;
using UnityEditor;
using Pathfinding;
using System.Collections;
using Pathfinding.Serialization.JsonFx;

namespace Pathfinding {
	/*
	#if !AstarRelease
	[CustomGraphEditor (typeof(CustomGridGraph),"CustomGrid Graph")]
	//[CustomGraphEditor (typeof(LineTraceGraph),"Grid Tracing Graph")]
	#endif
	*/
	[CustomGraphEditor (typeof(Astar.AstarGraphPlatform),"Grid Graph Platform")]
	public class AstarGraphPlatformEditor : GridGraphEditor {

//		public override void OnInspectorGUI (NavGraph target) {
//			base.OnInspectorGUI(target);
//			Astar.AstarGraphPlatform graph = target as Astar.AstarGraphPlatform;
//
//			// @TODO Move all of this into its own container
//			graph.testBool = EditorGUILayout.Toggle("Test Bool", graph.testBool);
//			graph.maxJumpDistance = EditorGUILayout.FloatField("Max Jump Distance", graph.maxJumpDistance);
//
////			data.source = ObjectField ("Source",data.source,typeof(Texture2D),false) as Texture2D;
//			graph.linkOutput = ObjectField("Link Output", graph.linkOutput, typeof(Transform), true) as Transform;
//			graph.jumpPrefab = ObjectField("Jump Prefab", graph.jumpPrefab, typeof(Pathfinding.NodeLink), false) as Pathfinding.NodeLink;

//			graph.linkOutput = (Transform)EditorGUILayout.ObjectField("Link Container", (Object)graph.linkOutput, typeof(Transform), true);
//			graph.jumpPrefab = (Pathfinding.NodeLink)EditorGUILayout.ObjectField("Jump Prefab", (Object)graph.jumpPrefab, typeof(Pathfinding.NodeLink), allowSceneObjects:false);
//		}

		public override void OnSceneGUI (NavGraph target) {
			GridGraph graph = target as GridGraph;
			if (graph.nodes == null) graph.nodes = new GridNode[0];

			base.OnSceneGUI(target);
		}
	}
}