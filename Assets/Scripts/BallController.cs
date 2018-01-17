using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour {

	public float throwForce;
	public float addedThrowForce;
	public float maxThrowForce;
	public bool attachedToMouse = false;
	public float lifespan;

	[HideInInspector]
	public bool hasScored = false;

	private Rigidbody rb;
	private float addedForce = 0f;
	private bool isThrowing = false;
	private int powerDirection = 1;
	private IEnumerator destroyBallCoroutine;
	private bool hasCollided = false;

	private float totalForce { get { return Mathf.Clamp (throwForce + addedForce, 0f, maxThrowForce); } }

	void Awake() {
		rb = GetComponent<Rigidbody> ();
	}

	void Update() {
		if (attachedToMouse) {
			transform.position = GameManager.i.mousePosition;
		}
	}

	void FixedUpdate() {
		if (attachedToMouse){
			if (Input.GetMouseButton (0)) {
				isThrowing = true;
				addedForce += Time.fixedDeltaTime * addedThrowForce * powerDirection;
				GameManager.i.powerSlider.value = Mathf.Clamp01(addedForce / (maxThrowForce - throwForce));
				if (powerDirection == 1 && GameManager.i.powerSlider.value == 1f) {
					powerDirection = -1;
				} else if (powerDirection == -1 && GameManager.i.powerSlider.value == 0f) {
					powerDirection = 1;
				}
			}

			if (isThrowing && !Input.GetMouseButton (0)) {
				isThrowing = false;
				attachedToMouse = false;
				rb.velocity = Quaternion.Euler (-60f, 0f, 0f) * Vector3.forward * totalForce;
				GameManager.i.TakeShot ();
				if (GameManager.i.isPlaying && GameManager.i.hasShotsLeft) {
					GameManager.i.CreateBall ();
				}
			}
		}
	}

	void OnCollisionEnter(Collision collision) {
		if (!attachedToMouse && !hasCollided && collision.gameObject.CompareTag ("Ground")) {
			hasCollided = true;
			if (!GameManager.i.hasShotsLeft && GameManager.i.lastBall == this) {
				GameManager.i.SendLevelFailEvent ();
				GameManager.i.GameOver ();
			}
			destroyBallCoroutine = DestroyBall ();
			StartCoroutine (destroyBallCoroutine);
		}
	}

	IEnumerator DestroyBall() {
		float elapsedTime = 0f;
		while (elapsedTime < lifespan) {
			yield return null;
			elapsedTime += Time.deltaTime;
		}
		yield return null;
		Destroy (gameObject);
	}

	public void DestroyBallImmediately() {
		if (destroyBallCoroutine != null) {
			StopAllCoroutines ();
			Destroy (gameObject);
		}
	}

}
