using UnityEngine;
using System.Collections;

public delegate void JumpCallback();

public class JumpController : MonoBehaviour {
	[Tooltip("Simulates jumps by clicking anywhere on the screen")]
	[SerializeField] bool debugClick;

	[Tooltip("How high should the jump be appear to be based upon the vertical jump distance")]
	[SerializeField] AnimationCurve jumpHeight = new AnimationCurve(new Keyframe(-4.5f, 2f), new Keyframe(0, 1.25f), new Keyframe(5f, 2f));

	[Tooltip("How long should the jump take based upon the vertical jump distance")]
	[SerializeField] AnimationCurve jumpTime = new AnimationCurve(new Keyframe(-10f, 0.85f), new Keyframe(0, 0.55f),  new Keyframe(10f, 0.85f));

	public bool hopping = false;
	
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			SetPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		}
	}

	public void SetPos (Vector3 pos, JumpCallback callback = null) {
		// Just in-case the position includes a z axis
		pos.z = 0.0f; 

		// Vertical difference of the jump (emulates force)
		float jumpDif = pos.y - transform.position.y; 

		StartCoroutine(Hop(pos, jumpHeight.Evaluate(jumpDif), jumpTime.Evaluate(jumpDif), callback));
	}
	
	IEnumerator Hop (Vector3 dest, float hopHeight, float time, JumpCallback callback) {
		if (hopping) yield break;
		
		hopping = true;
		var startPos = transform.position;
		var timer = 0.0f;
		
		while (timer <= 1.0f) {
			var height = Mathf.Sin(Mathf.PI * timer) * hopHeight;
			transform.position = Vector3.Lerp(startPos, dest, timer) + Vector3.up * height; 
			
			timer += Time.deltaTime / time;
			yield return null;
		}
	
		if (callback != null) callback();
		hopping = false;
	}
}
