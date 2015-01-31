using UnityEngine;
using System.Collections;

// Handles moving from one point to another on the screen
public class MoveController : MonoBehaviour {
	// Requires IsKinematic on Rigidbody2d
	public void SimpleMove (Vector3 dir) {
		transform.position += dir;
	}
}
