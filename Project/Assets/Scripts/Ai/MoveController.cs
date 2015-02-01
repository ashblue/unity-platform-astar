using UnityEngine;
using System.Collections;

// Handles moving from one point to another on the screen
public class MoveController : MonoBehaviour {
	// Requires IsKinematic on Rigidbody2d
	public void SimpleMove (Vector3 dir) {
		transform.position += dir;
	}

	// Moves the character, but keeps the y axis the same
	public void FeetMove (Vector3 dir) {
//		float prevY = transform.position.y;
		dir.y = 0f;
		transform.position += dir;
	}
}
