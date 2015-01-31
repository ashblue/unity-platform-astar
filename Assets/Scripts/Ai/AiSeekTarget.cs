using UnityEngine;
using System.Collections;

namespace Ai {
	// Relays mouse position to the seeker so it can be followed on screen
	public class AiSeekTarget : MonoBehaviour {
		[SerializeField] bool snapToMouse;
		[SerializeField] bool groundPos;

		void Update () {
			if (!snapToMouse) return;

			Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
			pos.z = 0f;
			transform.position = pos;
		}

		public Vector3 GetPos () {
			if (!groundPos) return transform.position;

			return transform.position;
		}
	}
}
