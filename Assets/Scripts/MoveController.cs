using UnityEngine;
using System.Collections;

public class MoveController : MonoBehaviour {
	// Requires IsKinematic on Rigidbody2d
	public void SimpleMove (Vector3 dir) {
		transform.position += dir;
	}
}
