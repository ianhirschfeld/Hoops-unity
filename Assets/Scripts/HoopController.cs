using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HoopController : MonoBehaviour {

	public int points;
	public float speed;
	public float range;
	public Text pointsText;

	void Awake() {
		SetPoints (points);
	}

	void Update () {
		if (speed > 0) {
			Vector3 position = transform.position;
			position.x = Mathf.PingPong (Time.time * speed, range * 2) - range;
			transform.position = position;
		}
	}

	void OnTriggerExit(Collider other) {
		BallController ball = other.gameObject.GetComponent<BallController> ();
		if (ball != null && !ball.hasScored) {
			ball.hasScored = true;
			GameManager.i.AddPoints (points);
		}
	}

	public void SetPoints(int pts) {
		points = pts;
		pointsText.text = "" + points;
	}

}
