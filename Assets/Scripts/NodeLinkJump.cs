using UnityEngine;
using System.Collections;
using Pathfinding;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pathfinding {
	[AddComponentMenu("Pathfinding/LinkJump")]
	public class NodeLinkJump : NodeLink {
		[SerializeField] Color linkColor;

		new public void OnDrawGizmos () {
			
			if (Start == null || End == null) return;
			
			Vector3 p1 = Start.position;
			Vector3 p2 = End.position;
			
			Gizmos.color = deleteConnection ? Color.red : linkColor;
			DrawGizmoBezierAlt(p1,p2);
		}

		void DrawGizmoBezierAlt (Vector3 p1, Vector3 p2) {
			
			Vector3 dir = p2-p1;
			
			if (dir == Vector3.zero) return;
			
			Vector3 normal = Vector3.Cross (Vector3.up,dir);
			Vector3 normalUp = Vector3.Cross (dir,normal);
			
			normalUp = normalUp.normalized;
			normalUp *= dir.magnitude*0.1f;
			
			Vector3 p1c = p1+normalUp;
			Vector3 p2c = p2+normalUp;
			
			Vector3 prev = p1;
			for (int i=1;i<=20;i++) {
				float t = i/20.0f;
				Vector3 p = AstarMath.CubicBezier (p1,p1c,p2c,p2,t);
				Gizmos.DrawLine (prev,p);
				prev = p;
			}
		}
	}
}
