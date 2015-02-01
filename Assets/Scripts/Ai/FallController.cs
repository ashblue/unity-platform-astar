using UnityEngine;
using System.Collections;

public delegate void FallCallback();

public class FallController : MonoBehaviour {
	[Tooltip("Simulates jumps by clicking anywhere on the screen")]
	[SerializeField] bool debugClick;

	[SerializeField] LayerMask whatIsGround;

	[Tooltip("Fall speed based upon distance fallen")]
	[SerializeField] AnimationCurve fallSpeed = new AnimationCurve(new Keyframe(0f, 4.5f), new Keyframe(10f, 5f));

	[Tooltip("How close to the ledge is considered past the ledge")]
	[SerializeField] float ledgeOffset = 0.2f;

	[HideInInspector] public bool falling;

	void Update() {
		if (debugClick && Input.GetMouseButtonDown(0)) {
			SetPos(Camera.main.ScreenToWorldPoint(Input.mousePosition), 2f);
		}
	}

	public void SetPos (Vector3 fallPoint, float moveSpeed, FallCallback callback = null) {
		fallPoint.z = 0f;

		RaycastHit2D groundHit = Physics2D.Raycast(fallPoint, Vector2.up * -1f, Mathf.Infinity, whatIsGround);
		if (groundHit.collider != null) {
			Debug.Log(string.Format("Start {0}, End {1}", fallPoint, groundHit.point));
			StartCoroutine(Fall(fallPoint, groundHit.point, moveSpeed, callback));
		}
	}

	IEnumerator Fall (Vector3 beginPos, Vector3 endPos, float moveSpeed, FallCallback callback) {
		if (falling) yield break;

		falling = true;

		// @TODO Normalized movement here looks kind of funny, might be worth using a harder stop
		// Move the character toward the drop point until they overshoot it
		while (Mathf.Abs(transform.position.x - beginPos.x) > ledgeOffset) {
			Vector3 dir = (beginPos - transform.position).normalized;
			dir *= moveSpeed * Time.fixedDeltaTime;
			dir.y = 0f;
			transform.position += dir;
			yield return null;
		}

		while (transform.position.y > endPos.y) {
			float fallDistance = Vector3.Distance(transform.position, endPos);
			Vector3 dir = (endPos - transform.position).normalized;
			dir *= fallSpeed.Evaluate(fallDistance) * Time.fixedDeltaTime;
			dir.x = 0f;
			transform.position += dir;
			yield return null;
		}

		transform.position = endPos;
		
		if (callback != null) callback();
		falling = false;
	}
}
