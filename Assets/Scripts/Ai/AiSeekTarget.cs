using UnityEngine;
using System.Collections;

namespace Ai {
	// Relays mouse position to the seeker so it can be followed on screen
	public class AiSeekTarget : MonoBehaviour {
		[SerializeField] bool snapToMouse;
		[SerializeField] bool snapToClick;

		[SerializeField] bool groundPos;
		[SerializeField] LayerMask whatIsGround;

		void Update () {
			if (snapToMouse) SetCamPos();
			if (snapToClick && Input.GetMouseButtonDown(0)) SetCamPos();
		}

		void SetCamPos () {
			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			pos.z = 0f;
			
			if (groundPos) {
				RaycastHit2D hit = Physics2D.Raycast(pos, Vector3.up * -1f, Mathf.Infinity, whatIsGround);
				if (hit.collider == null) return;
				transform.position = hit.point;
			} else {
				transform.position = pos;
			}
		}
	}
}
